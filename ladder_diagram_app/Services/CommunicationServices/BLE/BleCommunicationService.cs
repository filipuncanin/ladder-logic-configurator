using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ladder_diagram_app.Services.CommunicationServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ladder_diagram_app.Services.CommunicationServices.BLE
{
    /// <summary>
    /// Provides Bluetooth Low Energy (BLE) communication services for connecting to and interacting with a BLE device.
    /// </summary>
    public class BleCommunicationService : IDeviceCommunicationService, IDisposable
    {
        private BluetoothLEDevice? _bleDevice;
        private GattCharacteristic? _readConfigurationCharacteristic;
        private GattCharacteristic? _writeConfigurationCharacteristic;
        private GattCharacteristic? _readMonitorCharacteristic;
        private GattCharacteristic? _readOneWireCharacteristic;

        private readonly Guid _serviceUuid = Guid.Parse("00001234-0000-1000-8000-00805f9b34fb");
        private readonly Guid _readConfigurationCharUuid = Guid.Parse("0000FFF1-0000-1000-8000-00805f9b34fb");
        private readonly Guid _writeConfigurationCharUuid = Guid.Parse("0000FFF2-0000-1000-8000-00805f9b34fb");
        private readonly Guid _readMonitorCharUuid = Guid.Parse("0000FFF3-0000-1000-8000-00805f9b34fb");
        private readonly Guid _readOneWireCharUuid = Guid.Parse("0000FFF4-0000-1000-8000-00805f9b34fb");

        public event EventHandler<string>? ConfigurationReceived;
        public event EventHandler<string>? MonitorDataReceived;
        public event EventHandler<string>? OneWireDataReceived;
        public event EventHandler<bool>? ConnectionStatusChanged;

        /// <summary>
        /// Gets a value indicating whether the service is connected to a device.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the type of connection, which is "BLE".
        /// </summary>
        public string ConnectionType => "BLE";

        private StringBuilder _jsonConfigurationBuffer = new StringBuilder();
        private StringBuilder _jsonMonitorBuffer = new StringBuilder();
        private StringBuilder _jsonOneWireBuffer = new StringBuilder();

        private bool _monitorTaskRunning = false;
        private bool _oneWireTaskRunning = false;

        private readonly int ChunkSize = 250; // Maximum chunk size for data transfer, accounting for ATT overhead

        /// <summary>
        /// Attempts to connect to a BLE device with retries.
        /// </summary>
        /// <param name="deviceId">The ID of the device to connect to.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <returns>True if connection is successful, otherwise false.</returns>
        private async Task<bool> ConnectToDeviceWithRetry(string deviceId, int maxRetries = 5)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    _bleDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
                    if (_bleDevice != null) return true;
                    retryCount++;
                    await Task.Delay(1000 * retryCount);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BLE Connection attempt {retryCount + 1} failed: {ex.Message}");
                    retryCount++;
                    await Task.Delay(1000 * retryCount);
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves GATT services from the device with retries.
        /// </summary>
        /// <param name="device">The BLE device to query.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <returns>The GATT services result, or null if unsuccessful.</returns>
        private async Task<GattDeviceServicesResult?> GetServicesWithRetry(BluetoothLEDevice device, int maxRetries = 5)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                var result = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result != null && result.Status == GattCommunicationStatus.Success)
                    return result;
                retryCount++;
                await Task.Delay(1000 * retryCount);
            }
            return null;
        }

        /// <summary>
        /// Connects to a BLE device asynchronously.
        /// </summary>
        /// <param name="deviceId">The ID of the device to connect to.</param>
        /// <returns>True if connection is successful, otherwise false.</returns>
        public async Task<bool> ConnectAsync(string deviceId)
        {
            try
            {
                if (IsConnected) return true;

                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    Debug.WriteLine("BLE Connection failed: Device ID is null or empty");
                    return false;
                }

                bool connected = await ConnectToDeviceWithRetry(deviceId);
                if (!connected || _bleDevice == null)
                {
                    await DisconnectAsync();
                    Debug.WriteLine("BLE Failed to connect to device after multiple attempts");
                    return false;
                }

                var servicesResult = await GetServicesWithRetry(_bleDevice);
                if (servicesResult == null || servicesResult.Status != GattCommunicationStatus.Success)
                {
                    await DisconnectAsync();
                    Debug.WriteLine("BLE Failed to get services after multiple attempts");
                    return false;
                }

                bool setupCharacteristicsSuccess = await SetupCharacteristics(servicesResult);
                if (!setupCharacteristicsSuccess) return false;

                await RequestConfigurationAsync();

                IsConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);

                _jsonMonitorBuffer.Clear();
                _jsonOneWireBuffer.Clear();

                if (!_monitorTaskRunning)
                {
                    _monitorTaskRunning = true;
                    _ = Task.Run(() => ReadMonitorBle());
                }
                if (!_oneWireTaskRunning)
                {
                    _oneWireTaskRunning = true;
                    _ = Task.Run(() => ReadOneWireBle());
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the BLE device and cleans up resources.
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (!IsConnected || _bleDevice == null) return;

                IsConnected = false;
                _monitorTaskRunning = false;
                _oneWireTaskRunning = false;

                var servicesResult = await _bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (servicesResult.Status == GattCommunicationStatus.Success)
                {
                    foreach (var service in servicesResult.Services)
                    {
                        service.Dispose();
                    }
                }

                _readConfigurationCharacteristic = null;
                _writeConfigurationCharacteristic = null;
                _readMonitorCharacteristic = null;
                _readOneWireCharacteristic = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Disconnection failed: {ex.Message}");
            }
            finally
            {
                _bleDevice?.Dispose();
                _bleDevice = null;
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Sets up the required GATT characteristics for communication.
        /// </summary>
        /// <param name="servicesResult">The GATT services result.</param>
        /// <returns>True if setup is successful, otherwise false.</returns>
        private async Task<bool> SetupCharacteristics(GattDeviceServicesResult servicesResult)
        {
            try
            {
                foreach (var service in servicesResult.Services)
                {
                    if (service.Uuid == _serviceUuid)
                    {
                        var characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status != GattCommunicationStatus.Success)
                        {
                            continue;
                        }

                        foreach (var characteristic in characteristicsResult.Characteristics)
                        {
                            if (characteristic.Uuid == _readConfigurationCharUuid)
                                _readConfigurationCharacteristic = characteristic;
                            else if (characteristic.Uuid == _writeConfigurationCharUuid)
                                _writeConfigurationCharacteristic = characteristic;
                            else if (characteristic.Uuid == _readMonitorCharUuid)
                                _readMonitorCharacteristic = characteristic;
                            else if (characteristic.Uuid == _readOneWireCharUuid)
                                _readOneWireCharacteristic = characteristic;
                        }
                    }
                }

                if (_readConfigurationCharacteristic == null || _writeConfigurationCharacteristic == null ||
                    _readMonitorCharacteristic == null || _readOneWireCharacteristic == null)
                {
                    await DisconnectAsync();
                    Debug.WriteLine("BLE Required characteristics not found.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Characteristics Setup failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Requests the configuration from the device.
        /// </summary>
        public async Task RequestConfigurationAsync()
        {
            try
            {
                _jsonConfigurationBuffer.Clear();
                while (true)
                {
                    var readTask = _readConfigurationCharacteristic?.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask();
                    if (readTask == null)
                    {
                        Debug.WriteLine("BLE Read configuration failed: Characteristic is null");
                        break;
                    }
                    if (await Task.WhenAny(readTask, Task.Delay(5000)) != readTask)
                    {
                        Debug.WriteLine("BLE Read configuration timeout");
                        break;
                    }
                    var readResult = await readTask;

                    if (readResult.Status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine("BLE Failed to read configuration");
                        break;
                    }

                    var reader = DataReader.FromBuffer(readResult.Value);
                    byte[] data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    if (data.Length == 0)
                    {
                        string completeJson = _jsonConfigurationBuffer.ToString();
                        _jsonConfigurationBuffer.Clear();
                        ConfigurationReceived?.Invoke(this, completeJson);
                        break;
                    }

                    _jsonConfigurationBuffer.Append(Encoding.UTF8.GetString(data));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Reading Configuration failed: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Sends a JSON configuration to the device.
        /// </summary>
        /// <param name="configJson">The JSON configuration string to send.</param>
        /// <returns>True if sending is successful, otherwise false.</returns>
        public async Task<bool> SendConfigurationAsync(string configJson)
        {
            try
            {
                if (!IsConnected) return false;

                byte[] jsonBytes = Encoding.UTF8.GetBytes(configJson);

                for (int i = 0; i < jsonBytes.Length; i += ChunkSize)
                {
                    int chunkLength = Math.Min(ChunkSize, jsonBytes.Length - i);
                    byte[] chunk = new byte[chunkLength];
                    Array.Copy(jsonBytes, i, chunk, 0, chunkLength);

                    using var writer = new DataWriter();
                    writer.WriteBytes(chunk);
                    var writeResult = await _writeConfigurationCharacteristic?.WriteValueAsync(writer.DetachBuffer());
                    if (writeResult != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine("BLE Failed to write configuration");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Error writing configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Continuously reads monitor data from the device.
        /// </summary>
        private async Task ReadMonitorBle()
        {
            try
            {
                while (IsConnected && _bleDevice != null)
                {
                    var readTask = _readMonitorCharacteristic?.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask();
                    if (readTask == null)
                    {
                        Debug.WriteLine("BLE Read Monitor Data failed: Characteristic is null");
                        break;
                    }
                    if (await Task.WhenAny(readTask, Task.Delay(5000)) != readTask)
                    {
                        Debug.WriteLine("BLE Read Monitor Data timeout");
                        break;
                    }
                    var readResult = await readTask;

                    if (readResult.Status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine("BLE Failed to read monitor data");
                        break;
                    }

                    var reader = DataReader.FromBuffer(readResult.Value);
                    byte[] data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    if (data.Length == 0)
                    {
                        string completeJson = _jsonMonitorBuffer.ToString();
                        if (!string.IsNullOrEmpty(completeJson))
                        {
                            MonitorDataReceived?.Invoke(this, completeJson);
                            _jsonMonitorBuffer.Clear();
                        }
                    }
                    else
                    {
                        _jsonMonitorBuffer.Append(Encoding.UTF8.GetString(data));
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"BLE: Invalid JSON in monitor data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Error reading monitor data: {ex.Message}");
            }
        }

        /// <summary>
        /// Continuously reads one-wire data from the device.
        /// </summary>
        private async Task ReadOneWireBle()
        {
            try
            {
                while (IsConnected && _bleDevice != null)
                {
                    var readTask = _readOneWireCharacteristic?.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask();
                    if (readTask == null)
                    {
                        Debug.WriteLine("BLE Read One Wire Data failed: Characteristic is null");
                        break;
                    }
                    if (await Task.WhenAny(readTask, Task.Delay(5000)) != readTask)
                    {
                        Debug.WriteLine("BLE Read One Wire Data timeout");
                        break;
                    }
                    var readResult = await readTask;

                    if (readResult.Status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine("BLE Failed to read one wire data");
                        break;
                    }

                    var reader = DataReader.FromBuffer(readResult.Value);
                    byte[] data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    if (data.Length == 0)
                    {
                        string completeJson = _jsonOneWireBuffer.ToString();
                        if (!string.IsNullOrEmpty(completeJson))
                        {
                            OneWireDataReceived?.Invoke(this, completeJson);
                            _jsonOneWireBuffer.Clear();
                        }
                    }
                    else
                    {
                        _jsonOneWireBuffer.Append(Encoding.UTF8.GetString(data));
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"BLE: Invalid JSON in one wire data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Error reading one wire: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes of the service and releases resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_bleDevice == null) return;

                _monitorTaskRunning = false;
                _oneWireTaskRunning = false;

                var servicesTask = _bleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                var servicesResult = servicesTask.AsTask().GetAwaiter().GetResult();
                if (servicesResult.Status == GattCommunicationStatus.Success)
                {
                    foreach (var service in servicesResult.Services)
                    {
                        service.Dispose();
                    }
                }

                _readConfigurationCharacteristic = null;
                _writeConfigurationCharacteristic = null;
                _readMonitorCharacteristic = null;
                _readOneWireCharacteristic = null;

                _bleDevice.Dispose();
                _bleDevice = null;

                GC.Collect();
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLE Dispose failed: {ex.Message}");
            }
        }
    }
}
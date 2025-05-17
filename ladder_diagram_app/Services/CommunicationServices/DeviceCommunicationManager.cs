using System.Windows;
using System.Windows.Threading;
using ladder_diagram_app.Services.CommunicationServices.BLE.BluetoothSelection;
using ladder_diagram_app.Views;

namespace ladder_diagram_app.Services.CommunicationServices
{
    /// <summary>
    /// Manages device communication, handling connection, disconnection, and configuration exchange for MQTT and BLE protocols.
    /// </summary>
    public class DeviceCommunicationManager : IDisposable
    {
        public IDeviceCommunicationService? _communicationService { get; set; }
        private readonly BleDeviceWatcher _bleDeviceWatcher;
        private readonly Action<string> _onConfigurationReceived;
        private readonly Action<bool> _onConnectionStatusChanged;
        private readonly Action<string> _onMonitorDataReceived;
        private readonly Action<string> _onOneWireDataReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceCommunicationManager"/> class.
        /// </summary>
        /// <param name="onConfigurationReceived">Callback for handling received configuration data.</param>
        /// <param name="onConnectionStatusChanged">Callback for handling connection status changes.</param>
        /// <param name="onMonitorDataReceived">Callback for handling received monitor data.</param>
        /// <param name="onOneWireDataReceived">Callback for handling received one-wire data.</param>
        /// <exception cref="ArgumentNullException">Thrown if any callback is null.</exception>
        public DeviceCommunicationManager(
            Action<string> onConfigurationReceived,
            Action<bool> onConnectionStatusChanged,
            Action<string> onMonitorDataReceived,
            Action<string> onOneWireDataReceived)
        {
            _onConfigurationReceived = onConfigurationReceived ?? throw new ArgumentNullException(nameof(onConfigurationReceived));
            _onConnectionStatusChanged = onConnectionStatusChanged ?? throw new ArgumentNullException(nameof(onConnectionStatusChanged));
            _onMonitorDataReceived = onMonitorDataReceived ?? throw new ArgumentNullException(nameof(onMonitorDataReceived));
            _onOneWireDataReceived = onOneWireDataReceived ?? throw new ArgumentNullException(nameof(onOneWireDataReceived));
            _bleDeviceWatcher = new BleDeviceWatcher();
        }

        /// <summary>
        /// Retrieves the device ID based on the connection type (MQTT or BLE).
        /// </summary>
        /// <param name="owner">The owner window for displaying dialogs.</param>
        /// <param name="connectionType">The type of connection ("MQTT" or "BLE").</param>
        /// <returns>The device ID, or an empty string if no device is selected.</returns>
        private string GetDeviceId(Window owner, string connectionType)
        {
            if (connectionType == "MQTT")
            {
                var macInput = new NotificationWindow("Enter the device's MAC address", owner, NotificationButtons.OneInput, new[] { "" });
                macInput.ShowDialog();
                if (macInput.Result == true) return macInput.InputResults[0].ToUpper();
            }
            else
            {
                _bleDeviceWatcher.InitializeBle();
                var deviceSelection = new BleDeviceSelectionWindow(_bleDeviceWatcher.Devices, owner);
                if (deviceSelection.ShowDialog() == true)
                {
                    var selectedDevice = deviceSelection.SelectedDevice;
                    _bleDeviceWatcher.StopWatcher();
                    _bleDeviceWatcher.Dispose();
                    return selectedDevice?.Id ?? string.Empty;
                }
                _bleDeviceWatcher.StopWatcher();
                _bleDeviceWatcher.Dispose();
            }
            return string.Empty;
        }

        /// <summary>
        /// Connects to a device asynchronously using the specified connection type.
        /// </summary>
        /// <param name="owner">The owner window for displaying dialogs.</param>
        /// <param name="connectionType">The type of connection ("MQTT" or "BLE").</param>
        public async Task ConnectAsync(Window owner, string connectionType)
        {
            try
            {
                if (_communicationService != null && _communicationService.IsConnected)
                {
                    new NotificationWindow("Device is already connected", owner).Show();
                    return;
                }

                _communicationService = CommunicationServiceFactory.CreateService(connectionType);

                if (_communicationService != null)
                {
                    _communicationService.ConfigurationReceived -= (s, json) => _onConfigurationReceived(json);
                    _communicationService.MonitorDataReceived -= (s, json) => _onMonitorDataReceived(json);
                    _communicationService.OneWireDataReceived -= (s, json) => _onOneWireDataReceived(json);
                    _communicationService.ConnectionStatusChanged -= (s, isConnected) => _onConnectionStatusChanged(isConnected);
                }

                _communicationService.ConfigurationReceived += (s, json) => owner.Dispatcher.InvokeAsync(() => _onConfigurationReceived(json));
                _communicationService.MonitorDataReceived += (s, json) => owner.Dispatcher.InvokeAsync(() => _onMonitorDataReceived(json));
                _communicationService.OneWireDataReceived += (s, json) => owner.Dispatcher.InvokeAsync(() => _onOneWireDataReceived(json));
                _communicationService.ConnectionStatusChanged += (s, isConnected) => owner.Dispatcher.InvokeAsync(() => _onConnectionStatusChanged(isConnected));


                string deviceId = GetDeviceId(owner, connectionType);
                if (string.IsNullOrEmpty(deviceId)) return;

                bool success = await _communicationService.ConnectAsync(deviceId);
                if (!success) new NotificationWindow("Device Connected Unsuccessfully", owner).Show();
            }
            catch (Exception ex)
            {
                new NotificationWindow($"Connection failed: {ex.Message}", owner).Show();
            }
        }

        /// <summary>
        /// Disconnects from the device asynchronously.
        /// </summary>
        /// <param name="owner">The owner window for displaying dialogs.</param>
        public async Task DisconnectAsync(Window owner)
        {
            if (_communicationService != null && _communicationService.IsConnected)
            {
                await _communicationService.DisconnectAsync();
            }
            else
            {
                new NotificationWindow("Device is not connected", owner).Show();
            }
        }

        /// <summary>
        /// Sends a JSON configuration to the connected device.
        /// </summary>
        /// <param name="jsonConfig">The JSON configuration string to send.</param>
        /// <param name="owner">The owner window for displaying dialogs.</param>
        public async Task SendConfigurationAsync(string jsonConfig, Window owner)
        {
            if (_communicationService == null || !_communicationService.IsConnected)
            {
                new NotificationWindow("Device is not connected", owner).Show();
                return;
            }

            bool success = await _communicationService.SendConfigurationAsync(jsonConfig);
            new NotificationWindow(success ? "Configuration sent successfully" : "Configuration sent unsuccessfully", owner).Show();
        }

        /// <summary>
        /// Disposes of the communication service and BLE device watcher, releasing resources.
        /// </summary>
        public void Dispose()
        {
            if (_communicationService != null)
            {
                _communicationService.ConfigurationReceived -= (s, json) => _onConfigurationReceived(json);
                _communicationService.MonitorDataReceived -= (s, json) => _onMonitorDataReceived(json);
                _communicationService.OneWireDataReceived -= (s, json) => _onOneWireDataReceived(json);
                _communicationService.ConnectionStatusChanged -= (s, isConnected) => _onConnectionStatusChanged(isConnected);
                _communicationService.Dispose();
                _communicationService = null;
            }
            _bleDeviceWatcher?.Dispose();
        }
    }
}
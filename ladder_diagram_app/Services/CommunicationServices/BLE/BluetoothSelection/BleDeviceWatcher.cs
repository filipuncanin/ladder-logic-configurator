using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.Enumeration;

namespace ladder_diagram_app.Services.CommunicationServices.BLE.BluetoothSelection
{
    /// <summary>
    /// Monitors and manages Bluetooth Low Energy (BLE) devices using a device watcher.
    /// </summary>
    public class BleDeviceWatcher : IDisposable
    {
        private readonly ObservableCollection<DeviceInformation> _devices = new ObservableCollection<DeviceInformation>();
        private DeviceWatcher? _deviceWatcher;

        /// <summary>
        /// Gets the collection of discovered BLE devices.
        /// </summary>
        public ObservableCollection<DeviceInformation> Devices => _devices;

        /// <summary>
        /// Initializes and starts the BLE device watcher if not already running.
        /// </summary>
        public void InitializeBle()
        {
            if (_deviceWatcher != null && _deviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                return;
            }

            // Define properties to retrieve for each device
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
            // Filter for BLE devices using the BLE protocol ID
            string aqsFilter = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            // Create a device watcher for BLE association endpoints
            _deviceWatcher = DeviceInformation.CreateWatcher(
                aqsFilter,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint);

            // Attach event handlers
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Updated += DeviceWatcher_Updated;
            _deviceWatcher.Removed += DeviceWatcher_Removed;

            _deviceWatcher.Start();
        }

        /// <summary>
        /// Handles the addition of a new BLE device.
        /// </summary>
        /// <param name="sender">The device watcher that raised the event.</param>
        /// <param name="deviceInfo">Information about the added device.</param>
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Only add devices with a non-empty name
                    if (!string.IsNullOrWhiteSpace(deviceInfo.Name))
                    {
                        _devices.Add(deviceInfo);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BLE Device Watcher Error adding device: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Handles updates to an existing BLE device.
        /// </summary>
        /// <param name="sender">The device watcher that raised the event.</param>
        /// <param name="deviceInfoUpdate">Updated information about the device.</param>
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var device = _devices.FirstOrDefault(d => d.Id == deviceInfoUpdate.Id);
                    if (device != null)
                    {
                        device.Update(deviceInfoUpdate);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BLE Device Watcher Error updating device: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Handles the removal of a BLE device.
        /// </summary>
        /// <param name="sender">The device watcher that raised the event.</param>
        /// <param name="deviceInfoUpdate">Information about the removed device.</param>
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var device = _devices.FirstOrDefault(d => d.Id == deviceInfoUpdate.Id);
                    if (device != null)
                    {
                        _devices.Remove(device);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BLE Device Watcher Error removing device: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Stops the device watcher if it is running.
        /// </summary>
        public void StopWatcher()
        {
            if (_deviceWatcher != null && _deviceWatcher.Status != DeviceWatcherStatus.Stopped)
            {
                _deviceWatcher.Stop();
            }
        }

        /// <summary>
        /// Disposes of the device watcher and releases resources.
        /// </summary>
        public void Dispose()
        {
            StopWatcher();
            if (_deviceWatcher != null)
            {
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Updated -= DeviceWatcher_Updated;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;
                _deviceWatcher = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
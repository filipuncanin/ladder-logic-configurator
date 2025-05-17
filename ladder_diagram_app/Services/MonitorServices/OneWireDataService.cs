using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using ladder_diagram_app.Models.DeviceElement;
using ladder_diagram_app.Views;

namespace ladder_diagram_app.Services.MonitorServices
{
    /// <summary>
    /// Manages one-wire sensor data processing and UI updates for the main window.
    /// </summary>
    public class OneWireDataService
    {
        private readonly MainWindow _mainWindow;
        private readonly Device _device;
        private string? _lastOneWireMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireDataService"/> class.
        /// </summary>
        /// <param name="mainWindow">The main window for UI updates and notifications.</param>
        /// <param name="device">The device configuration containing one-wire input data.</param>
        public OneWireDataService(MainWindow mainWindow, Device device)
        {
            _mainWindow = mainWindow;
            _device = device;
            _lastOneWireMessage = null;
        }

        /// <summary>
        /// Clears the last stored one-wire message.
        /// </summary>
        public void DeleteLastOneWireMessage()
        {
            _lastOneWireMessage = null;
        }

        /// <summary>
        /// Processes incoming one-wire data and updates the UI asynchronously.
        /// </summary>
        /// <param name="oneWireData">The JSON string containing one-wire sensor data.</param>
        public void OnOneWireDataReceived(string oneWireData)
        {
            if (_mainWindow.Dispatcher.HasShutdownStarted || _mainWindow.Dispatcher.HasShutdownFinished)
                return;

            _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                ProcessOneWireMessage(oneWireData);
            });
        }

        /// <summary>
        /// Processes a one-wire message, parsing it and refreshing the sensor list if it differs from the last message.
        /// </summary>
        /// <param name="message">The JSON string to process.</param>
        private void ProcessOneWireMessage(string message)
        {
            try
            {
                if (_lastOneWireMessage != message)
                {
                    _lastOneWireMessage = message;

                    var jsonDoc = JsonDocument.Parse(_lastOneWireMessage);
                    var root = jsonDoc.RootElement;

                    RefreshOneWireSensors(root);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing one wire message: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the list of one-wire sensors displayed in the UI based on device configuration and optional MQTT data.
        /// </summary>
        /// <param name="root">The JSON root element containing MQTT data, if available.</param>
        private void RefreshOneWireSensors(JsonElement? root = null)
        {
            var sensorViewModels = new List<OneWireSensorViewModel>();

            // Populate sensors from device configuration
            for (int i = 0; i < _device.one_wire_inputs.Count; i++)
            {
                int pin = _device.one_wire_inputs[i];
                var names = _device.one_wire_inputs_names[i];
                var types = _device.one_wire_inputs_devices_types[i];
                var addresses = _device.one_wire_inputs_devices_addresses[i];

                for (int j = 0; j < addresses.Count; j++)
                {
                    sensorViewModels.Add(new OneWireSensorViewModel
                    {
                        Pin = pin,
                        Address = addresses[j],
                        Type = types[j],
                        SensorName = names[j],
                        IsInDevice = true,
                        IsFromMqtt = false
                    });
                }
            }

            // Merge with MQTT data, if available
            if (root.HasValue && root.Value.TryGetProperty("pins", out var pinsArray))
            {
                foreach (var pinElement in pinsArray.EnumerateArray())
                {
                    if (pinElement.TryGetProperty("pin", out var pinProp) && pinElement.TryGetProperty("addresses", out var addressesProp))
                    {
                        int pinNumber = pinProp.GetInt32();

                        foreach (var address in addressesProp.EnumerateArray())
                        {
                            string? addressValue = address.GetString();
                            if (addressValue != null)
                            {
                                var sensor = new OneWireSensor(addressValue);
                                var existingSensor = sensorViewModels.FirstOrDefault(s => s.Pin == pinNumber && s.Address == sensor.Address);
                                if (existingSensor != null)
                                {
                                    existingSensor.IsFromMqtt = true;
                                }
                                else
                                {
                                    sensorViewModels.Add(new OneWireSensorViewModel
                                    {
                                        Pin = pinNumber,
                                        Address = sensor.Address,
                                        Type = sensor.Type,
                                        SensorName = "",
                                        IsInDevice = false,
                                        IsFromMqtt = true
                                    });
                                }
                            }
                        }
                    }
                }
            }

            _mainWindow.OneWireItemsControl.ItemsSource = sensorViewModels;
        }

        /// <summary>
        /// Handles the click event for action buttons to add or remove one-wire sensors.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">The routed event arguments.</param>
        public void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(_device.DeviceInfo());

            var button = (Button)sender;
            var sensor = (OneWireSensorViewModel)button.Tag;

            int pinIndex = _device.one_wire_inputs.IndexOf(sensor.Pin);

            if (sensor.IsNotInDeviceAndFromMqtt)
            {
                // Add new sensor
                if (string.IsNullOrWhiteSpace(sensor.SensorName))
                {
                    new NotificationWindow("Sensor name cannot be empty!", _mainWindow).Show();
                    return;
                }

                if (string.IsNullOrWhiteSpace(sensor.Type) || string.IsNullOrWhiteSpace(sensor.Address))
                {
                    new NotificationWindow("Sensor Type / Address cannot be empty!", _mainWindow).Show();
                    return;
                }

                bool nameExists = _device.one_wire_inputs_names.SelectMany(list => list).Any(name => name.Equals(sensor.SensorName, StringComparison.OrdinalIgnoreCase));

                if (nameExists)
                {
                    new NotificationWindow($"Sensor with name '{sensor.SensorName}' already exists!", _mainWindow).Show();
                    return;
                }

                _device.one_wire_inputs_names[pinIndex].Add(sensor.SensorName);
                _device.one_wire_inputs_devices_types[pinIndex].Add(sensor.Type);
                _device.one_wire_inputs_devices_addresses[pinIndex].Add(sensor.Address);

                new NotificationWindow($"Sensor '{sensor.SensorName}' added successfully!", _mainWindow).Show();

                _mainWindow._devicePinManager.OneWireInputOptions.Add(sensor.SensorName);

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(_lastOneWireMessage))
                    {
                        try
                        {
                            var jsonDoc = JsonDocument.Parse(_lastOneWireMessage);
                            RefreshOneWireSensors(jsonDoc.RootElement);
                        }
                        catch
                        {
                            RefreshOneWireSensors();
                        }
                    }
                    else
                    {
                        RefreshOneWireSensors();
                    }
                });
            }
            else if (sensor.IsInDevice && pinIndex != -1)
            {
                // Remove existing sensor
                int sensorIndex = sensor.Address != null ? _device.one_wire_inputs_devices_addresses[pinIndex].IndexOf(sensor.Address) : -1;

                if (sensorIndex >= 0)
                {
                    string removedName = _device.one_wire_inputs_names[pinIndex][sensorIndex];
                    _device.one_wire_inputs_names[pinIndex].RemoveAt(sensorIndex);
                    _device.one_wire_inputs_devices_types[pinIndex].RemoveAt(sensorIndex);
                    _device.one_wire_inputs_devices_addresses[pinIndex].RemoveAt(sensorIndex);

                    new NotificationWindow($"Sensor '{removedName}' removed successfully from pin {sensor.Pin}!", _mainWindow).Show();

                    if (sensor.SensorName != null)
                    {
                        _mainWindow._devicePinManager.OneWireInputOptions.Remove(sensor.SensorName);
                    }

                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(_lastOneWireMessage))
                        {
                            try
                            {
                                var jsonDoc = JsonDocument.Parse(_lastOneWireMessage);
                                RefreshOneWireSensors(jsonDoc.RootElement);
                            }
                            catch
                            {
                                RefreshOneWireSensors();
                            }
                        }
                        else
                        {
                            RefreshOneWireSensors();
                        }
                    });
                }
                else
                {
                    new NotificationWindow($"Sensor with address '{sensor.Address}' not found on pin {sensor.Pin}!", _mainWindow).Show();
                }
            }
            else
            {
                new NotificationWindow($"Pin {sensor.Pin} not found in device configuration!", _mainWindow).Show();
            }
        }

        /// <summary>
        /// Represents a view model for one-wire sensors, used for UI display.
        /// </summary>
        private class OneWireSensorViewModel
        {
            public int Pin { get; set; }
            public string? Address { get; set; }
            public string? Type { get; set; }
            public string? SensorName { get; set; }
            public bool IsInDevice { get; set; }
            public bool IsFromMqtt { get; set; }
            public bool IsInDeviceAndFromMqtt => IsInDevice && IsFromMqtt;
            public bool IsInDeviceAndNotFromMqtt => IsInDevice && !IsFromMqtt;
            public bool IsNotInDeviceAndFromMqtt => !IsInDevice && IsFromMqtt;
        }

        /// <summary>
        /// Represents a one-wire sensor with address and type information.
        /// </summary>
        private class OneWireSensor
        {
            public string Address { get; set; }
            public string Type { get; set; }

            public OneWireSensor(string address)
            {
                Address = address;
                Type = GetSensorType(address);
            }

            /// <summary>
            /// Determines the sensor type based on the family code in the address.
            /// </summary>
            /// <param name="address">The sensor address.</param>
            /// <returns>The sensor type or "Unknown" if not recognized.</returns>
            private string GetSensorType(string address)
            {
                if (string.IsNullOrEmpty(address)) return "Unknown";

                string familyCode = address.Length >= 2 ? address.Substring(address.Length - 2, 2) : "00";

                switch (familyCode)
                {
                    case "10": return "DS18S20/DS1820 (Temperature Sensor)";
                    case "22": return "DS1822 (Temperature Sensor)";
                    case "28": return "DS18B20 (Temperature Sensor)";
                    case "3B": return "MAX31850 (Temperature Sensor)";
                    case "26": return "DS2438 (Smart Battery Monitor)";
                    case "1D": return "DS2423 (4k RAM with Counter)";
                    case "29": return "DS2408 (8-Channel Addressable Switch)";
                    case "12": return "DS2406/DS2407 (Dual Addressable Switch)";
                    case "20": return "DS2450 (4-Channel ADC)";
                    case "21": return "DS1921 (Thermochron)";
                    case "2D": return "DS2431 (1k EEPROM)";
                    case "01": return "DS1990A (Serial Number)";
                    case "04": return "DS2404 (RAM/Time)";
                    case "14": return "DS1971 (256-bit EEPROM)";
                    case "1F": return "DS2409 (MicroLAN Coupler)";
                    default: return $"Unknown (Family Code: {familyCode})";
                }
            }
        }
    }
}
using ladder_diagram_app.Views;
using System.Windows;

namespace ladder_diagram_app.Models.DeviceElement
{
    /// <summary>
    /// Represents a device in a ladder diagram application, encapsulating its properties and configuration.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// Gets or sets the logic voltage of the device.
        /// </summary>
        public double logic_voltage { get; set; }

        /// <summary>
        /// Gets or sets the list of digital input pins.
        /// </summary>
        public List<int> digital_inputs { get; set; }

        /// <summary>
        /// Gets or sets the names of the digital input pins.
        /// </summary>
        public List<string> digital_inputs_names { get; set; }

        /// <summary>
        /// Gets or sets the list of digital output pins.
        /// </summary>
        public List<int> digital_outputs { get; set; }

        /// <summary>
        /// Gets or sets the names of the digital output pins.
        /// </summary>
        public List<string> digital_outputs_names { get; set; }

        /// <summary>
        /// Gets or sets the list of analog input pins.
        /// </summary>
        public List<int> analog_inputs { get; set; }

        /// <summary>
        /// Gets or sets the names of the analog input pins.
        /// </summary>
        public List<string> analog_inputs_names { get; set; }

        /// <summary>
        /// Gets or sets the list of DAC output pins.
        /// </summary>
        public List<int> dac_outputs { get; set; }

        /// <summary>
        /// Gets or sets the names of the DAC output pins.
        /// </summary>
        public List<string> dac_outputs_names { get; set; }

        /// <summary>
        /// Gets or sets the list of one-wire input pins.
        /// </summary>
        public List<int> one_wire_inputs { get; set; }

        /// <summary>
        /// Gets or sets the names of the one-wire input devices.
        /// </summary>
        public List<List<string>> one_wire_inputs_names { get; set; }

        /// <summary>
        /// Gets or sets the types of one-wire input devices.
        /// </summary>
        public List<List<string>> one_wire_inputs_devices_types { get; set; }

        /// <summary>
        /// Gets or sets the addresses of one-wire input devices.
        /// </summary>
        public List<List<string>> one_wire_inputs_devices_addresses { get; set; }

        /// <summary>
        /// Gets or sets the number of PWM channels.
        /// </summary>
        public int pwm_channels { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of hardware timers.
        /// </summary>
        public int max_hardware_timers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device supports RTOS.
        /// </summary>
        public bool has_rtos { get; set; }

        /// <summary>
        /// Gets or sets the list of UART channels.
        /// </summary>
        public List<int> UART { get; set; }

        /// <summary>
        /// Gets or sets the list of I2C channels.
        /// </summary>
        public List<int> I2C { get; set; }

        /// <summary>
        /// Gets or sets the list of SPI channels.
        /// </summary>
        public List<int> SPI { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device supports USB.
        /// </summary>
        public bool USB { get; set; }

        /// <summary>
        /// Gets or sets the list of parent device names.
        /// </summary>
        public List<string> parent_devices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class with default empty values.
        /// </summary>
        public Device()
        {
            device_name = string.Empty;
            logic_voltage = 0;
            digital_inputs = [];
            digital_inputs_names = [];
            digital_outputs = [];
            digital_outputs_names = [];
            analog_inputs = [];
            analog_inputs_names = [];
            dac_outputs = [];
            dac_outputs_names = [];
            one_wire_inputs = [];
            one_wire_inputs_names = [];
            one_wire_inputs_devices_types = [];
            one_wire_inputs_devices_addresses = [];
            pwm_channels = 0;
            max_hardware_timers = 0;
            has_rtos = false;
            UART = [];
            I2C = [];
            SPI = [];
            USB = false;
            parent_devices = [];
        }

        /// <summary>
        /// Checks if the device is loaded by verifying if the device name is set.
        /// </summary>
        /// <returns>True if the device name is not null or empty, false otherwise.</returns>
        public bool IsDeviceLoaded()
        {
            return !string.IsNullOrEmpty(device_name);
        }

        /// <summary>
        /// Opens a dialog window to add parent devices to the current device.
        /// </summary>
        /// <param name="owner">The owner window for the dialog.</param>
        public void AddParentDevices(Window owner)
        {
            var addParentsWindow = new AddParentsWindow(parent_devices, owner);
            addParentsWindow.ShowDialog();
        }

        /// <summary>
        /// Updates the current device's properties from another device instance.
        /// </summary>
        /// <param name="device">The source device to copy properties from.</param>
        public void UpdateFrom(Device device)
        {
            if (device == null)
                return; // Exit if source device is null

            device_name = device.device_name ?? string.Empty;
            logic_voltage = device.logic_voltage;
            digital_inputs = device.digital_inputs != null ? new List<int>(device.digital_inputs) : new List<int>();
            digital_inputs_names = device.digital_inputs_names != null ? new List<string>(device.digital_inputs_names) : new List<string>();
            digital_outputs = device.digital_outputs != null ? new List<int>(device.digital_outputs) : new List<int>();
            digital_outputs_names = device.digital_outputs_names != null ? new List<string>(device.digital_outputs_names) : new List<string>();
            analog_inputs = device.analog_inputs != null ? new List<int>(device.analog_inputs) : new List<int>();
            analog_inputs_names = device.analog_inputs_names != null ? new List<string>(device.analog_inputs_names) : new List<string>();
            dac_outputs = device.dac_outputs != null ? new List<int>(device.dac_outputs) : new List<int>();
            dac_outputs_names = device.dac_outputs_names != null ? new List<string>(device.dac_outputs_names) : new List<string>();
            one_wire_inputs = device.one_wire_inputs != null ? new List<int>(device.one_wire_inputs) : new List<int>();
            one_wire_inputs_names = device.one_wire_inputs_names != null
                ? device.one_wire_inputs_names.Select(inner => inner != null ? new List<string>(inner) : new List<string>()).ToList()
                : new List<List<string>>();
            one_wire_inputs_devices_types = device.one_wire_inputs_devices_types != null
                ? device.one_wire_inputs_devices_types.Select(inner => inner != null ? new List<string>(inner) : new List<string>()).ToList()
                : new List<List<string>>();
            one_wire_inputs_devices_addresses = device.one_wire_inputs_devices_addresses != null
                ? device.one_wire_inputs_devices_addresses.Select(inner => inner != null ? new List<string>(inner) : new List<string>()).ToList()
                : new List<List<string>>();
            pwm_channels = device.pwm_channels;
            max_hardware_timers = device.max_hardware_timers;
            has_rtos = device.has_rtos;
            UART = device.UART != null ? new List<int>(device.UART) : new List<int>();
            I2C = device.I2C != null ? new List<int>(device.I2C) : new List<int>();
            SPI = device.SPI != null ? new List<int>(device.SPI) : new List<int>();
            USB = device.USB;
            parent_devices = device.parent_devices != null ? new List<string>(device.parent_devices) : new List<string>();
        }

        /// <summary>
        /// Validates the device's properties to ensure they meet basic requirements.
        /// </summary>
        /// <returns>True if the device is valid, false otherwise.</returns>
        public bool Validate()
        {
            // Check if device name is set
            if (string.IsNullOrEmpty(device_name)) return false;
            return true; // Additional validations can be added as needed
        }

        /// <summary>
        /// Generates a string representation of the device's properties.
        /// </summary>
        /// <returns>A formatted string containing device information, or a message if no device is loaded.</returns>
        public string DeviceInfo()
        {
            // Return placeholder if no device is loaded
            if (string.IsNullOrEmpty(device_name))
                return "No device loaded";

            // Build detailed device information string
            return $"Device Name: {device_name}\n" +
                   $"Logic Voltage: {logic_voltage}\n" +
                   $"Digital Inputs: [{string.Join(", ", digital_inputs)}]\n" +
                   $"Digital Inputs Names: [{string.Join(", ", digital_inputs_names)}]\n" +
                   $"Digital Outputs: [{string.Join(", ", digital_outputs)}]\n" +
                   $"Digital Outputs Names: [{string.Join(", ", digital_outputs_names)}]\n" +
                   $"Analog Inputs: [{string.Join(", ", analog_inputs)}]\n" +
                   $"Analog Inputs Names: [{string.Join(", ", analog_inputs_names)}]\n" +
                   $"DAC Outputs: [{string.Join(", ", dac_outputs)}]\n" +
                   $"DAC Outputs Names: [{string.Join(", ", dac_outputs_names)}]\n" +
                   $"One Wire Inputs: [{string.Join(", ", one_wire_inputs)}]\n" +
                   $"One Wire Inputs Names: [{FormatListOfLists(one_wire_inputs_names)}]\n" +
                   $"One Wire Inputs Devices Types: [{FormatListOfLists(one_wire_inputs_devices_types)}]\n" +
                   $"One Wire Inputs Devices Addresses: [{FormatListOfLists(one_wire_inputs_devices_addresses)}]\n" +
                   $"PWM Channels: {pwm_channels}\n" +
                   $"Max Hardware Timers: {max_hardware_timers}\n" +
                   $"Has RTOS: {has_rtos}\n" +
                   $"UART: [{string.Join(", ", UART)}]\n" +
                   $"I2C: [{string.Join(", ", I2C)}]\n" +
                   $"SPI: [{string.Join(", ", SPI)}]\n" +
                   $"USB: {USB}\n" +
                   $"Parent Devices: [{string.Join(", ", parent_devices)}]";
        }

        /// <summary>
        /// Formats a list of lists into a string representation for display.
        /// </summary>
        /// <param name="listOfLists">The list of lists to format.</param>
        /// <returns>A formatted string with semicolon-separated inner lists.</returns>
        private static string FormatListOfLists(List<List<string>> listOfLists)
        {
            if (listOfLists == null)
                return "null";

            // Join inner lists with commas and outer lists with semicolons
            return string.Join("; ",
                listOfLists.Select(innerList =>
                    $"[{string.Join(", ", innerList)}]"));
        }
    }
}

using System.Collections.ObjectModel;

namespace ladder_diagram_app.Models.DeviceElement
{
    /// <summary>
    /// Manages pin mapping options for a device, providing collections of available digital, analog, and one-wire pin names.
    /// </summary>
    public class DevicePinManager
    {
        /// <summary>
        /// Gets the collection of digital input pin names available for mapping.
        /// </summary>
        public ObservableCollection<string> DigitalInputOptions { get; }

        /// <summary>
        /// Gets the collection of digital output pin names available for mapping.
        /// </summary>
        public ObservableCollection<string> DigitalOutputOptions { get; }

        /// <summary>
        /// Gets the collection of analog input pin names available for mapping.
        /// </summary>
        public ObservableCollection<string> AnalogInputOptions { get; }

        /// <summary>
        /// Gets the collection of analog output (DAC) pin names available for mapping.
        /// </summary>
        public ObservableCollection<string> AnalogOutputOptions { get; }

        /// <summary>
        /// Gets the collection of one-wire input pin names available for mapping.
        /// </summary>
        public ObservableCollection<string> OneWireInputOptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePinManager"/> class with empty pin option collections.
        /// </summary>
        public DevicePinManager()
        {
            DigitalInputOptions = [];
            DigitalOutputOptions = [];
            AnalogInputOptions = [];
            AnalogOutputOptions = [];
            OneWireInputOptions = [];
        }

        /// <summary>
        /// Updates the pin option collections based on the provided device's pin names.
        /// </summary>
        /// <param name="device">The device whose pin names are used to update the options.</param>
        public void UpdateDevicePinOptions(Device device)
        {
            // Clear existing pin options
            DigitalInputOptions.Clear();
            DigitalOutputOptions.Clear();
            AnalogInputOptions.Clear();
            AnalogOutputOptions.Clear();
            OneWireInputOptions.Clear();

            // Populate digital input options
            foreach (var pin in device.digital_inputs_names)
                DigitalInputOptions.Add(pin.ToString());

            // Populate digital output options
            foreach (var pin in device.digital_outputs_names)
                DigitalOutputOptions.Add(pin.ToString());

            // Populate analog input options
            foreach (var pin in device.analog_inputs_names)
                AnalogInputOptions.Add(pin.ToString());

            // Populate analog output (DAC) options
            foreach (var pin in device.dac_outputs_names)
                AnalogOutputOptions.Add(pin.ToString());

            // Populate one-wire input options from nested lists
            foreach (var x in device.one_wire_inputs_names)
            {
                foreach (var y in x)
                    OneWireInputOptions.Add(y.ToString());
            }
        }
    }
}

namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents an ADC sensor variable in a ladder diagram, encapsulating sensor-specific properties and validation.
    /// </summary>
    public class ADCSensorVariable : Variable
    {
        /// <summary>
        /// Stores the type of the sensor.
        /// </summary>
        private string? _sensorType;

        /// <summary>
        /// Stores the PD_SCK pin identifier.
        /// </summary>
        private string? _pdSck;

        /// <summary>
        /// Stores the DOUT pin identifier.
        /// </summary>
        private string? _dout;

        /// <summary>
        /// Stores the low mapping value for the sensor.
        /// </summary>
        private double _mapLow;

        /// <summary>
        /// Stores the high mapping value for the sensor.
        /// </summary>
        private double _mapHigh;

        /// <summary>
        /// Stores the gain value for the sensor.
        /// </summary>
        private double _gain;

        /// <summary>
        /// Stores the sampling rate of the sensor.
        /// </summary>
        private string? _samplingRate;

        /// <summary>
        /// Stores the current value of the sensor.
        /// </summary>
        private string _value;

        /// <summary>
        /// Gets or sets the type of the sensor, notifying subscribers on change.
        /// </summary>
        public string? SensorType
        {
            get => _sensorType;
            set => SetField(ref _sensorType, value);
        }

        /// <summary>
        /// Gets or sets the PD_SCK pin identifier, notifying subscribers on change.
        /// </summary>
        public string? PD_SCK
        {
            get => _pdSck;
            set => SetField(ref _pdSck, value);
        }

        /// <summary>
        /// Gets or sets the DOUT pin identifier, notifying subscribers on change.
        /// </summary>
        public string? DOUT
        {
            get => _dout;
            set => SetField(ref _dout, value);
        }

        /// <summary>
        /// Gets or sets the low mapping value for the sensor, notifying subscribers on change.
        /// </summary>
        public double MapLow
        {
            get => _mapLow;
            set => SetField(ref _mapLow, value);
        }

        /// <summary>
        /// Gets or sets the high mapping value for the sensor, notifying subscribers on change.
        /// </summary>
        public double MapHigh
        {
            get => _mapHigh;
            set => SetField(ref _mapHigh, value);
        }

        /// <summary>
        /// Gets or sets the gain value for the sensor, notifying subscribers on change.
        /// </summary>
        public double Gain
        {
            get => _gain;
            set => SetField(ref _gain, value);
        }

        /// <summary>
        /// Gets or sets the sampling rate of the sensor, notifying subscribers on change.
        /// </summary>
        public string? SamplingRate
        {
            get => _samplingRate;
            set => SetField(ref _samplingRate, value);
        }

        /// <summary>
        /// Gets or sets the current value of the sensor, notifying subscribers on change.
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ADCSensorVariable"/> class with default values.
        /// </summary>
        public ADCSensorVariable()
        {
            Type = "ADC Sensor";
            _value = "▼";
            MapLow = 0.0;
            MapHigh = 100.0;
            Gain = 1.0;
        }

        /// <summary>
        /// Gets a value indicating whether the variable is valid based on required properties.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(SensorType) &&
                               !string.IsNullOrEmpty(PD_SCK) &&
                               !string.IsNullOrEmpty(DOUT) &&
                               Gain >= 0 &&
                               !string.IsNullOrEmpty(SamplingRate);

        /// <summary>
        /// Converts the variable's properties to a dictionary for export purposes.
        /// </summary>
        /// <returns>A dictionary containing the variable's properties and their values.</returns>
        public override Dictionary<string, object> ToExportDictionary()
        {
            return new Dictionary<string, object>
            {
                ["Type"] = Type,
                ["Name"] = Name,
                ["Sensor Type"] = SensorType ?? string.Empty,
                ["PD_SCK"] = PD_SCK ?? string.Empty,
                ["DOUT"] = DOUT ?? string.Empty,
                ["Map Low"] = MapLow,
                ["Map High"] = MapHigh,
                ["Gain"] = Gain,
                ["Sampling Rate"] = SamplingRate ?? string.Empty
            };
        }
    }
}

namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents a one-wire input variable in a ladder diagram, associated with a specific pin.
    /// </summary>
    public class OneWireInputVariable : Variable
    {
        /// <summary>
        /// Stores the name of the pin associated with the one-wire input.
        /// </summary>
        private string _pinName;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireInputVariable"/> class with an empty pin name.
        /// </summary>
        public OneWireInputVariable()
        {
            _pinName = string.Empty;
        }

        /// <summary>
        /// Gets or sets the name of the pin associated with the one-wire input, notifying subscribers on change.
        /// </summary>
        public string PinName
        {
            get => _pinName;
            set => SetField(ref _pinName, value);
        }

        /// <summary>
        /// Gets a value indicating whether the variable is valid based on the presence of a pin name.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(PinName);

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
                ["Pin"] = PinName
            };
        }
    }
}
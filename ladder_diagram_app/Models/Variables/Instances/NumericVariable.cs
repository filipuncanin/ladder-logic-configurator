namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents a numeric variable in a ladder diagram, encapsulating a double-precision value.
    /// </summary>
    public class NumericVariable : Variable
    {
        /// <summary>
        /// Stores the numeric value of the variable.
        /// </summary>
        private double _value;

        /// <summary>
        /// Gets or sets the numeric value of the variable, notifying subscribers on change.
        /// </summary>
        public double Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericVariable"/> class with default values.
        /// </summary>
        public NumericVariable()
        {
            Type = "Number";
            Value = 0.0; // Default value
        }

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
                ["Value"] = Value
            };
        }
    }
}
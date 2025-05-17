namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents a boolean variable in a ladder diagram, encapsulating a true/false value.
    /// </summary>
    public class BooleanVariable : Variable
    {
        /// <summary>
        /// Stores the boolean value of the variable.
        /// </summary>
        private bool _value;

        /// <summary>
        /// Gets or sets the boolean value of the variable, notifying subscribers on change.
        /// </summary>
        public bool Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanVariable"/> class with default values.
        /// </summary>
        public BooleanVariable()
        {
            Type = "Boolean";
            Value = false; // Default value
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

namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents a timer variable in a ladder diagram, encapsulating preset time, elapsed time, input, and output states.
    /// </summary>
    public class TimerVariable : Variable
    {
        /// <summary>
        /// Stores the preset time (PT) of the timer.
        /// </summary>
        private double _pt;

        /// <summary>
        /// Stores the elapsed time (ET) of the timer.
        /// </summary>
        private double _et;

        /// <summary>
        /// Stores the input (IN) state of the timer.
        /// </summary>
        private bool _in;

        /// <summary>
        /// Stores the output (Q) state of the timer.
        /// </summary>
        private bool _q;

        /// <summary>
        /// Stores the display value of the timer.
        /// </summary>
        private string _value;

        /// <summary>
        /// Gets or sets the preset time (PT) of the timer, notifying subscribers on change.
        /// </summary>
        public double PT
        {
            get => _pt;
            set => SetField(ref _pt, value);
        }

        /// <summary>
        /// Gets or sets the elapsed time (ET) of the timer, notifying subscribers on change.
        /// </summary>
        public double ET
        {
            get => _et;
            set => SetField(ref _et, value);
        }

        /// <summary>
        /// Gets or sets the input (IN) state of the timer, notifying subscribers on change.
        /// </summary>
        public bool IN
        {
            get => _in;
            set => SetField(ref _in, value);
        }

        /// <summary>
        /// Gets or sets the output (Q) state of the timer, notifying subscribers on change.
        /// </summary>
        public bool Q
        {
            get => _q;
            set => SetField(ref _q, value);
        }

        /// <summary>
        /// Gets or sets the display value of the timer, notifying subscribers on change.
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerVariable"/> class with default values.
        /// </summary>
        public TimerVariable()
        {
            Type = "Timer";
            _value = "▼"; // Default placeholder value
        }

        /// <summary>
        /// Gets a value indicating whether the variable is valid based on the preset time.
        /// </summary>
        public bool IsValid => PT >= 0;

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
                ["PT"] = PT,
                ["ET"] = ET,
                ["IN"] = IN,
                ["Q"] = Q
            };
        }
    }
}
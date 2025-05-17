namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Represents a counter variable in a ladder diagram, encapsulating preset and current values, count direction, and output states.
    /// </summary>
    public class CounterVariable : Variable
    {
        /// <summary>
        /// Stores the preset value of the counter.
        /// </summary>
        private double _pv;

        /// <summary>
        /// Stores the current value of the counter.
        /// </summary>
        private double _cv;

        /// <summary>
        /// Stores the count up state.
        /// </summary>
        private bool _cu;

        /// <summary>
        /// Stores the count down state.
        /// </summary>
        private bool _cd;

        /// <summary>
        /// Stores the up output state.
        /// </summary>
        private bool _qu;

        /// <summary>
        /// Stores the down output state.
        /// </summary>
        private bool _qd;

        /// <summary>
        /// Stores the display value of the counter.
        /// </summary>
        private string _value;

        /// <summary>
        /// Gets or sets the preset value (PV) of the counter, notifying subscribers on change.
        /// </summary>
        public double PV
        {
            get => _pv;
            set => SetField(ref _pv, value);
        }

        /// <summary>
        /// Gets or sets the current value (CV) of the counter, notifying subscribers on change.
        /// </summary>
        public double CV
        {
            get => _cv;
            set => SetField(ref _cv, value);
        }

        /// <summary>
        /// Gets or sets the count up (CU) state, resetting count down (CD) if set to true, and notifying subscribers on change.
        /// </summary>
        public bool CU
        {
            get => _cu;
            set
            {
                if (SetField(ref _cu, value) && value)
                    CD = false; // Ensure CD is false when CU is true
            }
        }

        /// <summary>
        /// Gets or sets the count down (CD) state, resetting count up (CU) if set to true, and notifying subscribers on change.
        /// </summary>
        public bool CD
        {
            get => _cd;
            set
            {
                if (SetField(ref _cd, value) && value)
                    CU = false; // Ensure CU is false when CD is true
            }
        }

        /// <summary>
        /// Gets or sets the up output (QU) state, notifying subscribers on change.
        /// </summary>
        public bool QU
        {
            get => _qu;
            set => SetField(ref _qu, value);
        }

        /// <summary>
        /// Gets or sets the down output (QD) state, notifying subscribers on change.
        /// </summary>
        public bool QD
        {
            get => _qd;
            set => SetField(ref _qd, value);
        }

        /// <summary>
        /// Gets or sets the display value of the counter, notifying subscribers on change.
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CounterVariable"/> class with default values.
        /// </summary>
        public CounterVariable()
        {
            Type = "Counter";
            _value = "▼"; // Default placeholder value
            CU = true; // Default to count up
        }

        /// <summary>
        /// Converts the variable's properties to a dictionary for export purposes.
        /// </summary>
        /// <returns>A dictionary containing the variable's properties and their values.</returns>
        public override Dictionary<string, object> ToExportDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                ["Type"] = Type,
                ["Name"] = Name,
                ["PV"] = PV,
                ["CU"] = CU,
                ["CD"] = CD,
                ["QU"] = QU,
                ["QD"] = QD
            };

            // Set CV based on count direction
            if (CU)
                dict["CV"] = 0; // Start at 0 for count up
            else if (CD)
                dict["CV"] = PV; // Start at preset value for count down

            return dict;
        }
    }
}
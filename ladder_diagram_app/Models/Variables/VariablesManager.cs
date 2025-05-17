using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ladder_diagram_app.Models.Variables.Instances;
using ladder_diagram_app.Models.DeviceElement;
using System.Globalization;
using ladder_diagram_app.Views;
using System.Diagnostics;

namespace ladder_diagram_app.Models.Variables
{
    /// <summary>
    /// Manages variables in a ladder diagram application, associating them with a device and maintaining lists for UI components.
    /// </summary>
    public class VariablesManager
    {
        /// <summary>
        /// Gets or sets the device associated with the variables.
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// Gets the collection of all variables.
        /// </summary>
        public ObservableCollection<Variable> VariablesList { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for contact elements in ladder diagrams.
        /// </summary>
        public ObservableCollection<string> VariablesListContacts { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for coil elements in ladder diagrams.
        /// </summary>
        public ObservableCollection<string> VariablesListCoils { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for mathematical operations.
        /// </summary>
        public ObservableCollection<string> VariablesListMath { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for comparison operations.
        /// </summary>
        public ObservableCollection<string> VariablesListCompare { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for counter operations.
        /// </summary>
        public ObservableCollection<string> VariablesListCounter { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for timer operations.
        /// </summary>
        public ObservableCollection<string> VariablesListTimer { get; } = [];

        /// <summary>
        /// Gets the collection of variable names for reset operations.
        /// </summary>
        public ObservableCollection<string> VariablesListReset { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="VariablesManager"/> class with a specified device.
        /// </summary>
        /// <param name="device">The device to associate with the variables.</param>
        public VariablesManager(Device device)
        {
            Device = device;
            VariablesList.CollectionChanged += VariablesList_CollectionChanged;
        }

        // ==================== VARIABLE LIST MANAGEMENT =======================
        /// <summary>
        /// Clears all variable lists.
        /// </summary>
        public void ClearVariablesList()
        {
            VariablesList.Clear();
            VariablesListContacts.Clear();
            VariablesListCoils.Clear();
            VariablesListMath.Clear();
            VariablesListCompare.Clear();
            VariablesListCounter.Clear();
            VariablesListTimer.Clear();
            VariablesListReset.Clear();
        }

        /// <summary>
        /// Adds a new variable to the list with specified properties.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable (e.g., Digital Input, Boolean).</param>
        /// <param name="owner">The owner window for displaying notifications.</param>
        /// <param name="pinName">The pin name for input/output variables.</param>
        /// <param name="sensorType">The sensor type for ADC sensors.</param>
        /// <param name="pdSck">The PD_SCK pin for ADC sensors.</param>
        /// <param name="dout">The DOUT pin for ADC sensors.</param>
        /// <param name="samplingRate">The sampling rate for ADC sensors.</param>
        /// <param name="mapLow">The low mapping value for ADC sensors.</param>
        /// <param name="mapHigh">The high mapping value for ADC sensors.</param>
        /// <param name="gain">The gain value for ADC sensors.</param>
        /// <param name="boolValue">The boolean value for Boolean variables.</param>
        /// <param name="numValue">The numeric value for Number variables.</param>
        /// <param name="pv">The preset value for Counter variables.</param>
        /// <param name="cv">The current value for Counter variables.</param>
        /// <param name="cu">The count-up flag for Counter variables.</param>
        /// <param name="cd">The count-down flag for Counter variables.</param>
        /// <param name="pt">The preset time for Timer variables.</param>
        /// <param name="et">The elapsed time for Timer variables.</param>
        /// <param name="timeValue">The time value for Time variables.</param>
        public void AddVariable(string name, string type, Window owner,
                                  string pinName = "",
                                  string sensorType = "", string pdSck = "", string dout = "", string samplingRate = "", double mapLow = 0.0, double mapHigh = 100.0, double gain = 1.0,
                                  bool boolValue = false,
                                  double numValue = 0.0,
                                  double pv = 0.0, double cv = 0.0, bool cu = true, bool cd = false,
                                  double pt = 0.0, double et = 0.0,
                                  double timeValue = 0.0)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                new NotificationWindow("A Variable must have a name", owner).Show();
                return;
            }
            if (VariablesList.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                new NotificationWindow("A Variable with the same name already exists", owner).Show();
                return;
            }

            Variable? newVariable = null;

            switch (type)
            {
                case "Digital Input":
                    if (VariablesList.Count(v => v.Type == "Digital Input") == Device.digital_inputs_names.Count)
                    {
                        if (Device.digital_inputs_names.Count() == 0)
                        {
                            new NotificationWindow("The device has no digital inputs", owner).Show();
                            return;
                        }
                        else
                        {
                            new NotificationWindow("The device has no more digital inputs", owner).Show();
                            return;
                        }
                    }
                    newVariable = new DigitalAnalogInputOutputVariable { Name = name, Type = type, PinName = pinName };
                    break;
                case "Digital Output":
                    if (VariablesList.Count(v => v.Type == "Digital Output") == Device.digital_outputs_names.Count)
                    {
                        if (Device.digital_outputs_names.Count() == 0)
                        {
                            new NotificationWindow("The device has no digital outputs", owner).Show();
                            return;
                        }
                        else
                        {
                            new NotificationWindow("The device has no more digital outputs", owner).Show();
                            return;
                        }
                    }
                    newVariable = new DigitalAnalogInputOutputVariable { Name = name, Type = type, PinName = pinName };
                    break;
                case "Analog Input":
                    if (VariablesList.Count(v => v.Type == "Analog Input") == Device.analog_inputs_names.Count)
                    {
                        if (Device.analog_inputs_names.Count() == 0)
                        {
                            new NotificationWindow("The device has no analog inputs", owner).Show();
                            return;
                        }
                        else
                        {
                            new NotificationWindow("The device has no more analog inputs", owner).Show();
                            return;
                        }
                    }
                    newVariable = new DigitalAnalogInputOutputVariable { Name = name, Type = type, PinName = pinName };
                    break;
                case "Analog Output":
                    if (VariablesList.Count(v => v.Type == "Analog Output") == Device.dac_outputs_names.Count)
                    {
                        if (Device.dac_outputs_names.Count == 0)
                        {
                            new NotificationWindow("The device has no analog outputs", owner).Show();
                            return;
                        }
                        else
                        {
                            new NotificationWindow("The device has no more analog outputs", owner).Show();
                            return;
                        }
                    }
                    newVariable = new DigitalAnalogInputOutputVariable { Name = name, Type = type, PinName = pinName };
                    break;
                case "One Wire Input":
                    if (VariablesList.Count(v => v.Type == "One Wire Input") >= Device.one_wire_inputs_names.Sum(innerList => innerList?.Count ?? 0))
                    {
                        if (Device.one_wire_inputs_names.Count == 0)
                        {
                            new NotificationWindow("The device has no one wire inputs", owner).Show();
                            return;
                        }
                        else
                        {
                            new NotificationWindow("The device has no more one wire inputs", owner).Show();
                            return;
                        }
                    }
                    newVariable = new OneWireInputVariable { Name = name, Type = type, PinName = pinName };
                    break;
                case "ADC Sensor":
                    if (Device.digital_outputs_names.Count == 0 || Device.digital_inputs_names.Count == 0)
                    {
                        new NotificationWindow("The device has no digital inputs or outputs to use for ADC sensor", owner).Show();
                        return;
                    }
                    newVariable = new ADCSensorVariable { Name = name, Type = type, SensorType = sensorType, PD_SCK = pdSck, DOUT = dout, MapLow = mapLow, MapHigh = mapHigh, Gain = gain, SamplingRate = samplingRate };
                    break;
                case "Boolean":
                    newVariable = new BooleanVariable { Name = name, Type = type, Value = boolValue };
                    break;
                case "Number":
                    newVariable = new NumericVariable { Name = name, Type = type, Value = numValue };
                    break;
                case "Counter":
                    newVariable = new CounterVariable { Name = name, Type = type, PV = pv, CV = cv, CU = cu, CD = cd };
                    break;
                case "Timer":
                    newVariable = new TimerVariable { Name = name, Type = type, PT = pt, ET = et };
                    break;
                case "Current Time":
                    if (VariablesList.Count(v => v.Type == "Current Time") == 1)
                    {
                        new NotificationWindow("There is already a variable that represents current time", owner).Show();
                        return;
                    }
                    newVariable = new TimeVariable { Name = name, Type = type };
                    break;
                case "Time":
                    newVariable = new TimeVariable { Name = name, Type = type, Value = timeValue };
                    break;
            }

            if (newVariable != null)
            {
                VariablesList.Add(newVariable);
                return;
            }

            new NotificationWindow($"Failed to add variable '{name}' of type '{type}'", owner).Show();
            return;
        }

        /// <summary>
        /// Deletes a variable from the list, handling expanded properties if necessary.
        /// </summary>
        /// <param name="variable">The variable to delete.</param>
        /// <param name="owner">The owner window for displaying notifications.</param>
        public void DeleteVariable(Variable variable, Window owner)
        {
            if (!variable.IsDeletable)
            {
                new NotificationWindow("This item cannot be deleted", owner).Show();
                return;
            }

            // Handle expanded ADC Sensor properties
            if (variable is ADCSensorVariable adcs && adcs.Value == "▲")
            {
                int index = VariablesList.IndexOf(variable);
                for (int i = 0; i < 7; i++)
                    VariablesList.RemoveAt(index + 1);
            }
            // Handle expanded Counter properties
            else if (variable is CounterVariable c && c.Value == "▲")
            {
                int index = VariablesList.IndexOf(variable);
                for (int i = 0; i < 6; i++)
                    VariablesList.RemoveAt(index + 1);
            }
            // Handle expanded Timer properties
            else if (variable is TimerVariable t && t.Value == "▲")
            {
                int index = VariablesList.IndexOf(variable);
                for (int i = 0; i < 4; i++)
                    VariablesList.RemoveAt(index + 1);
            }

            VariablesList.Remove(variable);
        }

        /// <summary>
        /// Toggles boolean values or expands/collapses variable properties when clicked.
        /// </summary>
        /// <param name="variable">The variable to modify.</param>
        public void VariableBooleanClick(Variable variable)
        {
            int index = VariablesList.IndexOf(variable);

            if (variable is BooleanVariable b)
            {
                b.Value = !b.Value;
                // Update Counter properties if CU or CD is toggled
                if (variable.Name == "     CU" || variable.Name == "     CD")
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        if (VariablesList[i].Type == "Counter" && VariablesList[i] is CounterVariable cnt)
                        {
                            if (variable.Name == "     CU")
                            {
                                cnt.CU = b.Value;
                                cnt.CD = !b.Value;
                                if (VariablesList[index + 1] is BooleanVariable cdBool)
                                    cdBool.Value = !b.Value;
                            }
                            else if (variable.Name == "     CD")
                            {
                                cnt.CD = b.Value;
                                cnt.CU = !b.Value;
                                if (VariablesList[index - 1] is BooleanVariable cdBool)
                                    cdBool.Value = !b.Value;
                            }
                            break;
                        }
                    }
                }
            }

            // Expand or collapse ADC Sensor properties
            if (variable is ADCSensorVariable adcs)
            {
                if (adcs.Value == "▼")
                {
                    VariablesList.Insert(index + 1, new DigitalAnalogInputOutputVariable { Name = "     Sensor Type", Type = "ADC Sensor Type", PinName = adcs.SensorType ?? string.Empty, IsDeletable = false });
                    VariablesList.Insert(index + 2, new DigitalAnalogInputOutputVariable { Name = "     PD_SCK", Type = "ADC Sensor Digital Output", PinName = adcs.PD_SCK ?? string.Empty, IsDeletable = false });
                    VariablesList.Insert(index + 3, new DigitalAnalogInputOutputVariable { Name = "     DOUT", Type = "ADC Sensor Digital Input", PinName = adcs.DOUT ?? string.Empty, IsDeletable = false });
                    VariablesList.Insert(index + 4, new NumericVariable { Name = "     Map Low", Type = "Number", Value = adcs.MapLow, IsDeletable = false });
                    VariablesList.Insert(index + 5, new NumericVariable { Name = "     Map High", Type = "Number", Value = adcs.MapHigh, IsDeletable = false });
                    VariablesList.Insert(index + 6, new NumericVariable { Name = "     Gain", Type = "Number", Value = adcs.Gain, IsDeletable = false });
                    VariablesList.Insert(index + 7, new DigitalAnalogInputOutputVariable { Name = "     Sampling Rate", Type = "ADC Sensor Sampling Rate", PinName = adcs.SamplingRate ?? string.Empty, IsDeletable = false });
                    adcs.Value = "▲";
                }
                else
                {
                    for (int i = index + 1; i <= index + 7; i++)
                        VariablesList.RemoveAt(index + 1);
                    adcs.Value = "▼";
                }
            }

            // Expand or collapse Counter properties
            if (variable is CounterVariable c)
            {
                if (c.Value == "▼")
                {
                    VariablesList.Insert(index + 1, new NumericVariable { Name = "     PV", Type = "Number", Value = c.PV, IsDeletable = false });
                    VariablesList.Insert(index + 2, new NumericVariable { Name = "     CV", Type = "Number ", Value = c.CV, IsDeletable = false });
                    VariablesList.Insert(index + 3, new BooleanVariable { Name = "     CU", Type = "Boolean", Value = c.CU, IsDeletable = false });
                    VariablesList.Insert(index + 4, new BooleanVariable { Name = "     CD", Type = "Boolean", Value = c.CD, IsDeletable = false });
                    VariablesList.Insert(index + 5, new BooleanVariable { Name = "     QU", Type = "Boolean ", IsDeletable = false });
                    VariablesList.Insert(index + 6, new BooleanVariable { Name = "     QD", Type = "Boolean ", IsDeletable = false });
                    c.Value = "▲";
                }
                else
                {
                    for (int i = index + 1; i <= index + 6; i++)
                        VariablesList.RemoveAt(index + 1);
                    c.Value = "▼";
                }
            }

            // Expand or collapse Timer properties
            if (variable is TimerVariable t)
            {
                if (t.Value == "▼")
                {
                    VariablesList.Insert(index + 1, new TimeVariable { Name = "     PT", Type = "Number", Value = t.PT, IsDeletable = false });
                    VariablesList.Insert(index + 2, new TimeVariable { Name = "     ET", Type = "Number ", Value = t.ET, IsDeletable = false });
                    VariablesList.Insert(index + 3, new BooleanVariable { Name = "     IN", Type = "Boolean ", IsDeletable = false });
                    VariablesList.Insert(index + 4, new BooleanVariable { Name = "     Q", Type = "Boolean ", IsDeletable = false });
                    t.Value = "▲";
                }
                else
                {
                    for (int i = index + 1; i <= index + 4; i++)
                        VariablesList.RemoveAt(index + 1);
                    t.Value = "▼";
                }
            }
        }

        /// <summary>
        /// Updates the double value of a variable or its parameters based on text input.
        /// </summary>
        /// <param name="variable">The variable to update.</param>
        /// <param name="inputText">The text input to parse as a double.</param>
        public void VariableTextBoxChange(Variable variable, string inputText)
        {
            if (!double.TryParse(inputText, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double parsedValue))
                return;

            int index = VariablesList.IndexOf(variable);

            if (variable is NumericVariable n)
                n.Value = parsedValue;
            else if (variable is TimeVariable t)
                t.Value = parsedValue;

            // Update ADC Sensor parameters
            if (variable.Name == "     Map Low")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.MapLow = parsedValue;
                        Debug.WriteLine($"Updating Map Low to : {adcs.MapLow}");
                        break;
                    }
                }
            }
            else if (variable.Name == "     Map High")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.MapHigh = parsedValue;
                        break;
                    }
                }
            }
            else if (variable.Name == "     Gain")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.Gain = parsedValue;
                        break;
                    }
                }
            }

            // Update Counter parameters
            else if (variable.Name == "     PV")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "Counter" && VariablesList[i] is CounterVariable cnt)
                    {
                        cnt.PV = parsedValue;
                        break;
                    }
                }
            }

            // Update Timer parameters
            else if (variable.Name == "     PT")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "Timer" && VariablesList[i] is TimerVariable tmr)
                    {
                        tmr.PT = parsedValue;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates ComboBox values for variables or their parameters.
        /// </summary>
        /// <param name="variable">The variable to update.</param>
        /// <param name="selectedValue">The selected ComboBox value.</param>
        public void VariableComboBoxChange(Variable variable, string selectedValue)
        {
            int index = VariablesList.IndexOf(variable);

            // Update ADC Sensor parameters
            if (variable.Name == "     Sensor Type")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.SensorType = selectedValue;
                        break;
                    }
                }
            }
            else if (variable.Name == "     PD_SCK")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.PD_SCK = selectedValue;
                        break;
                    }
                }
            }
            else if (variable.Name == "     DOUT")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.DOUT = selectedValue;
                        break;
                    }
                }
            }
            else if (variable.Name == "     Sampling Rate")
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (VariablesList[i].Type == "ADC Sensor" && VariablesList[i] is ADCSensorVariable adcs)
                    {
                        adcs.SamplingRate = selectedValue;
                        break;
                    }
                }
            }

            // Update PinName for DigitalAnalogInputOutput variables
            if (variable is DigitalAnalogInputOutputVariable daio)
            {
                daio.PinName = selectedValue;
            }
        }

        // ==================== COMBOBOX VALUE TRACKING =======================
        /// <summary>
        /// Handles changes to the VariablesList collection, updating related collections.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void VariablesList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (Variable variable in e.NewItems)
                    {
                        AddVariableToCollections(variable);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (Variable variable in e.OldItems)
                    {
                        RemoveVariableFromCollections(variable);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a variable to the appropriate collections based on its type.
        /// </summary>
        /// <param name="variable">The variable to add.</param>
        private void AddVariableToCollections(Variable variable)
        {
            if (variable.Type == "Digital Input")
            {
                VariablesListContacts.Add(variable.Name);
            }
            else if (variable.Type == "Digital Output")
            {
                VariablesListContacts.Add(variable.Name);
                VariablesListCoils.Add(variable.Name);
            }
            else if (variable.Type == "Analog Input" || variable.Type == "Analog Output")
            {
                VariablesListMath.Add(variable.Name);
                VariablesListCompare.Add(variable.Name);
            }
            else if (variable.Type == "One Wire Input")
            {
                VariablesListMath.Add(variable.Name);
                VariablesListCompare.Add(variable.Name);
            }
            else if (variable.Type == "ADC Sensor")
            {
                VariablesListMath.Add(variable.Name);
                VariablesListCompare.Add(variable.Name);
            }
            else if (variable.Type == "Boolean" && !variable.Name.Contains("     "))
            {
                VariablesListContacts.Add(variable.Name);
                VariablesListCoils.Add(variable.Name);
            }
            else if (variable.Type == "Number" && !variable.Name.Contains("     "))
            {
                VariablesListMath.Add(variable.Name);
                VariablesListCompare.Add(variable.Name);
            }
            else if (variable.Type == "Counter" && !variable.Name.Contains("     "))
            {
                VariablesListContacts.Add($"{variable.Name}.QU");
                VariablesListContacts.Add($"{variable.Name}.QD");
                VariablesListMath.Add($"{variable.Name}.PV");
                VariablesListMath.Add($"{variable.Name}.CV");
                VariablesListCompare.Add($"{variable.Name}.PV");
                VariablesListCompare.Add($"{variable.Name}.CV");
                VariablesListCounter.Add(variable.Name);
                VariablesListReset.Add(variable.Name);
            }
            else if (variable.Type == "Timer")
            {
                VariablesListContacts.Add($"{variable.Name}.IN");
                VariablesListContacts.Add($"{variable.Name}.Q");
                VariablesListMath.Add($"{variable.Name}.PT");
                VariablesListMath.Add($"{variable.Name}.ET");
                VariablesListCompare.Add($"{variable.Name}.PT");
                VariablesListCompare.Add($"{variable.Name}.ET");
                VariablesListTimer.Add(variable.Name);
                VariablesListReset.Add(variable.Name);
            }
            else if (variable.Type == "Current Time" && !variable.Name.Contains("     "))
            {
                VariablesListCompare.Add(variable.Name);
            }
            else if (variable.Type == "Time" && !variable.Name.Contains("     "))
            {
                VariablesListCompare.Add(variable.Name);
            }
        }

        /// <summary>
        /// Removes a variable from the appropriate collections based on its type.
        /// </summary>
        /// <param name="variable">The variable to remove.</param>
        private void RemoveVariableFromCollections(Variable variable)
        {
            if (variable.Type == "Digital Input")
            {
                VariablesListContacts.Remove(variable.Name);
            }
            else if (variable.Type == "Digital Output")
            {
                VariablesListContacts.Remove(variable.Name);
                VariablesListCoils.Remove(variable.Name);
            }
            else if (variable.Type == "Analog Input" || variable.Type == "Analog Output")
            {
                VariablesListMath.Remove(variable.Name);
                VariablesListCompare.Remove(variable.Name);
            }
            else if (variable.Type == "One Wire Input")
            {
                VariablesListMath.Remove(variable.Name);
                VariablesListCompare.Remove(variable.Name);
            }
            else if (variable.Type == "ADC Sensor")
            {
                VariablesListMath.Remove(variable.Name);
                VariablesListCompare.Remove(variable.Name);
            }
            else if (variable.Type == "Boolean" && !variable.Name.Contains("     "))
            {
                VariablesListContacts.Remove(variable.Name);
                VariablesListCoils.Remove(variable.Name);
            }
            else if (variable.Type == "Number" && !variable.Name.Contains("     "))
            {
                VariablesListMath.Remove(variable.Name);
                VariablesListCompare.Remove(variable.Name);
            }
            else if (variable.Type == "Counter" && !variable.Name.Contains("     "))
            {
                VariablesListContacts.Remove($"{variable.Name}.QU");
                VariablesListContacts.Remove($"{variable.Name}.QD");
                VariablesListMath.Remove($"{variable.Name}.PV");
                VariablesListMath.Remove($"{variable.Name}.CV");
                VariablesListCompare.Remove($"{variable.Name}.PV");
                VariablesListCompare.Remove($"{variable.Name}.CV");
                VariablesListCounter.Remove(variable.Name);
                VariablesListReset.Remove(variable.Name);
            }
            else if (variable.Type == "Timer")
            {
                VariablesListContacts.Remove($"{variable.Name}.IN");
                VariablesListContacts.Remove($"{variable.Name}.Q");
                VariablesListMath.Remove($"{variable.Name}.PT");
                VariablesListMath.Remove($"{variable.Name}.ET");
                VariablesListCompare.Remove($"{variable.Name}.PT");
                VariablesListCompare.Remove($"{variable.Name}.ET");
                VariablesListTimer.Remove(variable.Name);
                VariablesListReset.Remove(variable.Name);
            }
            else if (variable.Type == "Current Time" && !variable.Name.Contains("     "))
            {
                VariablesListCompare.Remove(variable.Name);
            }
            else if (variable.Type == "Time" && !variable.Name.Contains("     "))
            {
                VariablesListCompare.Remove(variable.Name);
            }
        }

        // ==================== VARIABLE VALIDATION FOR EXPORT ====================
        /// <summary>
        /// Validates all variables to ensure they meet export requirements.
        /// </summary>
        /// <param name="owner">The owner window for displaying notifications.</param>
        /// <returns>True if all variables are valid, false otherwise.</returns>
        public bool ValidateVariables(Window owner)
        {
            foreach (var variable in VariablesList)
            {
                if (variable.Type == "Digital Input" ||
                    variable.Type == "Digital Output" ||
                    variable.Type == "Analog Input" ||
                    variable.Type == "Analog Output")
                {
                    if (variable is DigitalAnalogInputOutputVariable daio && !daio.IsValid)
                    {
                        new NotificationWindow("All Analog/Digital Input/Output variables must have a mapped value selected", owner).Show();
                        return false;
                    }
                }
                else if (variable.Type == "One Wire Input")
                {
                    if (variable is OneWireInputVariable owi && !owi.IsValid)
                    {
                        new NotificationWindow("All One Wire Input variables must have a mapped value selected", owner).Show();
                        return false;
                    }
                }
                else if (variable.Type == "ADC Sensor")
                {
                    if (variable is ADCSensorVariable adcs && !adcs.IsValid)
                    {
                        new NotificationWindow("All ADC Sensor parameters must have a mapped value selected", owner).Show();
                        return false;
                    }
                }
                else if (variable.Type == "Timer")
                {
                    if (variable is TimerVariable tmr && !tmr.IsValid)
                    {
                        new NotificationWindow("Timers Preset Time must be >= 0", owner).Show();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
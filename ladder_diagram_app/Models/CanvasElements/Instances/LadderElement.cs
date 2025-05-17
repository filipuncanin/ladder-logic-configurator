using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ladder_diagram_app.Models.CanvasElements.Instances
{
    /// <summary>
    /// Represents a ladder diagram element (e.g., contact, coil, timer) with an associated image and variable selection ComboBoxes.
    /// </summary>
    public class LadderElement : Node
    {
        /// <summary>
        /// Stores the list of ComboBoxes for variable selection.
        /// </summary>
        private readonly List<ComboBox> _variableComboBoxes = [];

        /// <summary>
        /// Gets or sets the type of the ladder element (e.g., NOContact, Coil, AddMath).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the X-coordinate of the ladder element.
        /// </summary>
        public override double X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the ladder element.
        /// </summary>
        public override double Y { get; set; }

        /// <summary>
        /// Gets the width of the ladder element, based on the maximum of the image or ComboBox widths plus margins.
        /// </summary>
        public override double Width => Math.Max(Image?.Width ?? 0, _variableComboBoxes.Any() ? _variableComboBoxes.Max(cb => cb?.Width ?? 0) : 0) + 10; // 10 -> 5px margin sa svake strane


        /// <summary>
        /// Initializes a new instance of the <see cref="LadderElement"/> class with the specified type and variable lists.
        /// </summary>
        /// <param name="type">The type of the ladder element (e.g., NOContact, Coil).</param>
        /// <param name="variablesListContacts">List of variables for contact elements.</param>
        /// <param name="variablesListCoils">List of variables for coil elements.</param>
        /// <param name="variablesListMath">List of variables for math elements.</param>
        /// <param name="variablesListCompare">List of variables for compare elements.</param>
        /// <param name="variablesListCounter">List of variables for counter elements.</param>
        /// <param name="variablesListTimer">List of variables for timer elements.</param>
        /// <param name="variablesListReset">List of variables for reset elements.</param>
        public LadderElement(string type,
            ObservableCollection<string> variablesListContacts,
            ObservableCollection<string> variablesListCoils,
            ObservableCollection<string> variablesListMath,
            ObservableCollection<string> variablesListCompare,
            ObservableCollection<string> variablesListCounter,
            ObservableCollection<string> variablesListTimer,
            ObservableCollection<string> variablesListReset)
        {
            Type = type;

            // Initialize the image for the ladder element
            Image = new Image();
            if (!string.IsNullOrEmpty(Type))
            {
                // Set image dimensions based on element type
                (Image.Width, Image.Height) = Type switch
                {
                    "NOContact" => (48, 24),
                    "NCContact" => (48, 24),
                    "Coil" => (48, 24),
                    "OSPCoil" => (48, 24),
                    "SetCoil" => (48, 24),
                    "ResetCoil" => (48, 24),
                    "AddMath" => (114, 119),
                    "SubtractMath" => (114, 119),
                    "MultiplyMath" => (114, 119),
                    "DivideMath" => (114, 119),
                    "MoveMath" => (114, 104),
                    "GreaterCompare" => (114, 84),
                    "LessCompare" => (114, 84),
                    "GreaterOrEqualCompare" => (114, 84),
                    "LessOrEqualCompare" => (114, 84),
                    "EqualCompare" => (114, 84),
                    "NotEqualCompare" => (114, 84),
                    "OnDelayTimer" => (114, 54),
                    "OffDelayTimer" => (114, 54),
                    "CountUp" => (114, 54),
                    "CountDown" => (114, 54),
                    "Reset" => (114, 54),
                    _ => (0, 0) // Default case for unknown types
                };

                // Set image source based on element type
                string imagePath = Type switch
                {
                    "NOContact" => "pack://application:,,,/Resources/Contacts/no_contact.png",
                    "NCContact" => "pack://application:,,,/Resources/Contacts/nc_contact.png",
                    "Coil" => "pack://application:,,,/Resources/Coils/coil.png",
                    "OSPCoil" => "pack://application:,,,/Resources/Coils/one_shot_positive_coil.png",
                    "SetCoil" => "pack://application:,,,/Resources/Coils/set_coil.png",
                    "ResetCoil" => "pack://application:,,,/Resources/Coils/reset_coil.png",
                    "AddMath" => "pack://application:,,,/Resources/Math/add.png",
                    "SubtractMath" => "pack://application:,,,/Resources/Math/subtract.png",
                    "MultiplyMath" => "pack://application:,,,/Resources/Math/multiply.png",
                    "DivideMath" => "pack://application:,,,/Resources/Math/divide.png",
                    "MoveMath" => "pack://application:,,,/Resources/Math/move.png",
                    "GreaterCompare" => "pack://application:,,,/Resources/Compare/greater.png",
                    "LessCompare" => "pack://application:,,,/Resources/Compare/less.png",
                    "GreaterOrEqualCompare" => "pack://application:,,,/Resources/Compare/greater_or_equal.png",
                    "LessOrEqualCompare" => "pack://application:,,,/Resources/Compare/less_or_equal.png",
                    "EqualCompare" => "pack://application:,,,/Resources/Compare/equal.png",
                    "NotEqualCompare" => "pack://application:,,,/Resources/Compare/not_equal.png",
                    "OnDelayTimer" => "pack://application:,,,/Resources/Time_Count/on_delay_timer.png",
                    "OffDelayTimer" => "pack://application:,,,/Resources/Time_Count/off_delay_timer.png",
                    "CountUp" => "pack://application:,,,/Resources/Time_Count/count_up.png",
                    "CountDown" => "pack://application:,,,/Resources/Time_Count/count_down.png",
                    "Reset" => "pack://application:,,,/Resources/Time_Count/reset.png",
                    _ => "" // Default case for unknown types
                };

                // Set image source only if a valid path is provided
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Image.Source = new BitmapImage(new Uri(imagePath));
                }
            }

            // Determine the number of ComboBoxes based on element type
            int comboBoxCount = Type switch
            {
                "AddMath" or "SubtractMath" or "MultiplyMath" or "DivideMath" => 3,
                "MoveMath" or "GreaterCompare" or "LessCompare" or
                "GreaterOrEqualCompare" or "LessOrEqualCompare" or
                "EqualCompare" or "NotEqualCompare" => 2,
                _ => 1 // Default case for NOContact, NCContact, Coil, etc.
            };

            // Initialize ComboBoxes for variable selection
            for (int i = 0; i < comboBoxCount; i++)
            {
                var comboBox = new ComboBox
                {
                    Width = 100,
                    Height = 25,
                    Tag = this, // Associate ComboBox with this LadderElement
                    SelectedIndex = -1 // No initial selection
                };

                // Assign appropriate variable list based on element type
                if (Type.Contains("Contact"))
                    comboBox.ItemsSource = variablesListContacts;
                else if (Type.Contains("Coil"))
                    comboBox.ItemsSource = variablesListCoils;
                else if (Type.Contains("Math"))
                    comboBox.ItemsSource = variablesListMath;
                else if (Type.Contains("Compare"))
                    comboBox.ItemsSource = variablesListCompare;
                else if (Type.Contains("Timer"))
                    comboBox.ItemsSource = variablesListTimer;
                else if (Type.Contains("Count"))
                    comboBox.ItemsSource = variablesListCounter;
                else if (Type.Contains("Reset"))
                    comboBox.ItemsSource = variablesListReset;

                _variableComboBoxes.Add(comboBox);
            }
        }

        /// <summary>
        /// Gets a read-only list of the ComboBoxes used for variable selection.
        /// </summary>
        public IReadOnlyList<ComboBox> VariableComboBoxes => _variableComboBoxes.AsReadOnly();

        /// <summary>
        /// Gets the number of ComboBoxes associated with this ladder element.
        /// </summary>
        public int ComboBoxCount => _variableComboBoxes.Count;
    }
}

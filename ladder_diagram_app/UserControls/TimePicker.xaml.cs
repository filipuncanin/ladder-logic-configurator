using System;
using System.Windows;
using System.Windows.Controls;

namespace ladder_diagram_app.UserControls
{
    /// <summary>
    /// A user control for selecting and displaying time in a HH:mm:ss format.
    /// </summary>
    public partial class TimePicker : UserControl
    {
        /// <summary>
        /// Dependency property for the selected time, stored as a double in the format HHmmss.0.
        /// </summary>
        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(
                nameof(SelectedTime),
                typeof(double),
                typeof(TimePicker),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedTimeChanged));

        /// <summary>
        /// Gets or sets the selected time as a double in the format HHmmss.0.
        /// </summary>
        public double SelectedTime
        {
            get => (double)GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePicker"/> class.
        /// </summary>
        public TimePicker()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        /// <summary>
        /// Populates the hours, minutes, and seconds combo boxes with valid values.
        /// </summary>
        private void InitializeComboBoxes()
        {
            for (int i = 0; i <= 23; i++)
                HoursComboBox.Items.Add(i.ToString("D2"));
            for (int i = 0; i <= 59; i++)
            {
                MinutesComboBox.Items.Add(i.ToString("D2"));
                SecondsComboBox.Items.Add(i.ToString("D2"));
            }

            UpdateDisplayFromSelectedTime();
        }

        /// <summary>
        /// Handles changes to the <see cref="SelectedTime"/> property and updates the UI.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (TimePicker)d;
            picker.UpdateDisplayFromSelectedTime();
        }

        /// <summary>
        /// Updates the combo boxes and time display based on the current <see cref="SelectedTime"/> value.
        /// </summary>
        private void UpdateDisplayFromSelectedTime()
        {
            int hours = (int)(SelectedTime / 10000);
            int minutes = (int)((SelectedTime % 10000) / 100);
            int seconds = (int)(SelectedTime % 100);

            hours = Math.Clamp(hours, 0, 23);
            minutes = Math.Clamp(minutes, 0, 59);
            seconds = Math.Clamp(seconds, 0, 59);

            TimeDisplay.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            HoursComboBox.SelectedItem = hours.ToString("D2");
            MinutesComboBox.SelectedItem = minutes.ToString("D2");
            SecondsComboBox.SelectedItem = seconds.ToString("D2");
        }

        /// <summary>
        /// Opens the configuration popup when the time display is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void TimeDisplay_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfigPopup.IsOpen = true;
            HoursComboBox.Focus();
        }

        /// <summary>
        /// Updates the time preview when a combo box selection changes.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimePreview();
        }

        /// <summary>
        /// Applies the selected time and closes the configuration popup.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimePreview();
            ConfigPopup.IsOpen = false;
        }

        /// <summary>
        /// Updates the <see cref="SelectedTime"/> and time display based on the combo box selections.
        /// </summary>
        private void UpdateTimePreview()
        {
            int hours = HoursComboBox.SelectedItem != null ? int.Parse(HoursComboBox.SelectedItem.ToString()) : 0;
            int minutes = MinutesComboBox.SelectedItem != null ? int.Parse(MinutesComboBox.SelectedItem.ToString()) : 0;
            int seconds = SecondsComboBox.SelectedItem != null ? int.Parse(SecondsComboBox.SelectedItem.ToString()) : 0;

            hours = Math.Clamp(hours, 0, 23);
            minutes = Math.Clamp(minutes, 0, 59);
            seconds = Math.Clamp(seconds, 0, 59);

            SelectedTime = double.Parse($"{hours:D2}{minutes:D2}{seconds:D2}.0");
            TimeDisplay.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
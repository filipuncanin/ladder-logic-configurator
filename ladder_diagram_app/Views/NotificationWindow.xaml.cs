using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ladder_diagram_app.Views
{
    /// <summary>
    /// Defines the types of button configurations for the notification window.
    /// </summary>
    public enum NotificationButtons
    {
        None,        // No buttons, auto-closes after a delay
        YesNo,       // Yes and No buttons
        Ok,          // Single OK button
        OneInput,    // One input field with Confirm and Cancel buttons
        TwoInputs,   // Two input fields with Confirm and Cancel buttons
        ThreeInputs  // Three input fields with Confirm and Cancel buttons
    }

    /// <summary>
    /// A customizable notification window that supports various button configurations and input fields.
    /// </summary>
    public partial class NotificationWindow : Window
    {
        // Native Windows API methods for monitor information
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        /// Represents a rectangle with left, top, right, and bottom coordinates.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Contains information about a monitor's size and work area.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;           // Size of the structure
            public RECT rcMonitor;       // Monitor rectangle
            public RECT rcWork;         // Working area rectangle
            public uint dwFlags;        // Monitor flags
        }

        private bool? _result = null;           // Stores the dialog result (true/false for YesNo/Confirm/Cancel)
        private string[] _inputResults = null;   // Stores input field values for input-based dialogs

        /// <summary>
        /// Gets the dialog result (true for Yes/Confirm, false for No/Cancel, null if not set).
        /// </summary>
        public bool? Result => _result;

        /// <summary>
        /// Gets the array of input field values for input-based dialogs.
        /// </summary>
        public string[] InputResults => _inputResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationWindow"/> class.
        /// </summary>
        /// <param name="poruka">The message to display in the notification.</param>
        /// <param name="owner">The parent window that owns this dialog.</param>
        /// <param name="buttons">The button configuration for the dialog.</param>
        /// <param name="inputLabels">Optional labels for input fields (used for OneInput, TwoInputs, ThreeInputs).</param>
        public NotificationWindow(string poruka, Window owner, NotificationButtons buttons = NotificationButtons.None, string[] inputLabels = null)
        {
            InitializeComponent();
            MessageText.Text = poruka;

            // Set minimum and maximum window dimensions
            MinWidth = 200;
            MaxWidth = 600;

            this.Owner = owner;

            // Subscribe to owner window events for position and size updates
            owner.LocationChanged += Owner_PositionOrSizeChanged;
            owner.SizeChanged += Owner_PositionOrSizeChanged;
            owner.StateChanged += Owner_StateChanged;

            // Unsubscribe from events when window closes
            Closed += (s, e) =>
            {
                owner.LocationChanged -= Owner_PositionOrSizeChanged;
                owner.SizeChanged -= Owner_PositionOrSizeChanged;
                owner.StateChanged -= Owner_StateChanged;
            };

            // Position and focus handling on window load
            Loaded += (s, e) =>
            {
                UpdatePosition();
                if (buttons == NotificationButtons.OneInput || buttons == NotificationButtons.TwoInputs || buttons == NotificationButtons.ThreeInputs)
                {
                    Input1TextBox.Focus(); // Set focus to the first input field
                }
                else
                {
                    this.Focus(); // Set focus to the window for YesNo/Ok/None
                }
            };

            // Handle keyboard input
            KeyDown += (s, e) =>
            {
                switch (buttons)
                {
                    case NotificationButtons.YesNo:
                        if (e.Key == Key.Enter)
                        {
                            _result = true; // Yes
                            Close();
                        }
                        else if (e.Key == Key.Escape)
                        {
                            _result = false; // No
                            Close();
                        }
                        break;

                    case NotificationButtons.Ok:
                        if (e.Key == Key.Enter || e.Key == Key.Escape)
                        {
                            Close(); // Close on Enter or Escape
                        }
                        break;

                    case NotificationButtons.OneInput:
                    case NotificationButtons.TwoInputs:
                    case NotificationButtons.ThreeInputs:
                        if (e.Key == Key.Enter)
                        {
                            if (AreInputsValid(buttons))
                            {
                                // Store input values based on button configuration
                                if (buttons == NotificationButtons.OneInput)
                                    _inputResults[0] = Input1TextBox.Text;
                                else if (buttons == NotificationButtons.TwoInputs)
                                {
                                    _inputResults[0] = Input1TextBox.Text;
                                    _inputResults[1] = Input2TextBox.Text;
                                }
                                else
                                {
                                    _inputResults[0] = Input1TextBox.Text;
                                    _inputResults[1] = Input2TextBox.Text;
                                    _inputResults[2] = Input3TextBox.Text;
                                }
                                _result = true; // Confirm
                                Close();
                            }
                        }
                        else if (e.Key == Key.Escape)
                        {
                            _result = false; // Cancel
                            Close();
                        }
                        break;
                }
            };

            // Configure buttons and input fields based on button type
            switch (buttons)
            {
                case NotificationButtons.None:
                    Loaded += (s, e) =>
                    {
                        // Auto-close after 3 seconds
                        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                        timer.Tick += (sender, args) => { timer.Stop(); Close(); };
                        timer.Start();
                    };
                    break;

                case NotificationButtons.YesNo:
                    ButtonPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    ConfirmButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    YesButton.Click += (s, e) => { _result = true; Close(); }; // Yes button
                    NoButton.Click += (s, e) => { _result = false; Close(); }; // No button
                    break;

                case NotificationButtons.Ok:
                    ButtonPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    ConfirmButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    OkButton.Click += (s, e) => { Close(); }; // OK button
                    break;

                case NotificationButtons.OneInput:
                    InputPanel.Visibility = Visibility.Visible;
                    Input1Label.Text = inputLabels != null && inputLabels.Length > 0 ? inputLabels[0] : "Input 1";
                    Input1Label.Visibility = Visibility.Visible;
                    Input1TextBox.Visibility = Visibility.Visible;
                    ButtonPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Collapsed;
                    ConfirmButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    _inputResults = new string[1];
                    ConfirmButton.Click += (s, e) =>
                    {
                        if (AreInputsValid(buttons))
                        {
                            _inputResults[0] = Input1TextBox.Text;
                            _result = true; // Confirm
                            Close();
                        }
                    };
                    CancelButton.Click += (s, e) => { _result = false; Close(); }; // Cancel
                    AdjustLabelWidths();
                    break;

                case NotificationButtons.TwoInputs:
                    InputPanel.Visibility = Visibility.Visible;
                    Input1Label.Text = inputLabels != null && inputLabels.Length > 0 ? inputLabels[0] : "Input 1";
                    Input1Label.Visibility = Visibility.Visible;
                    Input1TextBox.Visibility = Visibility.Visible;
                    Input2Label.Text = inputLabels != null && inputLabels.Length > 1 ? inputLabels[1] : "Input 2";
                    Input2Label.Visibility = Visibility.Visible;
                    Input2TextBox.Visibility = Visibility.Visible;
                    InputPanel.Children[1].Visibility = Visibility.Visible;
                    ButtonPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Collapsed;
                    ConfirmButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    _inputResults = new string[2];
                    ConfirmButton.Click += (s, e) =>
                    {
                        if (AreInputsValid(buttons))
                        {
                            _inputResults[0] = Input1TextBox.Text;
                            _inputResults[1] = Input2TextBox.Text;
                            _result = true; // Confirm
                            Close();
                        }
                    };
                    CancelButton.Click += (s, e) => { _result = false; Close(); }; // Cancel
                    AdjustLabelWidths();
                    break;

                case NotificationButtons.ThreeInputs:
                    InputPanel.Visibility = Visibility.Visible;
                    Input1Label.Text = inputLabels != null && inputLabels.Length > 0 ? inputLabels[0] : "Input 1";
                    Input1Label.Visibility = Visibility.Visible;
                    Input1TextBox.Visibility = Visibility.Visible;
                    Input2Label.Text = inputLabels != null && inputLabels.Length > 1 ? inputLabels[1] : "Input 2";
                    Input2Label.Visibility = Visibility.Visible;
                    Input2TextBox.Visibility = Visibility.Visible;
                    Input3Label.Text = inputLabels != null && inputLabels.Length > 2 ? inputLabels[2] : "Input 3";
                    Input3Label.Visibility = Visibility.Visible;
                    Input3TextBox.Visibility = Visibility.Visible;
                    InputPanel.Children[1].Visibility = Visibility.Visible;
                    InputPanel.Children[2].Visibility = Visibility.Visible;
                    ButtonPanel.Visibility = Visibility.Visible;
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Collapsed;
                    ConfirmButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    _inputResults = new string[3];
                    ConfirmButton.Click += (s, e) =>
                    {
                        if (AreInputsValid(buttons))
                        {
                            _inputResults[0] = Input1TextBox.Text;
                            _inputResults[1] = Input2TextBox.Text;
                            _inputResults[2] = Input3TextBox.Text;
                            _result = true; // Confirm
                            Close();
                        }
                    };
                    CancelButton.Click += (s, e) => { _result = false; Close(); }; // Cancel
                    AdjustLabelWidths();
                    break;
            }
        }

        /// <summary>
        /// Validates input fields to ensure they are not empty for input-based dialogs.
        /// </summary>
        /// <param name="buttons">The button configuration to validate.</param>
        /// <returns>True if all required inputs are valid; otherwise, false.</returns>
        private bool AreInputsValid(NotificationButtons buttons)
        {
            if (buttons == NotificationButtons.OneInput)
            {
                return !string.IsNullOrWhiteSpace(Input1TextBox.Text);
            }
            else if (buttons == NotificationButtons.TwoInputs)
            {
                return !string.IsNullOrWhiteSpace(Input1TextBox.Text) &&
                       !string.IsNullOrWhiteSpace(Input2TextBox.Text);
            }
            else if (buttons == NotificationButtons.ThreeInputs)
            {
                return !string.IsNullOrWhiteSpace(Input1TextBox.Text) &&
                       !string.IsNullOrWhiteSpace(Input2TextBox.Text) &&
                       !string.IsNullOrWhiteSpace(Input3TextBox.Text);
            }
            return true;
        }

        /// <summary>
        /// Adjusts the width of visible input labels to match the widest label for consistent alignment.
        /// </summary>
        private void AdjustLabelWidths()
        {
            TextBlock[] labels = new[] { Input1Label, Input2Label, Input3Label };
            double maxWidth = 0;

            foreach (var label in labels)
            {
                if (label.Visibility == Visibility.Visible)
                {
                    label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    maxWidth = Math.Max(maxWidth, label.DesiredSize.Width);
                }
            }

            foreach (var label in labels)
            {
                if (label.Visibility == Visibility.Visible)
                {
                    label.Width = maxWidth;
                }
            }
        }

        /// <summary>
        /// Updates the position of the notification window to center it relative to the owner window or monitor.
        /// </summary>
        private void UpdatePosition()
        {
            if (Owner != null)
            {
                var hwnd = new WindowInteropHelper(Owner).Handle;
                var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    var monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);

                    var source = PresentationSource.FromVisual(Owner);
                    if (source != null)
                    {
                        var dpiScale = source.CompositionTarget.TransformToDevice.M11;

                        if (Owner.WindowState == WindowState.Maximized)
                        {
                            // Center on the monitor's work area for maximized owner
                            double screenWidth = (monitorInfo.rcWork.Right - monitorInfo.rcWork.Left) / dpiScale;
                            double screenHeight = (monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top) / dpiScale;
                            double screenLeft = monitorInfo.rcWork.Left / dpiScale;
                            double screenTop = monitorInfo.rcWork.Top / dpiScale;

                            this.Left = screenLeft + (screenWidth - this.ActualWidth) / 2;
                            this.Top = screenTop + (screenHeight - this.ActualHeight) / 2;
                        }
                        else
                        {
                            // Center relative to the owner window
                            this.Left = Owner.Left + (Owner.ActualWidth - this.ActualWidth) / 2;
                            this.Top = Owner.Top + (Owner.ActualHeight - this.ActualHeight) / 2;
                        }

                        // Adjust height if the dialog is taller than the owner
                        if (this.ActualHeight > Owner.ActualHeight)
                        {
                            this.Top = Owner.Top;
                            this.Height = Owner.ActualHeight;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles changes in the owner window's position or size to reposition the notification window.
        /// </summary>
        private void Owner_PositionOrSizeChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        /// <summary>
        /// Handles changes in the owner window's state (e.g., maximized/minimized) to reposition the notification window.
        /// </summary>
        private void Owner_StateChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }
    }
}
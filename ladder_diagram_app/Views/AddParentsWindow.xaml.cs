using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ladder_diagram_app.Views
{
    /// <summary>
    /// Window for adding and managing parent devices, centered relative to the owner window.
    /// </summary>
    public partial class AddParentsWindow : Window
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

        private readonly List<string> _parentDevices;
        /// <summary>
        /// Gets the observable collection of parent devices.
        /// </summary>
        public ObservableCollection<string> ParentDevices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddParentsWindow"/> class.
        /// </summary>
        /// <param name="parentDevices">The list of parent devices to manage.</param>
        /// <param name="owner">The owner window for positioning and event handling.</param>
        public AddParentsWindow(List<string> parentDevices, Window owner)
        {
            InitializeComponent();
            _parentDevices = parentDevices ?? new List<string>();
            ParentDevices = new ObservableCollection<string>(_parentDevices);
            DataContext = this;

            // Set window size constraints
            MinWidth = 200;
            MaxWidth = 600;

            this.Owner = owner;

            // Subscribe to owner window events for position and size updates
            owner.LocationChanged += Owner_PositionOrSizeChanged;
            owner.SizeChanged += Owner_PositionOrSizeChanged;
            owner.StateChanged += Owner_StateChanged;

            // Unsubscribe from owner events when window closes
            Closed += (s, e) =>
            {
                owner.LocationChanged -= Owner_PositionOrSizeChanged;
                owner.SizeChanged -= Owner_PositionOrSizeChanged;
                owner.StateChanged -= Owner_StateChanged;
            };

            // Position window and set focus on load
            Loaded += (s, e) =>
            {
                UpdatePosition();
                NewParentTextBox.Focus();
            };

            // Handle keyboard input for adding devices and closing window
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    string newParent = NewParentTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(newParent) && !ParentDevices.Contains(newParent, StringComparer.OrdinalIgnoreCase))
                    {
                        ParentDevices.Add(newParent.ToUpper());
                        NewParentTextBox.Text = string.Empty;
                        NewParentTextBox.Focus();
                    }
                    else if (string.IsNullOrWhiteSpace(newParent))
                    {
                        new NotificationWindow("Parent device name cannot be empty!", this).Show();
                    }
                    else
                    {
                        new NotificationWindow($"Parent device '{newParent}' already exists!", this).Show();
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    Close();
                }
            };
        }

        /// <summary>
        /// Adds a new parent device when the Add button is clicked.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void AddParentDevice_Click(object sender, RoutedEventArgs e)
        {
            string newParent = NewParentTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newParent) && !ParentDevices.Contains(newParent, StringComparer.OrdinalIgnoreCase))
            {
                ParentDevices.Add(newParent.ToUpper());
                NewParentTextBox.Text = string.Empty;
                NewParentTextBox.Focus();
            }
            else if (string.IsNullOrWhiteSpace(newParent))
            {
                new NotificationWindow("Parent device name cannot be empty!", this).Show();
            }
            else
            {
                new NotificationWindow($"Parent device '{newParent}' already exists!", this).Show();
            }
        }

        /// <summary>
        /// Removes a parent device when the Delete button is clicked.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void DeleteParentDevice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string parent)
            {
                ParentDevices.Remove(parent);
            }
        }

        /// <summary>
        /// Saves the parent devices list and closes the window.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _parentDevices.Clear();
            _parentDevices.AddRange(ParentDevices);
            DialogResult = true; // Indicate successful save
            Close();
        }

        /// <summary>
        /// Closes the window without saving changes.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Indicate cancellation
            Close();
        }

        /// <summary>
        /// Updates the window position to center it relative to the owner window or monitor.
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
        /// Handles changes in the owner window's position or size to reposition the dialog.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Owner_PositionOrSizeChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        /// <summary>
        /// Handles changes in the owner window's state (e.g., maximized/minimized) to reposition the dialog.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Owner_StateChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }
    }
}
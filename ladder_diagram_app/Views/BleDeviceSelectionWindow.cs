using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Devices.Enumeration;

namespace ladder_diagram_app.Views
{
    /// <summary>
    /// Represents a window for selecting a Bluetooth Low Energy (BLE) device from a list.
    /// </summary>
    public class BleDeviceSelectionWindow : Window
    {
        /// <summary>
        /// Gets the selected BLE device information.
        /// </summary>
        public DeviceInformation? SelectedDevice { get; private set; }

        /// <summary>
        /// The collection of available BLE devices.
        /// </summary>
        private readonly ObservableCollection<DeviceInformation> _devices;

        /// <summary>
        /// Initializes a new instance of the <see cref="BleDeviceSelectionWindow"/> class.
        /// </summary>
        /// <param name="devices">The collection of BLE devices to display.</param>
        /// <param name="owner">The parent window that owns this dialog.</param>
        public BleDeviceSelectionWindow(ObservableCollection<DeviceInformation> devices, Window owner)
        {
            Owner = owner;
            _devices = devices;
            InitializeComponents();
        }

        /// <summary>
        /// Sets up the UI components for the BLE device selection window.
        /// </summary>
        private void InitializeComponents()
        {
            // Set window properties
            Width = 300;
            Height = double.NaN;
            SizeToContent = SizeToContent.Height; // Dynamically adjust height based on content
            Title = "Select BLE Device";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            // Create main layout panel
            var stackPanel = new StackPanel();

            // Initialize ListBox for displaying BLE devices
            var listBox = new ListBox
            {
                Margin = new Thickness(5),
                DisplayMemberPath = "Name",
                MaxHeight = 300
            };
            listBox.ItemsSource = _devices;

            // Auto-select first item and set focus when ListBox is loaded
            listBox.Loaded += (s, e) =>
            {
                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                    listBox.Focus();
                }
            };

            // Handle keyboard navigation and selection
            listBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && listBox.SelectedItem != null)
                {
                    // Select device and close window on Enter key
                    SelectedDevice = listBox.SelectedItem as DeviceInformation;
                    DialogResult = true;
                    Close();
                }
                else if (e.Key == Key.Up)
                {
                    // Navigate up in the list
                    int newIndex = Math.Max(0, listBox.SelectedIndex - 1);
                    listBox.SelectedIndex = newIndex;
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    // Navigate down in the list
                    int newIndex = Math.Min(listBox.Items.Count - 1, listBox.SelectedIndex + 1);
                    listBox.SelectedIndex = newIndex;
                    e.Handled = true;
                }
            };

            // Create Select button
            var selectButton = new Button
            {
                Content = "Select",
                Margin = new Thickness(5),
                Width = 100,
                Height = 30
            };
            selectButton.Click += (s, e) =>
            {
                // Set selected device and close window
                SelectedDevice = listBox.SelectedItem as DeviceInformation;
                DialogResult = SelectedDevice != null;
                Close();
            };

            // Create Cancel button
            var cancelButton = new Button
            {
                Content = "Cancel",
                Margin = new Thickness(5),
                Width = 100,
                Height = 30
            };
            cancelButton.Click += (s, e) =>
            {
                // Close window without selecting a device
                DialogResult = false;
                Close();
            };

            // Create button panel for Select and Cancel buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 60
            };
            buttonPanel.Children.Add(selectButton);
            buttonPanel.Children.Add(cancelButton);

            // Add components to main panel
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(buttonPanel);

            // Set window content
            Content = stackPanel;
        }
    }
}
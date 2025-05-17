using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Windows.Threading;

using ladder_diagram_app.Models.CanvasElements;
using ladder_diagram_app.Models.DeviceElement;
using ladder_diagram_app.Models.Variables.Instances;
using ladder_diagram_app.Models.Variables;
using ladder_diagram_app.Views;
using ladder_diagram_app.Services.MonitorServices;
using ladder_diagram_app.Services.ImportExportServices;
using ladder_diagram_app.Services.CommunicationServices;
using ladder_diagram_app.Services.CanvasServices;

namespace ladder_diagram_app
{
    /// <summary>
    /// Main application window for managing ladder diagrams, device communication, and variables.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// List of supported ADC sensor types.
        /// </summary>
        public List<string> AdcSensorTypes { get; } = ["TM7711", "HX710B"];
        /// <summary>
        /// List of supported ADC sensor sampling rates.
        /// </summary>
        public List<string> AdcSensorSamplingRates { get; } = ["10Hz", "40Hz", "Temperature"];

        // Device Element
        private readonly Device _device;
        /// <summary>
        /// Manages device pin configurations.
        /// </summary>
        public readonly DevicePinManager _devicePinManager;
        /// <summary>
        /// Gets the device pin manager.
        /// </summary>
        public DevicePinManager DevicePinManager => _devicePinManager;

        // Variables
        private readonly VariablesManager _variablesManager;
        /// <summary>
        /// Gets the variables manager.
        /// </summary>
        public VariablesManager VariablesManager => _variablesManager;

        // Wires
        private readonly WiresManager _wiresManager;

        // Canvas Services
        private readonly CanvasManager _canvasManager;
        private readonly CanvasElementFinder _canvasElementFinder;
        private readonly CanvasInteractionManager _canvasInteractionManager;

        // Communication Services
        private readonly DeviceCommunicationManager _deviceCommunicationManager;

        // Monitor Services
        private readonly MonitorDataService _monitorDataService;
        private readonly OneWireDataService _oneWireDataService;

        // Import/Export Services
        private readonly ImportExportService _importExportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _device = new Device();
            _devicePinManager = new DevicePinManager();

            _variablesManager = new VariablesManager(_device);

            _wiresManager = new WiresManager();

            _canvasManager = new CanvasManager(
                canvas: MainCanvas,
                gridCanvas: GridCanvas,
                wiresManager: _wiresManager
            );

            _canvasElementFinder = new CanvasElementFinder(
                getWiresManager: () => _wiresManager
            );

            _canvasInteractionManager = new CanvasInteractionManager(
                canvas: MainCanvas,
                wiresManager: _wiresManager,
                elementFinder: _canvasElementFinder,
                canvasManager: _canvasManager,
                variablesManager: _variablesManager
            );

            _monitorDataService = new MonitorDataService(this);
            _oneWireDataService = new OneWireDataService(this, _device);

            _deviceCommunicationManager = new DeviceCommunicationManager(
                onConfigurationReceived: jsonConfig => Dispatcher.Invoke(() => OnConfigurationReceived(jsonConfig)),
                onMonitorDataReceived: jsonData => Dispatcher.Invoke(() => _monitorDataService.OnMonitorDataReceived(jsonData)),
                onOneWireDataReceived: jsonData => Dispatcher.Invoke(() => _oneWireDataService.OnOneWireDataReceived(jsonData)),
                onConnectionStatusChanged: isConnected => Dispatcher.Invoke(() => OnConnectionStatusChanged(isConnected))
            );

            _importExportService = new ImportExportService(
                variablesManager: _variablesManager,
                device: _device,
                wiresManager: _wiresManager,
                canvasManager: _canvasManager,
                devicePinManager: _devicePinManager
            );

            // Synchronize canvas sizes
            MainCanvas.Width = GridCanvas.ActualWidth;
            MainCanvas.Height = GridCanvas.ActualHeight;

            // Initialize canvas with a wire
            _wiresManager.AddWire();
            _canvasManager.UpdateCanvas();

            // Update canvas on grid size change
            GridCanvas.SizeChanged += (s, e) =>
            {
                _canvasManager.UpdateCanvas();
            };

            // Unselect elements when canvas loses focus, unless focus is on delete button
            GridCanvas.LostFocus += (s, e) =>
            {
                // Check that the focus has not moved to ButtonDelete
                if (Keyboard.FocusedElement != ButtonDelete)
                {
                    _canvasInteractionManager.UnselectEverything();
                }
            };

            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            this.Closing += Window_Closing;
        }

        /// <summary>
        /// Handles keyboard input for deleting elements or adding variables.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The key event arguments.</param>
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Delete selected variable from ListView
                if (ElementListView.IsKeyboardFocusWithin && ElementListView.SelectedItem is Variable selectedVariable)
                {
                    _variablesManager.DeleteVariable(selectedVariable, this);
                    e.Handled = true;
                }
                // Delete selected canvas element
                else if (_canvasInteractionManager.IsElementSelected())
                {
                    _canvasInteractionManager.DeleteSelected(this);
                    e.Handled = true;
                }
                else
                {
                    new NotificationWindow("Select an element or variable to delete", this).Show();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter && NameTextBox.IsKeyboardFocused)
            {
                ButtonAddVariable_Click(null, null);
                e.Handled = true;
            }
        }

        // ========================= IMPORT/EKSPORT ========================================================
        /// <summary>
        /// Imports a project from a JSON file, replacing current content after confirmation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_device.IsDeviceLoaded())
                {
                    var dialog = new NotificationWindow("Are you sure you want to import another project? The current content will be deleted.", this, NotificationButtons.YesNo);
                    dialog.ShowDialog();
                    if (dialog.Result == false) return;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Import Ladder Diagram"
                };

                if (openFileDialog.ShowDialog() != true) return;

                string jsonString = File.ReadAllText(openFileDialog.FileName);

                _importExportService.ImportFromJson(jsonString, this);
            }
            catch (Exception ex)
            {
                new NotificationWindow("Error loading configuration", this).Show();
                Debug.WriteLine($"Error loading config: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the current project to a JSON file.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            var jsonString = _importExportService.ExportToJson(this);
            if (jsonString == null)
                return;

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Export Ladder Diagram",
                    FileName = "ladder_diagram.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                    FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);
                    fileInfo.IsReadOnly = true;
                    new NotificationWindow("Export successful!", this).Show();
                }
            }
            catch (Exception ex)
            {
                new NotificationWindow($"Export failed: {ex.Message}", this).Show();
            }
        }

        // ========================= VARIABLES =============================================================
        /// <summary>
        /// Adds a new variable based on input name and type.
        /// </summary>
        /// <param name="sender">The sender object, can be null for keyboard-triggered calls.</param>
        /// <param name="e">The routed event arguments, can be null for keyboard-triggered calls.</param>
        private void ButtonAddVariable_Click(object? sender, RoutedEventArgs? e)
        {
            string name = NameTextBox.Text.Trim();
            string type = TypeComboBox.Text;

            _variablesManager.AddVariable(name, type, this);

            NameTextBox.Clear();
        }

        /// <summary>
        /// Deletes a variable when the delete button is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonDeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Variable variable)
            {
                _variablesManager.DeleteVariable(variable, this);
            }
        }

        /// <summary>
        /// Toggles the boolean value of a variable when its button is clicked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonChangeBoolean_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Variable variable)
            {
                _variablesManager.VariableBooleanClick(variable);
            }
        }

        /// <summary>
        /// Updates a variable's value when its text box changes, triggered by Enter or focus loss.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void TextBoxVariable_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is Variable variable)
            {
                bool isEnterPressed = e is KeyEventArgs keyArgs && keyArgs.Key == Key.Enter;
                bool isLostFocus = e is RoutedEventArgs && e.RoutedEvent == UIElement.LostFocusEvent;

                string inputText = tb.Text.Trim();

                if (isEnterPressed)
                    ElementListView.Focus();

                if (isEnterPressed || isLostFocus)
                {
                    _variablesManager.VariableTextBoxChange(variable, inputText);
                }
            }
        }

        /// <summary>
        /// Updates a variable's value when its combo box selection changes.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void ComboBoxVariable_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && cb.DataContext is Variable variable)
            {
                string? selectedValue = cb.SelectedItem != null ? cb.SelectedItem.ToString() : null;
                if (selectedValue != null) _variablesManager.VariableComboBoxChange(variable, selectedValue);   
            }
        }

        /// <summary>
        /// Restricts text box input to valid numbers or a minus sign.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arguments.</param>
        private void TextBoxVariable_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Get the current text and cursor position
                string currentText = textBox.Text;
                int caretIndex = textBox.CaretIndex;
                string newText;

                // If the entire text is selected, replace it with the new entry
                if (textBox.SelectedText == currentText && !string.IsNullOrEmpty(currentText))
                {
                    newText = e.Text;
                }
                else
                {
                    newText = currentText.Insert(caretIndex, e.Text);
                }

                // Allow input if result is empty, "-", or valid double
                e.Handled = !(string.IsNullOrEmpty(newText) ||
                              newText == "-" ||
                              double.TryParse(newText, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
            }
        }

        // ========================= DEVICE INFO ===========================================================
        /// <summary>
        /// Displays device information in a notification window.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            new NotificationWindow(_device.DeviceInfo(), this, NotificationButtons.Ok).Show();
        }

        // ========================= PARENT DEVICE(S) ======================================================
        /// <summary>
        /// Opens a window to manage parent devices.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonParentDevice_Click(object sender, RoutedEventArgs e)
        {
            _device.AddParentDevices(this);
        }

        // ========================= ADDING WIRE ===========================================================
        /// <summary>
        /// Adds a new wire to the canvas and updates the display.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ButtonAddWire_Click(object sender, RoutedEventArgs e)
        {
            _wiresManager.AddWire();
            _canvasManager.UpdateCanvas();
        }

        // ========================= CANVAS ================================================================
        /// <summary>
        /// Initiates drag-and-drop from the toolbar for adding elements.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Button button && button.Tag is string tag)
            {
                DragDrop.DoDragDrop(button, tag, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Highlights valid drop positions during drag operations on the canvas.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            _canvasInteractionManager.HandleDragOver(e);
        }

        /// <summary>
        /// Handles dropping elements onto the canvas.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The drag event arguments.</param>
        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            _canvasInteractionManager.HandleDrop(e, this);
        }

        /// <summary>
        /// Handles mouse movement for dragging elements on the canvas.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            _canvasInteractionManager.HandleMouseMove(e);
        }

        /// <summary>
        /// Handles mouse release for placing elements on the canvas.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _canvasInteractionManager.HandleMouseLeftButtonUp(e, this);
        }

        /// <summary>
        /// Handles mouse click for selecting elements or starting a drag operation.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _canvasInteractionManager.HandleMouseLeftButtonDown(e, ElementListView);
        }

        /// <summary>
        /// Deletes the selected canvas element or wire.
        /// </summary>
        /// <param name="sender">The sender object, can be null for keyboard-triggered calls.</param>
        /// <param name="e">The routed event arguments, can be null for keyboard-triggered calls.</param>
        private void ButtonDelete_Click(object? sender, RoutedEventArgs? e)
        {
            _canvasInteractionManager.DeleteSelected(this);
        }

        // ========================= DEVICE COMMUNICATION ==================================================
        /// <summary>
        /// Initiates connection to the device via MQTT or BLE.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            var connectionType = ConnectionMQTT.IsChecked == true ? "MQTT" : "BLE";
            await _deviceCommunicationManager.ConnectAsync(this, connectionType);
        }

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private async void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            await _deviceCommunicationManager.DisconnectAsync(this);
        }

        /// <summary>
        /// Sends the current configuration to the device.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private async void ButtonSendToDevice_Click(object sender, RoutedEventArgs e)
        {
            var jsonString = _importExportService.ExportToJson(this);
            if (jsonString == null) return;

            await _deviceCommunicationManager.SendConfigurationAsync(jsonString, this);
        }

        /// <summary>
        /// Handles received device configuration and updates the application state.
        /// </summary>
        /// <param name="jsonConfig">The JSON configuration string.</param>
        private void OnConfigurationReceived(string jsonConfig)
        {
            _importExportService.ImportFromJson(jsonConfig, this);

            _oneWireDataService.DeleteLastOneWireMessage();
        }

        /// <summary>
        /// Updates UI based on device connection status changes.
        /// </summary>
        /// <param name="isConnected">True if connected, false otherwise.</param>
        private void OnConnectionStatusChanged(bool isConnected)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // Update connection status display
                StatusRun.Text = isConnected ? $"Connected ({_deviceCommunicationManager._communicationService?.ConnectionType})" : "Not Connected";
                StatusRun.Foreground = isConnected ? Brushes.Green : Brushes.Red;

                // Show/hide monitor grid based on connection status
                MonitorGrid.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
                MonitorExpander.IsExpanded = isConnected;   // Open Expander only when connected

                // Adjust row height in parent grid
                var parentGrid = MonitorGrid.Parent as Grid;
                if (parentGrid != null)
                {
                    var row = Grid.GetRow(MonitorGrid);
                    parentGrid.RowDefinitions[row].Height = isConnected ? new GridLength(100) : GridLength.Auto;
                }

                // Show connection status notification
                new NotificationWindow(isConnected ? "Device Connected" : "Device Disconnected", this).Show();
            });
        }

        // ========================= MANAGING THE MONITOR / ONE WIRE SECTION  ==============================
        /// <summary>
        /// Adjusts the grid row height when the monitor expander is collapsed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void MonitorExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            // Reset the row height to Auto so that the Expander sticks to the bottom
            var parentGrid = MonitorGrid.Parent as Grid;
            if (parentGrid != null)
            {
                var row = Grid.GetRow(MonitorGrid);
                parentGrid.RowDefinitions[row].Height = GridLength.Auto;
            }
        }

        /// <summary>
        /// Handles adding or deleting one-wire sensors.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _oneWireDataService.ActionButton_Click(sender, e);
        }

        // ========================= CLOSING THE APP =======================================================
        /// <summary>
        /// Ensures proper cleanup when the application window is closing.
        /// </summary>
        /// <param name="sender">The sender object, can be null.</param>
        /// <param name="e">The cancel event arguments.</param>
        private async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_deviceCommunicationManager != null)
            {
                await _deviceCommunicationManager.DisconnectAsync(this);
                _deviceCommunicationManager.Dispose();
            }
        }
    }
}
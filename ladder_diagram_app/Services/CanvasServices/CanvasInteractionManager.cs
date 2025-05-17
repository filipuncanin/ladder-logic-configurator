using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ladder_diagram_app.Models.CanvasElements;
using ladder_diagram_app.Models.CanvasElements.Instances;
using ladder_diagram_app.Models.Variables;
using ladder_diagram_app.Views;

namespace ladder_diagram_app.Services.CanvasServices
{
    /// <summary>
    /// Manages user interactions with the canvas in a ladder diagram application, including dragging, dropping, selecting, and deleting elements.
    /// </summary>
    public class CanvasInteractionManager
    {
        private readonly Canvas _canvas;
        private readonly WiresManager _wiresManager;
        private readonly CanvasElementFinder _elementFinder;
        private readonly CanvasManager _canvasManager;
        private readonly VariablesManager _variablesManager;

        // Fields for dragging elements
        private Point _dragStartPosition;
        private bool _isDragging;
        private bool _isDraggingWire;

        // Fields for selecting and deleting elements, wires, and branches
        private Node? _selectedNode;
        private Wire? _selectedWire;
        private Line? _wireLine;

        // Tracks the currently highlighted object to manage highlight state
        private object? _highlightedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasInteractionManager"/> class.
        /// </summary>
        /// <param name="canvas">The canvas to interact with.</param>
        /// <param name="wiresManager">Manages wires on the canvas.</param>
        /// <param name="elementFinder">Finds elements based on cursor position.</param>
        /// <param name="canvasManager">Manages canvas updates.</param>
        /// <param name="variablesManager">Manages variables for ladder elements.</param>
        public CanvasInteractionManager(
            Canvas canvas,
            WiresManager wiresManager,
            CanvasElementFinder elementFinder,
            CanvasManager canvasManager,
            VariablesManager variablesManager)
        {
            _canvas = canvas;
            _wiresManager = wiresManager;
            _elementFinder = elementFinder;
            _canvasManager = canvasManager;
            _variablesManager = variablesManager;
        }

        /// <summary>
        /// Checks if an element or wire is currently selected.
        /// </summary>
        /// <returns>True if an element or wire is selected, otherwise false.</returns>
        public bool IsElementSelected()
        {
            return _selectedNode != null || _selectedWire != null;
        }

        /// <summary>
        /// Highlights the wire or branch closest to the specified position.
        /// </summary>
        /// <param name="currentPosition">The current cursor position on the canvas.</param>
        public void HighlightPosition(Point currentPosition)
        {
            Wire? closestWire = _elementFinder.FindClosestWire(currentPosition);
            var (closestBranch, isUpperLine) = _elementFinder.FindClosestBranch(currentPosition);

            // Clear previous highlight
            if (_highlightedObject != null)
            {
                if (_highlightedObject is Wire wire) wire.UnhighlightWire();
                else if (_highlightedObject is Branch branch) branch.UnhighlightBranch();
            }

            // Apply new highlight
            if (closestBranch != null)
            {
                closestBranch.HighlightBranch(isUpperLine);
                _highlightedObject = closestBranch;
            }
            else if (closestWire != null)
            {
                closestWire.HighlightWire();
                _highlightedObject = closestWire;
            }
        }

        /// <summary>
        /// Handles the drag-over event by highlighting the closest wire or branch.
        /// </summary>
        /// <param name="e">The drag event arguments.</param>
        public void HandleDragOver(DragEventArgs e)
        {
            HighlightPosition(e.GetPosition(_canvas));
        }

        /// <summary>
        /// Handles the drop event by placing a new element or branch on the canvas.
        /// </summary>
        /// <param name="e">The drag event arguments.</param>
        /// <param name="owner">The owner window for displaying notifications.</param>
        public void HandleDrop(DragEventArgs e, Window owner)
        {
            Point dropPosition = e.GetPosition(_canvas);
            string? droppedElement = e.Data.GetData(typeof(string)) as string;

            if (string.IsNullOrEmpty(droppedElement)) return;

            Wire? closestWire = _elementFinder.FindClosestWire(dropPosition);
            var (closestBranch, isUpperLine) = _elementFinder.FindClosestBranch(dropPosition);

            // Create new element or branch
            Node element = droppedElement != "Branch"
                ? new LadderElement(droppedElement,
                    _variablesManager.VariablesListContacts,
                    _variablesManager.VariablesListCoils,
                    _variablesManager.VariablesListMath,
                    _variablesManager.VariablesListCompare,
                    _variablesManager.VariablesListCounter,
                    _variablesManager.VariablesListTimer,
                    _variablesManager.VariablesListReset)
                : new Branch();

            if (closestBranch != null)
            {
                if (droppedElement.Contains("Coil"))
                {
                    new NotificationWindow("A Coil-type element cannot be placed in a branch", owner).Show();
                    closestBranch.UnhighlightBranch();
                    UnselectEverything();
                    _highlightedObject = null;
                    return;
                }

                List<Node> targetNodes = isUpperLine ? closestBranch.Nodes1 : closestBranch.Nodes2;

                // Inserting elements between others
                bool inserted = false;
                for (int i = 0; i < targetNodes.Count; i++)
                {
                    // If the new element is before the first
                    if (i == 0 && dropPosition.X < targetNodes[i].X)
                    {
                        targetNodes.Insert(0, element);
                        inserted = true;
                        break;
                    }
                    // If the new element is between two existing ones
                    if (i < targetNodes.Count - 1 && dropPosition.X > targetNodes[i].X && dropPosition.X < targetNodes[i + 1].X)
                    {
                        targetNodes.Insert(i + 1, element);
                        inserted = true;
                        break;
                    }
                }
                // If not inserted, add to the end
                if (!inserted)
                {
                    targetNodes.Add(element);
                }

                element.Parent = closestBranch;

                _canvasManager.UpdateCanvas();

                closestBranch.UnhighlightBranch();
                UnselectEverything();
                _highlightedObject = null;
            }
            else if (closestWire != null)
            {
                // Checking for Coil types and existence on the wire
                if (droppedElement.Contains("Coil"))
                {
                    LadderElement? foundCoilElement = closestWire.Nodes.OfType<LadderElement>().FirstOrDefault(el => el.Type.Contains("Coil"));

                    if (foundCoilElement != null)
                    {
                        var dialog = new NotificationWindow("The wire already contains a Coil. Do you want to replace it?", owner, NotificationButtons.YesNo);
                        dialog.ShowDialog();
                        if (dialog.Result == true)
                        {
                            closestWire.Nodes.Remove(foundCoilElement);
                            closestWire.Nodes.Add(element);
                            element.Parent = closestWire;
                        }
                    }
                    else
                    {
                        closestWire.Nodes.Add(element);
                        element.Parent = closestWire;
                    }
                }
                else
                {
                    // Inserting elements between others
                    bool inserted = false;
                    for (int i = 0; i < closestWire.Nodes.Count; i++)
                    {
                        // If the new element is before the first
                        if (i == 0 && dropPosition.X < closestWire.Nodes[i].X)
                        {
                            closestWire.Nodes.Insert(0, element);
                            inserted = true;
                            break;
                        }
                        // If the new element is between two existing ones
                        if (i < closestWire.Nodes.Count - 1)
                        {
                            if (dropPosition.X > closestWire.Nodes[i].X && dropPosition.X < closestWire.Nodes[i + 1].X)
                            {
                                closestWire.Nodes.Insert(i + 1, element);
                                inserted = true;
                                break;
                            }
                        }
                    }
                    // If not inserted, add to the end
                    if (!inserted)
                    {
                        closestWire.Nodes.Add(element);
                    }
                    element.Parent = closestWire;
                }

                _canvasManager.UpdateCanvas();

                closestWire.UnhighlightWire();
                UnselectEverything();
                _highlightedObject = null;
            }
        }

        /// <summary>
        /// Handles mouse movement, initiating and updating drag operations for elements or wires.
        /// </summary>
        /// <param name="e">The mouse event arguments.</param>
        public void HandleMouseMove(MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(_canvas);

            if ((_selectedNode != null || _selectedWire != null) && e.LeftButton == MouseButtonState.Pressed && !_isDragging && !_isDraggingWire)
            {
                Vector offset = currentPosition - _dragStartPosition;

                // Start dragging if moved beyond 5-pixel threshold
                if (Math.Abs(offset.X) > 5 || Math.Abs(offset.Y) > 5)   // 5 pixel threshold
                {
                    if (_selectedNode != null && _selectedNode.Image != null)
                    {
                        _isDragging = true;
                        _selectedNode.Image.CaptureMouse();

                        // Set the starting position of the branch image at the point where the branch drag started
                        if (!_canvas.Children.Contains(_selectedNode.Image))
                        {
                            _canvas.Children.Add(_selectedNode.Image);
                            Canvas.SetLeft(_selectedNode.Image, _dragStartPosition.X - _selectedNode.Image.Width / 2);
                            Canvas.SetTop(_selectedNode.Image, _dragStartPosition.Y - _selectedNode.Image.Height / 2);
                        }
                    }
                    else
                    {
                        _isDraggingWire = true;
                        _selectedWire?.SelectWire();

                        _wireLine = new Line
                        {
                            X1 = 0,
                            Y1 = currentPosition.Y,
                            X2 = _canvas.Width,
                            Y2 = currentPosition.Y,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 4
                        };
                        _canvas.Children.Add(_wireLine);
                        _wireLine.CaptureMouse();
                    }
                }
            }

            if (_isDraggingWire && _selectedWire != null && _wireLine != null)
            {
                _wireLine.Y1 = currentPosition.Y;
                _wireLine.Y2 = currentPosition.Y;
                e.Handled = true;
            }

            if (_isDragging)
            {
                Vector offset = currentPosition - _dragStartPosition;

                double newX = Canvas.GetLeft(_selectedNode?.Image) + offset.X;
                double newY = Canvas.GetTop(_selectedNode?.Image) + offset.Y;

                Canvas.SetLeft(_selectedNode?.Image, newX);
                Canvas.SetTop(_selectedNode?.Image, newY);

                // Update ComboBox positions for LadderElement
                if (_selectedNode is LadderElement element && element.Image != null)
                {
                    element.Image.Opacity = 0.8;
                    element.VariableComboBoxes[0].Opacity = 0.8;
                    if (element.VariableComboBoxes.Count > 1)
                        element.VariableComboBoxes[1].Opacity = 0.8;
                    if (element.VariableComboBoxes.Count > 2)
                        element.VariableComboBoxes[2].Opacity = 0.8;

                    // Position ComboBoxes based on element type
                    if (element.Type.Contains("Contact") || element.Type.Contains("Coil"))
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], newX - element.VariableComboBoxes[0].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], newY - 32);
                    }
                    else if (element.Type == "AddMath" || element.Type == "SubtractMath" || element.Type == "MultiplyMath" || element.Type == "DivideMath")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], newX - element.VariableComboBoxes[0].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], newY + 7);
                        Canvas.SetLeft(element.VariableComboBoxes[1], newX - element.VariableComboBoxes[1].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], newY + 47);
                        Canvas.SetLeft(element.VariableComboBoxes[2], newX - element.VariableComboBoxes[2].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[2], newY + 92);
                    }
                    else if (element.Type == "MoveMath")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], newX - element.VariableComboBoxes[0].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], newY + 27);
                        Canvas.SetLeft(element.VariableComboBoxes[1], newX - element.VariableComboBoxes[1].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], newY + 72);
                    }
                    else if (element.Type.Contains("Compare"))
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], newX - element.VariableComboBoxes[0].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], newY + 7);
                        Canvas.SetLeft(element.VariableComboBoxes[1], newX - element.VariableComboBoxes[1].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], newY + 52);
                    }
                    else if (element.Type.Contains("Timer") || element.Type.Contains("Count") || element.Type == "Reset")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], newX - element.VariableComboBoxes[0].Width / 2 + element.Image.Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], newY + 22);
                    }
                }

                HighlightPosition(currentPosition);

                _dragStartPosition = currentPosition;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles mouse left button release, finalizing drag operations for elements or wires.
        /// </summary>
        /// <param name="e">The mouse button event arguments.</param>
        /// <param name="owner">The owner window for displaying notifications.</param>
        public void HandleMouseLeftButtonUp(MouseButtonEventArgs e, Window owner)
        {
            if (!_isDragging && !_isDraggingWire) return;

            Point dropPosition = e.GetPosition(_canvas);
            Wire? closestWire = _elementFinder.FindClosestWire(dropPosition);
            var (closestBranch, isUpperLine) = _elementFinder.FindClosestBranch(dropPosition, _selectedNode as Branch);

            if (_selectedNode != null && _selectedNode.Image != null)
            {
                _selectedNode.Image.ReleaseMouseCapture();

                object? originalParent = _selectedNode.Parent;

                // Remove from original parent
                if (originalParent is Wire originalWire && originalWire.Nodes.Contains(_selectedNode))
                {
                    originalWire.Nodes.Remove(_selectedNode);
                }
                else if (originalParent is Branch originalBranch)
                {
                    if (!originalBranch.Nodes1.Remove(_selectedNode))
                        originalBranch.Nodes2.Remove(_selectedNode);
                }

                // Restore opacity
                _selectedNode.Image.Opacity = 1;
                if (_selectedNode is LadderElement el)
                {
                    el.VariableComboBoxes[0].Opacity = 1;
                    if (el.VariableComboBoxes.Count > 1)
                        el.VariableComboBoxes[1].Opacity = 1;
                    if (el.VariableComboBoxes.Count > 2)
                        el.VariableComboBoxes[2].Opacity = 1;
                }

                if (closestBranch != null)
                {
                    if (_selectedNode is LadderElement element && element.Type.Contains("Coil"))
                    {
                        new NotificationWindow("A Coil-type element cannot be placed in a branch", owner).Show();
                        if (originalParent is Wire wire)
                            wire.Nodes.Add(_selectedNode);
                    }
                    else
                    {
                        List<Node> targetNodes = isUpperLine ? closestBranch.Nodes1 : closestBranch.Nodes2;

                        // Insert based on X position
                        bool inserted = false;
                        for (int i = 0; i < targetNodes.Count; i++)
                        {
                            if (i == 0 && dropPosition.X < targetNodes[i].X)
                            {
                                targetNodes.Insert(0, _selectedNode);
                                inserted = true;
                                break;
                            }
                            if (i < targetNodes.Count - 1 && dropPosition.X > targetNodes[i].X && dropPosition.X < targetNodes[i + 1].X)
                            {
                                targetNodes.Insert(i + 1, _selectedNode);
                                inserted = true;
                                break;
                            }
                        }
                        if (!inserted)
                        {
                            targetNodes.Add(_selectedNode);
                        }
                        _selectedNode.Parent = closestBranch;
                    }
                }
                else if (closestWire != null)
                {
                    // Checking for Coil types and existence on the wire
                    if (_selectedNode is LadderElement element && element.Type.Contains("Coil"))
                    {
                        LadderElement? foundElement = closestWire.Nodes.OfType<LadderElement>().FirstOrDefault(el => el.Type.Contains("Coil"));

                        if (foundElement != null)
                        {
                            _canvas.Children.Remove(_selectedNode.Image);
                            if (_selectedNode is LadderElement x)
                                _canvas.Children.Remove(x.VariableComboBoxes[0]);
                            var dialog = new NotificationWindow("The wire already contains a Coil. Do you want to replace it?", owner, NotificationButtons.YesNo);
                            dialog.ShowDialog();
                            if (dialog.Result == true)
                            {
                                closestWire.Nodes.Remove(foundElement);
                                closestWire.Nodes.Add(_selectedNode);
                                _selectedNode.Parent = closestWire;
                            }
                            else
                            {
                                // Revert to original parent if user rejects replacement
                                if (originalParent is Wire wire)
                                    wire.Nodes.Add(_selectedNode);
                            }
                        }
                        else
                        {
                            closestWire.Nodes.Add(_selectedNode);
                            _selectedNode.Parent = closestWire;
                        }
                    }
                    else
                    {
                        // Inserting elements between others
                        bool inserted = false;
                        for (int i = 0; i < closestWire.Nodes.Count; i++)
                        {
                            // If the new element is before the first
                            if (i == 0 && dropPosition.X < closestWire.Nodes[i].X)
                            {
                                closestWire.Nodes.Insert(0, _selectedNode);
                                inserted = true;
                                break;
                            }
                            // If the new element is between two existing ones
                            if (i < closestWire.Nodes.Count - 1 && dropPosition.X > closestWire.Nodes[i].X && dropPosition.X < closestWire.Nodes[i + 1].X)
                            {
                                closestWire.Nodes.Insert(i + 1, _selectedNode);
                                inserted = true;
                                break;
                            }
                        }
                        if (!inserted)
                        {
                            closestWire.Nodes.Add(_selectedNode);
                        }
                        _selectedNode.Parent = closestWire; 
                    }
                }
                else
                {
                    // Revert to original parent if no valid drop location
                    if (originalParent is Wire wire)
                        wire.Nodes.Add(_selectedNode);
                    else if (originalParent is Branch branch)
                        (isUpperLine ? branch.Nodes1 : branch.Nodes2).Add(_selectedNode);
                }

                if (_highlightedObject is Wire hwire) hwire.UnhighlightWire();
                else if (_highlightedObject is Branch hbranch) hbranch.UnhighlightBranch();
                _highlightedObject = null;
                UnselectEverything();
                _canvasManager.UpdateCanvas();
                _isDragging = false;
                _selectedNode = null;
                e.Handled = true;
            }

            if (_selectedWire != null && _wireLine != null)
            {
                _wireLine.ReleaseMouseCapture();

                double dropY = e.GetPosition(_canvas).Y;

                _wiresManager.RemoveWire(_selectedWire);

                // Insert wire based on Y position
                bool inserted = false;
                for (int i = 0; i < _wiresManager.Wires.Count; i++)
                {
                    if (i == 0 && dropPosition.Y < _wiresManager.Wires[i].Y)
                    {
                        _wiresManager.InsertWire(_selectedWire, 0);
                        inserted = true;
                        break;
                    }
                    // If the new element is between two existing ones
                    if (i < _wiresManager.Wires.Count - 1 && dropPosition.Y > _wiresManager.Wires[i].Y && dropPosition.Y < _wiresManager.Wires[i + 1].Y)
                    {
                        _wiresManager.InsertWire(_selectedWire, i + 1);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted)
                {
                    _wiresManager.AddWire(_selectedWire);
                }

                _selectedWire.UnselectWire();
                _isDraggingWire = false;
                _selectedWire = null;

                _canvasManager.UpdateCanvas();
            }
        }

        /// <summary>
        /// Handles mouse left button down, selecting elements, wires, or branches.
        /// </summary>
        /// <param name="e">The mouse button event arguments.</param>
        /// <param name="elementListView">The ListView to clear selection from.</param>
        public void HandleMouseLeftButtonDown(MouseButtonEventArgs e, ListView elementListView)
        {
            _canvas.Focus(); // Ensure canvas gets focus on click
            elementListView.SelectedItem = null;    // Clear ListView selection

            Point clickPosition = e.GetPosition(_canvas);
            LadderElement? closestElement = _elementFinder.FindClosestElement(clickPosition);
            Wire? closestWire = _elementFinder.FindClosestWire(clickPosition);

            UnselectEverything();

            if (closestElement != null)
            {
                _selectedNode = closestElement;
                _selectedNode.HighlightNode();
                _dragStartPosition = e.GetPosition(_canvas);
                e.Handled = true;
                return;
            }

            // Perform hit test for lines
            double hitTestOffset = 5.0;
            var clickPoint = clickPosition;
            var hitArea = new EllipseGeometry(clickPoint, hitTestOffset, hitTestOffset);
            var hitTestParams = new GeometryHitTestParameters(hitArea);
            List<DependencyObject> hitResults = new List<DependencyObject>();

            VisualTreeHelper.HitTest(
                _canvas,
                null,
                result =>
                {
                    if (result.VisualHit != null)
                    {
                        hitResults.Add(result.VisualHit);
                    }
                    return HitTestResultBehavior.Continue;
                },
                hitTestParams);

            // Check for matches
            foreach (var hit in hitResults)
            {
                if (hit is Line clickedLine)
                {
                    // If the line has a Tag that is Branch, select that branch
                    if (clickedLine.Tag is Branch branch)
                    {
                        _selectedNode = branch;
                        branch.HighlightBranchRecursive();
                        _dragStartPosition = clickPosition;
                        return;
                    }
                    // If the line has a Tag that is Wire, select the wire
                    else if (clickedLine.Tag is Wire wire)
                    {
                        _selectedWire = wire;
                        _selectedWire.SelectWire();
                        _dragStartPosition = clickPosition;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Unselects all elements and wires, clearing highlights.
        /// </summary>
        public void UnselectEverything()
        {
            if (_selectedNode != null)
            {
                if (_selectedNode is LadderElement && _selectedNode.Image != null)
                {
                    _selectedNode.UnhighlightNode();
                }
                else if (_selectedNode is Branch branch)
                {
                    branch.UnhighlightBranchRecursive();
                }
                _selectedNode = null;
            }

            if (_selectedWire != null)
            {
                var previousLine = _selectedWire.WireLine;
                if (previousLine != null)
                {
                    previousLine.Stroke = Brushes.Black;
                    previousLine.StrokeThickness = 2;
                }
                _selectedWire = null;
            }
        }

        /// <summary>
        /// Deletes the currently selected element or wire.
        /// </summary>
        /// <param name="owner">The owner window for displaying notifications.</param>
        public void DeleteSelected(Window owner)
        {
            if (_selectedNode != null)
            {
                if (_selectedNode.Parent is Wire wire)
                {
                    wire.Nodes.Remove(_selectedNode);
                }
                else if (_selectedNode.Parent is Branch branch)
                {
                    if (branch.Nodes1.Contains(_selectedNode))
                        branch.Nodes1.Remove(_selectedNode);
                    else if (branch.Nodes2.Contains(_selectedNode))
                        branch.Nodes2.Remove(_selectedNode);
                }

                _selectedNode = null;
                _canvasManager.UpdateCanvas();
            }
            else if (_selectedWire != null)
            {
                _wiresManager.RemoveWire(_selectedWire);
                _selectedWire = null;
                _canvasManager.UpdateCanvas();
            }
            else
            {
                new NotificationWindow("Select an element to delete", owner).Show();
            }
        }
    }
}
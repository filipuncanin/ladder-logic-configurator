using ladder_diagram_app.Models.CanvasElements;
using System.Windows.Controls;
using System.Windows;
using ladder_diagram_app.Models.CanvasElements.Instances;

namespace ladder_diagram_app.Services.CanvasServices
{
    /// <summary>
    /// Manages the rendering and layout of canvas elements in a ladder diagram application.
    /// </summary>
    public class CanvasManager
    {
        private readonly Canvas _canvas;
        private readonly Grid _gridCanvas;
        private readonly WiresManager _wiresManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasManager"/> class.
        /// </summary>
        /// <param name="canvas">The canvas to render elements on.</param>
        /// <param name="gridCanvas">The grid containing the canvas for sizing reference.</param>
        /// <param name="wiresManager">Manages wires and their associated nodes.</param>
        public CanvasManager(Canvas canvas, Grid gridCanvas, WiresManager wiresManager)
        {
            _canvas = canvas;
            _gridCanvas = gridCanvas;
            _wiresManager = wiresManager;
        }

        /// <summary>
        /// Updates the canvas by adjusting its size, positioning elements, and rendering wires and nodes.
        /// </summary>
        public void UpdateCanvas()
        {
            // Calculate base canvas dimensions, accounting for scrollbars
            double baseWidth = _gridCanvas.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            double baseHeight = _gridCanvas.ActualHeight - SystemParameters.HorizontalScrollBarHeight;

            // Determine required canvas size based on wire and node dimensions
            double requiredWidth = _wiresManager.Wires.Any() ? _wiresManager.Wires.Max(w => w.Nodes.Sum(e => e.Width)) : 0;
            double requiredHeight = _wiresManager.Wires.Sum(w => w.Height) + 80;

            _canvas.Width = Math.Max(baseWidth, requiredWidth);
            _canvas.Height = Math.Max(baseHeight, requiredHeight);

            // Position wires and their nodes
            double startY = 60;
            foreach (var wire in _wiresManager.Wires)
            {
                wire.Width = _canvas.Width;
                wire.Y = startY;
                UpdateElementsParameters(wire.Nodes, wire.Y, 0, _canvas);

                startY += wire.Height;

                wire.Nodes = wire.Nodes.OrderBy(n => n.X).ToList();
            }

            // Re-adjust canvas size after positioning
            requiredWidth = _wiresManager.Wires.Any() ? _wiresManager.Wires.Max(w => w.Nodes.Sum(e => e.Width)) : 0;
            requiredHeight = _wiresManager.Wires.Sum(w => w.Height) + 80;

            _canvas.Width = Math.Max(baseWidth, requiredWidth);
            _canvas.Height = Math.Max(baseHeight, requiredHeight);

            // Render canvas elements
            _canvas.Children.Clear();
            foreach (var wire in _wiresManager.Wires)
            {
                _canvas.Children.Add(wire.WireLine);
                DrawNodes(wire.Nodes, wire.Y, 0, _canvas);
            }
        }

        /// <summary>
        /// Updates the positions and parameters of nodes (elements and branches) within a wire or branch.
        /// </summary>
        /// <param name="nodes">The list of nodes to update.</param>
        /// <param name="y1">The Y-coordinate of the parent wire or branch line.</param>
        /// <param name="startX">The starting X-coordinate for positioning nodes.</param>
        /// <param name="canvas">The canvas for positioning reference.</param>
        private void UpdateElementsParameters(List<Node> nodes, double y1, double startX, Canvas canvas)
        {
            double currentX = startX;

            foreach (var node in nodes)
            {
                if (node is LadderElement element)
                {
                    // Position coil elements at the right edge of the canvas
                    if (element.Type == "Coil" || element.Type == "OSPCoil" || element.Type == "SetCoil" || element.Type == "ResetCoil")
                    {
                        element.X = canvas.Width - element.Width / 2;
                    }
                    else
                    {
                        element.X = currentX + element.Width / 2;
                        currentX += element.Width;
                    }
                    element.Y = y1;
                }
                else if (node is Branch branch)
                {
                    branch.X = currentX + branch.Width / 2;
                    currentX += branch.Width;
                    branch.Y = y1;

                    double branchStartX = branch.X - branch.Width / 2 + 10;

                    // Recursively update nodes in both branch lines
                    UpdateElementsParameters(branch.Nodes1, branch.Y, branchStartX, canvas);
                    UpdateElementsParameters(branch.Nodes2, branch.Y2, branchStartX, canvas);

                    branch.Nodes1 = branch.Nodes1.OrderBy(n => n.X).ToList();
                    branch.Nodes2 = branch.Nodes2.OrderBy(n => n.X).ToList();
                }
            }
        }

        /// <summary>
        /// Draws nodes (elements and branches) and their associated UI components on the canvas.
        /// </summary>
        /// <param name="nodes">The list of nodes to draw.</param>
        /// <param name="y1">The Y-coordinate of the parent wire or branch line.</param>
        /// <param name="startX">The starting X-coordinate for positioning nodes.</param>
        /// <param name="canvas">The canvas to draw on.</param>
        private void DrawNodes(List<Node> nodes, double y1, double startX, Canvas canvas)
        {
            foreach (var node in nodes)
            {
                if (node is LadderElement element && element.Image != null)
                {
                    // Position the element image
                    Canvas.SetLeft(element.Image, element.X - element.Image.Width / 2);
                    Canvas.SetTop(element.Image, element.Y - element.Image.Height / 2);
                    canvas.Children.Add(element.Image);

                    // Position ComboBoxes based on element type
                    if (element.Type.Contains("Contact") || element.Type.Contains("Coil"))
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], element.X - element.VariableComboBoxes[0].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], element.Y - 45);
                        canvas.Children.Add(element.VariableComboBoxes[0]);
                    }
                    else if (element.Type == "AddMath" || element.Type == "SubtractMath" || element.Type == "MultiplyMath" || element.Type == "DivideMath")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], element.X - element.VariableComboBoxes[0].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], element.Y - 52.5);
                        Canvas.SetLeft(element.VariableComboBoxes[1], element.X - element.VariableComboBoxes[1].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], element.Y - 12.5);
                        Canvas.SetLeft(element.VariableComboBoxes[2], element.X - element.VariableComboBoxes[2].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[2], element.Y + 27.5);
                        canvas.Children.Add(element.VariableComboBoxes[0]);
                        canvas.Children.Add(element.VariableComboBoxes[1]);
                        canvas.Children.Add(element.VariableComboBoxes[2]);
                    }
                    else if (element.Type == "MoveMath")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], element.X - element.VariableComboBoxes[0].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], element.Y - 25);
                        Canvas.SetLeft(element.VariableComboBoxes[1], element.X - element.VariableComboBoxes[1].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], element.Y + 20);
                        canvas.Children.Add(element.VariableComboBoxes[0]);
                        canvas.Children.Add(element.VariableComboBoxes[1]);
                    }
                    else if (element.Type.Contains("Compare"))
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], element.X - element.VariableComboBoxes[0].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], element.Y - 35);
                        Canvas.SetLeft(element.VariableComboBoxes[1], element.X - element.VariableComboBoxes[1].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[1], element.Y + 10);
                        canvas.Children.Add(element.VariableComboBoxes[0]);
                        canvas.Children.Add(element.VariableComboBoxes[1]);
                    }
                    else if (element.Type.Contains("Timer") || element.Type.Contains("Count") || element.Type == "Reset")
                    {
                        Canvas.SetLeft(element.VariableComboBoxes[0], element.X - element.VariableComboBoxes[0].Width / 2);
                        Canvas.SetTop(element.VariableComboBoxes[0], element.Y - 5);
                        canvas.Children.Add(element.VariableComboBoxes[0]);
                    }
                }
                else if (node is Branch branch)
                {
                    // Draw branch lines
                    canvas.Children.Add(branch.UpperLine);
                    canvas.Children.Add(branch.LowerLine);
                    canvas.Children.Add(branch.LeftLine);
                    canvas.Children.Add(branch.RightLine);

                    double branchStartX = branch.X - branch.Width / 2 + 10;
                    // Recursively draw nodes in both branch lines
                    DrawNodes(branch.Nodes1, branch.Y, branchStartX, canvas);
                    DrawNodes(branch.Nodes2, branch.Y2, branchStartX, canvas);
                }
            }
        }
    }
}
using System.Windows.Media;
using System.Windows.Shapes;

namespace ladder_diagram_app.Models.CanvasElements.Instances
{
    /// <summary>
    /// Represents a wire in a ladder diagram, connecting nodes with a visual line.
    /// </summary>
    public class Wire
    {
        /// <summary>
        /// Stores the Y-coordinate of the wire.
        /// </summary>
        private double _y;

        /// <summary>
        /// Gets or sets the width of the wire.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the list of nodes connected by the wire.
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the line representing the wire visually.
        /// </summary>
        public Line WireLine { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wire"/> class with an empty node list and default line.
        /// </summary>
        public Wire()
        {
            Nodes = [];
            WireLine = new Line() { 
                X1 = 0, 
                Y1 = 0, 
                X2 = 0, 
                Y2 = 0, 
                Stroke = Brushes.Black, 
                StrokeThickness = 2, 
                Tag = this 
            };
        }

        /// <summary>
        /// Gets or sets the Y-coordinate of the wire, updating the wire line when set.
        /// </summary>
        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                UpdateWireLine();   // Refresh wire line position
            }
        }

        /// <summary>
        /// Updates the coordinates of the wire line based on current Y and Width values.
        /// </summary>
        private void UpdateWireLine()
        {
            WireLine.X1 = 0;
            WireLine.Y1 = Y;
            WireLine.X2 = Width;
            WireLine.Y2 = Y;
        }

        /// <summary>
        /// Gets the height of the wire, calculated based on the deepest branch Y2 value plus a margin.
        /// </summary>
        public double Height
        {
            get
            {
                return GetMaxY2FromBranches(Nodes) - Y + 125;   // Add minimum height margin
            }
        }

        /// <summary>
        /// Recursively calculates the maximum Y2 value from branches in the node list.
        /// </summary>
        /// <param name="nodes">The list of nodes to evaluate.</param>
        /// <returns>The maximum Y2 value, or the wire's Y if no branches are found.</returns>
        private double GetMaxY2FromBranches(List<Node> nodes)
        {
            double maxY2 = Y;
            foreach (var node in nodes)
            {
                if (node is Branch branch)
                {
                    // Compare current branch Y2
                    maxY2 = Math.Max(maxY2, branch.Y2);
                    // Recursively check Nodes2 for deeper branches
                    double maxY2FromNodes2 = GetMaxY2FromBranches(branch.Nodes2);
                    maxY2 = Math.Max(maxY2, maxY2FromNodes2);
                }
            }
            return maxY2;
        }

        /// <summary>
        /// Highlights the wire by setting its line to a red, thicker style.
        /// </summary>
        public void SelectWire()
        {
            WireLine.Stroke = Brushes.Red;
            WireLine.StrokeThickness = 4;
        }

        /// <summary>
        /// Resets the wire's line to its default black style.
        /// </summary>
        public void UnselectWire()
        {
            WireLine.Stroke = Brushes.Black;
            WireLine.StrokeThickness = 2;
        }

        /// <summary>
        /// Highlights the wire with a blue, dashed, thicker style.
        /// </summary>
        public void HighlightWire()
        {
            WireLine.Stroke = Brushes.Blue;
            WireLine.StrokeThickness = 4;
            WireLine.StrokeDashArray = [2, 1];
        }

        /// <summary>
        /// Resets the wire's line to its default black style, removing the dashed effect.
        /// </summary>
        public void UnhighlightWire()
        {
            WireLine.Stroke = Brushes.Black;
            WireLine.StrokeThickness = 2;
            WireLine.StrokeDashArray = null;
        }
    }
}

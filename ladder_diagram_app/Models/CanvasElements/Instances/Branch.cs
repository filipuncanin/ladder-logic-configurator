using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace ladder_diagram_app.Models.CanvasElements.Instances
{
    /// <summary>
    /// Represents a branch node in a ladder diagram, containing two lists of child nodes and visual lines for rendering.
    /// </summary>
    public class Branch : Node
    {
        /// <summary>
        /// Gets or sets the first list of child nodes in the branch.
        /// </summary>
        public List<Node> Nodes1 { get; set; }

        /// <summary>
        /// Gets or sets the second list of child nodes in the branch.
        /// </summary>
        public List<Node> Nodes2 { get; set; }

        /// <summary>
        /// Stores the Y-coordinate of the branch.
        /// </summary>
        private double _y;

        /// <summary>
        /// Stores the X-coordinate of the branch.
        /// </summary>
        private double _x;

        /// <summary>
        /// Gets or sets the Y-coordinate of the lower line of the branch, calculated based on child nodes.
        /// </summary>
        public double Y2 { get; set; }

        /// <summary>
        /// Gets or sets the upper horizontal line, Lower Horizontal Line, Left Vertical Line and Right Vertical Line of the branch.
        /// </summary>
        public Line UpperLine { get; set; }
        public Line LowerLine { get; set; }
        public Line LeftLine { get; set; }
        public Line RightLine { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Branch"/> class with default image and lines.
        /// </summary>
        public Branch()
        {
            // Initialize the branch icon with a drop shadow effect
            Image = new Image
            {
                Width = 48,
                Height = 24,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Branch/branch_icon.png")),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 10,
                    Opacity = 0.8
                }
            };

            // Initialize node lists
            Nodes1 = [];
            Nodes2 = [];

            // Initialize lines with default properties and tag them with this branch instance
            UpperLine = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 2, Tag = this };
            LowerLine = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 2, Tag = this };
            LeftLine = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 2, Tag = this };
            RightLine = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 2, Tag = this };
        }

        /// <summary>
        /// Gets or sets the Y-coordinate of the branch, updating lines and Y2 when set.
        /// </summary>
        public override double Y
        {
            get => _y;
            set
            {
                _y = value;
                Y2 = _y + CalculateY2(Nodes1);  // Update Y2 based on child nodes
                UpdateLines(); // Refresh line positions
            }
        }

        /// <summary>
        /// Gets or sets the X-coordinate of the branch, updating lines when set.
        /// </summary>
        public override double X
        {
            get => _x;
            set
            {
                _x = value;
                UpdateLines(); // Refresh line positions
            }
        }

        /// <summary>
        /// Updates the coordinates of the branch's lines based on current X, Y, and Y2 values.
        /// </summary>
        private void UpdateLines()
        {
            // Update upper horizontal line
            UpperLine.X1 = X - Width / 2 + 10;
            UpperLine.Y1 = Y;
            UpperLine.X2 = X + Width / 2 - 10;
            UpperLine.Y2 = Y;

            // Update lower horizontal line
            LowerLine.X1 = X - Width / 2 + 10;
            LowerLine.Y1 = Y2;
            LowerLine.X2 = X + Width / 2 - 10;
            LowerLine.Y2 = Y2;

            // Update left vertical line
            LeftLine.X1 = X - Width / 2 + 10;
            LeftLine.Y1 = Y;
            LeftLine.X2 = X - Width / 2 + 10;
            LeftLine.Y2 = Y2;

            // Update right vertical line
            RightLine.X1 = X + Width / 2 - 10;
            RightLine.Y1 = Y;
            RightLine.X2 = X + Width / 2 - 10;
            RightLine.Y2 = Y2;
        }

        /// <summary>
        /// Calculates the height (Y2 offset) of the branch based on its child nodes.
        /// </summary>
        /// <param name="nodes">The list of nodes to evaluate.</param>
        /// <returns>The calculated height, with a minimum of 125.</returns>
        private static double CalculateY2(List<Node> nodes)
        {
            double maxY2 = 125; // Default minimum height for empty or non-branch nodes

            // Iterate through nodes to calculate height
            foreach (Node node in nodes)
            {
                if (node is Branch branch)
                {
                    // Recursively calculate heights for sub-branch Nodes1 and Nodes2
                    double nodes1Height = CalculateY2(branch.Nodes1);
                    double nodes2Height = CalculateY2(branch.Nodes2);

                    // Sum heights as branches extend downward
                    double branchY2 = nodes1Height + nodes2Height;

                    // Update maxY2 if this branch is deeper
                    maxY2 = Math.Max(maxY2, branchY2);
                }
            }
            return maxY2;
        }

        /// <summary>
        /// Gets the total width of the branch, based on the wider of Nodes1 or Nodes2.
        /// </summary>
        public override double Width
        {
            get
            {
                double nodes1Width = CalculateTotalWidth(Nodes1);
                double nodes2Width = CalculateTotalWidth(Nodes2);
                // Return the maximum width plus padding, or default width of 130 if no nodes
                return Math.Max(nodes1Width, nodes2Width) != 0 ? Math.Max(nodes1Width, nodes2Width) + 20 : 130;
            }
        }

        /// <summary>
        /// Calculates the total width of a list of nodes.
        /// </summary>
        /// <param name="nodes">The list of nodes to measure.</param>
        /// <returns>The sum of the widths of all nodes.</returns>
        private static double CalculateTotalWidth(List<Node> nodes)
        {
            double totalWidth = 0;
            foreach (var node in nodes)
            {
                totalWidth += node.Width;
            }
            return totalWidth;
        }

        /// <summary>
        /// Highlights either the upper or lower line of the branch with a blue dashed style.
        /// </summary>
        /// <param name="isUpperLine">True to highlight the upper line, false for the lower line.</param>
        public void HighlightBranch(bool isUpperLine)
        {
            if (isUpperLine)
            {
                // Highlight upper line
                UpperLine.Stroke = Brushes.Blue; 
                UpperLine.StrokeThickness = 4;
                UpperLine.StrokeDashArray = [2, 1];
            }
            else
            {
                // Highlight lower line
                LowerLine.Stroke = Brushes.Blue;
                LowerLine.StrokeThickness = 4;
                LowerLine.StrokeDashArray = [2, 1];
            }
        }

        /// <summary>
        /// Resets the highlighting of both upper and lower lines to default black.
        /// </summary>
        public void UnhighlightBranch()
        {
            // Reset upper line
            UpperLine.Stroke = Brushes.Black;
            UpperLine.StrokeThickness = 2;
            UpperLine.StrokeDashArray = null;

            // Reset lower line
            LowerLine.Stroke = Brushes.Black;
            LowerLine.StrokeThickness = 2;
            LowerLine.StrokeDashArray = null;
        }

        /// <summary>
        /// Recursively highlights the current branch and all sub-branches with red lines.
        /// </summary>
        public void HighlightBranchRecursive()
        {
            // Highlight all lines of the current branch
            UpperLine.Stroke = Brushes.Red; UpperLine.StrokeThickness = 4; 
            LowerLine.Stroke = Brushes.Red; LowerLine.StrokeThickness = 4; 
            LeftLine.Stroke = Brushes.Red; LeftLine.StrokeThickness = 4; 
            RightLine.Stroke = Brushes.Red; RightLine.StrokeThickness = 4;

            // Recursively highlight sub-branches in Nodes1 and Nodes2
            foreach (var node in Nodes1.Concat(Nodes2))
            {
                if (node is Branch subBranch)
                {
                    subBranch.HighlightBranchRecursive();
                }
            }
        }

        /// <summary>
        /// Recursively resets the highlighting of the current branch and all sub-branches to default black lines.
        /// </summary>
        public void UnhighlightBranchRecursive()
        {
            // Reset all lines of the current branch
            UpperLine.Stroke = Brushes.Black; UpperLine.StrokeThickness = 2; 
            LowerLine.Stroke = Brushes.Black; LowerLine.StrokeThickness = 2; 
            LeftLine.Stroke = Brushes.Black; LeftLine.StrokeThickness = 2; 
            RightLine.Stroke = Brushes.Black; RightLine.StrokeThickness = 2;

            // Recursively reset sub-branches in Nodes1 and Nodes2
            foreach (var node in Nodes1.Concat(Nodes2))
            {
                if (node is Branch subBranch)
                {
                    subBranch.UnhighlightBranchRecursive();
                }
            }
        }

        /// <summary>
        /// Checks if the target branch is the same as this branch or nested within it.
        /// </summary>
        /// <param name="targetBranch">The branch to check for nesting.</param>
        /// <returns>True if the target branch is this branch or a descendant, false otherwise.</returns>
        public bool IsBranchNested(Branch targetBranch)
        {
            if (this == targetBranch) return true; // Same branch

            // Recursively check Nodes1 for nested branches
            foreach (var node in Nodes1)
            {
                if (node is Branch subBranch && subBranch.IsBranchNested(targetBranch))
                {
                    return true;
                }
            }

            // Recursively check Nodes2 for nested branches
            foreach (var node in Nodes2)
            {
                if (node is Branch subBranch && subBranch.IsBranchNested(targetBranch))
                {
                    return true;
                }
            }

            return false; // Not nested
        }
    }
}

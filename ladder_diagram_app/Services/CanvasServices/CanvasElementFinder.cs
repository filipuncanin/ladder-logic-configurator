using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using ladder_diagram_app.Models.CanvasElements;
using ladder_diagram_app.Models.CanvasElements.Instances;

namespace ladder_diagram_app.Services.CanvasServices
{
    /// <summary>
    /// Provides methods to find canvas elements (wires, ladder elements, and branches) based on cursor position in a ladder diagram application.
    /// </summary>
    public class CanvasElementFinder
    {
        private readonly Func<WiresManager> _getWiresManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanvasElementFinder"/> class with a function to retrieve the WiresManager.
        /// </summary>
        /// <param name="getWiresManager">A function that returns the WiresManager instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="getWiresManager"/> is null.</exception>
        public CanvasElementFinder(Func<WiresManager> getWiresManager)
        {
            _getWiresManager = getWiresManager ?? throw new ArgumentNullException(nameof(getWiresManager));
        }

        /// <summary>
        /// Finds the closest wire to the specified cursor position within a 20-pixel threshold.
        /// </summary>
        /// <param name="cursorPosition">The cursor position on the canvas.</param>
        /// <returns>The closest <see cref="Wire"/> if within threshold, otherwise null.</returns>
        public Wire? FindClosestWire(Point cursorPosition)
        {
            var wiresManager = _getWiresManager();
            var closestWire = wiresManager.Wires.MinBy(w => Math.Abs(w.Y - cursorPosition.Y));
            return closestWire != null && Math.Abs(closestWire.Y - cursorPosition.Y) <= 20 ? closestWire : null;
        }

        /// <summary>
        /// Finds the closest ladder element to the specified cursor position by checking if the cursor is within the element's bounds.
        /// </summary>
        /// <param name="cursorPosition">The cursor position on the canvas.</param>
        /// <returns>The closest <see cref="LadderElement"/> if found, otherwise null.</returns>
        public LadderElement? FindClosestElement(Point cursorPosition)
        {
            var wiresManager = _getWiresManager();

            LadderElement? FindElementInBounds(List<Node> nodes)
            {
                foreach (var node in nodes)
                {
                    if (node is LadderElement element)
                    {
                        if (element.Image == null) continue;

                        double left = Canvas.GetLeft(element.Image);
                        double top = Canvas.GetTop(element.Image);
                        double right = left + element.Image.Width;
                        double bottom = top + element.Image.Height;

                        if (cursorPosition.X >= left && cursorPosition.X <= right &&
                            cursorPosition.Y >= top && cursorPosition.Y <= bottom)
                        {
                            return element;
                        }
                    }
                    else if (node is Branch branch)
                    {
                        var foundInNodes1 = FindElementInBounds(branch.Nodes1);
                        if (foundInNodes1 != null)
                        {
                            return foundInNodes1;
                        }
                        var foundInNodes2 = FindElementInBounds(branch.Nodes2);
                        if (foundInNodes2 != null)
                        {
                            return foundInNodes2;
                        }
                    }
                }
                return null;
            }

            foreach (var wire in wiresManager.Wires)
            {
                var foundElement = FindElementInBounds(wire.Nodes);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the closest branch to the specified cursor position, considering upper or lower lines within a 20-pixel threshold.
        /// </summary>
        /// <param name="cursorPosition">The cursor position on the canvas.</param>
        /// <param name="selectedBranch">An optional branch to exclude nested branches of.</param>
        /// <returns>A tuple containing the closest <see cref="Branch"/> and a boolean indicating if the upper line was closest.</returns>
        public (Branch? ClosestBranch, bool IsUpperLine) FindClosestBranch(Point cursorPosition, Branch? selectedBranch = null)
        {
            var wiresManager = _getWiresManager();
            var allBranches = new List<(Branch Branch, int Depth)>();

            // Recursively collect all branches with their nesting depth
            void CollectBranches(List<Node> nodes, int depth = 0)
            {
                foreach (var node in nodes)
                {
                    if (node is Branch branch)
                    {
                        allBranches.Add((branch, depth));
                        CollectBranches(branch.Nodes1, depth + 1);
                        CollectBranches(branch.Nodes2, depth + 1);
                    }
                }
            }

            foreach (var wire in wiresManager.Wires)
            {
                CollectBranches(wire.Nodes);
            }

            if (!allBranches.Any())
            {
                return (null, false);
            }

            Branch? closestBranch = null;
            bool isUpperLine = false;
            double minDistance = double.MaxValue;
            int maxDepth = -1;

            foreach (var (branch, depth) in allBranches)
            {
                // Skip if the branch is nested within the selected branch
                if (selectedBranch != null && selectedBranch.IsBranchNested(branch))
                {
                    continue;
                }

                Line? upperLine = branch.UpperLine;
                Line? lowerLine = branch.LowerLine;

                if (upperLine == null || lowerLine == null)
                {
                    continue;
                }

                double upperX1 = upperLine.X1;
                double upperX2 = upperLine.X2;
                double upperY = upperLine.Y1;

                double lowerX1 = lowerLine.X1;
                double lowerX2 = lowerLine.X2;
                double lowerY = lowerLine.Y1;

                // Check proximity to upper line
                if (cursorPosition.X >= upperX1 && cursorPosition.X <= upperX2 &&
                    cursorPosition.Y >= upperY - 20 && cursorPosition.Y <= upperY + 20)
                {
                    double distanceToY1 = Math.Abs(upperY - cursorPosition.Y);
                    if (distanceToY1 < minDistance || (distanceToY1 == minDistance && depth > maxDepth))
                    {
                        minDistance = distanceToY1;
                        closestBranch = branch;
                        isUpperLine = true;
                        maxDepth = depth;
                    }
                }

                // Check proximity to lower line
                if (cursorPosition.X >= lowerX1 && cursorPosition.X <= lowerX2 &&
                    cursorPosition.Y >= lowerY - 20 && cursorPosition.Y <= lowerY + 20)
                {
                    double distanceToY2 = Math.Abs(lowerY - cursorPosition.Y);
                    if (distanceToY2 < minDistance || (distanceToY2 == minDistance && depth > maxDepth))
                    {
                        minDistance = distanceToY2;
                        closestBranch = branch;
                        isUpperLine = false;
                        maxDepth = depth;
                    }
                }
            }

            if (minDistance <= 20 && closestBranch != null)
            {
                return (closestBranch, isUpperLine);
            }
            return (null, false);
        }
    }
}
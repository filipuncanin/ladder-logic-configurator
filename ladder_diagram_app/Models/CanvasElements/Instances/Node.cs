using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ladder_diagram_app.Models.CanvasElements.Instances
{
    /// <summary>
    /// Abstract base class for nodes in a ladder diagram, providing common properties and methods for positioning and highlighting.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Gets or sets the X-coordinate of the node.
        /// </summary>
        public abstract double X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the node.
        /// </summary>
        public abstract double Y { get; set; }

        /// <summary>
        /// Gets or sets the parent object of the node, used to track hierarchical relationships.
        /// </summary>
        public object? Parent { get; set; }

        /// <summary>
        /// Gets the width of the node, typically based on its visual representation.
        /// </summary>
        public abstract double Width { get; }

        /// <summary>
        /// Gets or sets the image representing the node visually, if applicable.
        /// </summary>
        public virtual Image? Image { get; set; }


        /// <summary>
        /// Highlights the node by applying a red drop shadow effect to its image.
        /// </summary>
        public void HighlightNode()
        {
            // Apply highlight effect only if the node has an image
            if (Image != null) 
            {
                Image.Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 10,
                    Opacity = 0.8
                };
            }
        }

        /// <summary>
        /// Removes the highlight effect from the node by clearing its image effect.
        /// </summary>
        public void UnhighlightNode()
        {
            // Clear highlight effect only if the node has an image
            if (Image != null) 
            {
                Image.Effect = null;
            }
        }
    }
}

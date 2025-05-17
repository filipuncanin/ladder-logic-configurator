using ladder_diagram_app.Models.CanvasElements.Instances;

namespace ladder_diagram_app.Models.CanvasElements
{
    /// <summary>
    /// Manages a collection of wires in a ladder diagram, providing methods to add, remove, insert, and clear wires.
    /// </summary>
    public class WiresManager
    {
        /// <summary>
        /// Gets the list of wires managed by this instance.
        /// </summary>
        public List<Wire> Wires { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WiresManager"/> class with an empty wire list.
        /// </summary>
        public WiresManager()
        {
            Wires = [];
        }

        /// <summary>
        /// Adds a wire to the collection, creating a new wire if none is provided.
        /// </summary>
        /// <param name="wire">The wire to add, or null to create a new wire.</param>
        public void AddWire(Wire? wire = null)
        {
            Wires.Add(wire ?? new Wire()); // Use provided wire or create a new one
        }

        /// <summary>
        /// Removes a specified wire from the collection.
        /// </summary>
        /// <param name="wire">The wire to remove.</param>
        public void RemoveWire(Wire wire)
        {
            Wires.Remove(wire); // Remove the specified wire
        }

        /// <summary>
        /// Inserts a wire at the specified index in the collection.
        /// </summary>
        /// <param name="wire">The wire to insert.</param>
        /// <param name="index">The zero-based index at which to insert the wire.</param>
        public void InsertWire(Wire wire, int index)
        {
            Wires.Insert(index, wire); // Insert wire at the specified index
        }

        /// <summary>
        /// Clears all wires from the collection.
        /// </summary>
        public void ClearWires()
        {
            Wires.Clear(); // Remove all wires
        }
    }
}

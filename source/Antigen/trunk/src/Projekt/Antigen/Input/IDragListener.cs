using Antigen.Graphics;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Input
{
    /// <summary>
    /// Interface for dragging event listeners. Dragging events
    /// occur when the user holds down a mouse button while moving
    /// the cursor across the screen. Note that a single click
    /// is a corner case of dragging, firing one dragging start
    /// and one dragging stop event.
    /// </summary>
    interface IDragListener : IEventListener
    {
        /// <summary>
        /// Processes a dragging start event. This event
        /// is fired whenever the user presses a mouse button,
        /// potentially beginning a drag operation.
        /// </summary>
        /// <param name="info">Information about the starting
        /// point of the drag.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further dragging event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleDragStarted(ClickInfo info);

        /// <summary>
        /// Processes a mid-dragging event. This event is fired
        /// when the user moves the mouse cursor with one button
        /// held down.
        /// </summary>
        /// <param name="button">Button that is held down.</param>
        /// <param name="location">Current mouse cursor location.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further dragging event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleDragging(MouseButtons button, Coord<Point> location);

        /// <summary>
        /// Processes a dragging stop event. This event is fired
        /// when the user releases the mouse cursor after dragging.
        /// </summary>
        /// <param name="info">Information about the dragging end point.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further dragging event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleDragStopped(ClickInfo info);
    }
}

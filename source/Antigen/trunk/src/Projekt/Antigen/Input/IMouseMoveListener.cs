using Antigen.Graphics;
using Microsoft.Xna.Framework;

namespace Antigen.Input
{
    /// <summary>
    /// Interface for mouse movement listeners.
    /// </summary>
    interface IMouseMoveListener : IEventListener
    {
        /// <summary>
        /// Handles a mouse movement event.
        /// </summary>
        /// <param name="endPoint">The point where the mouse
        /// movement ended.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispatched
        /// to further mouse movement event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleMouseMove(Coord<Point> endPoint);
    }
}

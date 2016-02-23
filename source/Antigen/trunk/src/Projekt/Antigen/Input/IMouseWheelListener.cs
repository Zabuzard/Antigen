namespace Antigen.Input
{
    /// <summary>
    /// Interface for mouse wheel listeners.
    /// </summary>
    interface IMouseWheelListener : IEventListener
    {
        /// <summary>
        /// Interprets a mouse wheel rotation event.
        /// </summary>
        /// <param name="ticks">Number of ticks the mouse
        /// wheel has been rotated.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispatched
        /// to further mouse wheel event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleMouseWheelRotated(float ticks);
    }
}

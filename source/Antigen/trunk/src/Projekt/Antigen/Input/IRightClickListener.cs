namespace Antigen.Input
{
    /// <summary>
    /// Interface for right click event listeners.
    /// </summary>
    interface IRightClickListener : IEventListener
    {
        /// <summary>
        /// Handles a right click event.
        /// </summary>
        /// <param name="info">Information about the click
        /// that occured.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further right click event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleRightClick(ClickInfo info);
    }
}

namespace Antigen.Input
{
    /// <summary>
    /// Interface for left click event listeners.
    /// </summary>
    interface ILeftClickListener : IEventListener
    {
        /// <summary>
        /// Handles a left click event.
        /// </summary>
        /// <param name="info">Information about the click
        /// that occured.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further left click event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleLeftClick(ClickInfo info);
    }
}

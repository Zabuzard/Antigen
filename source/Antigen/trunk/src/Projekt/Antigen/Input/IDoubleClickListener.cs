namespace Antigen.Input
{
    /// <summary>
    /// Listener for double click events.
    /// </summary>
    interface IDoubleClickListener : IEventListener
    {
        /// <summary>
        /// Handle a double click event.
        /// </summary>
        /// <param name="info">The click info.</param>
        /// <returns><code>false</code> if events should be
        /// dispatched to further double click listeners;
        /// <code>true</code> otherwise.</returns>
        bool HandleDoubleLeftClick(ClickInfo info);
    }
}

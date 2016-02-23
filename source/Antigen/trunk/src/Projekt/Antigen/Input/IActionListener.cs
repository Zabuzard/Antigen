namespace Antigen.Input
{
    /// <summary>
    /// Interface for action event listeners.
    /// </summary>
    interface IActionListener : IEventListener
    {
        /// <summary>
        /// Processes an action performed by the user.
        /// Actions are abstract input events triggered
        /// by user input.
        /// </summary>
        /// <param name="action"></param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispatched
        /// to further action event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleActionPerformed(UserAction action);
    }
}

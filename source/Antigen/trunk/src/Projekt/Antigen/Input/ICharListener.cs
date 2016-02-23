namespace Antigen.Input
{
    /// <summary>
    /// Interface for character listeners.
    /// </summary>
    interface ICharListener : IEventListener
    {
        /// <summary>
        /// This method is called whenever the user
        /// has entered a character.
        /// </summary>
        /// <param name="character">The character entered
        /// by the user.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further character event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleCharEntered(char character);
    }
}
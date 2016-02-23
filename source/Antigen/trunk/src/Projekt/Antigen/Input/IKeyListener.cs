using Microsoft.Xna.Framework.Input;

namespace Antigen.Input
{
    /// <summary>
    /// Interface for key event listeners.
    /// </summary>
    interface IKeyListener : IEventListener
    {
        /// <summary>
        /// Handles a key press.
        /// </summary>
        /// <param name="key">The key pressed.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further key event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleKeyPress(Keys key);
        /// <summary>
        /// Handles a key release.
        /// </summary>
        /// <param name="key">The key released.</param>
        /// <returns><code>true</code> if the event that lead
        /// to this method being called should not be dispached
        /// to further key event listeners.
        /// <code>false</code> otherwise.</returns>
        bool HandleKeyRelease(Keys key);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hausaufgabe
{
    /// <summary>
    /// Utility class for more convenient interaction with a mouse.
    /// </summary>
    /// 
    /// To use an object of this class, update it with the current
    /// mouse state in every iteration of the game loop.
    /// 
    /// At the moment, this class only supplies informations about
    /// the mouse cursor position and left clicks. A left click is
    /// registered when the left mouse button is released after
    /// having been pressed in the previous mouse state.
    internal sealed class MouseWrapper
    {
        private bool mLeftClickInProgress;

        /// <summary>
        /// Indicates whether a left click has been registered.
        /// </summary>
        public bool LeftClicked { get; private set; }
        /// <summary>
        /// The current cursor position.
        /// </summary>
        public Point CursorPosition { get; private set; }

        /// <summary>
        /// Updates the wrapper with a new mouse state.
        /// </summary>
        /// Changes all properties according to the difference
        /// between the current mouse state and the mouse state
        /// supplied when Update was last called.
        /// <param name="currentState">The current mouse state.</param>
        public void Update(MouseState currentState)
        {
            CursorPosition = new Point(currentState.X, currentState.Y);
            LeftClicked    = false;

            if (currentState.LeftButton == ButtonState.Pressed)
            {
                mLeftClickInProgress = true;
            }
            else if (mLeftClickInProgress && currentState.LeftButton == ButtonState.Released)
            {
                LeftClicked          = true;
                mLeftClickInProgress = false;
            }
        }
    }
}

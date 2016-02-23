using Microsoft.Xna.Framework;

namespace Antigen.Logic
{
    interface IUpdateable
    {
        /// <summary>
        /// Updates the object
        /// </summary>
        /// <param name="gameTime">The game time</param>
        void Update(GameTime gameTime);
    }
}

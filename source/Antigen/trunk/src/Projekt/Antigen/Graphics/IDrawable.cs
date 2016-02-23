using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Graphics
{
    /// <summary>
    /// An object that could be drawn
    /// </summary>
    interface IDrawable
    {
        /// <summary>
        /// Draws the object
        /// </summary>
        /// <param name="gameTime">The game time</param>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="spriteFont">The sprite font</param>
        void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont);
    }
}

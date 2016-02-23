using System;
using System.Collections.Generic;
using Antigen.Content;
using Antigen.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    [Serializable]
    sealed class CellInfoPanel: HudItem
    {
        private readonly List<Bar> mBars;
        [NonSerialized]
        private Texture2D mPanelTexture;
        private const int BarLeftOffset = 110;
        private const int BarGap = 20;

        /// <summary>
        /// Creates a cell info panel
        /// </summary>
        /// <param name="input">The input dispatcher</param>
        /// <param name="window">The window</param>
        /// <param name="position">The position</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="bars">The bars that whould be displayed on the panel</param>
        public CellInfoPanel(InputDispatcher input, GameWindow window, Point position, int width, int height, List<Bar> bars) : base(input, window, position, width, height)
        {
            mBars = bars;
            Visible = false;
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentLoader contentLoader)
        {
            mPanelTexture = contentLoader.LoadTexture("RoundedRectangle");
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (!Visible) return;
            spriteBatch.Draw(mPanelTexture, new Rectangle(mWindowPosition.X, mWindowPosition.Y, mWidth, mHeight), Color.Gray * 0.7f);
            var y = BarGap;
            foreach (var bar in mBars)
            {
                bar.Draw(spriteBatch, spriteFont, new Vector2(mWindowPosition.X + BarLeftOffset, mWindowPosition.Y + y), true);
                y += BarGap;
            }
        }
    }
}

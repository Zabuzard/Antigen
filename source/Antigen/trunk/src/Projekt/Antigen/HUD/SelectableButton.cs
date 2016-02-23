using System;
using Antigen.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    /// <summary>
    /// FullSelectable if the button is selectable for every selected unit.
    /// PartSelectable if the button is selectable for a few selected units.
    /// NotSelectable if the button is not selectable for any selected unit.
    /// </summary>
    enum SelectedStyle
    {
        FullSelectable,
        PartSelectable,
        NotSelectable
    }

    /// <summary>
    /// A selectable button used for modes
    /// </summary>
    [Serializable]
    sealed class SelectableButton: Button
    {
        private bool mSelected;

        public SelectedStyle Selectable
        {
            set
            {
                if (value == SelectedStyle.FullSelectable) Color = Color.White;
                if (value == SelectedStyle.NotSelectable) Color = Color.Black;
                if (value == SelectedStyle.PartSelectable) Color = Color.Gray;
            }
        }

        private const int BorderWidth = 2;


        public bool Selected
        {
            set { mSelected = value; }
        } 

        /// <summary>
        /// Creates a new selectable button
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="window">The window</param>
        /// <param name="position">The position</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="selectable">How the button is selectable</param>
        public SelectableButton(InputDispatcher input, GameWindow window, Point position, int width, int height, SelectedStyle selectable) : base(input, window, position, width, height)
        {
            Selectable = selectable;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (mSelected)
                spriteBatch.Draw(ButtonTexture,
                    new Rectangle(mWindowPosition.X - BorderWidth,
                        mWindowPosition.Y - BorderWidth,
                        mWidth + BorderWidth * 2,
                        mHeight + BorderWidth * 2),
                    Color.Yellow);
            base.Draw(gameTime, spriteBatch, spriteFont);
        }
    }
}
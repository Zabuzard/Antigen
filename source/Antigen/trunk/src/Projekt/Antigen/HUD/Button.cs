using System;
using Antigen.Content;
using Antigen.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    /// <summary>
    /// A button that raises a click event
    /// </summary>
    [Serializable]
    class Button: HudItem
    {
        [NonSerialized]
        private Texture2D mButtonTexture;

        public Texture2D ButtonTexture
        {
            protected get { return mButtonTexture; }
            set { mButtonTexture = value; }
        }


        /// <summary>
        /// Creates a new button
        /// </summary>
        /// <param name="input">The input dispatcher</param>
        /// <param name="window">The window</param>
        /// <param name="position">The position</param>
        /// <param name="width">The width</param>ButtonTe
        /// <param name="height">The height</param>
        public Button(InputDispatcher input, GameWindow window, Point position, int width, int height) : base(input, window, position, width, height)
        {
            Caption = "";
            Color = Color.White;
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (!Visible) return;
            spriteBatch.Draw(ButtonTexture, new Rectangle(mWindowPosition.X, mWindowPosition.Y, mWidth, mHeight), Color);
            var fontSize = spriteFont.MeasureString(Caption);
            spriteBatch.DrawString(spriteFont,
                Caption,
                new Vector2(mWindowPosition.X + mWidth / 2, mWindowPosition.Y + mHeight / 2),
                Color.Black,
                0,
                new Vector2(fontSize.X / 2, fontSize.Y / 2),
                Math.Min(mWidth/fontSize.X, mHeight/fontSize.Y),
                SpriteEffects.None,
                0);
        }

        public Color Color { private get; set; }

        public string Caption { private get; set; }

        /// <inheritdoc />
        public override void LoadContent(ContentLoader contentLoader)
        {
            ButtonTexture = contentLoader.LoadTexture("RoundedRectangle");
        }
    }
}

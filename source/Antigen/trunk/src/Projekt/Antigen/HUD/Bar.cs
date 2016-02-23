using System;
using System.Globalization;
using Antigen.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    /// <summary>
    /// A bar to display properties
    /// </summary>
    [Serializable]
    sealed class Bar: ILoad
    {
        private int mWidth;
        private readonly int mHeight;
        private readonly Color mColor;

        [NonSerialized]
        private Texture2D mTexture;

        public double Value { get; set; }
        public double MaxValue { get; set; }
        public bool Visible { private get; set; }
        private string Caption { get; set; }

        public void SetWidth(int width)
        {
            mWidth = width;
        }

        /// <summary>
        /// Creates a new bar
        /// </summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <param name="color">The color</param>
        /// <param name="maxValue">The maximal value</param>
        /// <param name="caption">The caption</param>
        public Bar(int width, int height, Color color, int maxValue, string caption)
        {
            mWidth = width;
            mHeight = height;
            mColor = color;
            Caption = caption;
            MaxValue = maxValue;
            Visible = true;
        }

        /// <summary>
        /// Draws the bar
        /// </summary>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="spriteFont">The sprite font</param>
        /// <param name="position">The position</param>
        /// <param name="drawNumber">If the number should be drawn</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont, Vector2 position, bool drawNumber)
        {
            if (Visible)
            {
                spriteBatch.Draw(mTexture,
                    position,
                    null,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    new Vector2((float) mWidth / mTexture.Width, (float) mHeight / mTexture.Height),
                    SpriteEffects.None,
                    0);
                spriteBatch.Draw(mTexture,
                    position,
                    null,
                    mColor,
                    0f,
                    Vector2.Zero,
                    new Vector2((float) mWidth / mTexture.Width * (float) Math.Min((Value / MaxValue), 1),
                        (float) mHeight / mTexture.Height),
                    SpriteEffects.None,
                    0);
                if (Caption != "")
                {
                    var fontSize = spriteFont.MeasureString(Caption);
                    spriteBatch.DrawString(spriteFont,
                        Caption,
                        new Vector2(position.X, position.Y),
                        Color.White,
                        0,
                        new Vector2(fontSize.X, 0),
                        mHeight / fontSize.Y,
                        SpriteEffects.None,
                        0);
                }

                if (drawNumber)
                {
                    var numberSize = spriteFont.MeasureString(Value.ToString(CultureInfo.InvariantCulture));
                    spriteBatch.DrawString(spriteFont,
                        Value.ToString(CultureInfo.InvariantCulture),
                        new Vector2(position.X + mWidth / 2f, position.Y + mHeight / 2f),
                        Color.White,
                        0,
                        new Vector2(numberSize.X / 2, numberSize.Y / 2),
                        mHeight / numberSize.Y,
                        SpriteEffects.None,
                        0);
                }
            }
        }



        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            mTexture = contentLoader.LoadTexture("RoundedRectangle");
        }
    }
}

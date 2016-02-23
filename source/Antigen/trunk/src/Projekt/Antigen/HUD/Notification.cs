using System;
using Antigen.Achievements;
using Antigen.Content;
using Antigen.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    /// <summary>
    /// Show a notification during the game whenever a new Achievement is unlocked.
    /// </summary>
    sealed class Notification
    {
        private readonly GameWindow mWindow;
        private TimeSpan mTimeSpan;
        private readonly Texture2D mRectangle;
        private readonly string[] mWords;


        /// <summary>
        /// Create a new notification to show the player that a new achievement has been unlocked.
        /// </summary>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="window">The game window.</param>
        /// <param name="achievement">The achievement that has been unlocked.</param>
        public Notification(ContentLoader contentLoader, GameWindow window, AchievementNames achievement)
        {
            mWindow = window;
            mTimeSpan = new TimeSpan();
            mRectangle = contentLoader.LoadTexture("RoundedRectangle");
            mWords = achievement.ToString().Split('_');
        }

        /// <summary>
        /// Draw notification and update counter.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The spritebatch of the game.</param>
        /// <param name="spriteFont">The font of the game.</param>
        /// <param name="height">The height where to draw the left upper corner of the notification.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont, int height)
        {
            // Get window client bounds.
            var width = mWindow.ClientBounds.Width;
            var curHeight = mWindow.ClientBounds.Height;
            // Determine the multiplicator for the font.
            var size = 1f;
            var fontWidth = 0f;
            var fontHeight = 0f;
            foreach (var word in mWords)
            {
                if (spriteFont.MeasureString(word).X > fontWidth)
                {
                    fontWidth = spriteFont.MeasureString(word).X;
                }
                fontHeight += spriteFont.LineSpacing + 5;
            }
            while (size * fontWidth > width / 10f || size * fontHeight > curHeight / 10f)
            {
                size /= 1.5f;
            }
            spriteBatch.Begin();
            spriteBatch.Draw(mRectangle, new Rectangle(width / 100, height, width / 10, curHeight / 10), Color.Black * 0.7f);
            for (int index = 0; index < mWords.Length; index++)
            {
                var yPosition = height + index * (spriteFont.LineSpacing + 5) * size;
                var word = mWords[index];
                spriteBatch.DrawString(spriteFont,
                    word,
                    new Vector2(width / 100f, yPosition),
                    Color.White, 0.0f,
                    new Vector2(0, 0),
                    size,
                    new SpriteEffects(),
                    0.0f);
            }
            spriteBatch.End();
            // Destroy notification after 30 seconds.
            mTimeSpan += gameTime.ElapsedGameTime;
            if (mTimeSpan.TotalSeconds >= 30)
            {
                GameScreen.mNotifications.Remove(this);
            }
        }
    }
}

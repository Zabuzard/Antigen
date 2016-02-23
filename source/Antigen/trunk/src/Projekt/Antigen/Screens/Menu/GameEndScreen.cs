using Antigen.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// Screen displayed when the player wins or loses a game.
    /// Displays either a win or loss message.
    /// </summary>
    internal sealed class GameEndScreen : IScreen
    {
        /// <summary>
        /// Background texture.
        /// </summary>
        private Texture2D mMenuBackGround;
        /// <summary>
        /// The game window.
        /// </summary>
        private readonly GameWindow mWindow;
        /// <summary>
        /// Screen stack.
        /// </summary>
        private readonly ScreenManager mScreens;
        /// <summary>
        /// Text displayed to the user.
        /// </summary>
        private readonly string[] mText;
        /// <summary>
        /// Input manager.
        /// </summary>
        private readonly IInputService mInput;
        /// <summary>
        /// The reason for the end of the game.
        /// </summary>
        private readonly GameEndReason mGameEndReason;

        /// <summary>
        /// Create a new game end screen.
        /// </summary>
        /// <param name="screenManager">The game's screen manager.</param>
        /// <param name="window">The game window. Needed for client bounds to draw in.</param>
        /// <param name="input">The game's input manager.</param>
        /// <param name="contentLoader">The game's content loader.</param>
        /// <param name="gameEndReason">Reason why the game has ended.</param>
        public GameEndScreen(ScreenManager screenManager, GameWindow window, IInputService input,
            ContentLoader contentLoader, GameEndReason gameEndReason)
        {
            mGameEndReason = gameEndReason;
            ScreenEnabled = true;
            mWindow = window;
            mScreens = screenManager;

            LoadContent(contentLoader);
            if (gameEndReason == GameEndReason.Error)
            {
                mText = new string[2];
                mText[0] = "An error occurred";
                mText[1] = "Press any key to return";
            }
            else
            {
                mText = new string[3];
                mText[0] = gameEndReason == GameEndReason.Win ? "You win" : "You lose";
                mText[1] = gameEndReason == GameEndReason.Win
                    ? "Congratulations!"
                    : gameEndReason == GameEndReason.LossNoUnitsLeft
                        ? "Your cells all died."
                        : "Too little red blood cells left.";
                mText[2] = "Press any key";
            }
            mInput = input;
            mInput.GetKeyboard().KeyReleased += OnKeyReleased;
            mInput.GetMouse().MouseButtonReleased += OnMouseReleased;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc/>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            // Get window client bounds.
            var width = mWindow.ClientBounds.Width;
            var curHeight = mWindow.ClientBounds.Height;
            // Determine the multiplicator for the font.
            var size = 3f;
            var fontWidth = 0f;
            var fontHeight = 0f;
            foreach (var line in mText)
            {
                if (spriteFont.MeasureString(line).X > fontWidth)
                {
                    fontWidth = spriteFont.MeasureString(line).X;
                }
                fontHeight += 2 * spriteFont.LineSpacing;
            }
            while (size * fontWidth > width || size * fontHeight > curHeight)
            {
                size /= 1.5f;
            }
                
            spriteBatch.Begin();
            spriteBatch.Draw(mMenuBackGround,
                new Rectangle(0, 0, mWindow.ClientBounds.Width, mWindow.ClientBounds.Height),
                Color.White * 0.7f);

            for (var i = 0; i < mText.Length; i++)
            {
                spriteBatch.DrawString(spriteFont,
                    mText[i],
                    new Vector2(mWindow.ClientBounds.Width / 2f - size * spriteFont.MeasureString(mText[i]).X / 2,
                        mWindow.ClientBounds.Height / 2f - size * fontHeight / 2f
                        + size * spriteFont.LineSpacing * 2 * i),
                    Color.Black,
                    0.0f,
                    new Vector2(0, 0),
                    size,
                    new SpriteEffects(),
                    0.0f);
            }
            spriteBatch.End();
        }

        /// <inheritdoc/>
        public void LoadContent(ContentLoader contentLoader)
        {
            mMenuBackGround = contentLoader.LoadTexture("loadbackground");
        }

        /// <inheritdoc/>
        public bool ScreenEnabled { get; private set; }

        /// <inheritdoc/>
        public bool UpdateLower()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool DrawLower()
        {
            return false;
        }

        /// <summary>
        /// Destroy this screen, have a look at the stats of the last game.
        /// </summary>
        /// <param name="key">Parameter not used.</param>
        private void OnKeyReleased(Keys key)
        {
            RemoveThisScreen();
        }

        /// <summary>
        /// Destroy this screen, have a look at the stats of the last game.
        /// </summary>
        /// <param name="buttons">Parameter not used</param>
        private void OnMouseReleased(MouseButtons buttons)
        {
            RemoveThisScreen();   
        }

        /// <summary>
        /// Destroy this screen, have a look at the stats of the last game.
        /// </summary>
        private void RemoveThisScreen()
        {
            mInput.GetKeyboard().KeyReleased -= OnKeyReleased;
            mInput.GetMouse().MouseButtonReleased -= OnMouseReleased;
            ScreenEnabled = false;
            mScreens.Pop();
            if (mGameEndReason == GameEndReason.Error)
            {
                ((MenuScreen)mScreens.Peek()).EnableEvents();
                return;
            }
            mScreens.PushMenuScreen(typeof(StatScreen));
        }
    }

    /// <summary>
    /// Indicates why a game has ended.
    /// </summary>
    enum GameEndReason
    {
        Error,
        Win,
        LossNoUnitsLeft,
        LossLittleRedBloodCellsLeft
    }
}

using System;
using Antigen.Content;
using Antigen.GameManagement;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// This screen takes the responsibility to load a game.
    /// </summary>
    internal sealed class LoadScreen : IScreen
    {
        private Texture2D mMenuBackGround;
        private readonly GameWindow mWindow;
        private bool mInitiated;
        private readonly ScreenManager mScreens;
        private readonly ContentLoader mContentLoader;
        private readonly InputManager mInput;
        private readonly SaveGameManager mSaveGameManager;
        private readonly String mText;
        private readonly string mSaveName;
        private readonly Mode mMode;
        private readonly SpriteBatch mSpriteBatch;
        private readonly Difficulty mDifficulty;
        private readonly SettingsManager mSettings;

        /// <summary>
        /// Create a new LoadScreen. Doesn't listen to any events. Only shows a message.
        /// </summary>
        /// <param name="screenManager"></param>
        /// <param name="window">The game window. Needed for client bounds to draw in.</param>
        /// <param name="contentLoader">The game's contentloader for loading the background texture.</param>
        /// <param name="input"></param>
        /// <param name="saveGameManager">The game's savegamemanager for loading and saving the game.</param>
        /// <param name="mode">Operate depending on the mode.</param>
        /// <param name="saveName">Save or load game in a file with the given name.</param>
        /// <param name="spriteBatch">The spritebatch of the game.</param>
        /// <param name="difficulty">The difficulty of the game to be created.</param>
        /// <param name="settings">The settings manager of the game.</param>
        public LoadScreen(ScreenManager screenManager, GameWindow window, ContentLoader contentLoader, 
            InputManager input, SaveGameManager saveGameManager, Mode mode, string saveName, SpriteBatch spriteBatch, 
            Difficulty difficulty, SettingsManager settings)
        {
            ScreenEnabled = true;
            mScreens = screenManager;
            mWindow = window;
            mContentLoader = contentLoader;
            mInput = input;
            mSaveGameManager = saveGameManager;
            mSaveName = saveName;
            mSpriteBatch = spriteBatch;
            mDifficulty = difficulty;

            mMode = mode;
            mInitiated = false;
            LoadContent(contentLoader);
            mText = mode == Mode.Save ? "Saving..." : "Loading...";
            mSettings = settings;
        }

        /// <summary>
        /// If the screen has been drawed process the given operation depending on the mode.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (!mInitiated)
                return;

            GameScreen gameScreen;
            switch (mMode)
            {
                case Mode.New:
                    gameScreen = mScreens.InitializeGame.NewGame(mDifficulty);
                    mScreens.Pop();
                    mScreens.Push(gameScreen);
                    mScreens.Peek().LoadContent(mContentLoader);
                    break;
                case Mode.Save:
                    try
                    {
                        mSaveGameManager.Save(mSaveName);
                        mScreens.Pop();
                        mScreens.Pop();
                        ((MenuScreen) mScreens.Peek()).EnableEvents();
                    }
                    catch (Exception)
                    {
                        mScreens.Pop();
                        mScreens.PushGameEndScreen(GameEndReason.Error);
                    }
                    break;
                case Mode.Load:
                    try
                    {
                        var gameState = SaveGameManager.Load(mSaveName);
                        gameScreen = gameState.GameScreen;
                        if (gameScreen != null)
                        {
                            gameScreen.LoadGameState(mSpriteBatch,
                                mContentLoader,
                                mScreens,
                                mWindow,
                                mInput,
                                gameState.Stats,
                                mSettings);
                        }
                        mScreens.Pop();
                        mScreens.Push(gameScreen);
                        mScreens.Peek().LoadContent(mContentLoader);
                    }
                    catch (Exception)
                    {
                        mScreens.Pop();
                        mScreens.PushGameEndScreen(GameEndReason.Error);
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(mMenuBackGround,
                new Rectangle(0, 0, mWindow.ClientBounds.Width, mWindow.ClientBounds.Height),
                Color.White * 0.7f);
            spriteBatch.DrawString(spriteFont,
                mText,
                new Vector2(mWindow.ClientBounds.Width / 2f - spriteFont.MeasureString(mText).X / 2 /*size*/,
                    mWindow.ClientBounds.Height / 2f - spriteFont.MeasureString(mText).Y / 2  /*size*/),
                Color.Black);
            spriteBatch.End();
            mInitiated = true;

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
        /// Set the mode of the LoadScreen. 
        /// </summary>
        public enum Mode
        {
            New, Save, Load
        }
    }
}

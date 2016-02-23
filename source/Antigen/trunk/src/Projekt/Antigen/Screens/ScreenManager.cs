using System;
using System.Collections.Generic;
using Antigen.Content;
using Antigen.GameManagement;
using Antigen.Objects;
using Antigen.Screens.Menu;
using Antigen.Settings;
using Antigen.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;
using IDrawable = Antigen.Graphics.IDrawable;
using IUpdateable = Antigen.Logic.IUpdateable;


namespace Antigen.Screens
{
    /// <summary>
    /// Stack of screens that can be drawn and updated from top
    /// to bottom. Each screen can specify that screens located
    /// below it in the stack should not be drawn and/or updated.
    /// </summary>
    sealed class ScreenManager : IUpdateable, IDrawable, ILoad
    {
        /// <summary>
        /// Stack of screens currently used by the game.
        /// </summary>
        private readonly Stack<IScreen> mStack;

        /// <summary>
        /// The game's settings manager.
        /// </summary>
        private readonly SettingsManager mSettings;
        private readonly ContentLoader mContentLoader;
        private readonly InputManager mInput;
        private readonly SpriteBatch mSpriteBatch;
        public GameWindow Window { get; private set; }
        public IEndGame EndGame { get; private set; }
        public SaveGameManager SaveGame { private get; set; }
        public SoundWrapper SoundWrapper { get; set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Object that allows to initialize a new game.
        /// </summary>
        public InitializeGame InitializeGame { get; private set; }

        /// <summary>
        /// Holds the last <code>GameScreen</code> pushed onto the
        /// stack. 
        /// </summary>
        public GameScreen CurrentGameScreen { get; private set; }

        /// <summary>
        /// Creates an empty screen stack.
        /// </summary>
        public ScreenManager(IEndGame endGame,
            ContentLoader contentLoader,
            InputManager input,
            GameWindow window,
            SettingsManager settings,
            SpriteBatch spriteBatch,
            GraphicsDevice graphics,
            GraphicsDeviceManager graphicsDeviceManager)
        {
            Graphics = graphicsDeviceManager;
            GraphicsDevice = graphics;
            mSettings = settings;
            mStack = new Stack<IScreen>();
            mContentLoader = contentLoader;
            mSpriteBatch = spriteBatch;
            EndGame = endGame;
            mInput = input;
            Window = window;
            InitializeGame = new InitializeGame(this, mContentLoader, Window, input, settings, mSpriteBatch);
        }

        

        /// <summary>
        /// Pushes a screen onto the stack. The screen will be
        /// updated and drawn before any other screen currently on
        /// the stack.
        /// </summary>
        /// <param name="screen">A screen to be added to the stack.</param>
        public void Push(IScreen screen)
        {
            mStack.Push(screen);

            var gameScreen = screen as GameScreen;
            if (gameScreen != null)
                CurrentGameScreen = gameScreen;
        }

        /// <summary>
        /// Removes the first screen from the stack. This is the screen
        /// that was added by the last call to <code>Push</code>.
        /// </summary>
        /// <returns>The removed screen.</returns>
        public void Pop()
        {
            var screen = mStack.Pop();

            var destroy = screen as IDestroy;
            if (destroy != null)
                destroy.Destroy();

            var menu = screen as MenuScreen;
            if (menu != null)
                menu.DisableEvents();

            if (screen == CurrentGameScreen)
                CurrentGameScreen = null;
                    //mStack.OfType<GameScreen>().FirstOrDefault<GameScreen>();
        }

        /// <summary>
        /// Returns the first screen from the stack without removing
        /// it.
        /// </summary>
        /// <returns>The topmost screen.</returns>
        public IScreen Peek()
        {
            return mStack.Peek();
        }

        /// <summary>
        /// Draws all screens on the stack, starting from the
        /// bottom and proceeding to the top. If any screen's
        /// <code>DrawLower</code> method returns <code>false</code>,
        /// screens below that screen will not be drawn.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="spriteBatch">Sprite Batch to draw with</param>
        /// <param name="spriteFont">The sprite font to write with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            var screens = new List<IScreen>(mStack.Count);

            foreach (var screen in mStack)
            {
                if (screen.ScreenEnabled)
                    screens.Add(screen);
                if (!screen.DrawLower())
                    break;
            }

            screens.Reverse();

            foreach (var screen in screens)
                screen.Draw(gameTime, spriteBatch, spriteFont);
        }

        /// <summary>
        /// Updates all screens on the stack, starting at the
        /// top and proceeding to the bottom. If any screen's
        /// <code>UpdateLower</code> method returns <code>false</code>,
        /// screens below that screen will not be drawn.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        public void Update(GameTime gameTime)
        {
            foreach (var screen in mStack)
            {
                if (screen.ScreenEnabled)
                    screen.Update(gameTime);
                if (!screen.UpdateLower())
                    break;
            }
        }

        /// <summary>
        /// Loads all the screens' content.
        /// <param name="contentLoader">The game's central content loader.</param>
        /// </summary>
        public void LoadContent(ContentLoader contentLoader)
        {
            foreach (var screen in mStack)
                screen.LoadContent(contentLoader);
        }

        /// <summary>
        /// Push a new LoadScreen that will initalize a new game.
        /// </summary>
        /// <param name="mode">The mode determines whether the LoadScreen initialize a new game, load an old game or save the game.</param>
        /// <param name="saveName">Name of the file to be loaded or saved.</param>
        /// <param name="difficulty">The difficulty of a new game screen if mode is Mode.new.</param>
        public void PushLoadScreen(LoadScreen.Mode mode, string saveName, Difficulty difficulty=Difficulty.Medium)
        {
            Push(new LoadScreen(this,
                Window,
                mContentLoader,
                mInput,
                SaveGame,
                mode,
                saveName,
                mSpriteBatch,
                difficulty,
                mSettings));
        }

        /// <summary>
        /// Pushes a new menu screen of the given type on the screen stack.
        /// </summary>
        /// <param name="type">The type of the menu screen to be created.</param>
        public void PushMenuScreen(Type type)
        {
            IScreen screen = null;

            if (type == typeof(AchievementScreen))
                screen = new AchievementScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(AudioScreen))
                screen = new AudioScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(BindingsScreen))
                screen = new BindingsScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(LoadGameScreen))
                screen = new LoadGameScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(MainMenuScreen))
                screen = new MainMenuScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(NewGameScreen))
                screen = new NewGameScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(OptionsScreen))
                screen = new OptionsScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(PauseScreen))
                screen = new PauseScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(ResolutionScreen))
                screen = new ResolutionScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(SaveGameScreen))
                screen = new SaveGameScreen(mInput, Window, mContentLoader, this, mSettings);
            else if (type == typeof(StatScreen))
                screen = new StatScreen(mInput, Window, mContentLoader, this, mSettings);
            
            if (screen != null)
                Push(screen);
        }

        /// <summary>
        /// Pushes a game end screen onto the screen stack.
        /// </summary>
        /// <param name="reason">Reason why the game has ended.</param>
        public void PushGameEndScreen(GameEndReason reason)
        {
            Push(new GameEndScreen(this, Window, mInput, mContentLoader, reason));
        }

        /// <summary>
        /// Return a game state object containing the current game screen.
        /// </summary>
        /// <returns></returns>
        public GameState GetGameState()
        {
            return new GameState(CurrentGameScreen);
        }

        public void OnDeactivate(object sender, EventArgs e)
        {
            if (CurrentGameScreen != null) CurrentGameScreen.OnDeactivate();
        }
    }
}

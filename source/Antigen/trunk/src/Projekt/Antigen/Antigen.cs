using System;
using System.IO;
using Antigen.Content;
using Antigen.GameManagement;
using Antigen.Screens;
using Antigen.Screens.Menu;
using Antigen.Settings;
using Antigen.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using InputManager = Nuclex.Input.InputManager;

namespace Antigen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    internal sealed class Antigen : Game, IEndGame
    {
        /// <summary>
        /// Number of game updates per seconds.
        /// </summary>
        private const int Tps = 30;

        /// <summary>
        /// The game's central input manager.
        /// </summary>
        private InputManager mInput;

        /// <summary>
        /// The game's central sprite batch.
        /// </summary>
        private SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        /// The game's central screen manager.
        /// </summary>
        private ScreenManager mScreens;

        /// <summary>
        /// Font for drawing FPS Counter and menu.
        /// </summary>
        private SpriteFont mSpriteFont;

        /// <summary>
        /// The game's central settings manager.
        /// </summary>
        private SettingsManager mSettingsManager;

        private readonly GraphicsDeviceManager mGraphics;
        private SaveGameManager mSaveGameManager;
        private ContentLoader mContentLoader;
        private SoundEngine mSoundEngine;
        private SoundWrapper mSoundWrapper;

        /// <summary>
        /// Creates a new Antigen game instance.
        /// </summary>
        public Antigen()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1000 / Tps);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            mSpriteFont = Content.Load<SpriteFont>("res\\fonts\\MenuFont");

            mSettingsManager = new SettingsManager();
            mSettingsManager.OnSettingsUpdated += ApplySettingsChange;

            try
            {
                mSettingsManager.Load();
            }
            catch (IOException)
            {
                Console.WriteLine("An Exception of type IOException occured. The game now exits.");
                ExitGame();
                return;
            }

            mInput = new InputManager(Services, Window.Handle);
            mSoundEngine = new SoundEngine(Content, mSettingsManager);
            mContentLoader = new ContentLoader(Content, mSoundEngine);
            mScreens = new ScreenManager(this,
                mContentLoader,
                mInput,
                Window,
                mSettingsManager,
                SpriteBatch,
                GraphicsDevice,
                mGraphics);
            Deactivated += mScreens.OnDeactivate;
            mSaveGameManager = new SaveGameManager(mScreens);
            mScreens.SaveGame = mSaveGameManager;
            mSoundWrapper = new SoundWrapper(mSoundEngine, mScreens);
            mScreens.SoundWrapper = mSoundWrapper;
            Components.Add(mInput);

            mScreens.PushMenuScreen(typeof(MainMenuScreen));
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            mScreens.LoadContent(mContentLoader);
            mSoundWrapper.LoadContent(mContentLoader);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mScreens.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            mScreens.Draw(gameTime, SpriteBatch, mSpriteFont);
            base.Draw(gameTime);
        }

        /// <inheritdoc />
        public void ExitGame()
        {
            try
            {
                mSettingsManager.Save();
            }
            catch (IOException)
            {
                Console.WriteLine("An Exception of type IOException occured. The game now exits.");
            }
            Exit();
        }

        /// <summary>
        /// Applies resolution and fullscreen settings changes, updating
        /// the <code>GraphicsDevice</code> used by the game to use
        /// the new values.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Settings update event argutments.</param>
        private void ApplySettingsChange(object sender, SettingsUpdateEventArgs args)
        {
            //Resolution updates
            var res = args.NewSettings.Resolution;
            if (!args.OldSettings.Resolution.Equals(res))
            {
                mGraphics.PreferredBackBufferWidth = res.Horizontal;
                mGraphics.PreferredBackBufferHeight = res.Vertical;
                mGraphics.ApplyChanges();
            }

            //Fullscreen updates
            var fullscreen = args.NewSettings.Fullscreen;
            if (!args.OldSettings.Fullscreen == fullscreen)
            {
                mGraphics.ToggleFullScreen();
            }
            mGraphics.ApplyChanges();
        }
    }
}

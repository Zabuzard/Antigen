using Antigen.Content;
using Antigen.Input;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Screens;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Antigen.GameManagement
{
    /// <summary>
    /// This class provides the possibility to create a new game.
    /// </summary>
    sealed class InitializeGame
    {
        private readonly SettingsManager mSettings;
        private readonly ContentLoader mContentLoader;
        private readonly GameWindow mWindow;
        private readonly InputManager mInput;
        private readonly ScreenManager mScreens;
        private readonly SpriteBatch mSpriteBatch;
        private const int RedBloodCellThreshold = 1;

        /// <summary>
        /// Provide all content needed to initialize a new game screen.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="window">The game window.</param>
        /// <param name="screens">The game's central screen manager.</param>
        /// <param name="settings">The game's central settings manager.</param>
        /// <param name="spriteBatch"></param>
        public InitializeGame(ScreenManager screens, ContentLoader contentLoader, GameWindow window,
            InputManager input, SettingsManager settings, SpriteBatch spriteBatch)
        {
            mContentLoader = contentLoader;
            mWindow = window;
            mScreens = screens;
            mInput = input;
            mSettings = settings;
            mSpriteBatch = spriteBatch;
        }

        /// <summary>
        /// Initialize a new gamescreen.
        /// <param name="difficulty">The difficulty of the game to be created.</param>
        /// </summary>
        public GameScreen NewGame(Difficulty difficulty)
        {
            var keymap = mSettings.CurrentSettings.Keymap;
            var inputDispatcher = new InputDispatcher(mInput, keymap);
            var selectionManager = new SelectionManager();
            var objectCaches = new ObjectCaches(inputDispatcher, selectionManager);
            var gameScreen = new GameScreen(mSpriteBatch, mContentLoader, inputDispatcher,
                selectionManager, mWindow, mScreens, objectCaches, RedBloodCellThreshold, difficulty, mSettings);     
            inputDispatcher.CoordTranslation = gameScreen.GetCam();
            inputDispatcher.UnitLocator = objectCaches.SpatialCache;
            var rectSelector = new RectangularSelector(objectCaches.SpatialCache, selectionManager);
            var dclickSelector = new DoubleClickSelector(objectCaches.ListSelectable, selectionManager);
            objectCaches.Add(rectSelector);
            objectCaches.Add(dclickSelector);
            return gameScreen;
        }
    }

}
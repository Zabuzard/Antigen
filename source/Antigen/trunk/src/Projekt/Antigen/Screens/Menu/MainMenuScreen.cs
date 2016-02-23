using Antigen.Content;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// Shows the main menu.
    /// </summary>
    sealed class MainMenuScreen : MenuScreen
    {
        /// <summary>
        /// Initialize the needed menu items.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public MainMenuScreen(InputManager input, GameWindow window, ContentLoader contentLoader, 
            ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader,  screenManager, settings)
        {
            mMenuItems = new[] {"New Game", "Load Game", "Options", "Achievements", "Exit"};
        }

        /// <inheritdoc />
        protected override void MenuItemSelected(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(NewGameScreen));
                    break;
                case 1:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(LoadGameScreen));
                    break;
                case 2:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(OptionsScreen));
                    break;
                case 3:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(AchievementScreen));
                    break;
                case 4:
                    mScreens.EndGame.ExitGame();
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Down:
                    SelectedIndex++;
                    break;
                case Keys.Up:
                    SelectedIndex--;
                    break;
                case Keys.Enter:
                    MenuItemSelected(SelectedIndex);
                    break;
            }
        }
    }
}

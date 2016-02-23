using System;
using Antigen.Content;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// Show when the game is paused. Contains link back to main menu and back to game.
    /// Should allow loading the game.
    /// </summary>
    internal sealed class PauseScreen : MenuScreen
    {
        /// <summary>
        /// Initialize the needed menu items.
        /// </summary>
        /// <param name="input">The Input dispatcher of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public PauseScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mMenuItems = new[] {"Continue", "Save Game", "Load Game", "Options", "Stats", "Achievements", 
                "back to main menu", "Exit"};
            mHighlightColor = Color.Yellow;
        }

        /// <inheritdoc/>
        protected override void MenuItemSelected(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0:
                    DisableEvents();
                    mScreens.Pop();
                    break;
                case 1:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(SaveGameScreen));
                    break;
                case 2:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(LoadGameScreen));
                    break;
                case 3:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(OptionsScreen));
                    break;
                case 4:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(StatScreen));
                    break;
                case 5:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(AchievementScreen));
                    break;
                case 6:
                    // Remove Pause screen.
                    DisableEvents();
                    mScreens.Pop();
                    // Remove Game screen.
                    mScreens.Pop();
                    // Enable events for main menu screen.
                    if (mScreens.Peek() is MenuScreen)
                    {
                        ((MenuScreen)mScreens.Peek()).EnableEvents();
                    }
                    GC.Collect();
                    break;
                case 7:
                    mScreens.EndGame.ExitGame();
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void DrawBackground(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
        }
    }
}

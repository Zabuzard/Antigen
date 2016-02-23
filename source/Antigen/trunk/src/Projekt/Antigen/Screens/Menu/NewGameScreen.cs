using Antigen.Content;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
   /// <summary>
   /// Create a new game. Choose map and difficulty for the new game.
   /// </summary>
    internal sealed class NewGameScreen : MenuScreen
   {
       private readonly SettingsManager mSettings;
       private Difficulty mDifficulty;

       /// <summary>
       /// Allows to change the difficulty of the game.
       /// </summary>
       /// <param name="input">The input manager of the game.</param>
       /// <param name="window">The window of the game.</param>
       /// <param name="contentLoader">The content loader of the game.</param>
       /// <param name="screenManager">The screen manager of the game.</param>
       /// <param name="settings">The game's settings manager.</param>
       public NewGameScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mSettings = settings;
            mDifficulty = settings.CurrentSettings.Difficulty;
            mMenuItems = new [] {Difficulty.Easy.ToString(), Difficulty.Medium.ToString(), 
                Difficulty.Hard.ToString(), "back"};
           switch (mDifficulty)
           {
               case Difficulty.Easy:
                   mSelectedIndex = 0;
                   break;
               case Difficulty.Medium:
                   mSelectedIndex = 1;
                   break;
                case Difficulty.Hard:
                   mSelectedIndex = 2;
                   break;
           }
        }

        /// <inheritdoc />
        protected override void MenuItemSelected(int selectedIndex)
        {

            if (mMenuItems[selectedIndex].Equals("back"))
            {
                OnPressBackButton();

            }
            else
            {
                DisableEvents();
                mScreens.Pop();
                switch (selectedIndex)
                {
                    case 0:
                        {
                            mDifficulty = Difficulty.Easy;
                            break;
                        }
                    case 1:
                        {
                            mDifficulty = Difficulty.Medium;
                            break;
                        }
                    case 2:
                        {
                            mDifficulty = Difficulty.Hard;
                            break;
                        }
                }
                var settings = mSettings.CurrentSettings;
                settings.Difficulty = mDifficulty;
                mSettings.CurrentSettings = settings;
                mScreens.PushLoadScreen(LoadScreen.Mode.New, "", mDifficulty);
            }
        }
   }
}

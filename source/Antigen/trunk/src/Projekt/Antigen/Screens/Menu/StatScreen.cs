using Antigen.Content;
using Antigen.Settings;
using Antigen.Statistics;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// The stat screen displays some collected statistics on the game.
    /// </summary>
    sealed class StatScreen : MenuScreen
    {
        private readonly Color mEmptyColor;

        /// <summary>
        /// Initalize the screen and chose the entries of the screen.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public StatScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings) 
            : base(input, window, contentLoader, screenManager, settings)
        {
            mEmptyColor = Color.DarkSlateGray;
            var dict = GameScreen.mStats;
            mMenuItems = new string[dict.Count + 1];
            var i = 0;
            foreach (var statistic in dict)
            {
                if (statistic.Key == StatName.Playing_Time)
                {
                    mMenuItems[i] = statistic.Key.ToString().Replace('_', ' ') + " - " + statistic.Value / 60 + "min " +
                                    statistic.Value % 60 + "sec";
                }
                else
                {
                    mMenuItems[i] = statistic.Key.ToString().Replace('_', ' ') + " - " + statistic.Value;
                }
                i++;
            }
            mMenuItems[mMenuItems.Length - 1] = "back";
        }

        /// <inheritdoc/>
        protected override void MenuItemSelected(int selectedIndex)
        {
            if (selectedIndex != mMenuItems.Length - 1)
            {
                return;
            }
            OnPressBackButton();   
        }

        /// <inheritdoc/>
        protected override Color SelectColor(int i)
        {
            var tint = i == mMenuItems.Length - 1 ? i == mSelectedIndex ? mHighlightColor : mNormalColor : mEmptyColor;
            return tint;
        }
    }
}
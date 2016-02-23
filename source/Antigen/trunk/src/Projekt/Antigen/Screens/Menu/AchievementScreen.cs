using System.Collections.Generic;
using Antigen.Content;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// The achievements screen displays the possible achievements and whether they are already achieved or not.
    /// </summary>
    sealed class AchievementScreen : MenuScreen
    {
        private readonly List<int> mIndexList;
        private readonly Color mEmptyColor;

        /// <summary>
        /// Initialize the screen and chose the menu entries of the screen.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public AchievementScreen(InputManager input,
            GameWindow window,
            ContentLoader contentLoader,
            ScreenManager screenManager,
            SettingsManager settings) :
                base(input, window, contentLoader, screenManager, settings)
        {
            mEmptyColor = Color.DarkSlateGray;
            mIndexList = new List<int>();
            var dict = Achievements.Achievements.GetAchievements();
            mMenuItems = new string[dict.Count + 1];
            int i = 0;
            foreach (var achievement in dict)
            {
                if (achievement.Value)
                {
                    mIndexList.Add(i);
                }
                mMenuItems[i] = achievement.Key.ToString().Replace('_', ' ');
                i++;
            }
            mIndexList.Add(mMenuItems.Length - 1);
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
            return mIndexList.Contains(i) ? mMenuItems[i].Equals("back") ? i == SelectedIndex ? mHighlightColor : Color.Black : mNormalColor: mEmptyColor;
        }
    }
}

using System;
using System.Collections.Generic;
using Antigen.Content;
using Antigen.Input;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// The bindings screen allows to check on the shortcuts needed for the game.
    /// </summary>
    sealed class BindingsScreen : MenuScreen
    {
        /// <summary>
        /// The game's central settings manager.
        /// </summary>
        private readonly SettingsManager mSettings;
        private readonly IDictionary<char, UserAction> mMap;
        // Key events for change bindings may only take place on the last selected index.
        private int mCurrentSelected;
        // Provide the keys in the right order
        private readonly Tuple<UserAction, char>[]  mSortedArray;

        /// <summary>
        /// Screen should show bindings. Those will possibly be changeable.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public BindingsScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mInput.GetKeyboard().KeyReleased += ChangeBinding;
            mSettings = settings;
            mMap = settings.CurrentSettings.Keymap;
            // Sort the dictionary alphabetically.
            mSortedArray = new Tuple<UserAction, char>[mMap.Count];
            var j = 0;
            foreach (var keyValuePair in mMap)
            {
                mSortedArray[j] = new Tuple<UserAction, char>(keyValuePair.Value, keyValuePair.Key);
                j++;
            }
            Array.Sort(mSortedArray);
            mMenuItems = new string[mMap.Count + 1];
            for (var i = 0; i < mMap.Count; i++)
            {
                mMenuItems[i] = mSortedArray[i].Item1 + " - " + mSortedArray[i].Item2;
            }
            mMenuItems[mMenuItems.Length - 1] = "back";

            mCurrentSelected = -1;
        }

        /// <inheritdoc />
        protected override void MenuItemSelected(int selectedIndex)
        {
            if (selectedIndex == mMenuItems.Length - 1)
            {
                OnPressBackButton();
            }
            else
            {
                // Store latest pressed menu item. Now this one can be changed to another key by pressing it.
                mCurrentSelected = selectedIndex;
            }
        }

        /// <summary>
        /// React on key events.
        /// </summary>
        /// <param name="key">Change binding of most lately selected menu entry to key.</param>
        private void ChangeBinding(Keys key)
        {
            // This will be the case if no menu entry was selected before key press.
            if (mCurrentSelected < 0)
            {
                return;
            }
            var newKey = '?';
            // Cannot be converted to char (in this font).
            if (key.ToString().Length == 1)
            {
                newKey = key.ToString().ToLower()[0];
            }
            // Cannot assign the same key to two different actions.
            if (mMap.ContainsKey(newKey))
            {
                return;
            }
            // Update map.
            var oldKey = mSortedArray[mCurrentSelected].Item2;
            var value = mMap[oldKey];
            mMap.Remove(oldKey);
            mMap.Add(newKey, value);
            // Save new bindingsmap.
            var settings = mSettings.CurrentSettings;
            settings.Keymap = mMap;
            mSettings.CurrentSettings = settings;
            // Update menu entry.
            mSortedArray[mCurrentSelected] = new Tuple<UserAction, char>(value, newKey);
            mMenuItems[mCurrentSelected] = value + " - " + newKey;
        }

        /// <inheritdoc/>
        public override void DisableEvents()
        {
            base.DisableEvents();
            mInput.GetKeyboard().KeyReleased -= ChangeBinding;
        }

        /// <inheritdoc/>
        protected override Color SelectColor(int i)
        {
            var tint = base.SelectColor(i);
            if (i == mCurrentSelected)
            {
                tint = Color.Yellow;
            }
            return tint;
        }
    }
}

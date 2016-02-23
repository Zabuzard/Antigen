using System;
using System.Linq;
using Antigen.Content;
using Antigen.Settings;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// OptionsScreen. Show all submenus related to any options. Reachable from the main menu.
    /// </summary>
    internal sealed class OptionsScreen : MenuScreen
    {
        private readonly SettingsManager mSettings;

        /// <summary>
        /// Allows to choose a submenu for changing game options.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public OptionsScreen(InputManager input,
            GameWindow window,
            ContentLoader contentLoader,
            ScreenManager screenManager,
            SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)

        {
            mSettings = settings;
            var currentSettings = settings.CurrentSettings;

            var fullscreenItem = currentSettings.Fullscreen ? "Window" : "Fullscreen";
            if (mScreens.CurrentGameScreen == null)
            {
                mMenuItems = new[]
                {
                    "Camera Speed - " + Math.Round(mSettings.CurrentSettings.CameraScrollSpeed, 1) + "x",
                    "Audio",
                    "Bindings",
                    fullscreenItem,
                    "Resolution",
                    "back"
                };
            }
            else
            {
                mMenuItems = new[]
                {
                    "Camera Speed - " + Math.Round(mSettings.CurrentSettings.CameraScrollSpeed, 1) + "x",
                    "Audio",
                    "Bindings",
                    "back"
                };
            }
        }

        /// <inheritdoc />
        protected override
            void MenuItemSelected(int selectedIndex)
        {
            var settings = mSettings.CurrentSettings;
            switch (selectedIndex)
            {
                case 0:
                    settings.CameraScrollSpeed = ChangeCameraSpeed(settings.CameraScrollSpeed);
                    mSettings.CurrentSettings = settings;
                    mMenuItems[0] = "Camera Speed - " + Math.Round(settings.CameraScrollSpeed, 1) + "x";
                    break;
                case 1:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof(AudioScreen));
                    break;
                case 2:
                    DisableEvents();
                    mScreens.PushMenuScreen(typeof (BindingsScreen));
                    break;
                case 3:
                    if (mScreens.CurrentGameScreen == null)
                    {
                        if (mScreens.Graphics.IsFullScreen)
                        {
                            var customResolutions = Functions.GetCustomResolutions(false);
                            if (!customResolutions.Contains(settings.Resolution))
                            {
                                settings.Resolution = customResolutions.Last();
                            }
                        }
                        var switchToFullscreen = mMenuItems[3] == "Fullscreen";
                        settings.Fullscreen = switchToFullscreen;
                        mSettings.CurrentSettings = settings;
                        mMenuItems[3] = switchToFullscreen ? "Window" : "Fullscreen";
                    }
                    else
                    {
                        OnPressBackButton();
                    }
                    break;
                case 4:
                    if (mScreens.CurrentGameScreen == null)
                    {
                        DisableEvents();
                        mScreens.PushMenuScreen(typeof(ResolutionScreen));
                    }
                    break;
                case 5:
                    OnPressBackButton();
                    break;
            }
        }

        /// <summary>
        /// Change the volume.
        /// </summary>
        /// <param name="currentSpeed">The current camera speed.</param>
        /// <returns>Returns the new camera speed.</returns>
        private static float ChangeCameraSpeed(float currentSpeed)
        {
            currentSpeed += 0.1f;
            if (currentSpeed > 2.05f)
            {
                currentSpeed = 0.5f;
            }
            return currentSpeed;
        }
    }

}

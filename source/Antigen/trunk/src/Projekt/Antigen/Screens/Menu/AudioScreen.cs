using System;
using Antigen.Content;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// The audioscreen is a menu screen, that provides the possibility to change the sound volumes of the game.
    /// </summary>
    internal sealed class AudioScreen : MenuScreen
    {
        /// <summary>
        /// The game's central settings manager.
        /// </summary>
        private readonly SettingsManager mSettings;
        private float mMasterVolume;
        private float mMusicVolume;
        private float mSoundEffectVolume;

        /// <summary>
        /// Allows to change volume of sound effects and music independently.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public AudioScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mSettings = settings;
            mMasterVolume = settings.CurrentSettings.MasterVolume;
            mMusicVolume = settings.CurrentSettings.MusicVolume;
            mSoundEffectVolume = settings.CurrentSettings.SoundEffectVolume;
            mMenuItems = new[] {"Master Volume - " + Math.Round(100 * mMasterVolume) + "%", 
                "Music - " + Math.Round(100 * mMusicVolume) + "%", 
                "Sound Effects - " + Math.Round(100 * mSoundEffectVolume) + "%", "back"};
        }

        /// <inheritdoc />
        protected override void MenuItemSelected(int selectedIndex)
        {
            var settings = mSettings.CurrentSettings;

            switch (selectedIndex)
            {
                case 0:
                    mMasterVolume = ChangeVolume(mMasterVolume);
                    settings.MasterVolume = mMasterVolume;
                    mSettings.CurrentSettings = settings;
                    mMenuItems[0] = "Master Volume - " + Math.Round(100 * mMasterVolume) + "%";
                    break;
                case 1:
                    mMusicVolume = ChangeVolume(mMusicVolume);
                    settings.MusicVolume = mMusicVolume;
                    mSettings.CurrentSettings = settings;
                    mMenuItems[1] = "Music - " + Math.Round(100 * mMusicVolume) + "%";
                    break;
                case 2:
                    mSoundEffectVolume = ChangeVolume(mSoundEffectVolume);
                    settings.SoundEffectVolume = mSoundEffectVolume;
                    mSettings.CurrentSettings = settings;
                    mMenuItems[2] = "Sound Effects - " + Math.Round(100 * mSoundEffectVolume) + "%";
                    break;
                case 3:
                    OnPressBackButton();
                    break;
            }
        }

        /// <summary>
        /// Change the volume.
        /// </summary>
        /// <param name="currentSound">The parameter given is either the music volume or the soundeffect volume.</param>
        /// <returns>Returns the new sound volume.</returns>
        private static float ChangeVolume(float currentSound)
        {
            currentSound += 0.1f;
            // Should only go up to 1f, but > 1.0 fails at 100%.
            if (currentSound > 1.05f)
            {
                currentSound = 0.0f;
            }
            return (float)Math.Round(currentSound, 1);
        }
    }
}

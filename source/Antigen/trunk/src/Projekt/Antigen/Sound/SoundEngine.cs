using Antigen.Objects;
using Antigen.Settings;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Antigen.Sound
{
    /// <summary>
    /// Manages the loading of sound effects as well as
    /// their volume.
    /// </summary>
    sealed class SoundEngine : IDestroy
    {
        private readonly ContentManager mContentManager;
        private readonly SettingsManager mSettings;
        private SoundEffectInstance mCurrentMusic;

        /// <summary>
        /// Creates a sound engine based on the given content manager.
        /// </summary>
        /// <param name="contentManager">The project's contentmanager.</param>
        /// <param name="settingsManager">The settings manager of the game.</param>
        public SoundEngine(ContentManager contentManager, SettingsManager settingsManager)
        {
            mContentManager = contentManager;
            mSettings = settingsManager;
            mSettings.OnSettingsUpdated += OnVolumeChanged;
        }

        /// <summary>
        /// Change the volume of the music currently playing.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Settings update event arguments.</param>
        private void OnVolumeChanged(object sender, SettingsUpdateEventArgs e)
        {
            var master = e.NewSettings.MasterVolume;
            var music = e.NewSettings.MusicVolume;
            // Change volume of currently playing menu music if necessary.
            if (!e.OldSettings.MasterVolume.Equals(master) || !e.OldSettings.MasterVolume.Equals(music))
            {
                mCurrentMusic.Pause();
                Play(mCurrentMusic, SoundCategory.Music);
            }
        }

        /// <summary>
        /// Loads a sound effect, associating it with its
        /// sound category.
        /// </summary>
        /// <param name="fullyQualifiedIdentifier">Full path to the file containing the sound effect.</param>
        /// <returns>The loaded sound effect.</returns>
        public SoundEffect LoadSoundEffect(string fullyQualifiedIdentifier)
        {
            return mContentManager.Load<SoundEffect>(fullyQualifiedIdentifier);
        }

        /// <summary>
        /// Play a soundeffect with the respective volume.
        /// </summary>
        /// <param name="sound">The instance of the soundeffect to play.</param>
        /// <param name="category">The category of the sound, telling whether it's volume is music or effect.</param>
        public void Play(SoundEffectInstance sound, SoundCategory category)
        {
            var settings = mSettings.CurrentSettings;
            if (category.Equals(SoundCategory.Music))
            {
                sound.Volume = settings.MasterVolume * settings.MusicVolume;
            }
            else
            {
                sound.Volume =  settings.MasterVolume * settings.SoundEffectVolume;
            }
            if (category == SoundCategory.Music)
            {
                mCurrentMusic = sound;
            }
            sound.Play();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            mSettings.OnSettingsUpdated -= OnVolumeChanged;
        }
    }
}

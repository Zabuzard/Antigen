using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Antigen.Input;
using Antigen.Util;

namespace Antigen.Settings
{
    /// <summary>
    /// Manages user settings, providing methods to save and load them
    /// to and from disk. Also provides notifications to objects that need
    /// to update their state when settings change.
    /// </summary>
    sealed class SettingsManager
    {
        /// <summary>
        /// Path of the user settings file.
        /// </summary>
        private static readonly string sConfigFile = Functions.GetFolderPath() + "\\antigen.ini.bin";

        /// <summary>
        /// Raised whenever the current user settings are updated.
        /// </summary>
        public event EventHandler<SettingsUpdateEventArgs> OnSettingsUpdated;

        /// <summary>
        /// Raised whenever the user changes the desired resolution.
        /// </summary>
        public event EventHandler<ResolutionChangeEventArgs> OnResolutionChanged;

        /// <summary>
        /// Current game settings.
        /// </summary>
        private SettingsStore mCurrentSettings;

        /// <summary>
        /// Creates a new settings manager with default
        /// settings.
        /// </summary>
        public SettingsManager()
        {
            mCurrentSettings = Defaults();
        }

        /// <summary>
        /// Current user settings. Updating this property
        /// will raise the <see cref="OnSettingsUpdated"/> event.
        /// </summary>
        public SettingsStore CurrentSettings
        {
            get { return mCurrentSettings; }
            set
            {
                var oldSettings = mCurrentSettings;
                mCurrentSettings = value;
                if (!(oldSettings.Equals(value) || OnSettingsUpdated == null))
                    OnSettingsUpdated(this, new SettingsUpdateEventArgs(oldSettings, value));

                var oldRes = oldSettings.Resolution;
                var newRes = mCurrentSettings.Resolution;
                if (!(oldRes.Equals(newRes) || OnResolutionChanged == null))
                    OnResolutionChanged(this, new ResolutionChangeEventArgs());
            }
        }

        /// <summary>
        /// Saves the current settings to the user's settings file.
        /// </summary>
        /// <exception cref="IOException">if the settings cannot be written
        /// to the settings file for some reason.</exception>
        public void Save()
        {
            var formatter = new BinaryFormatter();

            Stream stream = null;
            try
            {
                stream = new FileStream(sConfigFile, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, CurrentSettings);
            }
            catch (IOException e)
            {
                throw new IOException(
                    string.Format("Could not save settings to file '{0}': {1}", sConfigFile, e.Message), e);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        /// <summary>
        /// Loads settings from the user's settings file. If the file does not exist,
        /// the default settings are restored. Updates <see cref="CurrentSettings"/>.
        /// </summary>
        /// <exception cref="IOException">if the settings file exists but cannot
        /// be read for some reason.</exception>
        public void Load()
        {
            var formatter = new BinaryFormatter();

            Stream stream = null;
            try
            {
                stream = new FileStream(sConfigFile, FileMode.Open, FileAccess.Read);
                CurrentSettings = (SettingsStore) formatter.Deserialize(stream);
            }
            catch (FileNotFoundException)
            {
                CurrentSettings = Defaults();
            }
            catch (InvalidCastException e)
            {
                throw new IOException(string.Format(
                    "Corrupt options file: '{0}' is not an Antigen configuration file.", sConfigFile), e);
            }
            catch (IOException e)
            {
                throw new IOException(string.Format(
                    "Could not load settings from file '{0}': {1}", sConfigFile, e.Message), e);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        /// <summary>
        /// Default settings.
        /// </summary>
        /// <returns>The default settings.</returns>
        private static SettingsStore Defaults()
        {
            var keymap = new Dictionary<char, UserAction>
            {
                {'q', UserAction.BuildAntibody},
                {'w', UserAction.BuildBcell},
                {'r', UserAction.BuildRedBloodcell},
                {'t', UserAction.BuildTCell},
                {'z', UserAction.BuildStemcell},
                {'a', UserAction.SelectAttackMode},
                {'f', UserAction.SelectFlowMode},
                {'d', UserAction.SelectCellDivisionMode},
                {'e', UserAction.BuildMacrophage},
                {'s', UserAction.SelectDefensiveMode}
            };

            return new SettingsStore
            {
                Difficulty = Difficulty.Medium,
                Resolution = new Resolution(1024, 798),
                Fullscreen = false,
                CameraScrollSpeed = 1.0f,
                MasterVolume = 1.0f,
                MusicVolume = 0.5f,
                SoundEffectVolume = 1.0f,
                Keymap = keymap
            };
        }
    }

    /// <summary>
    /// Arguments for settings update events. Provide access to both the
    /// old and the new settings.
    /// </summary>
    sealed class SettingsUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Settings before the update that triggered the update event.
        /// </summary>
        public SettingsStore OldSettings { get; private set; }
        /// <summary>
        /// Settings after the update that triggered the update event.
        /// </summary>
        public SettingsStore NewSettings { get; private set; }

        /// <summary>
        /// Creates new settings update event arguments.
        /// </summary>
        /// <param name="oldSettings">Settings before the update that triggered the update event.</param>
        /// <param name="newSettings">Settings after the update that triggered the update event.</param>
        public SettingsUpdateEventArgs(SettingsStore oldSettings, SettingsStore newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }
    }

    /// <summary>
    /// Resolution change event arguments.
    /// </summary>
    sealed class ResolutionChangeEventArgs : EventArgs
    {
    }
}

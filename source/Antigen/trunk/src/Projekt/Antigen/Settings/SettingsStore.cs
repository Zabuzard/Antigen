using System;
using System.Collections.Generic;
using Antigen.Input;

namespace Antigen.Settings
{
    /// <summary>
    /// Collection of settings that can be set by the user.
    /// </summary>
    [Serializable]
    struct SettingsStore
    {
        /// <summary>
        /// The default difficulty setting for new games.
        /// </summary>
        public Difficulty Difficulty { get; set; }
        /// <summary>
        /// Game window resolution.
        /// </summary>
        public Resolution Resolution { get; set; }
        /// <summary>
        /// Whether the game should run in fullscreen or in windowed
        /// mode.
        /// </summary>
        public bool Fullscreen { get; set; }
        /// <summary>
        /// Camera scroll speed factor. Ranges from 0.5f (slowest)
        /// to 2.0f (fastest).
        /// </summary>
        public float CameraScrollSpeed { get; set; }
        /// <summary>
        /// Master volume setting that affects all sounds played by
        /// the game. Ranges from <code>0.0f</code> to
        /// <code>1.0f</code>.
        /// </summary>
        public float MasterVolume { get; set; }
        /// <summary>
        /// Volume setting that affects only the game's background
        /// music. Ranges from <code>0.0f</code> to
        /// <code>1.0f</code>.
        /// </summary>
        public float MusicVolume { get; set; }
        /// <summary>
        /// Volume setting that affects only the game's sound effects.
        /// Ranges from <code>0.0f</code> to
        /// <code>1.0f</code>.
        /// </summary>
        public float SoundEffectVolume { get; set; }
        /// <summary>
        /// Keymap that provides key bindings for actions.
        /// </summary>
        public IDictionary<char, UserAction> Keymap { get; set; }
    }

    /// <summary>
    /// Difficulty setting, allowing the user to customise
    /// the game's difficulty.
    /// </summary>
    [Serializable]
    enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Represents a screen resolution with a horizontal
    /// and a vertical dimension in pixels.
    /// </summary>
    [Serializable]
    struct Resolution
    {
        /// <summary>
        /// Horizontal resolution.
        /// </summary>
        private readonly int mHorizontal;
        /// <summary>
        /// Vertical resolution.
        /// </summary>
        private readonly int mVertical;

        /// <summary>
        /// Creates a new screen resolution.
        /// </summary>
        /// <param name="horizontal">Amount of pixels
        /// along the horizontal axis.</param>
        /// <param name="vertical">Amount of pixels
        /// along the vertical axis.</param>
        public Resolution(int horizontal, int vertical)
        {
            mHorizontal = horizontal;
            mVertical = vertical;
        }

        /// <summary>
        /// Horizontal resolution in pixels.
        /// </summary>
        public int Horizontal { get { return mHorizontal; } }

        /// <summary>
        /// Vertical resolution in pixels.
        /// </summary>
        public int Vertical { get { return mVertical; } }
    }
}

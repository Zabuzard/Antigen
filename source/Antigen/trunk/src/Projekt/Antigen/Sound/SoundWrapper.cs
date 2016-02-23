using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Content;
using Antigen.Screens;
using Antigen.Screens.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Antigen.Sound
{
    /// <summary>
    /// A class to manage all sounds ever to
    /// be used in the game. Allows different
    /// categories of sounds to have their
    /// volume changed separately.
    /// </summary>
    sealed class SoundWrapper : ILoad
    {
        private readonly SoundEngine mSoundEngine;
        private readonly ScreenManager mScreens;
        private readonly Dictionary<string, Enum> mSoundList;
        private readonly Dictionary<Enum, SoundEffectInstance> mSound;
        private Point mWindowSize;

        /// <summary>
        /// Creates new SoundWrapper.
        /// <param name="soundEngine">The game's central sound engine.</param>
        /// </summary>
        public SoundWrapper(SoundEngine soundEngine, ScreenManager screenManager)
        {
            mSoundEngine = soundEngine;
            mScreens = screenManager;
            mSoundList = new Dictionary<string, Enum>{
                {"Attack1", Effect.Attack}, 
                {"Attack2", Effect.Debuff},
                {"Attack3", Effect.DeInfection},
                {"CellDivision", Effect.Division},
                {"GameMusic", Music.Game},
                {"TitleMusic", Music.Menu},
                {"Move1", Effect.Move1},
                {"Move2", Effect.Move2},
                {"Pop1", Effect.Destruction}
            };
            mSound = new Dictionary<Enum, SoundEffectInstance>();
            mScreens.Window.ClientSizeChanged += OnWindowSizeChanged;
            mWindowSize = new Point(mScreens.Window.ClientBounds.X, mScreens.Window.ClientBounds.Y);
        }

        private void OnWindowSizeChanged(object sender, EventArgs e)
        {
            mWindowSize = new Point(mScreens.Window.ClientBounds.X, mScreens.Window.ClientBounds.Y);
        }

        /// <summary>
        /// Load all sounds ever to be used.
        /// <param name="contentLoader">The game's central content loader.</param>
        /// </summary>
        public void LoadContent(ContentLoader contentLoader)
        {
            if (mSound.Any())
            {
                return;
            }
            foreach (var effect in mSoundList)
            {
                var thatEffect = contentLoader.LoadSoundEffect(effect.Key);
                var sound = thatEffect.CreateInstance();
                if (effect.Value is Music)
                {
                    sound.IsLooped = true;
                }
                mSound.Add(effect.Value, sound);
            }
        }

        /// <summary>
        /// Tell sound engine to play selected effect if gameScreen is on top and unit is in sight.
        /// </summary>
        /// <param name="effect">Type of sound effect.</param>
        /// <param name="position">Position of the Unit that wants to play an effect.</param>
        public void PlayEffect(Effect effect, Vector2 position)
        {
            // TODO Reenable move sound.
            if (effect == Effect.Move1 || effect == Effect.Move2)
            {
                return;
            }
            if (mSound[effect].State == SoundState.Playing)
            {
                return;
            }
            if (!(mScreens.Peek() is GameScreen))
            {
                return;
            }
            var leftUpperCorner = mScreens.CurrentGameScreen.GetCam().ToAbsolute(new Point(0, 0));
            var rightLowerCorner =
                mScreens.CurrentGameScreen.GetCam().ToAbsolute(new Point(mWindowSize.X, mWindowSize.Y));
            if (new Rectangle(leftUpperCorner.X, leftUpperCorner.Y, rightLowerCorner.X, rightLowerCorner.Y).Contains(new Point((int)position.X, (int)position.Y)))
            {   
                mSoundEngine.Play(mSound[effect], SoundCategory.Effect);
            }
            
        }

        /// <summary>
        /// Tell SoundEngine to play selected music if it's not already playing and screen is on top.
        /// </summary>
        /// <param name="music">Type of music.</param>
        public void PlayMusic(Music music)
        {
            // Only play musi if it's not already playing.
            if (mSound[music].State == SoundState.Playing)
            {
                return;
            }
            // Play menu music if screen on top is menu screen.
            if (music.Equals(Music.Menu))
            {
                if (!(mScreens.Peek() is MenuScreen))
                {
                    return;
                }
                // Kill all playing sounds.
                foreach (var sound in mSound.Where(sound => sound.Value.State == SoundState.Playing))
                    {
                        sound.Value.Stop();
                    }
            }
            // Play game music if screen on top is current game screen.
            else if (music.Equals(Music.Game))
            {
                if (mScreens.Peek() != mScreens.CurrentGameScreen)
                {
                    return;
                }
                // Kill menu music if playing.
                if (mSound[Music.Menu].State == SoundState.Playing)
                {
                    mSound[Music.Menu].Stop();
                }
            }
            mSoundEngine.Play(mSound[music], SoundCategory.Music);
        }
    }
}

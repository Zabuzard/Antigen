using System;
using Antigen.Sound;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Content
{
    /// <summary>
    /// Wrapper around <see cref="ContentManager"/> with convenience methods
    /// for loading textures, sounds, and maps.
    /// </summary>
    sealed class ContentLoader
    {

        /// <summary>
        /// Prefix added to texture identifiers in order to locate them
        /// in the content project.
        /// </summary>
        private const String TexturePrefix = "res\\obj";

        /// <summary>
        /// Prefix added to sound effect identifiers in order to located
        /// them in the content project.
        /// </summary>
        private const String SoundEffectPrefix = "res\\soundeffects";

        /// <summary>
        /// The project's content manager.
        /// </summary>
        private readonly ContentManager mContentManager;
        /// <summary>
        /// The project's sound engine.
        /// </summary>
        private readonly SoundEngine mSoundEngine;

        /// <summary>
        /// Creates a <code>ContentLoader</code> based on the
        /// given <code>ContentManager</code>.
        /// </summary>
        /// <param name="contentManager">The project's content manager.</param>
        /// <param name="soundEngine">The project's sound engine.</param>
        public ContentLoader(ContentManager contentManager, SoundEngine soundEngine)
        {
            mContentManager = contentManager;
            mSoundEngine = soundEngine;
        }

        /// <summary>
        /// Loads a texture.
        /// </summary>
        /// <param name="identifier">The texture's identifier.
        /// Must not include the path to the content folder
        /// for textures.</param>
        /// <returns>The loaded texture.</returns>
        public Texture2D LoadTexture(String identifier)
        {
            var id = TexturePrefix + "\\" + identifier;
            return mContentManager.Load<Texture2D>(id);
        }

        /// <summary>
        /// Loads a sound effect.
        /// </summary>
        /// <param name="identifier">The sound effect's identifier.
        /// Must not include the path to the content folder
        /// for sound effects.</param>
        /// <returns>The loaded sound effect.</returns>
        public SoundEffect LoadSoundEffect(String identifier)
        {
            var id = SoundEffectPrefix + "\\" + identifier;
            return mSoundEngine.LoadSoundEffect(id);
        }
    }
}

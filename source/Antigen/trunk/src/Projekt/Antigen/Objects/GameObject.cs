using System;
using Antigen.Logic.Collision;
using Microsoft.Xna.Framework;

namespace Antigen.Objects
{
    /// <summary>
    /// Base class for all game objects.
    /// </summary>
    [Serializable]
    abstract class GameObject : ISpatial
    {
        private Vector2 mPosition;

        /// <inheritdoc />
        public Vector2 Position
        {
            get { return mPosition; }
            set
            {
                mPosition = value;
                Hitbox = new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), Radius * 2, Radius * 2);
            }
        }
        /// <inheritdoc />
        public Vector2 OldPosition { get; set; }

        /// <inheritdoc />
        public int Radius { get; protected set; }

        /// <inheritdoc />
        public Rectangle Hitbox { get; private set; }

        /// <summary>
        /// Creates a new game object with default values for all
        /// fields.
        /// </summary>
        protected GameObject(Vector2 startPosition)
        {
            Position = startPosition;
            OldPosition = Position;
            Radius = 0;
        }
    }
}

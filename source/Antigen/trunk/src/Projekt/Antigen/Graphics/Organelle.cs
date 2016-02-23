using System;
using Microsoft.Xna.Framework;

namespace Antigen.Graphics
{
    /// <summary>
    /// A cell organelle
    /// </summary>
    [Serializable]
    sealed class Organelle
    {
        private static readonly Random sRandom = new Random();
        private Vector2 mVelocity;

        /// <summary>
        /// The object's center position in the cell,
        /// in relative coordinates.
        /// </summary>
        public Vector2 Position { get; private set; }
        public float Rotation { get; private set; }

        /// <summary>
        /// Updates the position and rotation
        /// </summary>
        /// <param name="moveRadius">The radius in which the organelle can move</param>
        public void Update(float moveRadius)
        {
            Rotation += 0.01f;
            if (Rotation > 2*Math.PI) Rotation = 0;

            mVelocity.X -= Position.X / moveRadius + (float)sRandom.NextDouble() * 0.5f - 0.25f;
            mVelocity.Y -= Position.Y / moveRadius + (float)sRandom.NextDouble() * 0.5f - 0.25f;

            mVelocity.Normalize();
            //mVelocity *= 0.5f;

            var newPosition = Position;
            newPosition += mVelocity;
            if (Vector2.Distance(Vector2.Zero, newPosition) <= moveRadius) Position = newPosition;
        }
    }
}

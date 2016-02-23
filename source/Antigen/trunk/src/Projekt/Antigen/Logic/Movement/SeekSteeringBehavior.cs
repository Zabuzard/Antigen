using System;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Simple steering behaviour that seeks a designated
    /// point on the map. Applies a force that changes
    /// the host's movement vector such that it points
    /// towards the target point.
    /// </summary>
    [Serializable]
    sealed class SeekSteeringBehavior : ISteeringBehavior
    {
        /// <summary>
        /// Maximum amount of force exerted by this behaviour.
        /// Increases acceleration and turning speed.
        /// </summary>
        private const float MaxForce = 0.5f;

        /// <summary>
        /// The target point.
        /// </summary>
        private readonly Vector2 mTarget;

        /// <summary>
        /// Creates a steering behaviour that seeks the
        /// given point.
        /// </summary>
        /// <param name="targetLocation">A point on the map.</param>
        public SeekSteeringBehavior(Vector2 targetLocation)
        {
            mTarget = targetLocation;
        }

        /// <inheritdoc />
        public void ApplySteeringForce(ICanMove host)
        {
            if (host.Position== mTarget)
                return;

            var direction = mTarget - host.Position;
            var desiredMovementVector = (direction * host.MaxVelocity) / direction.Length();

            var steering = (desiredMovementVector - host.MovementVector).Truncate(MaxForce);
            host.MovementVector = (host.MovementVector + steering).Truncate(host.MaxVelocity);
        }
    }
}

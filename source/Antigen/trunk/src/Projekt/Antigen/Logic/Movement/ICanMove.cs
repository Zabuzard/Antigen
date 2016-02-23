using Antigen.Logic.Collision;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Interface for movable objects.
    /// </summary>
    interface ICanMove : ISpatial
    {
        /// <summary>
        /// The implementor's current movement vector.
        /// When updating the position, the movement
        /// vector is added to the position vector.
        /// </summary>
        Vector2 MovementVector { get; set; }
        /// <summary>
        /// Maximum velocity on a scale from <code>1f</code>
        /// to <code>2f</code> for regular units and
        /// up to <code>4f</code> for extra-fast units.
        /// </summary>
        float MaxVelocity { get; }
        /// <summary>
        /// The implementor's sight radius, in pixels.
        /// </summary>
        float SightRange { get; }
        /// <summary>
        /// Area in which the implementor will search
        /// for obstacles to avoid.
        /// </summary>
        ICollidable AvoidanceHitbox { get; }
    }
}

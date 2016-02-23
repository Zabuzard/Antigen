using System;
using System.Collections.Generic;
using Antigen.Logic.Collision;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Composition of different steering behaviours.
    /// Setting the properties of a <code>SteeringManager</code>
    /// to non-<code>null</code> values enables the associated
    /// steering behaviours.
    /// </summary>
    [Serializable]
    sealed class SteeringManager : ISteeringBehavior
    {
        /// <summary>
        /// Seek behaviour. <code>null</code> if
        /// no seek steering should be applied.
        /// </summary>
        private ISteeringBehavior mSeekBehavior;
        /// <summary>
        /// Wander (= floating) behavior. <code>null</code>
        /// if no wander steering should be applied.
        /// </summary>
        private readonly ISteeringBehavior mFlowBehavior;
        /// <summary>
        /// Path following behaviour. <code>null</code> if
        /// no path should be followed.
        /// </summary>
        private FollowPathSteeringBehavior mPathFollowBehavior;

        private readonly AvoidanceSteeringBehavior mAvoidanceBehavior;

        /// <summary>
        /// Creates a new steering manager using the given map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="objectCollision">Object that provides collision
        /// detection with game objects.</param>
        public SteeringManager(Map.Map map, IObjectCollisionContainer objectCollision)
        {
            mFlowBehavior = new FlowSteeringBehavior(map);
            mAvoidanceBehavior = new AvoidanceSteeringBehavior(objectCollision);
        }

        /// <summary>
        /// Destination point for the seek behaviour.
        /// <code>null</code> if no seek steering
        /// should be applied.
        /// </summary>
        private Vector2? SeekTarget
        {
            set { mSeekBehavior = value != null ? new SeekSteeringBehavior(value.Value) : null; }
        }

        /// <summary>
        /// Path to be followed by the path following
        /// behaviour. <code>null</code> if no path
        /// should be followed.
        /// </summary>
        public Stack<Vector2> Path
        {
            set { mPathFollowBehavior = value != null ? new FollowPathSteeringBehavior(value) : null; }
        }

        /// <summary>
        /// Whether to apply the blood flow
        /// steering force.
        /// </summary>
        public bool ApplyFlow { private get; set; }

        /// <summary>
        /// Whether to apply a steering behaviour that
        /// attempts to avoid other game objects.
        /// </summary>
        public bool ApplyAvoidance { private get; set; }

        public ICollidable IgnoredUnit { private get; set; }

        /// <inheritdoc />
        public void ApplySteeringForce(ICanMove host)
        {
            if (mSeekBehavior != null)
                mSeekBehavior.ApplySteeringForce(host);
            if (ApplyFlow)
                mFlowBehavior.ApplySteeringForce(host);
            if (mPathFollowBehavior != null)
                mPathFollowBehavior.ApplySteeringForce(host);
            if (ApplyAvoidance)
                mAvoidanceBehavior.ApplySteeringForce(host, IgnoredUnit);
        }

        /// <summary>
        /// Resets all steering forces, resulting in a
        /// steering manager that will not change
        /// a host's movement manager when steering
        /// is applied.
        /// </summary>
        public void Reset()
        {
            SeekTarget = null;
            ApplyFlow = false;
            ApplyAvoidance = false;
            Path = null;
            IgnoredUnit = null;
        }
    }
}

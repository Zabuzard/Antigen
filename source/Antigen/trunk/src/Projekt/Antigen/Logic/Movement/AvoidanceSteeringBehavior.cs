using System;
using Antigen.Logic.Collision;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Class to implement collision avoidance for steering.
    /// </summary>
    [Serializable]
    sealed class AvoidanceSteeringBehavior : ISteeringBehavior
    {
        /// <summary>
        /// Force multiplier.
        /// </summary>
        private const float Force = 0.5f;
        /// <summary>
        /// Distance beyond which no collision avoidance is attempted.
        /// </summary>
        private const float Distance = 70;

        private readonly IObjectCollisionContainer mContainer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Used for unit grid information.</param>
        public AvoidanceSteeringBehavior(IObjectCollisionContainer container)
        {
            mContainer = container;
        }

        /// <inheritdoc />
        public void ApplySteeringForce(ICanMove host)
        {
            ApplySteeringForce(host, null);
        }

        /// <summary>
        /// Applies appropriate avoidance force.
        /// </summary>
        /// <param name="host">The cell to be affected.</param>
        /// <param name="ignoredUnit">A unit that is ignored by collision
        /// avoidance.</param>
        public void ApplySteeringForce(ICanMove host, ICollidable ignoredUnit)
        {
            var pos = host.Position;
            var mov = host.MovementVector;
            var aheadProto = (mov * Distance) / mov.Length();
            var ahead = pos + aheadProto;
            var ahead2 = pos + aheadProto * 0.5f;

            var mostThreatening = FindMostThreateningUnit(host, ahead, ahead2, ignoredUnit);
            if (mostThreatening == null)
                return;

            var avoidance = (ahead - mostThreatening.Position);
            avoidance = (avoidance * Force) / avoidance.Length();

            if (float.IsNaN(avoidance.X) || float.IsNaN(avoidance.Y))
                return;

            host.MovementVector = (host.MovementVector + avoidance).Truncate(host.MaxVelocity);
        }

        /// <summary>
        /// Find unit to be avoided.
        /// </summary>
        private ICollidable FindMostThreateningUnit(ICanMove host, Vector2 ahead, Vector2 ahead2, ICollidable ignoredUnit)
        {
            ICollidable mostThreatening = null;
            var candidates = mContainer.Collisions(host.AvoidanceHitbox, CollisionDetection.PairwiseHitboxCollisionDetection);

            var pos = host.Position;
            var mostThreatingAheadDistance = 0f;

            foreach (var c in candidates)
            {
                var cPos = c.Position;
                if (pos == cPos)
                    continue;

                if (ignoredUnit != null && c.Equals(ignoredUnit))
                    continue;

                var aheadDistance = Vector2.Distance(ahead, cPos);
                var ahead2Distance = Vector2.Distance(ahead2, cPos);

                var cRadius = c.Radius;
                if (aheadDistance < cRadius &&
                    ahead2Distance < cRadius)
                    continue;

                if (mostThreatening == null || ahead2Distance < mostThreatingAheadDistance)
                {
                    mostThreatening = c;
                    mostThreatingAheadDistance = aheadDistance;
                }
            }

            return mostThreatening;
        }

        /// <summary>
        /// Adjusts the host's avoidance hitbox. This is the area in which the host will
        /// look for obstacles to avoid.
        /// </summary>
        /// <param name="host">Any movable host. Will not be modified.</param>
        /// <param name="hitbox">The host's hitbox that will be modified.</param>
        public static void AdjustAvoidanceHitbox(ICanMove host, AvoidanceHitbox hitbox)
        {
            var position = host.Position;
            var oldPosition = host.OldPosition;

            if (position.Equals(oldPosition))
                return;

            hitbox.Position = position;
            hitbox.OldPosition = oldPosition;
            hitbox.Hitbox = AvoidanceHitboxRect(position, host.MovementVector);
        }

        /// <summary>
        /// Sets the host's avoidance hitbox based on its current position and movement.
        /// When the host's hitbox is non-<code>null</code>, use <see cref="AdjustAvoidanceHitbox"/>
        /// instead.
        /// </summary>
        /// <param name="host">Any movable object.</param>
        /// <returns>An avoidance hitbox for the host.</returns>
        public static AvoidanceHitbox MakeInitialAvoidanceHitbox(ICanMove host)
        {
            var position = host.Position;
            var rec = AvoidanceHitboxRect(position, host.MovementVector);
            return new AvoidanceHitbox(rec, position, host.OldPosition);
        }

        /// <summary>
        /// Creates the rectangle used for the avoidance hitbox.
        /// </summary>
        /// <param name="position">The host's current position.</param>
        /// <param name="movement">The host's current movement vector.</param>
        /// <returns>A rectangle for the new avoidance hitbox.</returns>
        private static Rectangle AvoidanceHitboxRect(Vector2 position, Vector2 movement)
        {
            var par = (movement * Distance) / movement.Length(); // parallel to movement
            var ortho = new Vector2(1, -movement.X / movement.Y); // orthogonal to movement
            ortho = (ortho * Distance) / ortho.Length();

            var r1 = position + ortho;
            var r2 = position - ortho;
            var r3 = position + par + ortho;
            var r4 = position + par - ortho;

            float xMax, xMin, yMax, yMin;
            Functions.MinMax(new[] {r1.X, r2.X, r3.X, r4.X}, out xMin, out xMax);
            Functions.MinMax(new[] {r1.Y, r2.Y, r3.Y, r4.Y}, out yMin, out yMax);

            return new Rectangle((int)xMin, (int)yMin, (int)(xMax - xMin), (int)(yMax - yMin));
        }

        /// <summary>
        /// Virtual collidable object that provides an avoidance hitbox.
        /// Objects can add these to the collision cache in order to benefit from
        /// collision optimisation.
        /// 
        /// Note that the object is virtual, meaning it will not show up in
        /// collision-related queries made by other objects.
        /// </summary>
        [Serializable]
        public sealed class AvoidanceHitbox : ICollidable
        {
            /// <summary>
            /// Creates an avoidance hitbox with the given properties.
            /// </summary>
            /// <param name="hitbox">Hitbox to look in.</param>
            /// <param name="position">The hitbox host's position.</param>
            /// <param name="oldPosition">The hitbox host's previous position.</param>
            public AvoidanceHitbox(Rectangle hitbox, Vector2 position, Vector2 oldPosition)
            {
                Hitbox = hitbox;
                Position = position;
                OldPosition = oldPosition;
            }
            
            /// <inheritdoc />
            public Rectangle Hitbox { get; set; }

            /// <inheritdoc />
            public bool IsVirtualCollidable
            {
                get { return true; }
            }

            /// <inheritdoc />
            public Vector2 Position { get; set; }

            /// <inheritdoc />
            public Vector2 OldPosition { get; set; }

            /// <inheritdoc />
            public int Radius
            {
	            get { throw new Exception("Avoidance hitbox has no radius."); }
            }

            /// <inheritdoc />
            public bool CollisionInLastTick
            {
                get { throw new Exception("Avoidance hitbox does not support collision result caching."); }
                set { throw new Exception("Avoidance hitbox does not support collision result caching."); }
            }
        }
    }
}

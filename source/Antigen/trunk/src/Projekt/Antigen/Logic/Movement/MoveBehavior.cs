using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antigen.Logic.Collision;
using Antigen.Logic.Pathfinding;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Movement behaviour that provides collision detection with
    /// both the map and other dynamic objects and attempts to
    /// adjust its movement to avoid collisions with nearby
    /// obstacles.
    /// 
    /// All methods specified by the <see cref="ICanMove"/>
    /// interface defer to the corresponding methods from
    /// the behaviour object's parent unit.
    /// </summary>
    [Serializable]
    sealed class MoveBehavior : IMoveBehavior
    {
        /// <summary>
        /// Radius of the arrival zone around the
        /// current destination. When an object
        /// enters this zone, the destination is
        /// considered reached.
        /// </summary>
        private const float ArrivalRadius = 30;

        /// <summary>
        /// Unit that this behaviour belongs to.
        /// </summary>
        private readonly ICanMove mParent;
        /// <summary>
        /// A steering manager.
        /// </summary>
        private readonly SteeringManager mSteering;
        /// <summary>
        /// Container that provides object-to-object collision
        /// information.
        /// </summary>
        private readonly IObjectCollisionContainer mObjectCollision;
        /// <summary>
        /// Container that provides object-to-map collision
        /// information.
        /// </summary>
        private readonly IMapCollisionContainer mMapCollision;
        /// <summary>
        /// Pathfinder for the current map.
        /// </summary>
        [NonSerialized]
        private Pathfinder mPathfinder;
        /// <summary>
        /// Uniformly distributed RNG.
        /// </summary>
        private readonly Random mRandom;

        /// <summary>
        /// Target that is currently being followed.
        /// <code>null</code> if target is currently
        /// being followed.
        /// </summary>
        private ICanMove mFollowTarget;
        /// <summary>
        /// Current destination. <code>null</code> if
        /// the parent unit is not currently attempting to
        /// reach a specific destination.
        /// </summary>
        private Vector2? mCurrentDestination;
        /// <summary>
        /// The task used to compute the current path.
        /// <code>null</code> if no path is currently
        /// being computed.
        /// </summary>
        [NonSerialized]
        private Task<Stack<Vector2>> mPathTask;

        /// <summary>
        /// Creates a colliding movement behaviour that
        /// belongs to the given object.
        /// </summary>
        /// <param name="parent">The game object this
        /// behaviour belongs to.</param>
        /// <param name="map">Map which the parent unit is
        /// located on.</param>
        /// <param name="objectCollision">Container that
        /// provides object-to-object collision information.
        /// Pass <code>null</code> if the host object should not
        /// react to collisions with other objects.</param>
        /// <param name="pathfinder">The game's pathfinder.</param>
        public MoveBehavior(ICanMove parent, Map.Map map,
            IObjectCollisionContainer objectCollision, Pathfinder pathfinder)
        {
            mParent = parent;
            mObjectCollision = objectCollision;
            mMapCollision = map;
            mPathfinder = pathfinder;
            mRandom = new Random();

            mSteering = new SteeringManager(map, objectCollision);
            if (mObjectCollision != null)
                mSteering.ApplyAvoidance = true;

            IsMoving = !MovementVector.Equals(Vector2.Zero);
        }

        /// <inheritdoc />
        public void MoveTo(Vector2 point)
        {
            mFollowTarget = null;
            mSteering.Reset();
            if (mObjectCollision != null)
                mSteering.ApplyAvoidance = true;
            mCurrentDestination = point;
            IsMoving = true;
            if (mPathfinder == null)
            {
                mPathfinder = ((Unit) mParent).mObjectCaches.Pathfinder;
            }
            mPathTask = mPathfinder.FindPath(mParent.Position, point);
        }

        /// <inheritdoc />
        public void Follow(ICanMove target)
        {
            mCurrentDestination = null;
            mSteering.Reset();
            if (mObjectCollision != null)
            {
                mSteering.ApplyAvoidance = true;
                var collidable = target as ICollidable;
                if (collidable != null)
                    mSteering.IgnoredUnit = collidable;
            }
                
            IsMoving = true;
            mFollowTarget = target;
        }

        /// <inheritdoc />
        public void Wander()
        {
            mFollowTarget = null;
            mCurrentDestination = null;
            mSteering.Reset();
            if (mObjectCollision != null)
                mSteering.ApplyAvoidance = true;
            IsMoving = true;
            mSteering.ApplyFlow = true;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            //Path has arrived
            if (mPathTask != null && mPathTask.IsCompleted)
            {
                mSteering.Path = mPathTask.Result;
                mPathTask = null;
            }
                
            //Nothing to do if we're not moving
            if (!IsMoving)
                return;

            var elapsedTimeMs = gameTime.ElapsedGameTime.Milliseconds;

            //Collision response
            var collidable = mParent as ICollidable;
            if (collidable != null && collidable.CollisionInLastTick)
            {
                OldPosition = Position;
                var delta = new Vector2(mRandom.Next(-11, 9) + 1, mRandom.Next(-11, 9) + 1);
                Position += delta;
                CollisionResponse.Resolve(collidable, mMapCollision, mObjectCollision);
                return;
            }

            //Path update for target following
            //for every iteration, which probably hogs performance
            //quite a bit. Perhaps it would be better to update
            //the path only under certain conditions.
            if (mFollowTarget != null)
            {
                mPathTask = mPathfinder.FindPath(mParent.Position, mFollowTarget.Position);
            }

            //Arrival at specific destination
            if (mCurrentDestination != null &&
                Vector2.Distance(mParent.Position, mCurrentDestination.Value) <= ArrivalRadius)
                Terminate();

            //Steering force application
            mSteering.ApplySteeringForce(this);
            OldPosition = Position;
            Position += MovementVector * elapsedTimeMs * 0.1f;

            //Reset the position if the object is now in a colliding state.
            if (collidable != null)
                CollisionResponse.Resolve(collidable, mMapCollision, mObjectCollision);
        }

        /// <inheritdoc />
        public void Terminate()
        {
            mFollowTarget = null;
            mCurrentDestination = null;
            mSteering.Reset();
            MovementVector = Vector2.Zero;
            IsMoving = false;
        }

        /// <inheritdoc />
        public Vector2 Position
        {
            get { return mParent.Position; }
            set { mParent.Position = value; }
        }

        /// <inheritdoc />
        public Vector2 MovementVector
        {
            get { return mParent.MovementVector; }
            set { mParent.MovementVector = value; }
        }

        /// <inheritdoc />
        public float MaxVelocity
        {
            get { return mParent.MaxVelocity; }
        }

        /// <inheritdoc />
        public bool IsMoving { get; private set; }

        /// <inheritdoc />
        public float SightRange
        {
            get { return mParent.SightRange; }
        }

        /// <inheritdoc />
        public int Radius
        {
            get { return mParent.Radius; }
        }

        /// <inheritdoc />
        public ICollidable AvoidanceHitbox
        {
            get { return mParent.AvoidanceHitbox; }
        }

        /// <inheritdoc />
        public Vector2 OldPosition
        {
            get { return mParent.OldPosition; }
            set { mParent.OldPosition = value; }
        }

        /// <inheritdoc />
        public Rectangle Hitbox
        {
            get { return mParent.Hitbox; }
        }
    }
}

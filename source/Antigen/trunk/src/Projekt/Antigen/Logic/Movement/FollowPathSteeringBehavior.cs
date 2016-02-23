using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Steering behaviour that follows a specified path.
    /// Objects controlled by this behaviour will attempt
    /// to reach each path node in order. When they reach
    /// the last node, they continue seeking it.
    /// </summary>
    [Serializable]
    sealed class FollowPathSteeringBehavior : ISteeringBehavior
    {
        /// <summary>
        /// Radius of the arrival zone around path nodes.
        /// </summary>
        private const float ArrivalRadius = 30;

        /// <summary>
        /// Current path. Invariant: the stack's first
        /// item is the node currently sought.
        /// </summary>
        private readonly Stack<Vector2> mPath;
        /// <summary>
        /// Current seek behaviour. Invariant: this
        /// behaviour seeks the first item of <see cref="mPath"/>.
        /// </summary>
        private ISteeringBehavior mSeekBehavior;

        /// <summary>
        /// Creates a behaviour that follows the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        public FollowPathSteeringBehavior(Stack<Vector2> path)
        {
            mPath = path;
            if (path.Count > 0)
                mSeekBehavior = new SeekSteeringBehavior(mPath.Peek());
        }

        /// <inheritdoc />
        public void ApplySteeringForce(ICanMove host)
        {
            //No path node left, hence don't apply any force.
            if (mPath.Count == 0)
                return;

            //Arrival at current path node, hence seek the next. Continue
            //seeking the current node if it is the last.
            var distToTarget = Vector2.Distance(host.Position, mPath.Peek());
            if (distToTarget <= ArrivalRadius && mPath.Count > 1)
            {
                mPath.Pop();
                mSeekBehavior = new SeekSteeringBehavior(mPath.Peek());
            }
                
            //Force application
            mSeekBehavior.ApplySteeringForce(host);
        }
    }
}

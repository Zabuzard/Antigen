using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Interface for movement behaviours.
    /// A movement behaviour can be used to move
    /// game objects in some way. Different
    /// move behaviours may take into account
    /// different characteristics of the
    /// environment, enemies in the vicinity
    /// and more.
    /// 
    /// Calling any of the movement methods
    /// terminates the current movement and
    /// initiates a new movement according to
    /// the called method.
    /// </summary>
    interface IMoveBehavior : ICanMove
    {
        /// <summary>
        /// Moves the object to the given point on the map.
        /// </summary>
        /// <param name="point">Destination coordinates</param>
        void MoveTo(Vector2 point);

        /// <summary>
        /// Follows the given target indefinitely.
        /// </summary>
        /// <param name="target">Another moveable
        /// game object.</param>
        void Follow(ICanMove target);

        /// <summary>
        /// Wanders around aimlessly indefinitely.
        /// </summary>
        void Wander();

        /// <summary>
        /// Updates the position and movement vector of
        /// the behaviour object's parent unit.
        /// </summary>
        /// <param name="gameTime">Current game time.</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Terminates the movement, stopping the parent
        /// object.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Indicates whether a movement is currently
        /// in progress. When this is <code>false</code>,
        /// the behaviour object's parent unit's movement
        /// vector is zero.
        /// </summary>
        bool IsMoving { get; }
    }
}

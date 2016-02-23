namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Implements an algorithm for pairwise collision detection.
    /// </summary>
    /// <param name="x">Any collidable object.</param>
    /// <param name="y">Any collidable object.</param>
    /// <returns>Whether <code>x</code> and <code>y</code>
    /// collide.</returns>
    delegate bool PairwiseCollisionDetection(ICollidable x, ICollidable y);

    /// <summary>
    /// Interface for collidable objects.
    /// </summary>
    interface ICollidable : ISpatial
    {
        /// <summary>
        /// Indicates that the object is only virtually
        /// collidable. This means that it can be added
        /// to <see cref="IObjectCollisionContainer"/>s
        /// and the containers can be queried for
        /// this object's collisions, but it won't
        /// appear in collision query results for
        /// other objects.
        /// </summary>
        bool IsVirtualCollidable { get; }
        /// <summary>
        /// Indicates that a collision for this object
        /// was detected during the last update cycle.
        /// </summary>
        bool CollisionInLastTick { get; set; }
    }
}

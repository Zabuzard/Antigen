using System.Collections.Generic;

namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Contains information about blocking units on the map.
    /// </summary>
    interface IObjectCollisionContainer
    {
        /// <summary>
        /// The list of objects that currently collide with the specified object,
        /// according to the given collision detection algorithm.
        /// 
        /// If <code>obj</code> is not present in the container, an empty list is returned.
        /// </summary>
        /// <param name="obj">Any collidable object.</param>
        /// <param name="cd">A collision detection algorithm.</param>
        /// <returns>All objects present in the container that collide with
        /// <code>obj</code>.</returns>
        IEnumerable<ICollidable> Collisions(ICollidable obj, PairwiseCollisionDetection cd);

        /// <summary>
        /// Adds a new object to the container. If the
        /// object is fully outside of the area covered
        /// by the container, it is not added.
        /// </summary>
        /// <param name="obj">Any collidable object.</param>
        void Add(ICollidable obj);

        /// <summary>
        /// Removes an object from the container.
        /// If <code>obj</code> was not contained in
        /// the container before, the container is
        /// not changed.
        /// </summary>
        /// <param name="obj">Any collidable object.</param>
        void Remove(ICollidable obj);

        /// <summary>
        /// Updates an object inside the container.
        /// If the object has not changed its
        /// position since the last tick, or if
        /// the object is not contained in the container,
        /// the container is not changed.
        /// </summary>
        /// <param name="obj">Any collidable object.</param>
        void Update(ICollidable obj);
    }
}

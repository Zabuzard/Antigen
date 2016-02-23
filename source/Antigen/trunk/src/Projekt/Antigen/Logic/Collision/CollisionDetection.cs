using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Collision
{
    [Serializable]
    static class CollisionDetection
    {
        /// <summary>
        /// Returns if a with objects collidable object collides with another object of such type.
        /// </summary>
        /// <param name="obj">Collidable object to check collision</param>
        /// <param name="container">Container of collidable objects</param>
        /// <returns>True if the objects collide</returns>
        public static bool CollidesWithObject(IObjectCollidable obj, IObjectCollisionContainer container)
        {
            return container.Collisions(obj, PairwiseCircleCollisionDetection).Any();
        }

        /// <summary>
        /// Returns if a collidable object collides with the map.
        /// </summary>
        /// <param name="obj">Collidable object</param>
        /// <param name="cont">Container which holds data of map collision</param>
        /// <returns>True if the object collides with the map</returns>
        public static bool CollidesWithMap(IMapCollidable obj, IMapCollisionContainer cont)
        {
            return cont.IsBlocking(obj.Hitbox);
        }

        /// <summary>
        /// True iff the smallest circles enclosing <code>x</code> and <code>y</code>
        /// intersect.
        /// </summary>
        /// <param name="x">Any collidable object.</param>
        /// <param name="y">Any collidable object.</param>
        /// <returns>Whether <code>x</code> and <code>y</code> collide.</returns>
        public static bool PairwiseCircleCollisionDetection(ICollidable x, ICollidable y)
        {
            var centerX = x.Position + new Vector2(x.Radius, x.Radius);
            var centerY = y.Position + new Vector2(y.Radius, y.Radius);
            return Vector2.Distance(centerX, centerY) <= x.Radius + y.Radius;
        }

        /// <summary>
        /// True iff <code>x</code>'s and <code>y</code>'s hitboxes intersect.
        /// </summary>
        /// <param name="x">Any collidable object.</param>
        /// <param name="y">Any collidable object.</param>
        /// <returns>Whether <code>x</code> and <code>y</code> collide.</returns>
        public static bool PairwiseHitboxCollisionDetection(ICollidable x, ICollidable y)
        {
            return x.Hitbox.Intersects(y.Hitbox);
        }
    }
}

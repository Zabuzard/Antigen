using System;

namespace Antigen.Logic.Collision
{
    [Serializable]
    static class CollisionResponse
    {
        /// <summary>
        /// Determines if there is a collision situation and if so, resolves it.
        /// </summary>
        /// <param name="obj">An object to be tested for collisions.</param>
        /// <param name="mapCollision">Provides map collision detection.</param>
        /// <param name="objectCollision">Provides object collision detection.</param>
        public static void Resolve(ICollidable obj, IMapCollisionContainer mapCollision, IObjectCollisionContainer objectCollision)
        {
            var objCollidable = obj as IObjectCollidable;
            var mapCollidable = obj as IMapCollidable;

            if ((mapCollidable == null || !CollisionDetection.CollidesWithMap(mapCollidable, mapCollision)) &&
                (objCollidable == null ||
                 !CollisionDetection.CollidesWithObject(objCollidable, objectCollision)))
            {
                obj.CollisionInLastTick = false;
                return;
            }

            obj.Position = obj.OldPosition;
            obj.CollisionInLastTick = true;
        }
    }
}

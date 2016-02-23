using System.Collections.Generic;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Cache for objects which have a physical presence
    /// in the game world.
    /// </summary>
    interface ISpatialCache
    {
        /// <summary>
        /// Retrieves all units in an area.
        /// </summary>
        /// <param name="area">The area to search.</param>
        /// <returns>All units which are at least partly contained
        /// in the given area.</returns>
        IEnumerable<Unit> UnitsInArea(Rectangle area);

        /// <summary>
        /// Retrieves all collidable objects in an area.
        /// </summary>
        /// <param name="area">The area to search.</param>
        /// <returns>All collidable objects which are at least partly contained
        /// in the given area.</returns>
        IEnumerable<ICollidable> CollidableObjectsInArea(Rectangle area);
    }
}

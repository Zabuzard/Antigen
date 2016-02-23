using Microsoft.Xna.Framework;

namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Contains information about blocking tiles on the map.
    /// </summary>
    interface IMapCollisionContainer
    {
        /// <summary>
        /// The width of the map.
        /// </summary>
        int GetWidth();

        /// <summary>
        /// The height of the map.
        /// </summary>
        int GetHeight();

        /// <summary>
        /// Returns if there is a blocking element in the rectangle section of the map.
        /// </summary>
        /// <param name="rect">Rectangle that determites the mapsection</param>
        /// <returns>True if there is a blocking element in the mapsection</returns>
        bool IsBlocking(Rectangle rect);
    }
}

using Antigen.Graphics;
using Microsoft.Xna.Framework;

namespace Antigen.Objects
{
    /// <summary>
    /// Interface for moveable objects.
    /// </summary>
    interface IMoveable
    {
        /// <summary>
        /// Moves the object to the given point on the map.
        /// </summary>
        /// <param name="point">Destination coordinates</param>
        /// <param name="gameTime">Current game time</param>
// ReSharper disable UnusedMemberInSuper.Global
        void MoveTo(Coord<Point> point, GameTime gameTime);
// ReSharper restore UnusedMemberInSuper.Global
    }
}

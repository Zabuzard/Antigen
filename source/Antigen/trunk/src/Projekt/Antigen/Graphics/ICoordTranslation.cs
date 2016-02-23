using Microsoft.Xna.Framework;

namespace Antigen.Graphics
{
    /// <summary>
    /// Infterface for objects that can translate relative into absolute coordinates.
    /// </summary>
    interface ICoordTranslation
    {
        /// <summary>
        /// Translates a relative coordinate into an absolute.
        /// </summary>
        /// <param name="coords">Coordinate to translate</param>
        /// <returns>Absolute coordinate</returns>
        Point ToAbsolute(Point coords);
    }
}

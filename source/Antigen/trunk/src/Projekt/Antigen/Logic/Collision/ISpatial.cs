using Microsoft.Xna.Framework;

namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Interface for objects that have a physical presence in the
    /// game world.
    /// </summary>
    interface ISpatial
    {
        /// <summary>
        /// The object's position in the game world.
        /// </summary>
        Vector2 Position { get; set; }
        /// <summary>
        /// The object's position in the game world
        /// in the last tick.
        /// </summary>
        Vector2 OldPosition { get; set; }
        /// <summary>
        /// Radius of the minimal circle that contains
        /// the whole object.
        /// </summary>
        int Radius { get; }
        /// <summary>
        /// Minimal rectangle that contains the whole object.
        /// </summary>
        Rectangle Hitbox { get; }
    }
}

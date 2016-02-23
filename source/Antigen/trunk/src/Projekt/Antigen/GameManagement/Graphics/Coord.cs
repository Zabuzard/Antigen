using Microsoft.Xna.Framework;

namespace Antigen.Graphics
{
    /// <summary>
    /// Represents data of type <code>T</code> which uses coordinates. Provides
    /// a view of an element with either absolute or screen-relative coordinates.
    /// 
    /// Note that <code>Absolute</code> and <code>Relative</code> are not the same
    /// object. This class is therefore best used with immutable data objects.
    /// </summary>
    /// <typeparam name="T">Data type which includes coordinates.</typeparam>
    sealed class Coord<T>
    {
        /// <summary>
        /// View of the data with absolute coordinates.
        /// </summary>
// TODO Remove this rule as soon as the member is used.
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public T Absolute { get; private set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global
        /// <summary>
        /// View of the data with relative coordinates.
        /// </summary>
// TODO Remove this rule as soon as the member is used.
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public T Relative { get; private set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Creates a new coordinate object with both absolute and
        /// relative coordinates. <code>absolute</code> and
        /// <code>relative</code> must be identical except for all
        /// fields which contain coordinate values.
        /// </summary>
        /// <param name="absolute">Data with absolute coordinates.</param>
        /// <param name="relative">Data with relative coordinates.</param>
        private Coord(T absolute, T relative)
        {
            Absolute = absolute;
            Relative = relative;
        }

        /// <summary>
        /// Creates a point with both absolute and relative coordinate information
        /// using a coordinate transformation.
        /// </summary>
        /// <param name="relative">The point in terms of screen-relative coordinates.</param>
        /// <param name="trans">The current coordinate transformation.</param>
        /// <returns>The given point with added absolute coordinate information.</returns>
        public static Coord<Point> MakeCoord(Point relative, ICoordTranslation trans)
        {
            return new Coord<Point>(trans.ToAbsolute(relative), relative);
        }
    }
}

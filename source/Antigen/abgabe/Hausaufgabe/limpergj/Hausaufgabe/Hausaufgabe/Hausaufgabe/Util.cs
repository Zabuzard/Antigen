using System;
using Microsoft.Xna.Framework;

namespace Hausaufgabe
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    static class Util
    {
        /// <summary>
        /// Computes the center point of a rectangle.
        /// </summary>
        /// If the width or height of the input rectangle
        /// is uneven, the corresponding coordinate is
        /// rounded towards negative infinity.
        /// <param name="rect">A rectangle.</param>
        /// <returns>The rectangle's center point.</returns>
        public static Point RectangleCenter(Rectangle rect)
        {
            var centerX = rect.X + (rect.Width  / 2);
            var centerY = rect.Y + (rect.Height / 2);
            return new Point(centerX, centerY);
        }

        /// <summary>
        /// Converts a point to a vector.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <returns>The corresponding vector.</returns>
        public static Vector2 PointToVector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Computes the square of maximum size that fits into
        /// a rectangle.
        /// </summary>
        /// If the rectangle is wider than high, the square's sides
        /// are as long as the rectangle is high. Otherwise,
        /// the square's sides are as long as the rectangle is wide.
        /// 
        /// Additionally, the rectangle is centered horizontically or
        /// vertically within the square, depending on whether the
        /// rectangle is wider or higher than the square.
        /// <param name="square">A square.</param>
        /// <returns>The centered maximum rectangle.</returns>
        public static Rectangle MaximumCenteredSquare(Rectangle square)
        {
            var width = square.Width;
            var height = square.Height;

            var squareSize = Math.Min(width, height);
            var xOffset = (width - squareSize) / 2;
            var yOffset = (height - squareSize) / 2;

            return new Rectangle(square.X + xOffset, square.Y + yOffset, squareSize, squareSize);
        }
    }
}

using System;
using Microsoft.Xna.Framework;

namespace Hausaufgabe
{
    public struct Circle
    {
        private readonly Point mCenter;
        private readonly int   mRadius;

        public Circle(Point center, int radius)
        {
            mCenter = center;
            mRadius = radius;
        }

        public static Point PointAtAngle(Circle circle, float angle)
        {
            var x = (int) Math.Round(circle.mCenter.X + circle.mRadius * Math.Cos(angle));
            var y = (int) Math.Round(circle.mCenter.Y + circle.mRadius * Math.Sin(angle));
            return new Point(x, y);
        }

        public static bool CollidesWith(Circle circle, Point point)
        {
            return Vector2.Distance(Util.PointToVector2(circle.mCenter), Util.PointToVector2(point)) <= circle.mRadius;
        }
    }
}

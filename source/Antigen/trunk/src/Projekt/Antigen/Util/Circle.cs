using Microsoft.Xna.Framework;

namespace Antigen.Util
{
    sealed class Circle
    {
        public Color Color { get; private set; }
        public Vector2 Position { get; private set; }
        public double Radius { get; private set; }

        public Circle(Color color, Vector2 position, double radius)
        {
            Color = color;
            Position = position;
            Radius = radius;
        }
    }
}

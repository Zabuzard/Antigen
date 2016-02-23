using System;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Map.Generation
{
    /// <summary>
    /// Class for map generation.
    /// </summary>
    sealed class PerlinCircle
    {
        private readonly PerlinLine mPerlinLine;

        public PerlinCircle(Random random, int valueCount, Range valueRange)
        {
            mPerlinLine = new PerlinLine(random, valueCount, valueRange, Math.PI * 2);
        }

        public Vector2 GetPoint(double radians)
        {
            var value = (float) mPerlinLine.GetValue(radians);
            return new Vector2((float) Math.Cos(radians) * value, (float) Math.Sin(radians) * value);
        }

        public Vector2 GetDirection(double radians)
        {
            var gradientVector = new Vector2((float) mPerlinLine.GetGradient(radians), (float) mPerlinLine.GetValue(radians));
            var rotatedVector = Vector2.Transform(gradientVector, Matrix.CreateRotationZ((float) radians));
            rotatedVector.Normalize();
            return rotatedVector;
        }
    }
}
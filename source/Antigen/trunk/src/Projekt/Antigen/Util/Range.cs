using System;

namespace Antigen.Util
{
    sealed class Range
    {
        private double Min { get; set; }
        private double Max { get; set; }

        public Range(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandomInt(Random random)
        {
            return random.Next((int) Min, (int) Max);
        }

        public double GetRandomDouble(Random random)
        {
            return Min + random.NextDouble() * (Max - Min);
        }
    }
}

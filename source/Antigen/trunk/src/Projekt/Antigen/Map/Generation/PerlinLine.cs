using System;
using Antigen.Util;

namespace Antigen.Map.Generation
{
    sealed class PerlinLine
    {
        private readonly Random mRandom;
        private readonly Range mValueRange;
        private readonly double mLength;
        private readonly double[] mValues;

        public PerlinLine(Random random, int valueCount, Range valueRange, double length)
        {
            mRandom = random;
            mValueRange = valueRange;
            mLength = length;
            mValues = new double[valueCount];
            NewValues();
        }

        private void NewValues()
        {
            for (var i = 0; i < mValues.Length; i++)
            {
                mValues[i] = mValueRange.GetRandomDouble(mRandom);
            }
        }

        public double GetValue(double position)
        {
            var valuePosition = position / mLength % 1 * mValues.Length;
            var weight = Math.Cos(valuePosition % 1 * Math.PI) / 2 + 0.5;
            var value = mValues[(int) valuePosition] * weight +
                        mValues[((int) valuePosition + 1) % mValues.Length] * (1 - weight);
            return value;
        }

        public double GetGradient(double position)
        {
            var valuePosition = position / mLength % 1 * mValues.Length;
            var sinGradient = Math.Sin(valuePosition % 1 * Math.PI) * Math.PI / 2;
            var valueGradient = (mValues[((int)valuePosition + 1) % mValues.Length] - mValues[(int)valuePosition]) / (mLength / mValues.Length);
            var gradient = valueGradient * sinGradient;
            return gradient;
        }
    }
}

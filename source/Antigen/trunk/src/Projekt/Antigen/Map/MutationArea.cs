using System;

namespace Antigen.Map
{
    /// <summary>
    /// Class for mutation areas. Respresents data of a mutation area.
    /// </summary>
    [Serializable]
    sealed class MutationArea
    {
        /// <summary>
        /// Type of a mutation area.
        /// </summary>
        public enum MutationType
        {
            None,
            Average,
            Lifepoints,
            Lifespan,
            AttackPower,
            MovementSpeed,
            CellDivisionRate,
            InfectionResistance,
            Sight
        }

        /// <summary>
        /// Minimal strength of an area.
        /// </summary>
        public const int MinStrength = 0;
        /// <summary>
        /// Maximal strength of an area.
        /// </summary>
        public const int MaxStrength = 30;

        private readonly MutationType mMutationType;
        private int mStrength;

        /// <summary>
        /// Creates a new mutation area with a given type and strength.
        /// </summary>
        /// <param name="thatMutationType">Type of the area</param>
        /// <param name="thatStrength">Strength of the area</param>
        public MutationArea(MutationType thatMutationType, int thatStrength)
        {
            mMutationType = thatMutationType;
            SetStrength(thatStrength);
        }

        /// <summary>
        /// Gets the type of the mutation area.
        /// </summary>
        /// <returns>Type of the mutation area</returns>
        public MutationType GetMutationType()
        {
            return mMutationType;
        }

        /// <summary>
        /// Gets the strength which is between min and max value.
        /// </summary>
        /// <returns>Strength to get</returns>
        public int GetStrength()
        {
            return mStrength;
        }

        /// <summary>
        /// Sets the strengt to the given value and respects min and max values.
        /// </summary>
        /// <param name="value">Value to set strength</param>
        private void SetStrength(int value)
        {
            if (value < MinStrength)
            {
                value = MinStrength;
            } else if (value > MaxStrength)
            {
                value = MaxStrength;
            }
            mStrength = value;
        }
    }
}

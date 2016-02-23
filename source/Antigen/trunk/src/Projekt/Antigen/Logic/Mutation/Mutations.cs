using System;
using Antigen.Map;
using Antigen.Objects.Units.Values;
using Antigen.Util;
using C5;

namespace Antigen.Logic.Mutation
{
    /// <summary>
    /// Various utility methods for mutations
    /// </summary>
    internal static class Mutations
    {
        private static readonly Random sRnd = new Random();

        /// <summary>
        /// Chance to mutate if object is in a mutation area.
        /// </summary>
        private const float MutationChanceInArea = 1f;
        /// <summary>
        /// Chance to mutate if object is not in a mutation area.
        /// </summary>
        private const float MutationChanceOutOfArea = 0.05f;
        /// <summary>
        /// Maximal chance to mutate the strain, applied only if a normal mutation already occurs.
        /// Depending on mutation areas strength.
        /// </summary>
        private const float MaxStrainChance = 0.5f;

        /// <summary>
        /// Size of the range for a maximal changement occuring
        /// if mutation areas strength is maximal.
        /// </summary>
        private const int MutationTableMaxChangementSize = 50;
        /// <summary>
        /// How much the changement range of a value can maximal be
        /// shifted if value corresponds to mutation areas type.
        /// </summary>
        private const int MutationTableMaxChangementShift = 15;
        /// <summary>
        /// How much the x value of function is scaled.
        /// </summary>
        private const float MutAlgoScaleFactor = 0.2f;
        /// <summary>
        /// How much the function is shifted to left on x-axis.
        /// </summary>
        private const float MutAlgoXLeftShift = 1f;

        /// <summary>
        /// If mutation should occur now.
        /// </summary>
        /// <param name="area">Area where unit is in</param>
        /// <returns>True if mutation should occur</returns>
        public static bool ShouldMutationOccur(MutationArea area)
        {
            return (area.GetMutationType() != MutationArea.MutationType.None && sRnd.NextDouble() <= MutationChanceInArea)
                    || (area.GetMutationType() == MutationArea.MutationType.None && sRnd.NextDouble() <= MutationChanceOutOfArea);
        }

        /// <summary>
        /// Mutates a given debuff table using the mutation areas properties.
        /// </summary>
        /// <param name="table">Table to mutate</param>
        /// <param name="area">Area the unit stands in</param>
        /// <returns>Mutated debuff table</returns>
        public static DebuffTable MutateDebuffTable(DebuffTable table, MutationArea area)
        {
            var nextDebuffs = (DebuffTable)table.Clone();
            var changement = false;
            switch (area.GetMutationType())
            {
                case MutationArea.MutationType.Lifepoints :
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.Lifepoints, 1);
                    break;
                case MutationArea.MutationType.Lifespan:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.Lifespan, 1);
                    break;
                case MutationArea.MutationType.AttackPower:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.AttackPower, 1);
                    break;
                case MutationArea.MutationType.MovementSpeed:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.MovementSpeed, 1);
                    break;
                case MutationArea.MutationType.CellDivisionRate:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.CellDivisionRate, 1);
                    break;
                case MutationArea.MutationType.InfectionResistance:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.CellDivisionRate, 1);
                    break;
                case MutationArea.MutationType.Sight:
                    changement = nextDebuffs.ChangeValue(DebuffTable.DebuffValue.CellDivisionRate, 1);
                    break;
            }
            if (changement)
            {
                return nextDebuffs;
            }

            //Try random changement
            var index = sRnd.Next(0, Enum.GetNames(typeof (DebuffTable.DebuffValue)).Length);
            nextDebuffs.ChangeValue((DebuffTable.DebuffValue)index, 1);

            return nextDebuffs;
        }

        /// <summary>
        /// Mutates a given mutation table using the mutation areas properties.
        /// </summary>
        /// <param name="table">Table to mutate</param>
        /// <param name="area">Area the unit stands in</param>
        /// <returns>Mutated mutation table</returns>
        public static MutationTable MutateMutationTable(MutationTable table, MutationArea area)
        {
            var nextMutations = (MutationTable) table.Clone();

            //Calculate size of ranges
            var areaStrengthInPerc = Functions.ValueInPerc(area.GetStrength(), MutationArea.MinStrength, MutationArea.MaxStrength);
            var rangeSize = (int)(MutationTableMaxChangementSize * areaStrengthInPerc);

            //Create range tables for every value where int[0] is lowerBound and int[1] upperBound
            var rangeTable = new HashDictionary<MutationTable.MutationValue, int[]>();
            foreach (var value in Enum.GetValues(typeof(MutationTable.MutationValue)))
            {
                var currentValue = nextMutations.GetValue((MutationTable.MutationValue) value);
                rangeTable.Add((MutationTable.MutationValue)value,
                    new[] { currentValue, currentValue + rangeSize });
            }

            //Shift range tables according to sum of all values
            var sum = nextMutations.SumOfAllValues();
            var positiveAmountPerc = MutAlgoPosAmountPercentage(sum, rangeTable.Count);

            foreach (var keyValuePair in rangeTable)
            {
                var currentRangeSize = keyValuePair.Value[1] - keyValuePair.Value[0];
                var shiftDown = (int) (currentRangeSize - (currentRangeSize * positiveAmountPerc));
                ShiftValueOfRangeTable(keyValuePair.Key, -shiftDown, rangeTable);
            }

            //Shift range table of mutation areas type to favour it
            var areaFavourShift = (int) Math.Round(MutationTableMaxChangementShift * areaStrengthInPerc);
            switch (area.GetMutationType())
            {
                case MutationArea.MutationType.Lifepoints :
                    ShiftValueOfRangeTable(MutationTable.MutationValue.Lifepoints, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.Lifespan:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.Lifespan, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.AttackPower:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.AttackPower, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.MovementSpeed:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.MovementSpeed, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.CellDivisionRate:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.CellDivisionRate, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.InfectionResistance:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.InfectionResistance, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.Sight:
                    ShiftValueOfRangeTable(MutationTable.MutationValue.Sight, areaFavourShift, rangeTable);
                    break;
                case MutationArea.MutationType.Average:
                    var avgShiftAmount = (int)Math.Floor((rangeTable.Count + 0f) / areaFavourShift);
                    foreach (var keyValuePair in rangeTable)
                    {
                        ShiftValueOfRangeTable(keyValuePair.Key, avgShiftAmount, rangeTable);
                    }
                    break;
            }

            //Apply a random value of each range to the mutation table
            foreach (var keyValuePair in rangeTable)
            {
                var nextValue = sRnd.Next(keyValuePair.Value[0], keyValuePair.Value[1] + 1);
                nextMutations.SetValue(keyValuePair.Key, nextValue);
            }

            return nextMutations;
        }

        /// <summary>
        /// Mutates a given strain, can remain the same.
        /// </summary>
        /// <param name="strain">Strain to mutate</param>
        /// <param name="area">Mutation area where object is in</param>
        /// <returns>Mutated strain</returns>
        public static String MutateStrain(String strain, MutationArea area)
        {
            var areaStrengthInPerc = Functions.ValueInPerc(area.GetStrength(), MutationArea.MinStrength, MutationArea.MaxStrength);
            var chance = MaxStrainChance * areaStrengthInPerc;

            return sRnd.Next() <= chance ? StrainGenerator.GetInstance().GenerateStrain() : strain;
        }

        /// <summary>
        /// Shifts a given value of range table by amount.
        /// </summary>
        /// <param name="value">Value to shift range of</param>
        /// <param name="amount">Amount to shift</param>
        /// <param name="rangeTable">RangeTable that contains range of value</param>
        private static void ShiftValueOfRangeTable(MutationTable.MutationValue value, int amount,
            IDictionary<MutationTable.MutationValue, int[]> rangeTable)
        {
            rangeTable[value][0] += amount;
            rangeTable[value][1] += amount;
        }

        /// <summary>
        /// Gets the amount for a positive buff in percentage.
        /// </summary>
        /// <param name="sum">Sum of all values</param>
        /// <param name="amountOfValues">Amount of values</param>
        /// <returns>Amount for a positive buff in percentage</returns>
        private static double MutAlgoPosAmountPercentage(int sum, int amountOfValues)
        {
            const float fullPercentage = 1f;

            double positiveAmountPerc = 0;
            if (sum.Equals(MutationTable.NeutralSingleValue))
            {
                positiveAmountPerc = 0.5f;
            }
            else if (sum < MutationTable.NeutralSingleValue)
            {
                positiveAmountPerc = fullPercentage - MutAlgoInternalFunc(sum, amountOfValues);
            }
            else if (sum > MutationTable.NeutralSingleValue)
            {
                sum *= -1;
                positiveAmountPerc = MutAlgoInternalFunc(sum, amountOfValues);
            }
            return positiveAmountPerc;
        }

        /// <summary>
        /// Internal mutation function from (-infinty, infinty) -> (0, infinity]
        /// being exponential and monotonically increasing where f(0) is 0.5.
        /// Converging to 0 from right to left and to 0 from right to -(amountOfValues * MaxSingleValue).
        /// </summary>
        /// <param name="sum">Sum of all values</param>
        /// <param name="amountOfValues">Amount of values</param>
        /// <returns>Value in (0, 0.5] being exponential and monotonically increasing,
        /// converging to 0 from right to left</returns>
        private static double MutAlgoInternalFunc(int sum, int amountOfValues)
        {
            const float fullPercentage = 1f;
            return Math.Exp(((sum + (amountOfValues * MutationTable.MaxSingleValue * MutAlgoScaleFactor)) /
                    (amountOfValues * MutationTable.MaxSingleValue * MutAlgoScaleFactor))
                    - MutAlgoXLeftShift) * (fullPercentage / 2f);
        }
    }
}

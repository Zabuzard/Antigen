using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;

namespace Antigen.Objects.Units.Values
{
    /// <summary>
    /// The AntibodyBuff containts information which values
    /// an Anitbody will change when in combat with enemies.
    /// Also BCells will have such objects to overgive it to their produced Antibodies.
    /// </summary>
    [Serializable]
    sealed class DebuffTable : ICloneable
    {
        public enum DebuffValue
        {
            Lifepoints = 0,
            Lifespan = 1,
            AttackPower = 2,
            MovementSpeed = 3,
            CellDivisionRate = 4,
            InfectionResistance = 5,
            Sight = 6
        }

        public const int MaxSingleValue = 10;
        private const int MinSingleValue = 0;
        private const int MaxSum = 25;

        private int Lifepoints { get; set; }
        private int Lifespan { get; set; }
        private int AttackPower { get; set; }
        private int MovementSpeed { get; set; }
        private int CellDivisionRate { get; set; }
        private int InfectionResistance { get; set; }
        private int Sight { get; set; }

        /// <summary>
        /// Maximal change of a single value during a debuff
        /// given in percentage of maximal possible value of this.
        /// </summary>
        private const float MaxDebuffChange = 0.75f;

        private readonly List<int> mValueList = new List<int>(Enum.GetNames(typeof(DebuffValue)).Length);

        /// <summary>
        /// Creates a new debuff table with given buff values all being between
        /// MinSingleValue and MaxSingleValue where sum mustn't exceed MaxSum.
        /// </summary>
        /// <param name="thatLifepoints">Lifepoints of the table</param>
        /// <param name="thatLifespan">Lifespan of the table</param>
        /// <param name="thatAttackPower">Attack power of the table</param>
        /// <param name="thatMovementSpeed">Movement speed of the table</param>
        /// <param name="thatCellDivisionRate">Cell division rate of the table</param>
        /// <param name="thatInfectionResistance">Infection resistance of the table</param>
        /// <param name="thatSight">Sight of the table</param>
        private DebuffTable(int thatLifepoints, int thatLifespan, int thatAttackPower,
            int thatMovementSpeed, int thatCellDivisionRate, int thatInfectionResistance, int thatSight)
        {
            Lifepoints = thatLifepoints;
            Lifespan = thatLifespan;
            AttackPower = thatAttackPower;
            MovementSpeed = thatMovementSpeed;
            CellDivisionRate = thatCellDivisionRate;
            InfectionResistance = thatInfectionResistance;
            Sight = thatSight;

            mValueList.Add(Lifepoints);
            mValueList.Add(Lifespan);
            mValueList.Add(AttackPower);
            mValueList.Add(MovementSpeed);
            mValueList.Add(CellDivisionRate);
            mValueList.Add(InfectionResistance);
            mValueList.Add(Sight);

            if (mValueList.Any(value => !IsValidValue(value)))
            {
                throw new InvalidValueException("Value must be between "
                                                    + MinSingleValue + "(inclusive) and " + MaxSingleValue + "(inclusive).");
            }
            if (!IsValidSum(SumOfAllValues()))
            {
                throw new InvalidValueException("Sum of all buff values must not exceed " + MaxSum + ".");
            }
        }

        /// <summary>
        /// Returns if value is in a valid state being
        /// between MinSingleValue and MaxSingleValue both inclusive.
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <returns>If value is valid</returns>
        private static bool IsValidValue(int value)
        {
            return value >= MinSingleValue && value <= MaxSingleValue;
        }

        /// <summary>
        /// Returns if sum of all buff values is in a valid state
        /// not exceeding MaxSum.
        /// </summary>
        /// <param name="sum">Sum to validate</param>
        /// <returns>If sum is valid</returns>
        private static bool IsValidSum(int sum)
        {
            return sum <= MaxSum;
        }

        /// <summary>
        /// Creates an average buff table where each value gets an average value.
        /// </summary>
        /// <returns>Created DebuffTable</returns>
        public static DebuffTable CreateAverageBuff()
        {
            var avg = (int)Math.Floor((MaxSum + 0f) / Enum.GetNames(typeof(DebuffValue)).Length);
            return new DebuffTable(avg, avg, avg, avg, avg, avg, avg);
        }

        /// <summary>
        /// Changes a given value by a given amount and returns if changement was made.
        /// </summary>
        /// <param name="debuffValue">Value to change</param>
        /// <param name="amount">Amount to change value</param>
        /// <returns>True if changement was made, false otherwhise</returns>
        public bool ChangeValue(DebuffValue debuffValue, int amount)
        {
            switch (debuffValue)
            {
                    case DebuffValue.Lifepoints:
                    if (IsValidValue(Lifepoints + amount))
                    {
                        var oldLifepoints = Lifepoints;
                        Lifepoints += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            Lifepoints = oldLifepoints;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.Lifespan:
                    if (IsValidValue(Lifespan + amount))
                    {
                        var oldLifespan = Lifespan;
                        Lifespan += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            Lifespan = oldLifespan;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.AttackPower:
                    if (IsValidValue(AttackPower + amount))
                    {
                        var oldAttackPower = AttackPower;
                        AttackPower += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            AttackPower = oldAttackPower;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.MovementSpeed:
                    if (IsValidValue(MovementSpeed + amount))
                    {
                        var oldMovementSpeed = MovementSpeed;
                        MovementSpeed += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            MovementSpeed = oldMovementSpeed;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.CellDivisionRate:
                    if (IsValidValue(CellDivisionRate + amount))
                    {
                        var oldCellDivisionRate = CellDivisionRate;
                        CellDivisionRate += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            CellDivisionRate = oldCellDivisionRate;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.InfectionResistance:
                    if (IsValidValue(InfectionResistance + amount))
                    {
                        var oldInfectionResistance = InfectionResistance;
                        InfectionResistance += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            InfectionResistance = oldInfectionResistance;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
                    case DebuffValue.Sight:
                    if (IsValidValue(Sight + amount))
                    {
                        var oldSight = Sight;
                        Sight += amount;
                        if (!IsValidSum(SumOfAllValues()))
                        {
                            Sight = oldSight;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Gets the given value or MinSingleValue - 1 if asked value is no valid value.
        /// </summary>
        /// <param name="debuffValue">Value to get</param>
        /// <returns>The value</returns>
        public int GetValue(DebuffValue debuffValue)
        {
            switch (debuffValue)
            {
                case DebuffValue.Lifepoints:
                    return Lifepoints;
                case DebuffValue.Lifespan:
                    return Lifespan;
                case DebuffValue.AttackPower:
                    return AttackPower;
                case DebuffValue.MovementSpeed:
                    return MovementSpeed;
                case DebuffValue.CellDivisionRate:
                    return CellDivisionRate;
                case DebuffValue.InfectionResistance:
                    return InfectionResistance;
                case DebuffValue.Sight:
                    return Sight;
                default :
                    return MinSingleValue - 1;
            }
        }

        /// <inheritdoc />
        public object Clone()
        {
            return new DebuffTable(Lifepoints,
                Lifespan,
                AttackPower,
                MovementSpeed,
                CellDivisionRate,
                InfectionResistance,
                Sight);
        }

        /// <summary>
        /// Returns the sum of all values.
        /// </summary>
        /// <returns>Sum of all values</returns>
        private int SumOfAllValues()
        {
            return mValueList.Sum();
        }

        /// <summary>
        /// Affects the given target with a given table of debuffs.
        /// </summary>
        /// <param name="table">Table of debuffs</param>
        /// <param name="target">Target to debuff</param>
        public static void AffectTargetWithDebuff(DebuffTable table, IDebuffable target)
        {
            //Lifepoints and Lifespan
            var lively = target as ILively;
            if (lively != null)
            {
                const float maxLifepointsChange = ValueStore.MaxLifepoints * MaxDebuffChange;
                float lifepoints = (maxLifepointsChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.Lifepoints);
                lively.ChangeLifepoints(-lifepoints);

                const double maxLifespanChange = ValueStore.MaxLifespan * MaxDebuffChange;
                double lifespan = (maxLifespanChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.Lifespan);
                lively.ChangeLifespan(-lifespan);
            }
            //AttackPower
            var canAttack = target as ICanAttack;
            if (canAttack != null)
            {
                const float maxAttackChange = ValueStore.MaxAttackPower * MaxDebuffChange;
                float attackPower = (maxAttackChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.AttackPower);
                canAttack.ChangeAttackPower(-attackPower);
            }
            //InfectionPower
            var canInfect = target as ICanInfect;
            if (canInfect != null)
            {
                const float maxInfectChange = ValueStore.MaxInfectionPower * MaxDebuffChange;
                var infectionPower = (int)((maxInfectChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.AttackPower));
                canInfect.ChangeInfectionPower(-infectionPower);
            }
            //DeInfectionPower
            var canDeInfect = target as ICanDeInfect;
            if (canDeInfect != null)
            {
                const float maxDeInfectChange = ValueStore.MaxDeInfectionPower * MaxDebuffChange;
                var deInfectionPower = (int)((maxDeInfectChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.AttackPower));
                canDeInfect.ChangeDeInfectionPower(-deInfectionPower);
            }
            //BaseSpeed
            var moveable = target as IHasSpeed;
            if (moveable != null)
            {
                const float maxSpeedChange = ValueStore.MaxBaseSpeed * MaxDebuffChange;
                var speed = (int)((maxSpeedChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.MovementSpeed));
                moveable.ChangeBaseSpeed(-speed);
            }
            //CellDivisionrate
            var canCellDivision = target as ICanCellDivision;
            if (canCellDivision != null)
            {
                const double maxDivisionRateChange = ValueStore.MaxCellDivisionRate * MaxDebuffChange;
                double divisionRate = (maxDivisionRateChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.CellDivisionRate);
                //Negated because logic is reversed
                canCellDivision.ChangeCellDivisionRate(divisionRate);
            }
            //InfectionResistance
            var isInfectable = target as IInfectable;
            if (isInfectable != null)
            {
                const float maxInfectionResistanceChange = ValueStore.MaxInfectionResistance * MaxDebuffChange;
                var infectionResistance = (int)((maxInfectionResistanceChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.InfectionResistance));
                isInfectable.ChangeInfectionResistance(-infectionResistance);
            }
            //Sight
            var hasSight = target as IHasSight;
            if (hasSight != null)
            {
                const float maxSightChange = ValueStore.MaxSight * MaxDebuffChange;
                var sight = (int)((maxSightChange / MaxSingleValue)
                    * table.GetValue(DebuffValue.Sight));
                hasSight.ChangeSight(-sight);
            }
        }
    }
}

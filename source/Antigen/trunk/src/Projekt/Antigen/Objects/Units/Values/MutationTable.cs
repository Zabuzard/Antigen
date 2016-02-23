using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;

namespace Antigen.Objects.Units.Values
{
    /// <summary>
    /// The MutationTable contains information
    /// about buffed values of the object.
    /// </summary>
    [Serializable]
    sealed class MutationTable : ICloneable
    {
        public enum MutationValue
        {
            Lifepoints = 0,
            Lifespan = 1,
            AttackPower = 2,
            MovementSpeed = 3,
            CellDivisionRate = 4,
            InfectionResistance = 5,
            Sight = 6
        }

        public const int MaxSingleValue = 100;
        public const int NeutralSingleValue = 0;
        private const int MinSingleValue = -MaxSingleValue;

        private int Lifepoints { get; set; }
        private int Lifespan { get; set; }
        private int AttackPower { get; set; }
        private int MovementSpeed { get; set; }
        private int CellDivisionRate { get; set; }
        private int InfectionResistance { get; set; }
        private int Sight { get; set; }

        /// <summary>
        /// Maximal change of a single value during a mutation
        /// given in percentage of maximal possible value of this.
        /// </summary>
        private const float MaxMutationChange = 0.5f;

        private readonly List<int> mValueList = new List<int>(Enum.GetNames(typeof(MutationValue)).Length);

        /// <summary>
        /// Creates a new MutationTable with given values.
        /// Where values are between MinSingleValue and MaxSingleValue.
        /// </summary>
        /// <param name="thatLifepoints">Lifepoints of the mutation</param>
        /// <param name="thatLifespan">Lifespan of the mutation</param>
        /// <param name="thatAttackPower">Attack power of the mutation</param>
        /// <param name="thatMovementSpeed">Speed of the mutation</param>
        /// <param name="thatCellDivisionRate">Cell division rate of the mutation</param>
        /// <param name="thatInfectionResistance">Infection resistance of the mutation</param>
        /// <param name="thatSight">Sight of the mutation</param>
        private MutationTable(int thatLifepoints, int thatLifespan, int thatAttackPower,
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
        }

        /// <summary>
        /// Sets a given value to a value and returns if changement was made.
        /// </summary>
        /// <param name="mutationValue">Value to set</param>
        /// <param name="value">Value to set for</param>
        public void SetValue(MutationValue mutationValue, int value)
        {
            if (!IsValidValue(value))
            {
                return;
            }

            switch (mutationValue)
            {
                case MutationValue.Lifepoints:
                    Lifepoints = value;
                    break;
                case MutationValue.Lifespan:
                    Lifespan = value;
                    break;
                case MutationValue.AttackPower:
                    AttackPower = value;
                    break;
                case MutationValue.MovementSpeed:
                    MovementSpeed = value;
                    break;
                case MutationValue.CellDivisionRate:
                    CellDivisionRate = value;
                    break;
                case MutationValue.InfectionResistance:
                    InfectionResistance = value;
                    break;
                case MutationValue.Sight:
                    Sight = value;
                    break;
            }
        }

        /// <summary>
        /// Gets the given value or MinSingleValue - 1 if asked value is no valid value.
        /// </summary>
        /// <param name="debuffValue">Value to get</param>
        /// <returns>The value</returns>
        public int GetValue(MutationValue debuffValue)
        {
            switch (debuffValue)
            {
                case MutationValue.Lifepoints:
                    return Lifepoints;
                case MutationValue.Lifespan:
                    return Lifespan;
                case MutationValue.AttackPower:
                    return AttackPower;
                case MutationValue.MovementSpeed:
                    return MovementSpeed;
                case MutationValue.CellDivisionRate:
                    return CellDivisionRate;
                case MutationValue.InfectionResistance:
                    return InfectionResistance;
                case MutationValue.Sight:
                    return Sight;
                default:
                    return MinSingleValue - 1;
            }
        }

        /// <inheritdoc />
        public object Clone()
        {
            return new MutationTable(Lifepoints,
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
        public int SumOfAllValues()
        {
            return mValueList.Sum();
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
        /// Creates an empty MutationTable where every entry is NeutralSingleValue.
        /// </summary>
        /// <returns>Empty MutationTable</returns>
        public static MutationTable CreateEmptyTable()
        {
            return new MutationTable(NeutralSingleValue, NeutralSingleValue, NeutralSingleValue, NeutralSingleValue,
                NeutralSingleValue, NeutralSingleValue, NeutralSingleValue);
        }

        /// <summary>
        /// Affects a given object with a mutation table using its values.
        /// </summary>
        /// <param name="table">Table to affect object with</param>
        /// <param name="obj">Object to affect</param>
        /// <param name="values">Values of object</param>
        public static void AffectObjectWithMutation(MutationTable table, IMutable obj, ValueStore values)
        {
            //Lifepoints and Lifespan
            var lively = obj as ILively;
            if (lively != null)
            {
                const float maxLifepointsChange = ValueStore.MaxLifepoints * MaxMutationChange;
                float lifepoints = (maxLifepointsChange / MaxSingleValue)
                    * table.GetValue(MutationValue.Lifepoints);
                values.MutationLifepoints = lifepoints;

                const double maxLifespanChange = ValueStore.MaxLifespan * MaxMutationChange;
                double lifespan = (maxLifespanChange / MaxSingleValue)
                    * table.GetValue(MutationValue.Lifespan);
                values.MutationLifespan = lifespan;
            }
            //AttackPower
            var canAttack = obj as ICanAttack;
            if (canAttack != null)
            {
                const float maxAttackChange = ValueStore.MaxAttackPower * MaxMutationChange;
                float attackPower = (maxAttackChange / MaxSingleValue)
                    * table.GetValue(MutationValue.AttackPower);
                values.MutationAttackPower = attackPower;
            }
            //InfectionPower
            var canInfect = obj as ICanInfect;
            if (canInfect != null)
            {
                const float maxInfectChange = ValueStore.MaxInfectionPower * MaxMutationChange;
                var infectionPower = (int)((maxInfectChange / MaxSingleValue)
                    * table.GetValue(MutationValue.AttackPower));
                values.MutationInfectionPower = infectionPower;
            }
            //DeInfectionPower
            var canDeInfect = obj as ICanDeInfect;
            if (canDeInfect != null)
            {
                const float maxDeInfectChange = ValueStore.MaxDeInfectionPower * MaxMutationChange;
                var deInfectionPower = (int)((maxDeInfectChange / MaxSingleValue)
                    * table.GetValue(MutationValue.AttackPower));
                values.MutationDeInfectionPower = deInfectionPower;
            }
            //BaseSpeed
            var moveable = obj as IHasSpeed;
            if (moveable != null)
            {
                const float maxSpeedChange = ValueStore.MaxBaseSpeed * MaxMutationChange;
                var speed = (int)((maxSpeedChange / MaxSingleValue)
                    * table.GetValue(MutationValue.MovementSpeed));
                values.MutationBaseSpeed = speed;
            }
            //CellDivisionrate
            var canCellDivision = obj as ICanCellDivision;
            if (canCellDivision != null)
            {
                const double maxDivisionRateChange = ValueStore.MaxCellDivisionRate * MaxMutationChange;
                double divisionRate = (maxDivisionRateChange / MaxSingleValue)
                    * table.GetValue(MutationValue.CellDivisionRate);
                //Negated because logic is reversed
                values.MutationCellDivisionRate = -divisionRate;
            }
            //InfectionResistance
            var isInfectable = obj as IInfectable;
            if (isInfectable != null)
            {
                const float maxInfectionResistanceChange = ValueStore.MaxInfectionResistance * MaxMutationChange;
                var infectionResistance = (int)((maxInfectionResistanceChange / MaxSingleValue)
                    * table.GetValue(MutationValue.InfectionResistance));
                values.MutationInfectionResistance = infectionResistance;
            }
            //Sight
            var hasSight = obj as IHasSight;
            if (hasSight != null)
            {
                const float maxSightChange = ValueStore.MaxSight * MaxMutationChange;
                var sight = (int)((maxSightChange / MaxSingleValue)
                    * table.GetValue(MutationValue.Sight));
                values.MutationSight = sight;
            }
        }
    }
}

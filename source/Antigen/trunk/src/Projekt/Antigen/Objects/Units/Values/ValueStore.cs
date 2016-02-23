using System;
using Antigen.Settings;
using Antigen.Util;

namespace Antigen.Objects.Units.Values
{
    [Serializable]
    sealed class ValueStore
    {
        private const int MinPseudoRange = 0;
        private const int MaxPseudoRange = 100;
        private const float MinDpsRange = 0.1f;
        private const float MaxDpsRange = 6.5f;
        /// <summary>
        /// Border of a value which is assigned if the maximal value of this is wanted during spawn.
        /// </summary>
        private const float MaxSpawnValueBorder = 0.5f;
        /// <summary>
        /// Amount of pixel that is represented by MinSightValue.
        /// </summary>
        private const float MinSightValueAsPixel = 100;
        /// <summary>
        /// Amount of pixel that is represented by MaxSightValue.
        /// </summary>
        private const float MaxSightValueAsPixel = 500;
        /// <summary>
        /// Minimal value of the BaseSpeed float format.
        /// </summary>
        private const float MinBaseSpeedFloatFormat = 1f;
        /// <summary>
        /// Maximal value of the BaseSpeed float format.
        /// </summary>
        private const float MaxBaseSpeedFloatFormat = 2f;
        /// <summary>
        /// Value of the special mode for BaseSpeed in the float format.
        /// </summary>
        private const float SpecialBaseSpeedFloatFormat = 4f;

        public const int NoValue = int.MinValue;
        private const int MinSpawnValue = 0;
        private const int MaxSpawnValue = 10;
        /// <summary>
        /// Factor that will be applied for (MaxSpawnValue - MinSpawnValue) and
        /// added to the SpawnValue if Difficulty is easy, subtracted if hard.
        /// </summary>
        private const float SpawnDifficultyInfluence = 0.2f;

        /// <summary>
        /// Used for special modes like super fast BaseSpeed.
        /// </summary>
        public const int SpecialSpawnValue = 20;

        public const float MinLifepoints = MinPseudoRange;
        public const float MaxLifepoints = MaxPseudoRange;
        public const float MinLifespan = 0;
        private const double MinLifespanAtSpawn = 60;
        public const double MaxLifespan = 1200;
        private const float MinAttackPower = MinDpsRange;
        public const float MaxAttackPower = MaxDpsRange;
        private const int MinInfectionPower = MinPseudoRange;
        public const int MaxInfectionPower = MaxPseudoRange;
        private const float MinDeInfectionPower = MinDpsRange;
        public const float MaxDeInfectionPower = MaxDpsRange;
        private const int MinBaseSpeed = MinPseudoRange;
        public const int MaxBaseSpeed = MaxPseudoRange;
        /// <summary>
        /// Value of special super fast BaseSpeed mode.
        /// </summary>
        private const int SpecialBaseSpeed = MaxPseudoRange * 2;
        private const double MinCellDivisionRate = 5;
        public const double MaxCellDivisionRate = 150;
        private const int MinInfectionResistance = MinPseudoRange;
        public const int MaxInfectionResistance = MaxPseudoRange;
        private const int MinSight = MinPseudoRange;
        public const int MaxSight = MaxPseudoRange;

        private float Lifepoints { get; set; }
        private double Lifespan { get; set; }
        private float AttackPower { get; set; }
        private int InfectionPower { get; set; }
        private float DeInfectionPower { get; set; }
        private int BaseSpeed { get; set; }
        private double CellDivisionRate { get; set; }
        private int InfectionResistance { get; set; }
        private int Sight { get; set; }

        public float MutationLifepoints { private get; set; }
        public double MutationLifespan { private get; set; }
        public float MutationAttackPower { private get; set; }
        public int MutationInfectionPower { private get; set; }
        public float MutationDeInfectionPower { private get; set; }
        public int MutationBaseSpeed { private get; set; }
        public double MutationCellDivisionRate { private get; set; }
        public int MutationInfectionResistance { private get; set; }
        public int MutationSight { private get; set; }

        /// <summary>
        /// Difficulty of the game. Influences starting values of friendly units.
        /// </summary>
        private static Difficulty sDifficulty = Difficulty.Medium;

        /// <summary>
        /// Creates a new ValueStore object that specifies all values for a GameObject.
        /// ValueStore are created using MinSpawnValue to MaxSpawnValue or NoValue
        /// if object has no such value.
        /// </summary>
        /// <param name="lifepoints">Lifepoints to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="lifespan">Lifespan to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="attackPower">AttackPower to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="infectionPower">InfectionPower to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="deInfectionPower">DeInfectionPower to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="baseSpeed">BaseSpeed to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="cellDivisionRate">CellDivisionRate to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="infectionResistance">InfectionResistance to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="sight">Sight to spawn with between
        /// MinSpawnValue and MaxSpawnValue or NoValue if has no</param>
        /// <param name="side">Side of the unit for this values object</param>
        public ValueStore(int lifepoints, int lifespan, int attackPower,
            int infectionPower, int deInfectionPower, int baseSpeed,
            int cellDivisionRate, int infectionResistance, int sight, Unit.UnitSide side)
        {
            MutationLifepoints = 0;
            MutationLifespan = 0;
            MutationAttackPower = 0;
            MutationInfectionPower = 0;
            MutationDeInfectionPower = 0;
            MutationBaseSpeed = 0;
            MutationCellDivisionRate = 0;
            MutationInfectionResistance = 0;
            MutationSight = 0;

            //Apply difficulty on friendly units
            if (side == Unit.UnitSide.Friendly)
            {
                lifepoints = ApplyDifficultyOnSpawnValue(lifepoints);
                attackPower = ApplyDifficultyOnSpawnValue(attackPower);
                infectionPower = ApplyDifficultyOnSpawnValue(infectionPower);
                deInfectionPower = ApplyDifficultyOnSpawnValue(deInfectionPower);
                cellDivisionRate = ApplyDifficultyOnSpawnValue(cellDivisionRate);
            }

            //Lifepoints
            if (IsSpawnValueValid(lifepoints))
            {
                const float lifepointsPerStep = (MaxLifepoints * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue);
                ChangeLifepoints(MinLifepoints + ((lifepoints - MinSpawnValue) * lifepointsPerStep));
            }
            else
            {
                Lifepoints = NoValue;
            }
            //Lifespan
            if (IsSpawnValueValid(lifespan))
            {
                const double lifespanPerStep = (MaxLifespan * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue);
                ChangeLifespan(MinLifespanAtSpawn + ((lifespan - MinSpawnValue) * lifespanPerStep));
            }
            else
            {
                Lifespan = NoValue;
            }
            //AttackPower
            if (IsSpawnValueValid(attackPower))
            {
                const float attackPowerPerStep = (MaxAttackPower * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue);
                ChangeAttackPower(MinAttackPower + ((attackPower - MinSpawnValue) * attackPowerPerStep));
            }
            else
            {
                AttackPower = NoValue;
            }
            //InfectionPower
            if (IsSpawnValueValid(infectionPower))
            {
                const int infectionPowerPerStep = (int)((MaxInfectionPower * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue));
                ChangeInfectionPower(MinInfectionPower + ((infectionPower - MinSpawnValue) * infectionPowerPerStep));
            }
            else
            {
                InfectionPower = NoValue;
            }
            //DeInfectionPower
            if (IsSpawnValueValid(deInfectionPower))
            {
                const float deInfectionPowerPerStep = (MaxDeInfectionPower * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue);
                ChangeDeInfectionPower(MinDeInfectionPower + ((deInfectionPower - MinSpawnValue) * deInfectionPowerPerStep));
            }
            else
            {
                DeInfectionPower = NoValue;
            }
            //BaseSpeed
            if (IsSpawnValueValid(baseSpeed))
            {
                const int baseSpeedPerStep = (int)((MaxBaseSpeed * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue));
                ChangeBaseSpeed(MinBaseSpeed + ((baseSpeed - MinSpawnValue) * baseSpeedPerStep));
            }
            else if (baseSpeed == SpecialSpawnValue)
            {
                BaseSpeed = SpecialBaseSpeed;
            }
            else
            {
                BaseSpeed = NoValue;
            }
            //CellDivisionRate
            if (IsSpawnValueValid(cellDivisionRate))
            {
                const double cellDivisionRatePerStep = (MaxCellDivisionRate * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue);
                //Negated because logic is reversed
                ChangeCellDivisionRate(MaxCellDivisionRate - ((cellDivisionRate - MinSpawnValue) * cellDivisionRatePerStep));
            }
            else
            {
                CellDivisionRate = NoValue;
            }
            //InfectionResistance
            if (IsSpawnValueValid(infectionResistance))
            {
                const int infectionResistancePerStep = (int)((MaxInfectionResistance * MaxSpawnValueBorder)
                    / (MaxSpawnValue - MinSpawnValue));
                ChangeInfectionResistance(MinInfectionResistance + ((infectionResistance - MinSpawnValue) * infectionResistancePerStep));
            }
            else
            {
                InfectionResistance = NoValue;
            }
            //Sight
            if (IsSpawnValueValid(sight))
            {
                const int sightPerStep = (int)((MaxSight * MaxSpawnValueBorder) / (MaxSpawnValue - MinSpawnValue));
                ChangeSight(MinSight + ((sight - MinSpawnValue) * sightPerStep));
            }
            else
            {
                Sight = NoValue;
            }
        }

        /// <summary>
        /// Changes the lifepoints by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeLifepoints(float amount)
        {
            var nextLifepoints = Lifepoints + amount;
            Lifepoints = nextLifepoints;
        }

        /// <summary>
        /// Gets the lifepoints.
        /// </summary>
        /// <returns>The lifepoints</returns>
        public float GetLifepoints()
        {
            var lifepoints = Lifepoints + MutationLifepoints;
            if (lifepoints < MinLifepoints)
            {
                return MinLifepoints;
            }
            if (lifepoints > MaxLifepoints)
            {
                return MaxLifepoints;
            }
            return lifepoints;
        }

        /// <summary>
        /// Changes the lifespan by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeLifespan(double amount)
        {
            var nextLifespan = Lifespan + amount;
            Lifespan = nextLifespan;
        }

        /// <summary>
        /// Gets the lifespan.
        /// </summary>
        /// <returns>The lifespan</returns>
        public double GetLifespan()
        {
            var lifespan = Lifespan + MutationLifespan;
            if (lifespan < MinLifespan)
            {
                return MinLifespan;
            }
            if (lifespan > MaxLifespan)
            {
                return MaxLifespan;
            }
            return lifespan;
        }

        /// <summary>
        /// Changes the attack power by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeAttackPower(float amount)
        {
            var nextAttackPower = AttackPower + amount;
            AttackPower = nextAttackPower;
        }

        /// <summary>
        /// Gets the attack power.
        /// </summary>
        /// <returns>The attack power</returns>
        public float GetAttackPower()
        {
            var attackPower = AttackPower + MutationAttackPower;
            if (attackPower < MinAttackPower)
            {
                return MinAttackPower;
            }
            if (attackPower > MaxAttackPower)
            {
                return MaxAttackPower;
            }
            return attackPower;
        }

        /// <summary>
        /// Changes the infection power by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeInfectionPower(int amount)
        {
            var nextInfectionPower = InfectionPower + amount;
            InfectionPower = nextInfectionPower;
        }

        /// <summary>
        /// Gets the infection power.
        /// </summary>
        /// <returns>The infection power</returns>
        public int GetInfectionPower()
        {
            var infectionPower = InfectionPower + MutationInfectionPower;
            if (infectionPower < MinInfectionPower)
            {
                return MinInfectionPower;
            }
            if (infectionPower > MaxInfectionPower)
            {
                return MaxInfectionPower;
            }
            return infectionPower;
        }

        /// <summary>
        /// Changes the de infection power by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeDeInfectionPower(float amount)
        {
            var nextDeInfectionPower = DeInfectionPower + amount;
            DeInfectionPower = nextDeInfectionPower;
        }

        /// <summary>
        /// Gets the deinfection power.
        /// </summary>
        /// <returns>The deinfection power</returns>
        public float GetDeInfectionPower()
        {
            var deInfectionPower = DeInfectionPower + MutationDeInfectionPower;
            if (deInfectionPower < MinDeInfectionPower)
            {
                return MinDeInfectionPower;
            }
            if (deInfectionPower > MaxDeInfectionPower)
            {
                return MaxDeInfectionPower;
            }
            return deInfectionPower;
        }

        /// <summary>
        /// Changes the base speed by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeBaseSpeed(int amount)
        {
            var nextBaseSpeed = BaseSpeed + amount;
            BaseSpeed = nextBaseSpeed;
        }

        /// <summary>
        /// Gets the base speed.
        /// </summary>
        /// <returns>The base speed</returns>
        public int GetBaseSpeed()
        {
            var baseSpeed = BaseSpeed + MutationBaseSpeed;
            if (baseSpeed < MinBaseSpeed)
            {
                return MinBaseSpeed;
            }
            if (baseSpeed > MaxBaseSpeed)
            {
                return MaxBaseSpeed;
            }
            return baseSpeed;
        }

        /// <summary>
        /// Changes the cell division rate by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeCellDivisionRate(double amount)
        {
            var nextCellDivisionRate = CellDivisionRate + amount;
            CellDivisionRate = nextCellDivisionRate;
        }

        /// <summary>
        /// Gets the cell division rate.
        /// </summary>
        /// <returns>The cell division rate</returns>
        public double GetCellDivisionRate()
        {
            var cellDivisionRate = CellDivisionRate + MutationCellDivisionRate;
            if (cellDivisionRate < MinCellDivisionRate)
            {
                return MinCellDivisionRate;
            }
            if (cellDivisionRate > MaxCellDivisionRate)
            {
                return MaxCellDivisionRate;
            }
            return cellDivisionRate;
        }

        /// <summary>
        /// Changes the infection resistance by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeInfectionResistance(int amount)
        {
            var nextInfectionResistance = InfectionResistance + amount;
            InfectionResistance = nextInfectionResistance;
        }

        /// <summary>
        /// Gets the infection resistance.
        /// </summary>
        /// <returns>The infection resistance</returns>
        public int GetInfectionResistance()
        {
            var infectionResistance = InfectionResistance + MutationInfectionResistance;
            if (infectionResistance < MinInfectionResistance)
            {
                return MinInfectionResistance;
            }
            if (infectionResistance > MaxInfectionResistance)
            {
                return MaxInfectionResistance;
            }
            return infectionResistance;
        }

        /// <summary>
        /// Changes the sight by given amount.
        /// </summary>
        /// <param name="amount">Amount to change</param>
        public void ChangeSight(int amount)
        {
            var nextSight = Sight + amount;
            Sight = nextSight;
        }
// ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Gets the sight.
        /// </summary>
        /// <returns>The sight</returns>
        public int GetSight()
        {
            var sight = Sight + MutationSight;
            if (sight < MinSight)
            {
                return MinSight;
            }
            if (sight > MaxSight)
            {
                return MaxSight;
            }
            return sight;
        }

        /// <summary>
        /// Applies the current setted diffulty to a spawn value using SpawnDifficultyInfluence.
        /// </summary>
        /// <param name="value">Value to apply difficulty on it</param>
        /// <returns>Value with difficulty applied</returns>
        private int ApplyDifficultyOnSpawnValue(int value)
        {
            if (value.Equals(NoValue))
            {
                return value;
            }

            int nextValue = value;
            switch (sDifficulty)
            {
                case Difficulty.Easy:
                    nextValue = value + (int) Math.Ceiling((MaxSpawnValue - MinSpawnValue) * SpawnDifficultyInfluence);
                    break;
                case Difficulty.Hard:
                    nextValue = value - (int) Math.Ceiling((MaxSpawnValue - MinSpawnValue) * SpawnDifficultyInfluence);
                    break;
            }
            if (nextValue < MinSpawnValue)
            {
                nextValue = MinSpawnValue;
            }
            else if (nextValue > MaxSpawnValue)
            {
                nextValue = MaxSpawnValue;
            }
            return nextValue;
        }

        /// <summary>
        /// Converts the given sight to an absolute pixel range.
        /// </summary>
        /// <param name="sight">Sight to convert</param>
        /// <returns>Converted sight in pixel</returns>
        public static int ConvertSightToPixel(int sight)
        {
            return (int)((Functions.ValueInPerc(sight, MinSight, MaxSight) * (MaxSightValueAsPixel - MinSightValueAsPixel))
                + MinSightValueAsPixel);
        }

        /// <summary>
        /// Sets the difficulty which influences starting values of friendly units.
        /// </summary>
        /// <param name="thatDifficulty">Difficulty to set</param>
        public static void SetDifficulty(Difficulty thatDifficulty)
        {
            sDifficulty = thatDifficulty;
        }

        /// <summary>
        /// Converts the given baseSpeed value to the float format which is used by movement.
        /// </summary>
        /// <param name="baseSpeed">BaseSpeed to convert</param>
        /// <returns>Converted speed in the float format</returns>
        public static float ConvertBaseSpeedToFloatFormat(int baseSpeed)
        {
            if (baseSpeed.Equals(SpecialBaseSpeed))
            {
                return SpecialBaseSpeedFloatFormat;
            }
            var speedPerc = Functions.ValueInPerc(baseSpeed, MinBaseSpeed, MaxBaseSpeed);
            return speedPerc * (MaxBaseSpeedFloatFormat - MinBaseSpeedFloatFormat) + MinBaseSpeedFloatFormat;
        }

        /// <summary>
        /// If the spawn value is valid. Meaning between the bounds and not NoValue.
        /// Does not consider special modes.
        /// </summary>
        /// <param name="spawnValue">Value to validate</param>
        /// <returns>True if value is valid, false otherwhise</returns>
        private static bool IsSpawnValueValid(int spawnValue)
        {
            return spawnValue != NoValue && spawnValue >= MinSpawnValue && spawnValue <= MaxSpawnValue;
        }
    }
}

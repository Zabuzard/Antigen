using System;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Sound;
using Antigen.Statistics;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Offensive.Attack
{
    /// <summary>
    /// Implements a basic attack behavior for units.
    /// Can attack untis with another.
    /// </summary>
    [Serializable]
    sealed class AttackBehavior : IAttackBehavior
    {
        /// <summary>
        /// Object to attack with.
        /// </summary>
        private readonly ICanAttack mObj;
        [NonSerialized]
        private readonly SoundWrapper mSoundWrapper;
        private readonly IStatisticIncrementer mStats;

        /// <inheritdoc />
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// Creates a new basic attack behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to attack with</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        public AttackBehavior(ICanAttack obj, SoundWrapper soundWrapper, IStatisticIncrementer stats)
        {
            mStats = stats;
            mSoundWrapper = soundWrapper;
            mObj = obj;
            IsAttacking = false;
        }

        /// <inheritdoc />
        public void Attack(IAttackable target, GameTime time)
        {
            if (!Functions.IsOffensiveConstellation((Unit)mObj, (Unit)target, false))
            {
                return;
            }

            IsAttacking = true;

            var lively = target as ILively;
            if (lively != null)
            {
                var lifepointLoss = (float)(mObj.GetAttackPower() * time.ElapsedGameTime.TotalSeconds);
                lively.ChangeLifepoints(-lifepointLoss);

                //Play sound when unit attacks
                var unit = mObj as Unit;
                if (unit.GetSide() == Unit.UnitSide.Friendly && mSoundWrapper != null)
                {
                    mSoundWrapper.PlayEffect(Effect.Attack, unit.Position);
                }

                //Takeover strain as antigen if target is destroyed
                if (!lively.IsAlive() && lively is IHasStrain && mObj is IHasAntigen)
                {
                    ((IHasAntigen) mObj).Antigen = ((IHasStrain) lively).Strain;

                    //Increment collected antigen statistic
                    if (unit.GetSide() == Unit.UnitSide.Friendly)
                    {
                        mStats.IncrementStatistic(StatName.Collected_Antigens);
                    }
                }
            }

            IsAttacking = false;
        }
    }
}

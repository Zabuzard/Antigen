using System;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Sound;
using Antigen.Statistics;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Offensive
{
    /// <summary>
    /// Implements a offensive behavior for units which can attack, debuff and deinfect.
    /// </summary>
    [Serializable]
    sealed class OffensiveBehavior
    {
        /// <summary>
        /// All offensive modes.
        /// </summary>
        public enum OffensiveMode
        {
            Attack,
            Debuff,
            DeInfection
        }

        private readonly OffensiveMode mMode;
        private readonly IAttackBehavior mAttackBehavior;
        private readonly IDebuffBehavior mDebuffBehavior;
        private readonly IDeInfectionBehavior mDeInfectionBehavior;

        /// <summary>
        /// Creates a new basic offensive behavior that can attack others.
        /// </summary>
        /// <param name="obj">Object to attack with</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        public OffensiveBehavior(ICanOffensive obj, SoundWrapper soundWrapper, IStatisticIncrementer stats)
        {
            var canAttack = obj as ICanAttack;
            var canDebuff = obj as ICanDebuff;
            var canDeInfect = obj as ICanDeInfect;
            if (canAttack != null)
            {
                mMode = OffensiveMode.Attack;
                mAttackBehavior = new AttackBehavior(canAttack, soundWrapper, stats);
            }
            else if (canDebuff != null)
            {
                mMode = OffensiveMode.Debuff;
                mDebuffBehavior = new DebuffBehavior(canDebuff, soundWrapper);
            }
            else if (canDeInfect != null)
            {
                mMode = OffensiveMode.DeInfection;
                mDeInfectionBehavior = new DeInfectionBehavior(canDeInfect, soundWrapper);
            }
        }

        /// <inheritdoc />
        public void OffensiveAction(IOffensiveable target, GameTime gameTime)
        {
            switch (mMode)
            {
                    case OffensiveMode.Attack:
                        var attackable = target as IAttackable;
                        if (attackable != null)
                        {
                            mAttackBehavior.Attack(attackable, gameTime);
                        }
                        break;
                    case OffensiveMode.Debuff:
                        var debuffable = target as IDebuffable;
                        if (debuffable != null)
                        {
                            mDebuffBehavior.Debuff(debuffable);
                        }
                        break;
                    case OffensiveMode.DeInfection:
                        var canInfect = target as ICanInfect;
                        if (canInfect != null)
                        {
                            mDeInfectionBehavior.Deinfect(canInfect, gameTime);
                        }
                        break;
            }
        }

        /// <summary>
        /// Returns if the object is currently doing offensive actions to other objects.
        /// </summary>
        public bool IsOffensive()
        {
            switch (mMode)
            {
                case OffensiveMode.Attack:
                    return mAttackBehavior.IsAttacking;
                case OffensiveMode.Debuff:
                    return mDebuffBehavior.IsDebuffing;
                case OffensiveMode.DeInfection:
                    return mDeInfectionBehavior.IsDeinfecting;
            }
            return false;
        }
    }
}

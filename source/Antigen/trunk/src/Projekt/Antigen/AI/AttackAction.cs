using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive.Attack;
using Antigen.Objects;
using Antigen.Objects.Units;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    [Serializable]
    sealed class AttackAction: IAiAction
    {
        private Unit mTarget;
        private Unit mLastTarget;
        private readonly IAttackBehavior mAttackBehavior;
        private readonly IMoveBehavior mMoveBehavior;
        private readonly Unit mUnit;
        private readonly Dictionary<DecisionPoints, float> mPoints;
        private double mAttackTimer;
        private const int MinAttackDist = 15;
        private const double MaxAttackTimer = 1;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveBehavior"></param>
        /// <param name="attackBehavior"></param>
        /// <param name="unit"></param>
        /// <param name="points"></param>
        public AttackAction(IMoveBehavior moveBehavior, IAttackBehavior attackBehavior, Unit unit, Dictionary<DecisionPoints, float> points)
        {
            mMoveBehavior = moveBehavior;
            mAttackBehavior = attackBehavior;
            mUnit = unit;
            mPoints = points;
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            var result = mPoints[DecisionPoints.AttackBasic];
            if (lastAction is AttackAction) result += mPoints[DecisionPoints.AttackActionAgain];
            var enemyCount = units.Count(unit => unit.GetSide() == Unit.UnitSide.Enemy);
            result += enemyCount * mPoints[DecisionPoints.AttackPointsPerEnemy];
            result -= (float) mAttackTimer * mPoints[DecisionPoints.AttackDuration];
            result -= units.Count(unit => unit is ICanAttack && unit.GetSide() == Unit.UnitSide.Friendly) * mPoints[DecisionPoints.AttackSubPointsPerFriendly];
            if (units.TrueForAll(unit => unit.GetSide() == Unit.UnitSide.Enemy || unit == mLastTarget)) result = 0;
            return result;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            var result = 0;
            mAttackTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (!(lastAction is AttackAction)) mLastTarget = null;
            if (mTarget == null || !units.Contains(mTarget) || mAttackTimer > MaxAttackTimer)
            {
                mLastTarget = mTarget ?? mLastTarget;
                mTarget = units.Where(unit => unit != mLastTarget && unit.GetSide() != Unit.UnitSide.Enemy).OrderBy(
                    unit => Vector2.Distance(mUnit.Position, unit.Position)).FirstOrDefault();
                mAttackTimer = 0;
            }
            else
            {
                bool closeEnough = true;
                var offensiveGameObject = mUnit as GameObject;
                var targetGameObject = mTarget as GameObject;
                if (targetGameObject != null && offensiveGameObject != null)
                {
                    var dist = targetGameObject.Position.Distance(offensiveGameObject.Position);
                    dist -= (targetGameObject.Radius + offensiveGameObject.Radius);
                    closeEnough = dist <= MinAttackDist;
                }

                if (!closeEnough) mMoveBehavior.MoveTo(mTarget.Position);
                if (closeEnough)
                {
                    mAttackBehavior.Attack(mTarget, gameTime);
                    mAttackTimer = 0;
                    if (!mTarget.IsAlive()) result = 1;
                }
            }
            return result;
        }
    }
}

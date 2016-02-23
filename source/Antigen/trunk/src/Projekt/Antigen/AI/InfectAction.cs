using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects;
using Antigen.Objects.Units;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    [Serializable]
    sealed class InfectAction: IAiAction
    {
        private Unit mTarget;
        private readonly Unit mUnit;
        private readonly Dictionary<DecisionPoints, float> mPoints;
        private readonly IInfectionBehavior mInfectionBehavior;
        private readonly IMoveBehavior mMoveBehavior;
        private const int MinAttackDist = 15;

        public InfectAction(IInfectionBehavior infectionBehavior, IMoveBehavior moveBehavior, Unit unit, Dictionary<DecisionPoints, float> points)
        {
            mInfectionBehavior = infectionBehavior;
            mMoveBehavior = moveBehavior;
            mUnit = unit;
            mPoints = points;
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            var result = mPoints[DecisionPoints.InfectBasic];
            if (lastAction is InfectAction) result += mPoints[DecisionPoints.InfectActionAgain];
            if (units.Any(unit => unit.GetSide() != Unit.UnitSide.Enemy && ((IInfectable) unit).GetInfectionResistance() < ((Virus) mUnit).GetInfectionPower()))
                result += 10;
            else result = 0;
            return result;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            var result = 0;
            if (mTarget == null || !units.Contains(mTarget))
            {
                mTarget = units.Where(unit => unit is IInfectable && ((IInfectable)unit).GetInfectionResistance() < ((Virus)mUnit).GetInfectionPower()).OrderBy(unit => Vector2.Distance(mUnit.Position, unit.Position)).FirstOrDefault();
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

                if (!closeEnough)
                    mMoveBehavior.MoveTo(mTarget.Position);
                else
                {
                    mInfectionBehavior.Infect((IInfectable)mTarget);
                    result = 1;
                }            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.CellDivision;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    [Serializable]
    sealed class DivisionAction: IAiAction
    {
        private readonly Unit mUnit;
        private readonly IDivisionBehavior mDivisionBehavior;
        private readonly IDivisionContentLoader mContentLoader;
        private readonly Dictionary<DecisionPoints, float> mPoints;
        private readonly AiBrain mAiBrain;
        private double mDivisionTimer;

        public DivisionAction(Unit unit, IDivisionBehavior divisionBehavior, IDivisionContentLoader contentLoader, Dictionary<DecisionPoints, float> points, AiBrain aiBrain)
        {
            mUnit = unit;
            mDivisionBehavior = divisionBehavior;
            mContentLoader = contentLoader;
            mPoints = points;
            mAiBrain = aiBrain;
            mDivisionTimer = ((ICanCellDivision) mUnit).GetCellDivisionRate();
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            var result = mPoints[DecisionPoints.DivisionBasic];
            if (units.All(unit => unit.GetSide() == Unit.UnitSide.Enemy)) result += mPoints[DecisionPoints.DivisionOnlyEnemies];                             
            if (lastAction is DivisionAction) result += mPoints[DecisionPoints.DivisionActionAgain];
            result += (float) (1 - mDivisionTimer / ((ICanCellDivision) mUnit).GetCellDivisionRate()) * mPoints[DecisionPoints.DivisionRemainingTime];
            result += 1f / mAiBrain.GetEmemiesCount() * mPoints[DecisionPoints.DivisionEnemiesCount];
            return result;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            var result = 0;
            if (!(lastAction is DivisionAction)) mDivisionTimer = ((ICanCellDivision)mUnit).GetCellDivisionRate();
            if (mDivisionTimer > 0)
            {
                mDivisionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                mDivisionBehavior.DivideCell((ICanCellDivision)mUnit, mContentLoader);
                mDivisionTimer = ((ICanCellDivision)mUnit).GetCellDivisionRate();
                result = 1;
            }
            return result;
        }

        /// <summary>
        /// Gets the remaining duration until the cell division is finished.
        /// If not cell dividing this is equals GetCellDivisionRate().
        /// Duration is in seconds.
        /// </summary>
        /// <returns>Remaining duration until the division is completed in seconds</returns>
        public double GetRemainingCellDivisionDuration()
        {
            return mDivisionTimer;
        }
    }
}
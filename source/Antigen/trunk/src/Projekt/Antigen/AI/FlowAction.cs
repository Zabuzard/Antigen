using System;
using System.Collections.Generic;
using Antigen.Logic.Movement;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    [Serializable]
    sealed class FlowAction: IAiAction
    {
        private readonly IMoveBehavior mFlowBehavior;
        private readonly Dictionary<DecisionPoints, float> mPoints;

        public FlowAction(IMoveBehavior flowBehavior, Dictionary<DecisionPoints, float> points)
        {
            mFlowBehavior = flowBehavior;
            mPoints = points;
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            var result = mPoints[DecisionPoints.FlowBasic];
            return result;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            mFlowBehavior.Wander();
            return 0;
        }
    }
}

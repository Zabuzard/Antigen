using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.Collision;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Microsoft.Xna.Framework;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.AI
{
    /// <summary>
    /// The AI control object
    /// </summary>
    [Serializable]
    sealed class BasicAiControl: IUpdateable
    {
        private readonly ISpatialCache mSpatialCache;
        private readonly Unit mUnit;
        private readonly List<IAiAction> mActionList;
        private IAiAction mLastAction;
        private double mActionTimer;
        private float[] mPoints;
        private float mScore;
        private readonly AiBrain mAiBrain;
        private readonly Dictionary<DecisionPoints, float> mDecisionPoints;
        private const int SecondsPerDecision = 1;

        /// <summary>
        /// Creates a new AI control object
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="spatialCache"></param>
        /// <param name="actionList"></param>
        /// <param name="aiBrain"></param>
        /// <param name="decisionPoints"></param>
        public BasicAiControl(Unit unit, ISpatialCache spatialCache, List<IAiAction> actionList, AiBrain aiBrain, Dictionary<DecisionPoints, float> decisionPoints)
        {
            mUnit = unit;
            mSpatialCache = spatialCache;
            mActionList = actionList;
            mAiBrain = aiBrain;
            mDecisionPoints = decisionPoints;
        }

        private IAiAction ChooseActionMax(List<Unit> units)
        {
            mPoints = new float[mActionList.Count];
            for (var i = 0; i < mActionList.Count; i++)
            {
                mPoints[i] = mActionList[i].GetPoints(mLastAction, new List<Unit>(units));
            }
            var index = Array.IndexOf(mPoints, mPoints.Max());
            return mActionList[index];
        }

        public IAiAction GetLastAction()
        {
            return mLastAction;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            var sight = ValueStore.ConvertSightToPixel(mUnit.GetSight());
            var area = new Rectangle(
                (int)(mUnit.Position.X - sight),
                (int)(mUnit.Position.Y - sight),
                sight * 2,
                sight * 2);

            var list = mSpatialCache.UnitsInArea(area).ToList();
            mAiBrain.AddFriendlyUnits(list.Where(unit => unit.GetSide() != Unit.UnitSide.Enemy));
            mActionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            var action = mLastAction;
            if (mActionTimer < 0)
            {
                mActionTimer = SecondsPerDecision;
                action = ChooseActionMax(new List<Unit>(list));
            }
            var score = action.DoAction(list, mLastAction, gameTime);
            if (score > 0)
            {
                mScore += score;
                mAiBrain.AmITheBest(mScore, mDecisionPoints);
            }
            mLastAction = action;
        }
    }
}

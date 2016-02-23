using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Objects;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.AI
{
    enum DecisionPoints
    {
        AttackBasic,
        AttackActionAgain,
        AttackPointsPerEnemy,
        AttackDuration,
        AttackSubPointsPerFriendly,
        DivisionBasic,
        DivisionOnlyEnemies,
        DivisionActionAgain,
        DivisionRemainingTime,
        DivisionEnemiesCount,
        EscapeBasic,
        EscapeHasAntigen,
        FlowBasic,
        InfectBasic,
        InfectActionAgain
    }

    [Serializable]
    sealed class AiBrain: IUpdateable
    {
        private readonly ObjectCaches mObjectCaches;
        private ISet<Unit> mFriendlyUnits;
        private Dictionary<DecisionPoints, float> mBestPoints;
        private float mBestScore;
        private static readonly Random sRandom = new Random();

        public AiBrain(ObjectCaches objectCaches)
        {
            mObjectCaches = objectCaches;
            mBestPoints = new Dictionary<DecisionPoints, float>();
            foreach (var decisionPoints in Enum.GetValues(typeof(DecisionPoints)).Cast<DecisionPoints>())
            {
                mBestPoints.Add(decisionPoints, 1);
            }
            mBestPoints[DecisionPoints.DivisionEnemiesCount] = 30;
        }

        public int GetEmemiesCount()
        {
            return mObjectCaches.ListEnemy.Count;
        }

        public void AddFriendlyUnits(IEnumerable<Unit> friendlyUnits)
        {
            mFriendlyUnits.UnionWith(friendlyUnits);
        }

        public void Update(GameTime gameTime)
        {
            mFriendlyUnits = new HashSet<Unit>();
        }

        public void AmITheBest(float score, Dictionary<DecisionPoints, float> decisionPoints)
        {
            if (score > mBestScore)
            {
                mBestScore = score;
                mBestPoints = decisionPoints;
            }
        }

        public Dictionary<DecisionPoints, float> GetDecisionPoints()
        {
            var points = mBestPoints.ToDictionary(point => point.Key, point => point.Value * (float) (1 + (sRandom.NextDouble() - 0.5) / 10));
            return points;
        }
    }
}

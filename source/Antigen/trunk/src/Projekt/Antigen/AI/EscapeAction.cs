using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Logic.Movement;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    [Serializable]
    sealed class EscapeAction: IAiAction
    {
        private readonly IMoveBehavior mMoveBehavior;
        private readonly Unit mUnit;
        private readonly Dictionary<DecisionPoints, float> mPoints;

        public EscapeAction(IMoveBehavior moveBehavior, Unit unit, Dictionary<DecisionPoints, float> points)
        {
            mMoveBehavior = moveBehavior;
            mUnit = unit;
            mPoints = points;
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            var result = mPoints[DecisionPoints.EscapeBasic];
            if (units.OfType<IHasAntigen>().Any(unit => unit.Antigen == ((IHasStrain) mUnit).Strain)) result += mPoints[DecisionPoints.EscapeHasAntigen];
            return result;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            if (units.All(unit => unit.GetSide() != Unit.UnitSide.Friendly)) return 0;
            var posX = units.Where(unit => unit.GetSide() != Unit.UnitSide.Friendly).Average(unit => unit.Position.X);
            var posY = units.Where(unit => unit.GetSide() != Unit.UnitSide.Friendly).Average(unit => unit.Position.Y);
            mMoveBehavior.MoveTo(mUnit.Position - new Vector2(posX, posY));
            return 0;
        }
    }
}

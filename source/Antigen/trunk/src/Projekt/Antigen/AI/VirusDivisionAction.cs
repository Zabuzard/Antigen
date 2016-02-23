using System;
using System.Collections.Generic;
using Antigen.Logic.CellDivision;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    /// <summary>
    /// A virus division action
    /// </summary>
    [Serializable]
    sealed class VirusDivisionAction: IAiAction
    {
        private readonly Virus mVirus;
        private readonly IDivisionBehavior mDivisionBehavior;
        private readonly IDivisionContentLoader mContentLoader;
        private double mDivisionTimer;
        private const float ManyPoints = 1000;

        /// <summary>
        /// Creates a new action
        /// </summary>
        /// <param name="virus"></param>
        /// <param name="divisionBehavior"></param>
        /// <param name="contentLoader"></param>
        public VirusDivisionAction(Virus virus, IDivisionBehavior divisionBehavior, IDivisionContentLoader contentLoader)
        {
            mVirus = virus;
            mDivisionBehavior = divisionBehavior;
            mContentLoader = contentLoader;
            mDivisionTimer = mVirus.GetCellDivisionRate();
        }

        /// <inheritdoc />
        public float GetPoints(IAiAction lastAction, List<Unit> units)
        {
            return mVirus.IsInfecting() ? ManyPoints : 0;
        }

        /// <inheritdoc />
        public float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime)
        {
            var result = 0;
            if (mDivisionTimer > 0)
            {
                mDivisionTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                mDivisionBehavior.DivideCell(mVirus, mContentLoader);
                mDivisionTimer = mVirus.GetCellDivisionRate();
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
using System;
using Antigen.Input;
using Antigen.Logic.CellDivision;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.UnitModes
{
    /// <summary>
    /// Control for basic division
    /// Divides a object and creates new ones.
    /// </summary>
    [Serializable]
    sealed class CellDivisionControl : IModeControl
    {
        /// <summary>
        /// Behaviour which receives the division order.
        /// </summary>
        private readonly IDivisionBehavior mReceiver;
        /// <summary>
        /// Object to divide.
        /// </summary>
        private readonly ICanCellDivision mObj;
        private readonly IDivisionContentLoader mLoader;

        /// <summary>
        /// Duration timer for cell division. If it reached 0 the division is completed.
        /// </summary>
        private double mDurationTimer;

        /// <summary>
        /// Creates a new basic division control object
        /// which gives order to divide the given cell.
        /// </summary>
        /// <param name="obj">Unit to divide</param>
        /// <param name="receiver">Division order receiving behavior</param>
        /// <param name="loader">Content loader for with division created units</param>
        public CellDivisionControl(ICanCellDivision obj, IDivisionBehavior receiver,
            IDivisionContentLoader loader)
        {
            mReceiver = receiver;
            mLoader = loader;
            mObj = obj;
            mDurationTimer = CalcMaximalDuration(mObj);
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            if (mDurationTimer > 0)
            {
                mDurationTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                mReceiver.DivideCell(mObj, mLoader);
                mDurationTimer = CalcMaximalDuration(mObj);
            }
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            return false;
        }

        /// <summary>
        /// Gets the remaining duration until the cell division is finished.
        /// If not cell dividing this is equals GetCellDivisionRate().
        /// Duration is in seconds.
        /// </summary>
        /// <returns>Remaining duration until the division is completed in seconds</returns>
        public double GetRemainingCellDivisionDuration()
        {
            return mDurationTimer;
        }

        /// <inheritdoc />
        public void Activate()
        {
        }

        /// <inheritdoc />
        public void Terminate()
        {
            mDurationTimer = CalcMaximalDuration(mObj);
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }

        /// <summary>
        /// Calculates the duration rate for a division with
        /// this unit and if Stemcell for the given DivisionResult.
        /// </summary>
        /// <param name="obj">Object to calculate duration for its divison</param>
        /// <returns>Duration in milliseconds</returns>
        private static double CalcMaximalDuration(ICanCellDivision obj)
        {
            var duration = obj.GetCellDivisionRate();
            return duration;
        }
    }
}

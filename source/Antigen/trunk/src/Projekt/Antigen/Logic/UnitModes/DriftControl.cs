using System;
using Antigen.Input;
using Antigen.Logic.Movement;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.UnitModes
{
    /// <summary>
    /// Control for flow movement.
    /// Moves a object all the time according to the blood flow.
    /// </summary>
    [Serializable]
    sealed class DriftControl : IModeControl
    {
        /// <summary>
        /// Behaviour which receives the move order.
        /// </summary>
        private readonly IMoveBehavior mReceiver;

        /// <summary>
        /// Creates a new flow movement control object which
        /// orders the behaviour all time long to move according to the flow.
        /// </summary>
        /// <param name="receiver">Move order receiving behaviour</param>
        public DriftControl(IMoveBehavior receiver)
        {
            mReceiver = receiver;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            return false;
        }

        /// <inheritdoc />
        public void Activate()
        {
            mReceiver.Wander();
        }

        /// <inheritdoc />
        public void Terminate()
        {
            mReceiver.Terminate();
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }
    }
}

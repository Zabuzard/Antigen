using System;
using System.Linq;
using Antigen.Input;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Objects.Properties;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.AntigenExchange
{
    /// <summary>
    /// Control for antigen exchanges.
    /// Can exchange antigen with a right clicked target.
    /// </summary>
    [Serializable]
    sealed class AntigenExchangeControl : IRightClickListener, IUpdateable
    {
        /// <summary>
        /// Modes of this control
        /// </summary>
        private enum Mode
        {
            HasAntigen,
            AntigenProvider
        }
        /// <summary>
        /// Minimal distance to target in pixel for an antigen exchange.
        /// </summary>
        private const int MinTriggerDist = 15;
        /// <summary>
        /// Behaviour which receives the exchange order.
        /// </summary>
        private readonly IAntigenExchangeBehavior mExchangeReceiver;
        /// <summary>
        /// Object to exchange for.
        /// </summary>
        private readonly IHasAntigen mHasAntigenObj;
        /// <summary>
        /// Object to exchange for.
        /// </summary>
        private readonly IAntigenProvider mAntigenProviderObj;
        /// <summary>
        /// Current target provider.
        /// </summary>
        private IAntigenProvider mProvider;
        /// <summary>
        /// Current target receiver.
        /// </summary>
        private IHasAntigen mReceiver;

        private readonly Mode mMode;

        /// <summary>
        /// Creates a new antigen exchange control object
        /// which gives order to exchange anigen with the given cell.
        /// </summary>
        /// <param name="thatHasAntigenObj">Object to exchange for</param>
        /// <param name="exchangeReceiver">Exchange order receiving behavior</param>
        /// <param name="input">Input dispatcher to register at</param>
        public AntigenExchangeControl(IHasAntigen thatHasAntigenObj, IAntigenExchangeBehavior exchangeReceiver, InputDispatcher input)
        {
            if (input != null)
            {
                input.RegisterListener(this);
            }
            mExchangeReceiver = exchangeReceiver;
            mHasAntigenObj = thatHasAntigenObj;
            mMode = Mode.HasAntigen;
        }

        /// <summary>
        /// Creates a new antigen exchange control object
        /// which gives order to exchange anigen with the given cell.
        /// </summary>
        /// <param name="thatAntigenProviderObj">Object to exchange for</param>
        /// <param name="exchangeReceiver">Exchange order receiving behavior</param>
        /// <param name="input">Input dispatcher to register at</param>
        public AntigenExchangeControl(IAntigenProvider thatAntigenProviderObj, IAntigenExchangeBehavior exchangeReceiver, InputDispatcher input)
        {
            if (input != null)
            {
                input.RegisterListener(this);
            }
            mExchangeReceiver = exchangeReceiver;
            mAntigenProviderObj = thatAntigenProviderObj;
            mMode = Mode.AntigenProvider;
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            if (mExchangeReceiver == null)
            {
                    return false;
            }
            if (mMode == Mode.HasAntigen)
            {
                if (mHasAntigenObj is ISelectable && !((ISelectable) mHasAntigenObj).Selected)
                {
                    return false;
                }
                foreach (var obj in info.ObjectsUnderCursor.OfType<IAntigenProvider>())
                {
                    mProvider = obj;
                    break;
                }
            }
            else
            {
                if (mAntigenProviderObj is ISelectable && !((ISelectable)mAntigenProviderObj).Selected)
                {
                    return false;
                }
                foreach (var obj in info.ObjectsUnderCursor.OfType<IHasAntigen>())
                {
                    mReceiver = obj;
                    break;
                }
            }
            
            return false;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            if (mMode == Mode.HasAntigen)
            {
                if (mProvider == null)
                {
                    return;
                }
                var provider = mProvider as GameObject;
                var host = mHasAntigenObj as GameObject;
                if (provider == null || host == null)
                    return;

                var dist = provider.Position.Distance(host.Position);
                dist -= (provider.Radius + host.Radius);
                if (dist > MinTriggerDist)
                {
                    return;
                }
                var providerL = mProvider as ILively;
                if (providerL != null && !providerL.IsAlive())
                {
                    mProvider = null;
                    return;
                }
                mExchangeReceiver.Exchange(mProvider);
                mProvider = null;
            }
            else
            {
                if (mReceiver == null)
                {
                    return;
                }
                var receiver = mReceiver as GameObject;
                var host = mAntigenProviderObj as GameObject;
                if (receiver == null || host == null)
                    return;

                var dist = receiver.Position.Distance(host.Position);
                dist -= (receiver.Radius + host.Radius);
                if (dist > MinTriggerDist)
                {
                    return;
                }
                var receiverL = mReceiver as ILively;
                if (receiverL != null && !receiverL.IsAlive())
                {
                    mReceiver = null;
                    return;
                }
                mExchangeReceiver.Exchange(mReceiver);
                mReceiver = null;
            }
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }
    }
}

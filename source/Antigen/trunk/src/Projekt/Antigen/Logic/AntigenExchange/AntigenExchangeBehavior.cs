using System;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Util;

namespace Antigen.Logic.AntigenExchange
{
    /// <summary>
    /// Implements a basic antigen exchange behavior
    /// which exchanges antigen of a provider with a given object.
    /// </summary>
    [Serializable]
    sealed class AntigenExchangeBehavior : IAntigenExchangeBehavior
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
        /// Object to exchange for.
        /// </summary>
        private readonly IHasAntigen mHasAntigenObj;
        /// <summary>
        /// Object to exchange for.
        /// </summary>
        private readonly IAntigenProvider mAntigenProviderObj;
        private readonly Mode mMode;

        /// <summary>
        /// Creates a new basic antigen exchange behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to exchange antigen for</param>
        public AntigenExchangeBehavior(IHasAntigen obj)
        {
            mHasAntigenObj = obj;
            mMode = Mode.HasAntigen;
        }

        /// <summary>
        /// Creates a new basic antigen exchange behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to exchange antigen for</param>
        public AntigenExchangeBehavior(IAntigenProvider obj)
        {
            mAntigenProviderObj = obj;
            mMode = Mode.AntigenProvider;
        }

        /// <inheritdoc />
        public void Exchange(IAntigenProvider provider)
        {
            if (mMode != Mode.HasAntigen)
            {
                return;
            }
            var host = mHasAntigenObj as Unit;
            var providerU = provider as Unit;

            if (!Functions.IsFriendlySideConstellation(host, providerU))
            {
                return;
            }
            if (!host.IsAbleToOffensive())
            {
                return;
            }
            if (!Functions.IsTargetAbleToReceiveOffensive(providerU, false))
            {
                return;
            }
            var antigen = provider.ProvideAntigen();
            if (!antigen.Equals(""))
            {
                mHasAntigenObj.Antigen = antigen;
            }
        }

        /// <inheritdoc />
        public void Exchange(IHasAntigen receiver)
        {
            if (mMode != Mode.AntigenProvider)
            {
                return;
            }
            var host = mAntigenProviderObj as Unit;
            var receiverU = receiver as Unit;

            if (!Functions.IsFriendlySideConstellation(host, receiverU))
            {
                return;
            }
            if (!host.IsAbleToOffensive())
            {
                return;
            }
            if (!Functions.IsTargetAbleToReceiveOffensive(receiverU, false))
            {
                return;
            }
            var antigen = mAntigenProviderObj.ProvideAntigen();
            if (!antigen.Equals(""))
            {
                receiver.Antigen = antigen;
            }
        }
    }
}

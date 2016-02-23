using Antigen.Objects.Properties;

namespace Antigen.Logic.AntigenExchange
{
    /// <summary>
    /// Interface for objects that exchange their antigen with providers.
    /// </summary>
    interface IAntigenExchangeBehavior
    {
        /// <summary>
        /// Exchanges Antigen with a given provider.
        /// </summary>
        /// <param name="provider">Object that provides an antigen</param>
        void Exchange(IAntigenProvider provider);
        /// <summary>
        /// Exchanges Antigen with a given receiver.
        /// </summary>
        /// <param name="receiver">Object to exchange antigen for</param>
        void Exchange(IHasAntigen receiver);
    }
}

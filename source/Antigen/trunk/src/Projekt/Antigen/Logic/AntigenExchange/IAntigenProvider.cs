namespace Antigen.Logic.AntigenExchange
{
    /// <summary>
    /// Interface for objects provides antigen for exchange with other object.
    /// </summary>
    interface IAntigenProvider
    {
        /// <summary>
        /// Provides the one antigen for another object.
        /// Own antigen value must be set to empty.
        /// </summary>
        /// <returns>Antigen of this unit in exchange for another object</returns>
        string ProvideAntigen();
    }
}

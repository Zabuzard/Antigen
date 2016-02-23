namespace Antigen.Objects.Properties
{
    /// <summary>
    /// Interface for objects that have antigens against strains.
    /// </summary>
    interface IHasAntigen
    {
        /// <summary>
        /// Antigen of the object.
        /// </summary>
        string Antigen { get; set; }
    }
}

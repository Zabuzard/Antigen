namespace Antigen.Objects.Properties
{
    /// <summary>
    /// Interface for objects that have strains like viruses.
    /// </summary>
    interface IHasStrain
    {
        /// <summary>
        /// Strain of the object.
        /// </summary>
        string Strain { get; }
    }
}

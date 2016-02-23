namespace Antigen.Statistics
{
    /// <summary>
    /// Names of the statistics collected during each game.
    /// </summary>
    enum StatName
    {
        //Resharper hint: Names are inconsistent because a char
        //not commonly used is needed to be replaced by whitespace whenever converting this to string.
// ReSharper disable InconsistentNaming
        Playing_Time,
        Collected_Antigens,
        Killed_Bacteriums,
        Killed_Viruses,
        Lost_red_Bloodcells,
        Performed_celldivisions,
        Performed_mutations
// ReSharper restore InconsistentNaming
    }
}

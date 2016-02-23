namespace Antigen.Achievements
{
    /// <summary>
    /// Enumeration of all achievements possible to unlock during the game.
    /// </summary>
    internal enum AchievementNames
    {
        // Use this for initializing a variable of type AchievementNames with null.
        None,
        //Resharper hint: The names of these members have to be
        //inconsistent because a char is needed to replace by whitespace while converting to string.
// ReSharper disable InconsistentNaming
        Collect_Antigen,
        Kill_100_Bacteriums,
        Kill_100_Viruses,
        Perform_100_Cell_Divisions,
        Perform_100_Mutations
// ReSharper restore InconsistentNaming
    }
}

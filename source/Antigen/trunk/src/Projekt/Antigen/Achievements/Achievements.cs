using System.Collections.Generic;
using System.IO;
using Antigen.GameManagement;
using Antigen.Util;

namespace Antigen.Achievements
{
    /// <summary>
    /// Static class to update the global achievements.
    /// </summary>
    static class Achievements
    {
        private static Dictionary<AchievementNames, bool> sAchievements;

        /// <summary>
        /// Call this method to get the current global achievements.
        /// </summary>
        /// <returns>Return the dicitionary with the achievements' names true if they're unlocked, false otherwise.</returns>
        public static Dictionary<AchievementNames, bool> GetAchievements()
        {
            if (!File.Exists(Functions.GetFolderPath() + "\\achievements.bin"))
            {
                return new Dictionary<AchievementNames, bool>
                {
                    {AchievementNames.Collect_Antigen, false},
                    {AchievementNames.Kill_100_Bacteriums, false},
                    {AchievementNames.Kill_100_Viruses, false},
                    {AchievementNames.Perform_100_Cell_Divisions, false},
                    {AchievementNames.Perform_100_Mutations, false}
                };
            }
            return
                (Dictionary<AchievementNames, bool>)
                    SaveGameManager.Deserialize(Functions.GetFolderPath() + "\\achievements.bin");
        }

        /// <summary>
        /// /// This method is called if a new achievement is unlocked.
        /// </summary>
        /// <param name="name">The name of the unlocked achievement</param>
        /// <returns>Return true iff a new achievement has been unlocked, false otherwise.</returns>
        public static bool UnlockAchievement(AchievementNames name)
        {
            if (sAchievements == null)
            {
                sAchievements = GetAchievements();
            }
            if (!sAchievements.ContainsKey(name) || sAchievements[name])
            {
                return false;
            }
            sAchievements[name] = true;
            SaveGameManager.Serialize(sAchievements, Functions.GetFolderPath() + "\\achievements.bin");
            return true;
        }
    }
}

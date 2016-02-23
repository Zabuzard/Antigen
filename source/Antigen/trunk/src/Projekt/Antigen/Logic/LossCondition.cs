using System.Collections.Generic;
using System.Linq;
using Antigen.Objects.Units;
using Antigen.Screens.Menu;

namespace Antigen.Logic
{
    /// <summary>
    /// Provides a function that checks the loss condition.
    /// </summary>
    static class LossCondition
    {
        /// <summary>
        /// Indicates whether the player has lost the game. This
        /// is the case when either the number of red bloodcells
        /// falls below a certain threshold or all player units are
        /// dead.
        /// </summary>
        /// <param name="friendlyUnits">List of all friendly
        /// units currently present in the game.</param>
        /// <param name="redBloodcells">List of all red bloodcells
        /// currently in the game.</param>
        /// <param name="redBloodcellThreshold">Threshold below which
        /// the number of red bloodcells in the game may not fall.</param>
        /// <returns><code>true</code> if the player has lost the
        /// game, <code>false</code> otherwise.</returns>
        public static GameEndReason? PlayerHasLost(IEnumerable<Unit> friendlyUnits, IEnumerable<Unit> redBloodcells, int redBloodcellThreshold)
        {
            if (!friendlyUnits.Any())
                return GameEndReason.LossNoUnitsLeft;
            if (redBloodcells.Count() < redBloodcellThreshold)
                return GameEndReason.LossLittleRedBloodCellsLeft;

            return null;
        }
    }
}

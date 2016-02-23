using System.Collections.Generic;
using System.Linq;
using Antigen.Objects.Units;
using Antigen.Screens.Menu;

namespace Antigen.Logic
{
    /// <summary>
    /// Provides a function that checks the win condition.
    /// </summary>
    static class WinCondition
    {
        /// <summary>
        /// Indicates whether the player has won the game. This
        /// is the case when all enemy units have been eliminated.
        /// </summary>
        /// <param name="enemyUnits">List of all enemy units currently present
        /// in the game.</param>
        /// <returns>A corresponding <code>GameEndReason</code> if the player
        /// has won; otherwise <code>null</code>.</returns>
        public static GameEndReason? PlayerHasWon(IEnumerable<Unit> enemyUnits)
        {
            return enemyUnits.Any() ? (GameEndReason?)null : GameEndReason.Win;
        }
    }
}

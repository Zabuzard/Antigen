using System.Collections.Generic;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.AI
{
    interface IAiAction
    {
        /// <summary>
        /// Returns the points of the action.
        /// The points represent the probability that this action will be choosen.
        /// The higher the points the higher the probability.
        /// </summary>
        /// <param name="lastAction">The last action that was executed</param>
        /// <param name="units">The units in the sight radius of the unit</param>
        /// <returns>The points</returns>
        float GetPoints(IAiAction lastAction, List<Unit> units);

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="units">The units in the sight radius of the unit</param>
        /// <param name="lastAction">The last action that was executed</param>
        /// <param name="gameTime">The game time</param>
        /// <returns>The credit points the action gets</returns>
        float DoAction(List<Unit> units, IAiAction lastAction, GameTime gameTime);
    }
}
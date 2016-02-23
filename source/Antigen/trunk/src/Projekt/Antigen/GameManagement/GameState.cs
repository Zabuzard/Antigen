using System;
using System.Collections.Generic;
using Antigen.Screens;
using Antigen.Statistics;

namespace Antigen.GameManagement
{
    /// <summary>
    /// Represent a game state with all relevant information.
    /// </summary>
    [Serializable]
    sealed class GameState
    {
        public GameScreen GameScreen { get; private set; }
        public Dictionary<StatName, int> Stats { get; private set; }

        /// <summary>
        /// Create a new game state of the current game screen.
        /// </summary>
        /// <param name="currentGameScreen">Take the current game screen from the screen manager.</param>
        public GameState(GameScreen currentGameScreen)
        {
            GameScreen = currentGameScreen;
            Stats = GameScreen.mStats;
        }
    }
}

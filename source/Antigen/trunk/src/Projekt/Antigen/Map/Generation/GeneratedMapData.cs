using Microsoft.Xna.Framework;

namespace Antigen.Map.Generation
{
    sealed class GeneratedMapData
    {
        public Vector2 PlayerStart { get; private set; }
        public Vector2 EnemyStart { get; private set; }
        public int TilesCount { get; private set; }
        public int TileSize { get; private set; }

        public GeneratedMapData(Vector2 playerStart, Vector2 enemyStart, int tilesCount, int tileSize)
        {
            PlayerStart = playerStart;
            EnemyStart = enemyStart;
            TilesCount = tilesCount;
            TileSize = tileSize;
        }
    }
}

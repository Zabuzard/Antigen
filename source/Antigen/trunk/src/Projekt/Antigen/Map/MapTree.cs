using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Antigen.Map
{
    /// <summary>
    /// Implementation of a tree for collision detection and path finding.
    /// </summary>
    sealed class MapTree
    {
        /// <summary>
        /// Amount of splits at every distribution of this tree which must be quadratic.
        /// </summary>
        private const int QuadraticSplitAmount = 16;

        /// <summary>
        /// Walkable state of this node.
        /// </summary>
        private State mState;
        /// <summary>
        /// List of subnodes if needed.
        /// </summary>
        private List<MapTree> mNodes;
        /// <summary>
        /// Boundary rectangle of this node.
        /// </summary>
        private Rectangle mBounds;

        /// <summary>
        /// All possible states of a tree node.
        /// </summary>
        private enum State
        {
            Walkable,
            Unknown,
            NotWalkable
        };

        /// <summary>
        /// Creates a new MapTree.
        /// </summary>
        /// <param name="map">The game map</param>
        public MapTree(Map map)
        {
            if (map.GetWidth() % 2 != 0 || map.GetHeight() % 2 != 0)
            {
                throw new MapCorruptException("Height or width of the map is not eval.");
            }
            mBounds = new Rectangle(0, 0, map.GetWidth(), map.GetHeight());
            
            Initialize(map);
        }

        /// <summary>
        /// Create new MapTree.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="rect">The space this node occupies.</param>
        private MapTree(Map map, Rectangle rect)
        {
            mBounds = rect;
            Initialize(map);
        }

        /// <summary>
        /// Initializes a tree node with already given bounds and map.
        /// </summary>
        /// <param name="map">The map object that provides walkable
        /// state information.</param>
        private void Initialize(Map map)
        {
            mState = DetermineState(map);

            if (mState == State.Unknown)
            {
                mNodes = new List<MapTree>(QuadraticSplitAmount);
                Split(map);
            }
        }

        /// <summary>
        /// Determine whether node is walkable (1), not walkable (-1) or ambiguous (0).
        /// </summary>
        /// <param name="map">The map object that provides walkable state information.</param>
        /// <returns>Int representing walkability.</returns>
        private State DetermineState(Map map)
        {
            var hasWalkable = false;
            var hasNotWalkable = false;

            for (var x = mBounds.X; x < mBounds.X + mBounds.Width; x++)
            {
                for (var y = mBounds.Y; y < mBounds.Y + mBounds.Height; y++)
                {
                    if (map.IsWalkable(x, y))
                    {
                        hasWalkable = true;
                    }
                    else
                    {
                        hasNotWalkable = true;
                    }
                    //Break out if result already known
                    if (hasWalkable && hasNotWalkable)
                    {
                        break;
                    }
                }
            }

            if (hasWalkable && hasNotWalkable)
            {
                return State.Unknown;
            }
            //Now one must be false
            if (hasWalkable)
            {
                return State.Walkable;
            }
            if (hasNotWalkable)
            {
                return State.NotWalkable;
            }
            throw new MapCorruptException("There is a pixel that is not 'walkable' nor 'not walkable' in rectangle: " + mBounds);
        }

        /// <summary>
        /// Split node into "QuadtraticSplitAmount"-times subnodes.
        /// </summary>
        /// <param name="map">The map object that provides walkable
        /// state information.</param>
        private void Split(Map map)
        {
            var amountNodesOneDir = Math.Sqrt(QuadraticSplitAmount);
            var nextWidth = (int)Math.Ceiling(mBounds.Width / amountNodesOneDir);
            var nextHeight = (int)Math.Ceiling(mBounds.Height / amountNodesOneDir);

            var x = mBounds.X;
            var y = mBounds.Y;

            for (var yj = 0; yj < amountNodesOneDir; yj++)
            {
                for (var xi = 0; xi < amountNodesOneDir; xi++)
                {
                    var subNode = new Rectangle(x + (xi * nextWidth), y + (yj * nextHeight), nextWidth, nextHeight);
                    mNodes.Add(new MapTree(map, subNode));
                }
            }
        }

        /// <summary>
        /// Determine whether given rectangle is blocked by this node.
        /// </summary>
        /// <param name="rect">The area in question.</param>
        /// <returns>True if blocking, false otherwise.</returns>
        public bool IsBlocking(Rectangle rect)
        {
            if (mState == State.Walkable)
                return false;

            if (mState == State.NotWalkable)
                return true;

            var blocking = false;

            for (var i = 0; i < QuadraticSplitAmount; i++)
            {
                if (!mNodes[i].mBounds.Intersects(rect))
                {
                    continue;
                }
                blocking |= mNodes[i].IsBlocking(rect);

                //Break if solution is already known
                if (blocking)
                {
                    break;
                }
            }
            return blocking;
        }
    }
}

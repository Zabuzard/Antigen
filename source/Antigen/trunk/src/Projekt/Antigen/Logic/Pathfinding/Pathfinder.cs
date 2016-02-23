using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antigen.Logic.Collision;
using Antigen.Objects;
using Antigen.Util.Graph;
using Microsoft.Xna.Framework;
using Nuclex.Support.Collections;

namespace Antigen.Logic.Pathfinding
{
    /// <summary>
    /// Implementation of simple pathfinding.
    /// </summary>
    sealed class Pathfinder : IDestroy, IUpdateable
    {
        /// <summary>
        /// Side length of grid squares. Must be
        /// divisible by two.
        /// </summary>
        private const int SquareSize = 40;
        /// <summary>
        /// Amount of searchs tries for a nearest walkable position.
        /// </summary>
        private const int SearchTries = 100000;
        /// <summary>
        /// Maximum number of pathfinder tasks started during each
        /// game tick.
        /// </summary>
        private const int MaxQueriesPerUpdate = 10;

        /// <summary>
        /// Graph that connects each walkable node with its
        /// walkable neighbours.
        /// </summary>
        private IDictionary<Vector2, Node<Vector2, int>> mGraph;

        /// <summary>
        /// Indicates for each grid square whether it is
        /// walkable.
        /// </summary>
        private bool[,] mWalkable;
        /// <summary>
        /// Used for map collision tests.
        /// </summary>
        private IMapCollisionContainer mMapCollision;
        /// <summary>
        /// Used for object collision tests.
        /// </summary>
        private ISpatialCache mSpatialCache;

        private readonly Queue<Task<Stack<Vector2>>>  mTaskQueue;

        /// <summary>
        /// Creates a pathfinder based on the given caches.
        /// </summary>
        /// <param name="mapCollision">Map collision information.</param>
        /// <param name="spatialCache">Spatial object information.</param>
        public Pathfinder(IMapCollisionContainer mapCollision, ISpatialCache spatialCache)
        {
            SetContainer(mapCollision, spatialCache);
            mTaskQueue = new Queue<Task<Stack<Vector2>>>();
        }

        /// <summary>
        /// Creates a task for a pathfinding operation. When the task finishes,
        /// its result is a path between <code>pos</code> and <code>dest</code>.
        /// The task should not be started manually; rather, the pathfinder will
        /// schedule it appropriately to avoid load spikes when many pathfinder
        /// queries occur at once.
        /// </summary>
        /// <param name="pos">Starting position.</param>
        /// <param name="dest">Destination position.</param>
        /// <returns>A pathfinding task.</returns>
        public Task<Stack<Vector2>> FindPath(Vector2 pos, Vector2 dest)
        {
            var task = new Task<Stack<Vector2>>(() => RealFindPath(pos, dest));
            mTaskQueue.Enqueue(task);
            return task;
        }

        /// <summary>
        /// Find shortest path between pos and dest.
        /// Can throw a PathfinderException if not able to create a path.
        /// </summary>
        /// <param name="pos">start position</param>
        /// <param name="dest">end position</param>
        private Stack<Vector2> RealFindPath(Vector2 pos, Vector2 dest)
        {
            pos = ValidatePosition(pos);
            dest = ValidatePosition(dest);

            if (mWalkable == null)
            {
                LoadContent();
            }

            var startCoords = CoordsToGraphNode(pos);
            var endCoords = CoordsToGraphNode(dest);

            if (mWalkable != null)
            {
                var endGrid = GraphNodeCoordsToGrid(endCoords);
                if (!mWalkable[endGrid.X, endGrid.Y])
                    endCoords = FindNearestValidPoint(endCoords, true, false);

                var startGrid = GraphNodeCoordsToGrid(startCoords);
                if (!mWalkable[startGrid.X, startGrid.Y])
                    startCoords = FindNearestValidPoint(startCoords, true, false);
            }

            // Not yet analyzed nodes
            var openSet = new HashSet<Node<Vector2, int>>();
            var openQueue = new PairPriorityQueue<int, Node<Vector2, int>>();
            var start = mGraph[startCoords];
            var end = mGraph[endCoords];
            openSet.Add(start);
            openQueue.Enqueue(0, start);

            // Analyzed nodes that have a path from start to themselves
            var closedSet = new HashSet<Node<Vector2, int>>();

            // Link optimal origin node to every node
            var predecessors = new Dictionary<Node<Vector2, int>, Node<Vector2, int>>();

            // Link each analyzed node with its distance from start
            var currentDistance = new Dictionary<Node<Vector2, int>, int> { { start, 0 } };

            // Analyze nodes
            while (openQueue.Count > 0)
            {
                // Node with lowest estimated cost to end
                var current = openQueue.Dequeue().Item;

                // If already at end, return
                if (Vector2.Distance(current.Value, end.Value) < SquareSize)
                    return ReconstructPath(predecessors, end);

                openSet.Remove(current);
                closedSet.Add(current);

                // Update neighboring nodes
                foreach (var outEdge in current.OutEdges)
                {
                    var neighbor = outEdge.Successor;

                    if (closedSet.Contains(neighbor))
                        continue;

                    var tempCurrentDistance = currentDistance[current] + outEdge.Tag;

                    if (openSet.Contains(neighbor) && tempCurrentDistance >= currentDistance[neighbor])
                        continue;

                    predecessors[neighbor] = current;
                    currentDistance[neighbor] = tempCurrentDistance;

                    if (openSet.Add(neighbor))
                        openQueue.Enqueue(-(tempCurrentDistance + Heuristic(neighbor.Value, end.Value)), neighbor);
                    //The PrioQ used for the open queue is a max queue, therefore the priorities have to be inverted.
                }
            }

            var failPath = new Stack<Vector2>();
            failPath.Push(dest);
            return failPath;
        }

        /// <summary>
        /// Distance functtion for A* graph nodes.
        /// </summary>
        /// <param name="start">Start node.</param>
        /// <param name="end">End nodes.</param>
        /// <returns>The exact distance between <code>start</code> and <code>end</code>.</returns>
        private static int Distance(Vector2 start, Vector2 end)
        {
            return (int)Math.Abs(start.X - end.X) + (int)Math.Abs(start.Y - end.Y);
        }

        /// <summary>
        /// Admissible heuristic function for the A* algorithm.
        /// </summary>
        /// <param name="start">The node which is currently being analysed.</param>
        /// <param name="end">The goal node.</param>
        /// <returns>An admissible heuristic for the distance between the two nodes.</returns>
        private static int Heuristic(Vector2 start, Vector2 end)
        {
            return (int)Math.Abs(start.X - end.X) + (int)Math.Abs(start.Y - end.Y);
        }

        /// <summary>
        /// Determine appropriate path.
        /// </summary>
        /// <param name="cameFrom">List of nodes and origin.</param>
        /// <param name="current">Destination node</param>
        /// <returns>Shortest path from start to destination node.</returns>
        private static Stack<Vector2> ReconstructPath(IDictionary<Node<Vector2, int>, Node<Vector2, int>> cameFrom,
            Node<Vector2, int> current)
        {
            var path = new Stack<Vector2>();
            var cur = current;
            while (cur != null)
            {
                path.Push(cur.Value);
                cameFrom.TryGetValue(cur, out cur);
            }
            return path;
        }

        /// <summary>
        /// Used in collision reaction to find nearest valid point to stand on.
        /// </summary>
        /// <param name="dest">The point in whose vicinity is to be searched.</param>
        /// <param name="considerMap">Whether to consider walkability when searching for a valid point.</param>
        /// <param name="considerObjects">Whether to consider colliding objects when searching for a
        /// valid point.</param>
        /// <returns>One of the closest valid neighbours of <code>dest</code>.</returns>
        public Vector2 FindNearestValidPoint(Vector2 dest, bool considerMap, bool considerObjects)
        {
            dest = ValidatePosition(dest);

            var end = CoordsToGrid(dest);
            var x = end.X;
            var y = end.Y;

            var center = MakeValidVector(x, y, considerMap, considerObjects);
            if (center != null)
                return center.Value;

            for (var step = 1; step <= SearchTries; step++)
            {
                //top and bottom row
                for (var col = x - step; col <= x + step; col++)
                {
                    var topRow = MakeValidVector(col, y - step, considerMap, considerObjects);
                    if (topRow != null)
                        return topRow.Value;

                    var botRow = MakeValidVector(col, y - step, considerMap, considerObjects);
                    if (botRow != null)
                        return botRow.Value;
                }

                //left and right columns, excluding the corners which have been processed above
                for (var row = y - step + 1; row <= y + step - 1; row++)
                {
                    var leftCol = MakeValidVector(x - step, row, considerMap, considerObjects);
                    if (leftCol != null)
                        return leftCol.Value;

                    var rightCol = MakeValidVector(x + step, row, considerMap, considerObjects);
                    if (rightCol != null)
                        return rightCol.Value;
                }
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Returns the center of the square whose top-left point is <code>(col, row)</code>
        /// if that point is valid, otherwise <code>null</code>.
        /// </summary>
        /// <param name="col">The column (x-coordinate).</param>
        /// <param name="row">The row (y-coordinate).</param>
        /// <param name="considerMap">Whether to consider non-walkable points invalid.</param>
        /// <param name="considerObjects">Whether to consider points blocked by collidable objects
        /// invalid.</param>
        /// <returns>The valid center point as a vector, or <code>null</code>.</returns>
        private Vector2? MakeValidVector(int col, int row, bool considerMap, bool considerObjects)
        {
            if (col <= 0 || col >= mWalkable.GetLength(0) || row <= 0 || row >= mWalkable.GetLength(1) ||
                (considerMap && !mWalkable[col, row]) ||
                (considerObjects && mSpatialCache.CollidableObjectsInArea(GridRectangle(col, row)).Any()))
                return null;

            return GridToCoords(col, row);
        }

        /// <summary>
        /// Initialize contant fields if null after deserialization.
        /// </summary>
        private void LoadContent()
        {
            SetContainer(mMapCollision, mSpatialCache);
        }

        /// <summary>
        /// Set the static members correctly after deserialisation.
        /// </summary>
        /// <param name="container">The map container.</param>
        /// <param name="container2">The saved object collision container.</param>
        private void SetContainer(IMapCollisionContainer container, ISpatialCache container2)
        {
            // Basically proceed all that is done in the constructor and LoadContent, but here it's in static context.
            mMapCollision = container;
            mSpatialCache = container2;
            var gridSize = container.GetHeight() / SquareSize;
            mWalkable = new bool[gridSize, gridSize];
            for (var x = 0; x < gridSize; ++x)
            {
                for (var y = 0; y < gridSize; ++y)
                {
                    mWalkable[x, y] = !mMapCollision.IsBlocking(GridRectangle(x, y));
                }
            }

            mGraph = MakeGraph();
        }

        /// <summary>
        /// Creates a graph from all walkable grid squares. Every walkable
        /// square is a node and has outgoing edges to all adjacent walkable
        /// squares.
        /// </summary>
        /// <returns>A mapping from square centres to the corresponding
        /// graph nodes.</returns>
        private IDictionary<Vector2, Node<Vector2, int>> MakeGraph()
        {
            var nodes = new List<Node<Vector2, int>>();
            for (var x = 0; x < mWalkable.GetLength(0); x++)
            {
                for (var y = 0; y < mWalkable.GetLength(1); y++)
                {
                    if (!mWalkable[x, y])
                        continue;

                    var cur = GridToCoords(x, y);
                    var neighbors = GetNeighborNodes(x, y).
                        Select(neighbor => MakeOutEdge(new Point(x, y), neighbor)).
                        ToArray();
                    nodes.Add(new Node<Vector2, int>(cur, neighbors));
                }
            }
            return nodes.BuildGraph();
        }

        /// <summary>
        /// Creates an outgoing edge from <code>predecessor</code> to
        /// <code>successor</code>.
        /// 
        /// Both vectors must be given in grid coordinates
        /// (i.e. <code>(1,1)</code> for the real coordinates
        /// <code>(SquareSize, SquareSize)</code>.
        /// </summary>
        /// <param name="predecessor">The node from which the edge originates.</param>
        /// <param name="successor">The node which the edge targets.</param>
        /// <returns>The edge.</returns>
        private static OutEdge<Node<Vector2, int>, int> MakeOutEdge(Point predecessor, Point successor)
        {
            var result = new OutEdge<Node<Vector2, int>, int>(
                new Node<Vector2, int>(GridToCoords(successor.X, successor.Y)),
                Distance(GridToCoords(predecessor.X, predecessor.Y), GridToCoords(successor.X, successor.Y)));
            return result;
        }

        /// <summary>
        /// Find neighbors of a given node in four directions. The node is
        /// given in grid coordinates.
        /// </summary>
        /// <returns>List of neighboring nodes.</returns>
        private IEnumerable<Point> GetNeighborNodes(int x, int y)
        {
            var nodes = new List<Point>();
            var xMax = mWalkable.GetLength(0);
            var yMax = mWalkable.GetLength(1);

            // Go up one square.
            if (y - 1 > 0 && mWalkable[x, y - 1])
                nodes.Add(new Point(x, y - 1));
            // Go up and right one square.
            if (y - 1 > 0 && x + 1 < xMax && mWalkable[x + 1, y - 1])
                nodes.Add(new Point(x + 1, y - 1));
            // Go right one square.
            if (x + 1 < xMax && mWalkable[x + 1, y])
                nodes.Add(new Point(x + 1, y));
            // Go right and down one square.
            if (x + 1 < xMax && y + 1 < yMax && mWalkable[x + 1, y + 1])
                nodes.Add(new Point(x + 1, y + 1));
            // Go down one square.
            if (y + 1 < yMax && mWalkable[x, y + 1])
                nodes.Add(new Point(x, y + 1));
            // Go down and left one square.
            if (y + 1 < yMax && x - 1 > 0 && mWalkable[x - 1, y + 1])
                nodes.Add(new Point(x - 1, y + 1));
            // Go left one square.
            if (x - 1 > 0 && mWalkable[x - 1, y])
                nodes.Add(new Point(x - 1, y));
            // Go left and up one square.
            if (x - 1 > 0 && y - 1 > 0 && mWalkable[x - 1, y - 1])
                nodes.Add(new Point(x - 1, y - 1));

            return nodes;
        }

        /// <summary>
        /// Calculates the real coordinates of the center of the
        /// grid square that corresponds to the given grid coordinates.
        /// </summary>
        /// <returns>The corresponding real coordinates.</returns>
        private static Vector2 GridToCoords(int x, int y)
        {
            return new Vector2(x, y) * SquareSize +
                new Vector2(SquareSize / 2, SquareSize / 2);
        }

        /// <summary>
        /// Calculates the graph node coordinates corresponding to a vector
        /// given in real coordinates. These are the real coordinates of
        /// the centre of the grid square in which the input vector
        /// is located.
        /// </summary>
        /// <param name="coords">A vector located within the map area.</param>
        /// <returns>The real coordinates of the corresponding square centre.</returns>
        private static Vector2 CoordsToGraphNode(Vector2 coords)
        {
            var x = (int)coords.X;
            var y = (int)coords.Y;
            x = x - (x % SquareSize) + (SquareSize / 2);
            y = y - (y % SquareSize) + (SquareSize / 2);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Calculates the grid coordinates of the given graph node
        /// coordinates. The graph node coordinates must be the real
        /// coordinates of the centre of a grid square.
        /// </summary>
        /// <param name="coords">A graph node vector.</param>
        /// <returns>The grid coordinates of the corresponding
        /// grid square.</returns>
        private static Point GraphNodeCoordsToGrid(Vector2 coords)
        {
            return new Point(
                (int)Math.Floor(coords.X / SquareSize),
                (int)Math.Floor(coords.Y / SquareSize));
        }

        /// <summary>
        /// Returns the grid coordinates of the square in which
        /// the <code>coords</code> vector is located.
        /// </summary>
        /// <param name="coords">A vector in world coordinates.</param>
        /// <returns>The grid index corresponding to the square in
        /// which <code>coords</code> is located.</returns>
        private static Point CoordsToGrid(Vector2 coords)
        {
            var x = (int)coords.X;
            var y = (int)coords.Y;
            x = x - (x % SquareSize) + (SquareSize / 2);
            y = y - (y % SquareSize) + (SquareSize / 2);
            return new Point(x / SquareSize, y / SquareSize);
        }

        /// <summary>
        /// Returns the grid rectangle with the specified
        /// grid coordinates.
        /// </summary>
        /// <param name="col">The grid column.</param>
        /// <param name="row">The grid row.</param>
        /// <returns>The rectangle corresponding to the grid
        /// square, in world coordinates.</returns>
        private static Rectangle GridRectangle(int col, int row)
        {
            return new Rectangle(
                col * SquareSize,
                row * SquareSize,
                SquareSize,
                SquareSize);
        }

        /// <summary>
        /// Validates the position using map width and height.
        /// </summary>
        /// <param name="position">Position to validate</param>
        /// <returns>Validated position not out of map</returns>
        private Vector2 ValidatePosition(Vector2 position)
        {
            var nextPos = position;
            if (position.X < 0)
            {
                nextPos.X = 0;
            }
            else if (position.X >= mMapCollision.GetWidth())
            {
                nextPos.X = mMapCollision.GetWidth() - 1;
            }
            if (position.Y < 0)
            {
                nextPos.Y = 0;
            }
            else if (position.Y >= mMapCollision.GetHeight())
            {
                nextPos.Y = mMapCollision.GetHeight() - 1;
            }
            return nextPos;
        }

        /// <inheritdoc />
        public void Destroy()
        {
            mMapCollision = null;
            mSpatialCache = null;
            mWalkable = null;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            for (var i = 0; i < MaxQueriesPerUpdate; i++)
            {
                if (mTaskQueue.Count == 0)
                    break;

                mTaskQueue.Dequeue().Start();
            }
        }
    }
}
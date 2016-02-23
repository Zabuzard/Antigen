using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Collision
{
    /// <summary>
    /// Cache for collidable objects that speeds up collision detection
    /// via spatial indexing.
    /// 
    /// Objects added to this cache may not use their bounding box
    /// when checking for equality.
    /// </summary>
    [Serializable]
    sealed class UnitQuadtree : ISpatialCache, IObjectCollisionContainer
    {
        /// <summary>
        /// Default value for the minimum node size.
        /// </summary>
        private const int DefaultMinSize = 70;

        /// <summary>
        /// The tree's root node.
        /// </summary>
        private readonly Node mTree;
        /// <summary>
        /// Associates objects added to this tree with the node(s) they
        /// are contained in.
        /// </summary>
        private readonly IDictionary<ICollidable, IList<Node>> mNodeCache;
        /// <summary>
        /// The list of leaves. May be updated whenever an object is
        /// added, removed or updated.
        /// </summary>
        private readonly ICollection<Node> mLeafCache;

        /// <summary>
        /// Creates a new tree whose spatial index spans the indicated
        /// area. Coordinates are assumed to range from <code>(0,0)</code>
        /// to <code>(width, height)</code>. Adding an object which has
        /// coordinates outside this range, or updating an object to have
        /// coordinates outside this range, is an error.
        /// </summary>
        /// <param name="width">Area width.</param>
        /// <param name="height">Area height.</param>
        public UnitQuadtree(int width, int height)
            : this(width, height, DefaultMinSize)
        {
        }

        /// <summary>
        /// Internal constructor for testing purposes.
        /// </summary>
        internal UnitQuadtree() : this(4000, 4000, 400)
        {   
        }

        /// <summary>
        /// Private base constructor.
        /// </summary>
        /// <param name="width">Area width.</param>
        /// <param name="height">Area height.</param>
        /// <param name="minSize">Minimum size of nodes.</param>
        private UnitQuadtree(int width, int height, int minSize)
        {
            mNodeCache = new Dictionary<ICollidable, IList<Node>>();
            mLeafCache = new HashSet<Node>();
            mTree = new Node(new Rectangle(0, 0, width, height),
                mNodeCache, mLeafCache, minSize);
        }

        /// <inheritdoc />
        public void Add(ICollidable obj)
        {
            mTree.Add(obj);
        }

        /// <inheritdoc />
        public void Remove(ICollidable obj)
        {
            mTree.RemoveDown(obj);
        }

        /// <summary>
        /// List of buckets where each bucket contains one or more
        /// objects that may collide with each other. Objects
        /// in different buckets are guaranteed not to collide with
        /// each other.
        /// </summary>
        /// <returns>The list of collision candidates.</returns>
        public IEnumerable<IEnumerable<ICollidable>> CollisionBuckets()
        {
            return mLeafCache.Select(leaf => leaf.NonVirtualObjects.Cast<ICollidable>()).
                Where(objs => objs.Any());
        }

        /// <inheritdoc />
        public IEnumerable<ICollidable> Collisions(ICollidable obj, PairwiseCollisionDetection cd)
        {
            return CollisionBucket(obj).
                Where(obj2 => !obj2.IsVirtualCollidable && !obj2.Equals(obj) && cd(obj, obj2));
        }

        /// <inheritdoc />
        public IEnumerable<ICollidable> CollidableObjectsInArea(Rectangle area)
        {
            var set = new HashSet<ICollidable>();
            mTree.ObjectsInArea(area, set);
            return set;
        }

        /// <inheritdoc />
        public IEnumerable<Unit> UnitsInArea(Rectangle area)
        {
            return CollidableObjectsInArea(area).OfType<Unit>();
        }

        /// <inheritdoc />
        public void Update(ICollidable obj)
        {
            if (obj.Position.Equals(obj.OldPosition))
                return;

            IList<Node> nodes;
            mNodeCache.TryGetValue(obj, out nodes);
            if (nodes == null || nodes.Count == 0)
            {
                Add(obj);
                return;
            }

            var nodesCopy = new Node[nodes.Count];
            nodes.CopyTo(nodesCopy, 0);

            for (var i = 1; i < nodesCopy.Length; i++)
                nodesCopy[i].RemoveUp(obj);

            nodesCopy[0].BubbleUp(obj);
        }

        /// <summary>
        /// Checks the tree for consistency. This is only
        /// intended for debugging purposes.
        /// </summary>
        public void ConsistencyCheck()
        {
            mTree.ConsistencyCheck();

            foreach (var pair in mNodeCache)
            {
                foreach (var node in pair.Value)
                    if (!node.NonVirtualObjects.Contains(pair.Key))
                        throw new Exception("Inconsistent node cache.");
            }
        }

        /// <summary>
        /// Returns a list of collision candidates for the specified object.
        /// 
        /// If the object has not been added to the collision cache, an
        /// empty list is returned.
        /// </summary>
        /// <param name="obj">Any collidable object.</param>
        /// <returns>A list of objects that may collide with the specified
        /// object. Includes <code>obj</code> itself.</returns>
        private IEnumerable<ICollidable> CollisionBucket(ICollidable obj)
        {
            IList<Node> nodes;
            mNodeCache.TryGetValue(obj, out nodes);

            return nodes == null
                ? new ICollidable[0]
                : mNodeCache[obj].SelectMany(node => node.NonVirtualObjects);
        }

        /// <summary>
        /// Quadtree node.
        /// </summary>
        [Serializable]
        private sealed class Node
        {
            /// <summary>
            /// The node's parent.
            /// </summary>
            private readonly Node mParent;
            /// <summary>
            /// Child nodes. This array has length 0 for leaves
            /// (which have no children) and length 4 for all other nodes.
            /// </summary>
            private readonly Node[] mChildren;
            /// <summary>
            /// Area that this node spans.
            /// </summary>
            private readonly Rectangle mBounds;
            /// <summary>
            /// Virtual subset of objects added to this tree.
            /// </summary>
            private readonly List<ICollidable> mVirtualObjects;
            /// <summary>
            /// Non-virtual subset of objects added to this tree.
            /// </summary>
            private readonly List<ICollidable> mNonVirtualObjects;  
            /// <summary>
            /// The central node cache that is used for object-to-node lookup.
            /// </summary>
            private readonly IDictionary<ICollidable, IList<Node>> mNodeCache;
            /// <summary>
            /// The central leaf cache that holds all leaves.
            /// </summary>
            private readonly ICollection<Node> mLeafCache;
            /// <summary>
            /// Whether the node is a leaf. A node is a leaf iff it
            /// has no children.
            /// </summary>
            private readonly bool mLeaf;
            /// <summary>
            /// Whether this node is the tree's node, i.e. has no children.
            /// </summary>
            private readonly bool mRoot;
            /// <summary>
            /// The objects held by the node. DO NOT
            /// manipulate this list.
            /// </summary>
            public IList<ICollidable> NonVirtualObjects
            {
                get { return mNonVirtualObjects; }
            }

            /// <summary>
            /// Creates a collision tree, returning the root node. The whole tree
            /// is created at once but is initally empty.
            /// </summary>
            /// <param name="bounds">The area spanned by the tree.</param>
            /// <param name="nodeCache">A cache that associates objects with
            /// the list of nodes they are contained in. This collection is changed
            /// whenever the tree is updated.</param>
            /// <param name="leafCache">A cache that always contains exactle the virtual
            /// leaves of the whole tree.</param>
            /// <param name="minSize">The minimum area size of individual nodes.</param>
            public Node(Rectangle bounds, IDictionary<ICollidable, IList<Node>> nodeCache,
                ICollection<Node> leafCache, int minSize)
                : this(null, bounds, nodeCache, leafCache, minSize)
            {
            }

            /// <summary>
            /// Creates a tree node.
            /// </summary>
            /// <param name="parent">The node's parent. <code>null</code> if the created node
            /// should become the root of a tree.</param>
            /// <param name="bounds">The node's bounds.</param>
            /// <param name="nodeCache">A cache that associates objects with
            /// the list of nodes they are contained in. This collection is changed
            /// whenever the created node is updated.</param>
            /// <param name="leafCache">A cache that always contains exactle the virtual
            /// leaves of the whole tree.</param>
            /// <param name="minSize">The minimum area size of individual nodes.</param>
            private Node(Node parent, Rectangle bounds, IDictionary<ICollidable, IList<Node>> nodeCache,
                ICollection<Node> leafCache, int minSize)
            {
                mParent = parent;
                mRoot = parent == null;
                mBounds = bounds;
                mVirtualObjects = new List<ICollidable>();
                mNonVirtualObjects = new List<ICollidable>();
                mNodeCache = nodeCache;
                mLeafCache = leafCache;

                var x = mBounds.X;
                var y = mBounds.Y;
                var leftWidth = (int)Math.Ceiling((double)mBounds.Width / 2); // Width of left child nodes.
                var rightWidth = (int)Math.Floor((double)mBounds.Width / 2); // Width of right child nodes.
                var topHeight = (int)Math.Ceiling((double)mBounds.Height / 2); // Height of top child nodes.
                var bottomHeight = (int)Math.Floor((double)mBounds.Height / 2); // Height of bottom child nodes.

                mLeaf = false;
                if (rightWidth < minSize || bottomHeight < minSize)
                {
                    mChildren = new Node[0];
                    mLeaf = true;
                    leafCache.Add(this);
                    return;
                }

                var topLeftChild =
                    new Node(this, new Rectangle(x, y, leftWidth, topHeight),
                        nodeCache, leafCache, minSize);
                var topRightChild =
                    new Node(this, new Rectangle(x + leftWidth, y, rightWidth, topHeight),
                        nodeCache, leafCache, minSize);
                var bottomLeftChild =
                    new Node(this, new Rectangle(x, y + topHeight, leftWidth, bottomHeight),
                        nodeCache, leafCache, minSize);
                var bottomRightChild =
                    new Node(this, new Rectangle(x + leftWidth, y + topHeight, rightWidth, bottomHeight),
                        nodeCache, leafCache, minSize);
                mChildren = new[] {topLeftChild, topRightChild, bottomLeftChild, bottomRightChild};
            }

            /// <summary>
            /// Adds <code>obj</code> to this subtree.
            /// </summary>
            /// <param name="obj">An object whose bounding box is
            /// contained in the are spanned by this subtree.</param>
            /// <exception cref="UnitQuadtreeException">if <code>obj</code>
            /// is (partially) outside the tree area.</exception>
            public void Add(ICollidable obj)
            {
                TrickleDown(obj);
            }

            /// <summary>
            /// Removes <code>obj</code> from the subtree whose root
            /// this node is. If <code>obj</code> is not present in
            /// the tree, nothing happens.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            public void RemoveDown(ICollidable obj)
            {
                if (mLeaf)
                    RemoveObject(obj);
                else
                    foreach (var child in mChildren)
                        child.RemoveDown(obj);
            }

            /// <summary>
            /// Removes <code>obj</code> from this node.
            /// If <code>obj</code> is not contained in this
            /// node, nothing happens.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            public void RemoveUp(ICollidable obj)
            {
                RemoveObject(obj);
            }

            /// <summary>
            /// Removes <code>obj</code> from this node and
            /// trickles it down again starting at the first
            /// valid predecessor of this node. This method
            /// can be used to update an object's position.
            /// 
            /// If <code>obj</code> is wholly contained in
            /// this object's bounds, nothing happens.
            /// </summary>
            /// <param name="obj"></param>
            public void BubbleUp(ICollidable obj)
            {
                // ReSharper hint: Rectangle.Contains is actually pure.
                // ReSharper disable ImpureMethodCallOnReadonlyValueField
                if (mRoot || mBounds.Contains(obj.Hitbox))
                // ReSharper restore ImpureMethodCallOnReadonlyValueField
                    return;

                RemoveUp(obj);
                TrickleDownFromFirstValidParent(obj);
            }

            /// <summary>
            /// Retrieves all non-virtual objects whose hitboxes intersect with the given
            /// area and which are stored in this subtree.
            /// </summary>
            /// <param name="area">The area to search in.</param>
            /// <param name="objs">The set of objects that has already been
            /// found.</param>
            public void ObjectsInArea(Rectangle area, ISet<ICollidable> objs)
            {
                if (!area.Intersects(mBounds))
                    return;

                if (mLeaf)
                {
                    foreach (var obj in mNonVirtualObjects)
                    {
                        if (area.Intersects(obj.Hitbox))
                            objs.Add(obj);
                    }
                    return;
                }

                foreach (var child in mChildren)
                    child.ObjectsInArea(area, objs);
            }

            /// <summary>
            /// Adds an object to this subtree, trickling it down
            /// to the all leaf nodes whose areas intersect it.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            private void TrickleDown(ICollidable obj)
            {
                // Case 0: The object doesn't fit into this node's bounds,
                // so there is nothing to do.
                if (!obj.Hitbox.Intersects(mBounds))
                    return;

                // Case 1: This is not a leaf node. Trickle the object down.
                if (!mLeaf)
                {
                    foreach (var child in mChildren)
                        child.TrickleDown(obj);
                    return;
                }
                
                // Case 2: This is a leaf node. Add the object
                AddObject(obj);
            }

            /// <summary>
            /// Recursively finds the first predecessor that wholly contains
            /// <code>obj</code> and trickles <code>obj</code> down from there.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            /// <exception cref="UnitQuadtreeException">if no predecessor
            /// of this node wholly contains <code>obj</code>.</exception>
            private void TrickleDownFromFirstValidParent(ICollidable obj)
            {
                // ReSharper hint: Rectangle.Contains is actually pure.
                // ReSharper disable ImpureMethodCallOnReadonlyValueField
                if (mParent.mBounds.Contains(obj.Hitbox) || mParent.mRoot)
                // ReSharper restore ImpureMethodCallOnReadonlyValueField
                    mParent.TrickleDown(obj);
                else
                    mParent.TrickleDownFromFirstValidParent(obj);
            }

            /// <summary>
            /// Removes <code>obj</code> from this node's objects
            /// and updates the node cache accordingly. If
            /// <code>obj</code> is not contained in this node,
            /// nothing happens.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            private void RemoveObject(ICollidable obj)
            {
                var cache = obj.IsVirtualCollidable ? mVirtualObjects : mNonVirtualObjects;
                if (!cache.Remove(obj))
                    return;

                var nodes = mNodeCache[obj];
                if (nodes.Count == 1) // This is the last node, so remove the whole node list.
                    mNodeCache.Remove(obj);
                else
                    nodes.Remove(this);    
            }

            /// <summary>
            /// Adds an object to this node's object list and
            /// associates this node with the object in the
            /// node caches.
            /// </summary>
            /// <param name="obj">Any collidable object.</param>
            private void AddObject(ICollidable obj)
            {
                var cache = obj.IsVirtualCollidable ? mVirtualObjects : mNonVirtualObjects;
                cache.Add(obj);

                IList<Node> nodes;
                mNodeCache.TryGetValue(obj, out nodes);
                if (nodes != null)
                    nodes.Add(this);
                else
                    mNodeCache[obj] = new List<Node> { this };
            }

            /// <summary>
            /// Checks a number of invariants hold in this subtree.
            /// Intended for testing and debugging purposes.
            /// </summary>
            public void ConsistencyCheck()
            {
                if (mLeaf != mLeafCache.Contains(this))
                    throw new UnitQuadtreeException("Inconsistent leaf cache.");

                // ReSharper hint: Rectangle.Contains is actually pure.
                // ReSharper disable ImpureMethodCallOnReadonlyValueField
                if (!mLeaf && mChildren.Any(child => !mBounds.Contains(child.mBounds)))
                // ReSharper restore ImpureMethodCallOnReadonlyValueField
                    throw new UnitQuadtreeException("Parent node bounds don't contain at least one of the child nodes' bounds.");

                foreach (var obj in mVirtualObjects)
                {
                    if (!mNodeCache[obj].Contains(this))
                        throw new UnitQuadtreeException("Inconsistent node cache.");
                    if (!obj.Hitbox.Intersects(mBounds))
                        throw new UnitQuadtreeException("Object in node with incorrect bounds.");
                }
            }
        }
    }

    /// <summary>
    /// Exception used to indicate an error with the unit collision tree.
    /// </summary>
    internal sealed class UnitQuadtreeException : Exception
    {
        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnitQuadtreeException(string message) : base(message)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Content;
using Antigen.Input;
using Antigen.Logic.Collision;
using Antigen.Logic.Pathfinding;
using Antigen.Logic.Selection;
using Antigen.Objects.Units;
using IDrawable = Antigen.Graphics.IDrawable;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.Objects
{
    /// <summary>
    /// Provides caches of game object, using data structures optimised
    /// for certain tasks. The caches are synchronised with each other
    /// automatically.
    /// 
    /// This class's <code>ICollection</code> instance provides access to
    /// the collection of all game objects in no specific order.
    /// </summary>
    [Serializable]
    sealed class ObjectCaches
    {
        /// <summary>
        /// The game's input dispatcher.
        /// </summary>
        private readonly InputDispatcher mInputDispatcher;
        /// <summary>
        /// Objects queued up for deferred addition to the caches.
        /// </summary>
        private readonly IList<object> mDeferredAdded;
        /// <summary>
        /// Objects queued up for deferred removal from the caches.
        /// </summary>
        private readonly IList<object> mDeferredRemoved;

        /// <summary>
        /// List of all game objects currently present in any
        /// of the caches.
        /// </summary>
        private IList<object> List { get; set; }

        /// <summary>
        /// List of all drawable game objects.
        /// </summary>
        public IList<IDrawable> ListDrawable { get; private set; }

        /// <summary>
        /// List of all updateable game objects.
        /// </summary>
        public IList<IUpdateable> ListUpdateable { get; private set; }

        /// <summary>
        /// List of all game objects which need to initially load some
        /// resources.
        /// </summary>
        public IList<ILoad> ListLoad { get; private set; }
        
        /// <summary>
        /// List of all units currently present in the game.
        /// </summary>
        public IList<Unit> ListUnit { get; private set; }

        /// <summary>
        /// List of all friendly units currently present in the game.
        /// </summary>
        public IList<Unit> ListFriendly { get; private set; }

        /// <summary>
        /// List of all red blood cells currently present in the game.
        /// </summary>
        public IList<RedBloodcell> ListRedBloodcell { get; private set; }

        /// <summary>
        /// List of all enemy units.
        /// </summary>
        public IList<Unit> ListEnemy { get; private set; }

        /// <summary>
        /// List of selectable units.
        /// </summary>
        public IList<ISelectable> ListSelectable { get; private set; } 

        /// <summary>
        /// Cache that efficiently provides collision information.
        /// </summary>
        public IObjectCollisionContainer ObjectCollision { get; private set; }

        /// <summary>
        /// Cache that supports efficient spatial queries. Note that
        /// objects are added to this cache automatically only if they
        /// are also <see cref="ICollidable"/>.
        /// </summary>
        public ISpatialCache SpatialCache { get; private set; }

        /// <summary>
        /// The game's pathfinder, which caches map information to provide
        /// accelerated pathfinding.
        /// </summary>
        public Pathfinder Pathfinder
        {
            get { return mPathfinder; }
            set { mPathfinder = value; }
        }

        [NonSerialized]
        private Pathfinder mPathfinder;

        /// <summary>
        /// The game's selection manager.
        /// </summary>
        private readonly SelectionManager mSelectionManager;

        private Map.Map mCurrentMap;

        /// <summary>
        /// The map currently in use. Setting this value will also update
        /// the <see cref="SpatialCache"/> to use the provided
        /// map.
        /// </summary>
        public Map.Map CurrentMap
        {
            get { return mCurrentMap; }
            set
            {
                mCurrentMap = value;
                var cache = new UnitQuadtree(value.GetWidth(), value.GetHeight());
                ObjectCollision = cache;
                SpatialCache = cache;
                
                foreach (var collidable in List.OfType<ICollidable>())
                {
                    ObjectCollision.Add(collidable);
                }

                Pathfinder = new Pathfinder(mCurrentMap, SpatialCache);
            }
        }

        /// <summary>
        /// Creates empty object caches.
        /// </summary>
        /// <param name="inputDispatcher">The game's input dispatcher. Game objects
        /// which implement any of the listener interfaces are registered to receive
        /// the corresponding events from this dispatcher.</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        public ObjectCaches(InputDispatcher inputDispatcher, SelectionManager selectionManager)
        {
            mInputDispatcher = inputDispatcher;
            mSelectionManager = selectionManager;

            mDeferredAdded = new List<object>();
            mDeferredRemoved = new List<object>();

            List = new List<object>();
            ListDrawable = new List<IDrawable>();
            ListUpdateable = new List<IUpdateable>();
            ListLoad = new List<ILoad>();
            ListUnit = new List<Unit>();
            ListFriendly = new List<Unit>();
            ListRedBloodcell = new List<RedBloodcell>();
            ListEnemy = new List<Unit>();
            ListSelectable = new List<ISelectable>();
        }

        /// <summary>
        /// Adds <code>obj</code> to all caches it belongs into.
        /// </summary>
        /// <param name="obj">Any game object.</param>
        private void RealAdd(object obj)
        {
            //Add new caches here!
            List.Add(obj);

            var drawable = obj as IDrawable;
            if (drawable != null)
                ListDrawable.Add(drawable);

            var updateable = obj as IUpdateable;
            if (updateable != null)
                ListUpdateable.Add(updateable);

            var load = obj as ILoad;
            if (load != null)
                ListLoad.Add(load);

            var unit = obj as Unit;
            if (unit != null)
            {
                ListUnit.Add(unit);

                switch (unit.GetSide())
                {
                    case Unit.UnitSide.Enemy:
                        ListEnemy.Add(unit);
                        break;
                    case Unit.UnitSide.Friendly:
                        ListFriendly.Add(unit);
                        break;
                }
            }

            var red = obj as RedBloodcell;
            if (red != null)
                ListRedBloodcell.Add(red);

            var collidable = obj as IObjectCollidable;
            if (collidable != null)
                ObjectCollision.Add(collidable);

            var selectable = obj as ISelectable;
            if (selectable != null)
                ListSelectable.Add(selectable);

            mInputDispatcher.RegisterListener(obj);
        }

        /// <summary>
        /// Removes <code>obj</code> from all caches it was present in.
        /// </summary>
        /// <param name="obj">Any game object.</param>
        private void RealRemove(object obj)
        {
            //Add new caches here!
            var drawable = obj as IDrawable;
            if (drawable != null)
                ListDrawable.Remove(drawable);

            var updateable = obj as IUpdateable;
            if (updateable != null)
                ListUpdateable.Remove(updateable);

            var load = obj as ILoad;
            if (load != null)
                ListLoad.Remove(load);

            var unit = obj as Unit;
            if (unit != null)
            {
                ListUnit.Remove(unit);

                switch (unit.GetSide())
                {
                    case Unit.UnitSide.Enemy:
                        ListEnemy.Remove(unit);
                        break;
                    case Unit.UnitSide.Friendly:
                        ListFriendly.Remove(unit);
                        break;
                }
            }

            var red = obj as RedBloodcell;
            if (red != null)
                ListRedBloodcell.Remove(red);

            var collidable = obj as IObjectCollidable;
            if (collidable != null)
                ObjectCollision.Remove(collidable);

            var selectable = obj as ISelectable;
            if (selectable != null)
            {
                ListSelectable.Remove(selectable);
                mSelectionManager.Remove(selectable);
            }
                
            List.Remove(obj);
            mInputDispatcher.DeregisterListener(obj);
        }

        /// <summary>
        /// Adds a game object to the object caches. The object is
        /// added to each relevant cache automatically, depending on
        /// which interfaces it implements.
        /// 
        /// Additionally, if the object implements one of the listener
        /// interfaces, it is registered to receive the corresponding
        /// events.
        /// 
        /// Note that some caches may allow duplicate objects while
        /// others may not.
        /// 
        /// Note that the addition is deferred until
        /// <see cref="ApplyDeferredUpdates"/> is called. This means that,
        /// in a single-threaded setting,
        /// immediately after this method finishes, the caches will not
        /// contain <code>obj</code>.
        /// </summary>
        /// <param name="obj">A new game object.</param>
        public void Add(object obj)
        {
            mDeferredAdded.Add(obj);
        }

        /// <summary>
        /// Removes an object from the game caches. The object is
        /// deleted from each cache it is currently present in.
        /// 
        /// Additionally, if the object implements one of the listener
        /// interfaces, it is unregistered.
        /// 
        /// Note that if duplicates of <code>obj</code> are present
        /// in any of the caches, they may not all be removed.
        /// 
        /// Note that the removal is deferred until
        /// <see cref="ApplyDeferredUpdates"/> is called. This means that,
        /// in a single-threaded setting,
        /// immediately after this method finishes, the caches will still
        /// contain <code>obj</code>.
        /// </summary>
        /// <param name="obj">Any game object.</param>
        /// <returns>Whether the element was found in and removed from
        /// any of the caches.</returns>
        public void Remove(object obj)
        {
            mDeferredRemoved.Add(obj);
        }

        /// <summary>
        /// Applies all deferred updates queued by calls to
        /// <see cref="Add"/> and <see cref="Remove"/>.
        /// </summary>
        public void ApplyDeferredUpdates()
        {
            foreach (var obj in mDeferredAdded)
                RealAdd(obj);

            mDeferredAdded.Clear();

            foreach (var obj in mDeferredRemoved)
                RealRemove(obj);

            mDeferredRemoved.Clear();
        }
    }
}

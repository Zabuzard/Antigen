using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Graphics;
using Antigen.Logic.Collision;
using Antigen.Objects;
using Antigen.Objects.Units;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.Input
{
    /// <summary>
    /// Enumeration of abstract actions which are
    /// assignable to keys.
    /// </summary>
    internal enum UserAction
    {
        NoAction,
        BuildAntibody,
        BuildBcell,
        BuildMacrophage,
        BuildRedBloodcell,
        BuildStemcell,
        BuildTCell,
        SelectAttackMode,
        SelectCellDivisionMode,
        SelectFlowMode,
        SelectDefensiveMode
    }

    /// <summary>
    /// Provides information for a click event
    /// like location of the click or objects there.
    /// </summary>
    internal sealed class ClickInfo
    {
        /// <summary>
        /// The button that was clicked.
        /// </summary>
        public MouseButtons Button { get; private set; }

        /// <summary>
        /// Location of the click.
        /// </summary>
        public Coord<Point> Location { get; private set; }

        /// <summary>
        /// Game objects at this location.
        /// </summary>
        public Unit[] ObjectsUnderCursor { get; private set; }

        /// <summary>
        /// Creates a new click info with location and several objects there.
        /// </summary>
        /// <param name="button">The button that was clicked.</param>
        /// <param name="location">Location of the click.</param>
        /// <param name="objectsUnderCursor">Game objects at this location</param>
        public ClickInfo(MouseButtons button, Coord<Point> location, Unit[] objectsUnderCursor)
        {
            Button = button;
            Location = location;
            ObjectsUnderCursor = objectsUnderCursor;
        }
    }

    /// <summary>
    /// Dispatches input 'events' to listeners. This class serves the following
    /// purposes:
    /// 
    /// <list type="number">
    /// <item>Control the order in which events are dispatched to listeners using
    /// <see cref="IEventListener.EventOrder"/>.</item>
    /// <item>Allow objects to specify that events should not be dispatched to other objects
    /// using the return value of the <code>Handle*</code> methods
    /// (f.ex. <see cref="ILeftClickListener.HandleLeftClick"/>).</item>
    /// <item>Collect supplementary information for the events and dispatch this info to the
    /// listeners.</item>
    /// </list>
    /// </summary>
    [Serializable]
    internal sealed class InputDispatcher : IDestroy, IUpdateable
    {
        /// <summary>
        /// If two clicks are apart at most this amount of milliseconds,
        /// they are counted as a double-click.
        /// </summary>
        private const int DoubleClickElapsedTimeMillis = 500;

        /// <summary>
        /// Indicates whether the user is currently dragging with the
        /// left mouse button held down.
        /// </summary>
        private bool mLeftDragging;
        /// <summary>
        /// Indicates whether the user is currently dragging with the
        /// right mouse button held down.
        /// </summary>
        private bool mRightDragging;

        /// <summary>
        /// Timespan since the last left click, in milliseconds. Used
        /// for doubleclick recognition.
        /// </summary>
        private long mElapsedTimeSinceLastClick;
        /// <summary>
        /// Left click listeners.
        /// </summary>
        private readonly SortedList<ILeftClickListener> mLeftClickListeners;
        /// <summary>
        /// Right click listeners.
        /// </summary>
        private readonly SortedList<IRightClickListener> mRightClickListeners;
        /// <summary>
        /// Character event listeners.
        /// </summary>
        private readonly SortedList<ICharListener> mCharListeners;
        /// <summary>
        /// Mouse movements listeners.
        /// </summary>
        private readonly SortedList<IMouseMoveListener> mMouseMoveListeners;
        /// <summary>
        /// Key listeners.
        /// </summary>
        private readonly SortedList<IKeyListener> mKeyListeners;
        /// <summary>
        /// Mouse wheel listeners.
        /// </summary>
        private readonly SortedList<IMouseWheelListener> mMouseWheelListeners;
        /// <summary>
        /// Dragging listeners.
        /// </summary>
        private readonly SortedList<IDragListener> mDragListeners;
        /// <summary>
        /// Action listeners.
        /// </summary>
        private readonly SortedList<IActionListener> mActionListeners;
        /// <summary>
        /// Double click listeners.
        /// </summary>
        private readonly SortedList<IDoubleClickListener> mDoubleClickListeners; 
        /// <summary>
        /// Queue of deferred listener additions.
        /// </summary>
        private readonly ICollection<object> mDeferredAdd;
        /// <summary>
        /// Queue of deferred listener removals.
        /// </summary>
        private readonly ICollection<object> mDeferredRemove;

        /// <summary>
        /// The game's input manager which this class receives events from.
        /// </summary>
        [NonSerialized]
        private IInputService mInput;
        /// <summary>
        /// Mapping from characters entered to actions performed. Used for
        /// generating action events.
        /// </summary>
        private IDictionary<char, UserAction> mKeyMap;

        /// <summary>
        /// Object that can be used to locate objects at a certain location.
        /// MUST be set before using the input dispatcher.
        /// </summary>
        public ISpatialCache UnitLocator { private get; set; }

        /// <summary>
        /// Coordinate translation function used by the dispatcher in calculating
        /// the absolute coordinates of mouse events.
        /// </summary>
        public ICoordTranslation CoordTranslation { private get; set; }

        /// <summary>
        /// Creates a new InputDispatcher.
        /// 
        /// <b>Note:</b> <see cref="ICoordTranslation"/> must be set to a non-null
        /// value before using this class.
        /// </summary>
        /// <param name="input">Underlying input services which provide low-level
        /// input events.</param>
        /// <param name="keyMap">Key map to be used for translating characters entered
        /// by the user to game actions.</param>
        public InputDispatcher(IInputService input, IDictionary<char, UserAction> keyMap)
        {
            mInput = input;
            mKeyMap = keyMap;

            var comparer = new EventListenerComparer();
            mLeftClickListeners = new SortedList<ILeftClickListener>(comparer);
            mRightClickListeners = new SortedList<IRightClickListener>(comparer);
            mCharListeners = new SortedList<ICharListener>(comparer);
            mMouseMoveListeners = new SortedList<IMouseMoveListener>(comparer);
            mKeyListeners = new SortedList<IKeyListener>(comparer);
            mMouseWheelListeners = new SortedList<IMouseWheelListener>(comparer);
            mDragListeners = new SortedList<IDragListener>(comparer);
            mActionListeners = new SortedList<IActionListener>(comparer);
            mDoubleClickListeners = new SortedList<IDoubleClickListener>(comparer);

            mDeferredAdd = new List<object>();
            mDeferredRemove = new List<object>();

            input.GetMouse().MouseButtonPressed += OnMouseButtonPressed;
            input.GetMouse().MouseButtonReleased += OnMouseButtonReleased;
            input.GetMouse().MouseMoved += OnMouseMoved;
            input.GetMouse().MouseWheelRotated += OnMouseWheelRotated;
            input.GetKeyboard().KeyPressed += OnKeyPress;
            input.GetKeyboard().KeyReleased += OnKeyRelease;
            input.GetKeyboard().CharacterEntered += OnCharacterEntered;
        }

        /// <summary>
        /// Register delegates at the inputmanager after loading an old game state.
        /// </summary>
        /// <param name="input">The inputmanager of the game.</param>
        /// <param name="keyMap">The keymap as it is currently set.</param>
        public void LoadGameState(IInputService input, IDictionary<char, UserAction> keyMap)
        {
            mKeyMap = keyMap;
            mInput = input;

            input.GetMouse().MouseButtonPressed += OnMouseButtonPressed;
            input.GetMouse().MouseButtonReleased += OnMouseButtonReleased;
            input.GetMouse().MouseMoved += OnMouseMoved;
            input.GetMouse().MouseWheelRotated += OnMouseWheelRotated;
            input.GetKeyboard().KeyPressed += OnKeyPress;
            input.GetKeyboard().KeyReleased += OnKeyRelease;
            input.GetKeyboard().CharacterEntered += OnCharacterEntered;

            ApplyDeferredUpdates();
        }

        /// <summary>
        /// Dispatches mouse wheel events.
        /// </summary>
        /// <param name="ticks">Number of ticks the mouse wheel has been rotated.</param>
        private void OnMouseWheelRotated(float ticks)
        {
            foreach (var listener in mMouseWheelListeners)
                if (listener.HandleMouseWheelRotated(ticks))
                    break;
        }

        /// <summary>
        /// Dispatches character events and action events.
        /// </summary>
        /// <param name="character">The character that was entered.</param>
        private void OnCharacterEntered(char character)
        {
            foreach (var listener in mCharListeners)
                if (listener.HandleCharEntered(character))
                    break;

            UserAction act;
            mKeyMap.TryGetValue(character, out act);
            if (act.Equals(UserAction.NoAction))
                return;

            foreach (var listener in mActionListeners)
                if (listener.HandleActionPerformed(act))
                    break;
        }

        /// <summary>
        /// Dispatches key press events.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        private void OnKeyPress(Keys key)
        {
            foreach (var listener in mKeyListeners)
                if (listener.HandleKeyPress(key))
                    break;
        }

        /// <summary>
        /// Dispatches key release events.
        /// </summary>
        /// <param name="key">The key that was released.</param>
        private void OnKeyRelease(Keys key)
        {
            foreach (var listener in mKeyListeners)
                if (listener.HandleKeyRelease(key))
                    break;
        }

        /// <summary>
        /// Dispatches mouse movements and dragging events.
        /// </summary>
        /// <param name="x">X coordinate of the movement end point.</param>
        /// <param name="y">Y coordinate of the movement end point.</param>
        private void OnMouseMoved(float x, float y)
        {
            if (x < 0 || y < 0)
                return;

            var location = Coord<Point>.MakeCoord(new Point((int) x, (int) y), CoordTranslation);
            foreach (var listener in mMouseMoveListeners)
                if (listener.HandleMouseMove(location))
                    break;

            if (mLeftDragging)
                foreach (var listener in mDragListeners)
                    if (listener.HandleDragging(MouseButtons.Left, location))
                        break;

            if (mRightDragging)
                foreach (var listener in mDragListeners)
                    if (listener.HandleDragging(MouseButtons.Right, location))
                        break;
        }

        /// <summary>
        /// Dispatches dragging events.
        /// </summary>
        /// <param name="button">The pressed mouse button.</param>
        private void OnMouseButtonPressed(MouseButtons button)
        {
            if (button != MouseButtons.Left && button != MouseButtons.Right)
                return;

            var info = MakeClickInfo(button);

            switch (button)
            {
                case MouseButtons.Left:
                    mLeftDragging = true;
                    break;
                case MouseButtons.Right:
                    mRightDragging = true;
                    break;
            }

            foreach (var listener in mDragListeners)
                if (listener.HandleDragStarted(info))
                    break;
        }

        /// <summary>
        /// Dispatches left or right click events, depending on which button was released,
        /// as well as dragging events.
        /// </summary>
        /// <param name="button">The released button.</param>
        private void OnMouseButtonReleased(MouseButtons button)
        {
            var info = MakeClickInfo(button);

            // Drag events
            switch (button)
            {
                case MouseButtons.Left:
                    mLeftDragging = false;
                    break;
                case MouseButtons.Right:
                    mRightDragging = false;
                    break;
            }

            foreach (var listener in mDragListeners)
                if (listener.HandleDragStopped(info))
                    break;

            // Click events
            switch (button)
            {
                case MouseButtons.Left:
                    foreach (var listener in mLeftClickListeners)
                        if (listener.HandleLeftClick(info))
                            break;
                    ProcessDoubleClick(info);
                    break;
                case MouseButtons.Right:
                    foreach (var listener in mRightClickListeners)
                        if (listener.HandleRightClick(info))
                            break;
                    break;
            }

            // Double click elapsed time reset
            mElapsedTimeSinceLastClick = 0;
        }

        /// <summary>
        /// Dispatches a double click info to the corresponding listeners if a double
        /// click has occured.
        /// </summary>
        /// <param name="info">Click info.</param>
        private void ProcessDoubleClick(ClickInfo info)
        {
            if (mElapsedTimeSinceLastClick <= 0 || mElapsedTimeSinceLastClick > DoubleClickElapsedTimeMillis)
                return;

            foreach (var listener in mDoubleClickListeners)
                if (listener.HandleDoubleLeftClick(info))
                    break;
        }

        /// <summary>
        /// Collects click information, querying the game world as necessary.
        /// </summary>
        /// <param name="button">Mouse button that was clicked.</param>
        /// <returns>Information about a click.</returns>
        private ClickInfo MakeClickInfo(MouseButtons button)
        {
            var mouseState = mInput.GetMouse().GetState();
            var relativeLocation = new Point(mouseState.X, mouseState.Y);
            var location = Coord<Point>.MakeCoord(relativeLocation, CoordTranslation);

            var objectsAtLoc = LocateObjectsAt(location.Absolute);

            return new ClickInfo(button, location, objectsAtLoc);
        }

        /// <summary>
        /// Retrieves all objects located at the given location.
        /// </summary>
        /// <param name="location">A location.</param>
        /// <returns>A list of game objects located at
        /// <code>location</code>.</returns>
        private Unit[] LocateObjectsAt(Point location)
        {
            var rect = new Rectangle(location.X, location.Y, 1, 1);
            return UnitLocator.UnitsInArea(rect).ToArray();
        }

        /// <summary>
        /// Registers an object for each event type it can process. More precisely,
        /// <code>listener</code> will receive events corresponding to each
        /// <code>*Listener</code> interface it implements.
        /// 
        /// Note that listeners will begin to receive events only after
        /// the first call to <see cref="ApplyDeferredUpdates"/> following
        /// a call to this method.
        /// </summary>
        /// <param name="listener">The listener to register.</param>
        public void RegisterListener(object listener)
        {
            mDeferredAdd.Add(listener);
        }

        /// <summary>
        /// Deregisters a listener, causing it to receive no more events.
        /// 
        /// Note that listeners will stop receiving events only after
        /// the first call to <see cref="ApplyDeferredUpdates"/> following
        /// a call to this method.
        /// 
        /// If <code>listener</code> was not registered to begin with,
        /// this method does nothing.
        /// </summary>
        /// <param name="listener">The listener to deregister.</param>
        public void DeregisterListener(object listener)
        {
            mDeferredRemove.Add(listener);
        }

        /// <summary>
        /// Applies deferred listener additions and removal queued by calls
        /// to <see cref="RegisterListener"/>.
        /// </summary>
        public void ApplyDeferredUpdates()
        {
            foreach (var listener in mDeferredAdd)
                RealAddListener(listener);

            foreach (var listener in mDeferredRemove)
                RealRemoveListener(listener);

            mDeferredAdd.Clear();
            mDeferredRemove.Clear();
        }

        /// <summary>
        /// Adds a listener to all relevant dispatch queues,
        /// depending on which interfaces it implements.
        /// </summary>
        /// <param name="listener">An object implementing
        /// one or more of the <code>*EventListener</code>
        /// interfaces.</param>
        private void RealAddListener(object listener)
        {
            var lcl = listener as ILeftClickListener;
            if (lcl != null) mLeftClickListeners.Add(lcl);

            var rcl = listener as IRightClickListener;
            if (rcl != null) mRightClickListeners.Add(rcl);

            var kl = listener as IKeyListener;
            if (kl != null) mKeyListeners.Add(kl);

            var cl = listener as ICharListener;
            if (cl != null) mCharListeners.Add(cl);

            var mml = listener as IMouseMoveListener;
            if (mml != null) mMouseMoveListeners.Add(mml);

            var mwl = listener as IMouseWheelListener;
            if (mwl != null) mMouseWheelListeners.Add(mwl);

            var dl = listener as IDragListener;
            if (dl != null) mDragListeners.Add(dl);

            var al = listener as IActionListener;
            if (al != null) mActionListeners.Add(al);

            var dcl = listener as IDoubleClickListener;
            if (dcl != null) mDoubleClickListeners.Add(dcl);
        }

        /// <summary>
        /// Removes an event from all dispatch queues it was previously
        /// a part of.
        /// </summary>
        /// <param name="listener">A listener.</param>
        private void RealRemoveListener(object listener)
        {
            var lcl = listener as ILeftClickListener;
            if (lcl != null) mLeftClickListeners.Remove(lcl);

            var rcl = listener as IRightClickListener;
            if (rcl != null) mRightClickListeners.Remove(rcl);

            var kl = listener as IKeyListener;
            if (kl != null) mKeyListeners.Remove(kl);

            var cl = listener as ICharListener;
            if (cl != null) mCharListeners.Remove(cl);

            var mml = listener as IMouseMoveListener;
            if (mml != null) mMouseMoveListeners.Remove(mml);

            var mwl = listener as IMouseWheelListener;
            if (mwl != null) mMouseWheelListeners.Remove(mwl);

            var dl = listener as IDragListener;
            if (dl != null) mDragListeners.Remove(dl);

            var al = listener as IActionListener;
            if (al != null) mActionListeners.Remove(al);

            var dcl = listener as IDoubleClickListener;
            if (dcl != null) mDoubleClickListeners.Remove(dcl);
        }

        /// <summary>
        /// Compares event listeners according to their
        /// <see cref="IEventListener.EventOrder"/>.
        /// </summary>
        [Serializable]
        private sealed class EventListenerComparer : IComparer<IEventListener>
        {
            /// <inheritdoc />
            public int Compare(IEventListener x, IEventListener y)
            {
                return x.EventOrder.CompareTo(y.EventOrder);
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            mInput.GetMouse().MouseButtonPressed -= OnMouseButtonPressed;
            mInput.GetMouse().MouseButtonReleased -= OnMouseButtonReleased;
            mInput.GetMouse().MouseMoved -= OnMouseMoved;
            mInput.GetMouse().MouseWheelRotated -= OnMouseWheelRotated;
            mInput.GetKeyboard().KeyPressed -= OnKeyPress;
            mInput.GetKeyboard().KeyReleased -= OnKeyRelease;
            mInput.GetKeyboard().CharacterEntered -= OnCharacterEntered;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            mElapsedTimeSinceLastClick += gameTime.ElapsedGameTime.Milliseconds;
        }
    }
}

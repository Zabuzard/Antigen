using System;
using Antigen.Content;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = Antigen.Graphics.IDrawable;

namespace Antigen.HUD
{
    /// <summary>
    /// An item of the HUD.
    /// 
    /// Suppresses click and drag start events inside its area.
    /// </summary>
    [Serializable]
    abstract class HudItem: ILoad, IDrawable, ILeftClickListener, IDragListener, IDestroy, IMouseMoveListener
    {
        private Point mWindowBounds;
        protected readonly int mWidth;
        protected readonly int mHeight;
        [NonSerialized]
        private GameWindow mWindow;
        protected Point mWindowPosition;
        private Point mPosition;

        public string Hint { get; set; }

        public bool ShowHint { get; private set; }

        public bool Visible { protected get; set; }

        private Point Position
        {
            get { return mPosition; }
            set
            {
                if (mWindow.ClientBounds.Width != 0 && mWindow.ClientBounds.Height != 0)
                {
                    mWindowPosition.X = (value.X + mWindow.ClientBounds.Width) % mWindow.ClientBounds.Width;
                    mWindowPosition.Y = (value.Y + mWindow.ClientBounds.Height) % mWindow.ClientBounds.Height;
                }            
                mPosition = value;
            }
        }

        /// <summary>
        /// An event that raises if the item is clicked
        /// </summary>
        public event EventHandler<EventArgs> OnClick;

        /// <summary>
        /// Creates a new HUD item
        /// </summary>
        /// <param name="input">The input dispatcher</param>
        /// <param name="window">The window</param>
        /// <param name="position">The position</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        protected HudItem(InputDispatcher input, GameWindow window, Point position, int width, int height)
        {
            mWindowBounds = new Point(window.ClientBounds.Width, window.ClientBounds.Height);
            Visible = true;
            ShowHint = false;
            Hint = "";
            mWidth = width;
            mHeight = height;
            mWindow = window;
            Position = position;
            input.RegisterListener(this);
            window.ClientSizeChanged += OnSizeChanged;
            OnSizeChanged(null, null);
        }

        /// <summary>
        /// Moves the HUD item to the right place if the window size is changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void OnSizeChanged(object sender, EventArgs e)
        {
            Position = Position;
        }

        /// <summary>
        /// Determines whether the given point lies within the bounds of
        /// this HUD element.
        /// </summary>
        /// <param name="point">A location.</param>
        /// <returns><code>true</code> if <code>point</code> denotes a
        /// location in the area this HUD item encompasses;
        /// <code>false</code> otherwise.</returns>
        private bool IntersectsWith(Point point)
        {
            return mWindowPosition.X <= point.X &&
                   mWindowPosition.X + mWidth >= point.X &&
                   mWindowPosition.Y <= point.Y &&
                   mWindowPosition.Y + mHeight >= point.Y;
        }

        /// <inheritdoc />
        public bool HandleLeftClick(ClickInfo info)
        {
            if (!(Visible && IntersectsWith(info.Location.Relative)))
                return false;

            if (OnClick != null) OnClick(this, new EventArgs());
            return true;
        }

        /// <inheritdoc />
        public virtual EventOrder EventOrder
        {
            get { return EventOrder.HudUpper; }
        }

        public bool HandleMouseMove(Coord<Point> endPoint)
        {
            ShowHint = Visible && IntersectsWith(endPoint.Relative) && Hint != "";
            return false;
        }

        /// <inheritdoc />
        public abstract void LoadContent(ContentLoader contentLoader);
        /// <inheritdoc />
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont);

        /// <inheritdoc />
        public virtual bool HandleDragStarted(ClickInfo info)
        {
            return Visible && IntersectsWith(info.Location.Relative);
        }

        /// <inheritdoc />
        public virtual bool HandleDragging(Nuclex.Input.MouseButtons button, Coord<Point> location)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual bool HandleDragStopped(ClickInfo info)
        {
            return false;
        }

        /// <summary>
        /// Load content after deserialization.
        /// </summary>
        /// <param name="gameWindow">The gamewindow.</param>
        /// <param name="contentLoader">The contentloader of the game.</param>
        public virtual void LoadGameState(GameWindow gameWindow, ContentLoader contentLoader)
        {   
            mWindow = gameWindow;
            mWindow.ClientSizeChanged += OnSizeChanged;
            LoadContent(contentLoader);
            OnSizeChanged(mWindow, new EventArgs());
        }

        /// <summary>
        /// Change position of hud items if window size changed via menu.
        /// </summary>
        public void Update()
        {
            if (mWindow.ClientBounds.Height != mWindowBounds.Y || mWindow.ClientBounds.Width != mWindowBounds.X)
            {
                mWindowBounds = new Point(mWindow.ClientBounds.Width, mWindow.ClientBounds.Y);
                Position = Position;
            }
        }

        /// <inheritdoc />
        public virtual void Destroy()
        {
            mWindow.ClientSizeChanged -= OnSizeChanged;
        }
    }
}
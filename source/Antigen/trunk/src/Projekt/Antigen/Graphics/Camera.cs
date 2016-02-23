
using System;
using Antigen.Input;
using Antigen.Objects;
using Antigen.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Antigen.Graphics
{
    /// <summary>
    /// Camera used to move and zoom in the game and to calculate transition between absolute and relative coordinates.
    /// </summary>
    [Serializable]
    sealed class Camera: ICoordTranslation, IKeyListener, IMouseMoveListener, IMouseWheelListener, IDestroy
    {
        private Matrix mTransformMatrix;
        private Vector2 mPosition;
        private float mZoom = 1;
        [NonSerialized]
        private GameWindow mWindow;
        private readonly int mMapWidth, mMapHeight;
        private Point mMouseCoord;
        private bool mPlusButtonPressed, mMinusButtonPressed;
        private float mScrollSpeedFactor; // Corresponds to scroll speed setting.
        private InputDispatcher mInput;
        [NonSerialized]
        private SettingsManager mSettings;

        private bool mUpButtonPressed;
        private bool mDownButtonPressed;
        private bool mRightButtonPressed;
        private bool mLeftButtonPressed;

        private const float MinZoom = 0.1f;
        private const float MaxZoom = 5f;
        private const float BaseScrollSpeedInPxPerSec = 300;
        private const int ScrollSensitiveBorderWidth = 10;
        private const float ZoomAmoutPerSec = 2f;
        private const float ZoomAmoutPerTick = 0.1f;
        private const float ScrollAmountPerZoomAmount = 1000;

        /// <summary>
        /// Creates a new Camera 
        /// </summary>
        /// <param name="input">an input manager</param>
        /// <param name="window">The window in which the scene is rendered to detect the borders</param>
        /// <param name="mapWidth">The width of the map</param>
        /// <param name="mapHeight">The height of the map</param>
        /// <param name="settings">The game's central settings manager.</param>
        /// <param name="startPos">Starting position of camera</param>
        public Camera(InputDispatcher input, GameWindow window, int mapWidth, int mapHeight, SettingsManager settings, Vector2 startPos)
        {
            mInput = input;
            input.RegisterListener(this);
            mWindow = window;
            mMapWidth = mapWidth;
            mMapHeight = mapHeight;
            mScrollSpeedFactor = settings.CurrentSettings.CameraScrollSpeed;
            settings.OnSettingsUpdated += OnSettingsUpdated;
            mSettings = settings;
            mWindow.ClientSizeChanged += OnSizeChanged;
            UpdateMatrix();
            Position = startPos;
        }

        /// <summary>
        /// Load contents after deserialization.
        /// </summary>
        /// <param name="window">The game window.</param>
        /// <param name="input">The inputdispatcher of the current game.</param>
        /// <param name="settings">The settingsmanager of the game.</param>
        public void LoadGameState(GameWindow window, InputDispatcher input, SettingsManager settings)
        {
            mSettings = settings;
            mInput = input;
            input.RegisterListener(this);
            mWindow = window;
            mWindow.ClientSizeChanged += OnSizeChanged;
            UpdateMatrix();
            OnSizeChanged(mWindow, new EventArgs());
        }

        /// <summary>
        /// Event that detects if the window changed its size.
        /// </summary>
        /// <param name="sender">Object that raises the event</param>
        /// <param name="eventArgs">Additional event information</param>
        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            if (mWindow.ClientBounds.Width <= 0)
                return;

            Zoom = Zoom;
            UpdateMatrix();
        }

        /// <summary>
        /// Updates scroll speed according to changed settings.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Settings change parameters.</param>
        private void OnSettingsUpdated(object sender, SettingsUpdateEventArgs args)
        {
            mScrollSpeedFactor = args.NewSettings.CameraScrollSpeed;
        }

        /// <summary>
        /// The position of the camera.
        /// </summary>
        private Vector2 Position
        {
            get { return mPosition; }
            set
            {
                if (value.X > mMapWidth - (mWindow.ClientBounds.Width / mZoom / 2))
                    value.X = mMapWidth - (mWindow.ClientBounds.Width / mZoom / 2);
                if (value.Y > mMapHeight - (mWindow.ClientBounds.Height / mZoom / 2))
                    value.Y = mMapHeight - (mWindow.ClientBounds.Height / mZoom / 2);
                if (value.X < mWindow.ClientBounds.Width / mZoom / 2)
                    value.X = mWindow.ClientBounds.Width / mZoom / 2;
                if (value.Y < mWindow.ClientBounds.Height / mZoom / 2)
                    value.Y = mWindow.ClientBounds.Height / mZoom / 2;
                mPosition = value; 
                UpdateMatrix();
            }
        }

        /// <summary>
        /// The tranformation matrix based on the the camera settings used to draw at the correct position. 
        /// </summary>
        public Matrix TransformMatrix
        {
            get { return mTransformMatrix; }
        }

        /// <summary>
        /// The zoom factor of the camrea.
        /// </summary>
        private float Zoom
        {
            get { return mZoom; }
            set
            {
                if (value > MaxZoom || value < MinZoom ||
                    value <
                    Math.Max((float) mWindow.ClientBounds.Width / mMapWidth,
                        (float) mWindow.ClientBounds.Height / mMapHeight))
                {
                    value = Math.Min(MaxZoom, Math.Max(MinZoom, value));
                    value = Math.Max(value, Math.Max((float)mWindow.ClientBounds.Width / mMapWidth, (float)mWindow.ClientBounds.Height / mMapHeight));
                    mZoom = value;
                    Position = Position;
                }
                else
                {
                    var newPosition = Position;
                    newPosition.X += (mMouseCoord.X / (float)mWindow.ClientBounds.Width - 0.5f) * (value - mZoom) / mZoom / mZoom * ScrollAmountPerZoomAmount;
                    newPosition.Y += (mMouseCoord.Y / (float)mWindow.ClientBounds.Height - 0.5f) * (value - mZoom) / mZoom / mZoom * ScrollAmountPerZoomAmount;
                    mZoom = value;
                    Position = newPosition;
                }   
            }
        }

        /// <inheritdoc />
        public bool HandleMouseMove(Coord<Point> endPoint)
        {
            mMouseCoord = endPoint.Relative;
            return false;
        }

        /// <inheritdoc />
        public bool HandleKeyPress(Keys key)
        {
            if ((key == Keys.Add) || (key == Keys.OemPlus)) mPlusButtonPressed = true;
            if ((key == Keys.Subtract) || (key == Keys.OemMinus)) mMinusButtonPressed = true;
            if (key == Keys.Up) mUpButtonPressed = true;
            if (key == Keys.Down) mDownButtonPressed = true;
            if (key == Keys.Right) mRightButtonPressed = true;
            if (key == Keys.Left) mLeftButtonPressed = true;
            if (key == Keys.Space) Zoom = MinZoom;
            return false;
        }

        /// <inheritdoc />
        public bool HandleKeyRelease(Keys key)
        {
            if ((key == Keys.Add) || (key == Keys.OemPlus)) mPlusButtonPressed = false;
            if ((key == Keys.Subtract) || (key == Keys.OemMinus)) mMinusButtonPressed = false;
            if (key == Keys.Up) mUpButtonPressed = false;
            if (key == Keys.Down) mDownButtonPressed = false;
            if (key == Keys.Right) mRightButtonPressed = false;
            if (key == Keys.Left) mLeftButtonPressed = false;
            return false;
        }

        /// <summary>
        /// The update method which scrolls the camrera if the mouse cursor is on the border of the screen.
        /// </summary>
        /// <param name="gameTime"></param>    
        public void Update(GameTime gameTime)
        {
            if (mPlusButtonPressed)
            {
                Zoom += ZoomAmoutPerSec * (float)gameTime.ElapsedGameTime.TotalSeconds * Zoom;
            }
            if (mMinusButtonPressed) Zoom -= ZoomAmoutPerSec * (float)gameTime.ElapsedGameTime.TotalSeconds * Zoom;
            
            var newPosition = Position;
            var scrollAmountinPx = (float)(BaseScrollSpeedInPxPerSec * mScrollSpeedFactor * gameTime.ElapsedGameTime.TotalSeconds / Zoom);
            if (mMouseCoord.X >= mWindow.ClientBounds.Width - ScrollSensitiveBorderWidth || mRightButtonPressed)
            {
                newPosition.X += scrollAmountinPx;
            }
            if (mMouseCoord.X <= ScrollSensitiveBorderWidth || mLeftButtonPressed)
            {
                newPosition.X -= scrollAmountinPx;
            }
            if (mMouseCoord.Y >= mWindow.ClientBounds.Height - ScrollSensitiveBorderWidth || mDownButtonPressed)
            {
                newPosition.Y += scrollAmountinPx;
            }
            if (mMouseCoord.Y <= ScrollSensitiveBorderWidth || mUpButtonPressed)
            {
                newPosition.Y -= scrollAmountinPx;
            }
            Position = newPosition; 
        }

        /// <summary>
        /// Updates the transformation matrix according to the settings of the camera.
        /// </summary>
        private void UpdateMatrix()
        {
            mTransformMatrix = Matrix.CreateTranslation(-mPosition.X, -mPosition.Y, 0) * Matrix.CreateScale(mZoom) * Matrix.CreateTranslation((float)mWindow.ClientBounds.Width / 2, (float)mWindow.ClientBounds.Height / 2, 0);
        }

        /// <inheritdoc />
        public Point ToAbsolute(Point coords)
        {
            var absoluteVector = Vector2.Transform(new Vector2(coords.X,coords.Y), Matrix.Invert(mTransformMatrix));
            return new Point((int)absoluteVector.X, (int)absoluteVector.Y);
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }

        /// <inheritdoc />
        public bool HandleMouseWheelRotated(float ticks)
        {
            Zoom += ZoomAmoutPerTick * ticks * Zoom;
            return true;
        }

        /// <inheritdoc />
        public void Destroy()
        {
            mInput.DeregisterListener(this);
            mWindow.ClientSizeChanged -= OnSizeChanged;
            mSettings.OnSettingsUpdated -= OnSettingsUpdated;
        }
    }
}
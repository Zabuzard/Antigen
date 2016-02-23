using System;
using System.Linq;
using Antigen.Content;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.Collision;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;
using IDrawable = Antigen.Graphics.IDrawable;

namespace Antigen.Logic.Selection
{
    /// <summary>
    /// Provides the user with the capability to select multiple objects
    /// in a rectangular area and gives the corresponding visual feedback.
    /// </summary>
    [Serializable]
    sealed class RectangularSelector : IDragListener, IDrawable, ILoad
    {
        /// <summary>
        /// Selection manager that selected objects are registered
        /// with.
        /// </summary>
        private readonly SelectionManager mSelectionManager;
        /// <summary>
        /// Container that provides object collision information.
        /// Used to locate all objects in a rectangular area.
        /// </summary>
        private readonly ISpatialCache mUnitCache;

        /// <summary>
        /// Texture for displaying the selection area.
        /// </summary>
        [NonSerialized]
        private Texture2D mRectTexture;

        /// <summary>
        /// Start location of the current selection.
        /// <code>null</code> if no selection is currently
        /// being attempted.
        /// </summary>
        private Coord<Point> mStartLocation;
        /// <summary>
        /// Current selection area.
        /// <code>null</code> if no selection is
        /// currently being attempted.
        /// </summary>
        private Rectangle? mArea;

        /// <summary>
        /// Creates a selector for rectangular areas.
        /// </summary>
        /// <param name="unitCache">Container that provides object collision
        /// information.</param>
        /// <param name="manager">The selection manager that objects will
        /// be registered with.</param>
        public RectangularSelector(ISpatialCache unitCache, SelectionManager manager)
        {
            mUnitCache = unitCache;
            mSelectionManager = manager;
        }

        /// <inheritdoc />
        public bool HandleDragStarted(ClickInfo info)
        {
            if (info.Button != MouseButtons.Left)
                return false;

            mStartLocation = info.Location;
            return false;
        }

        /// <inheritdoc />
        public bool HandleDragging(MouseButtons button, Coord<Point> location)
        {
            if (button != MouseButtons.Left || mStartLocation == null)
                return false;

            var startLocation = mStartLocation.Absolute;
            var currentEndLocation = location.Absolute;
            mArea = Functions.RectangleFromTo(startLocation, currentEndLocation);
            return false;
        }

        /// <inheritdoc />
        public bool HandleDragStopped(ClickInfo info)
        {
            if (mStartLocation == null || info.Button != MouseButtons.Left)
                return false;

            var startLocation = mStartLocation.Absolute;
            var endLocation = info.Location.Absolute;
            var area = Functions.RectangleFromTo(startLocation, endLocation);

            var objs = mUnitCache.UnitsInArea(area).OfType<ISelectable>().ToList();
            if (objs.Count == 0)
                mSelectionManager.DeselectAll();
            else
                mSelectionManager.SelectExactly(objs);

            mStartLocation = null;
            mArea = null;
            return false;
        }

        /// <inheritdoc />
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (mArea == null) { return; }
            var color = Color.Yellow;
            color *= 0.5f;
            spriteBatch.Draw(mRectTexture, mArea.Value, color);
        }

        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            mRectTexture = contentLoader.LoadTexture("Point");
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.Selector; }
        }
    }
}

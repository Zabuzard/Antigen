using System.Collections.Generic;
using Antigen.Content;
using Antigen.Settings;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// Shows possible resolution and allows to change to them.
    /// </summary>
    sealed class ResolutionScreen : MenuScreen
    {
        private readonly SettingsManager mSettings;
        private readonly List<Resolution> mCustomResolutions;
        private readonly Texture2D[] mScrollItems;
        private readonly Rectangle[] mScrollItemsPositions;
        private readonly Color[] mScrollItemsColor;
        private readonly Color mScrollItemSelectedColor;
        private readonly Color mScrollItemNormalColor;
        private int mUpperItem;

        /// <summary>
        /// Allows to change the window size.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public ResolutionScreen(InputManager input, GameWindow window, ContentLoader contentLoader, ScreenManager screenManager, SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mInput.GetMouse().MouseWheelRotated += OnMouseWheelRotated;
            mUpperItem = 0;
            mScrollItemsPositions = new Rectangle[2];
            mScrollItemsColor = new Color[2];
            mScrollItemNormalColor = Color.White;
            mScrollItemSelectedColor = Color.Yellow;
            mScrollItems = new Texture2D[2];
            mScrollItems[0] = contentLoader.LoadTexture("UpArrow");
            mScrollItems[1] = contentLoader.LoadTexture("DownArrow");
            mSettings = settings;
            mCustomResolutions = Functions.GetCustomResolutions(mScreens.Graphics.IsFullScreen);
            mMenuItems = new string[mCustomResolutions.Count + 1];
            for (var i = 0; i < mCustomResolutions.Count; i++)
            {
                mMenuItems[i] = mCustomResolutions[i].Horizontal + " x " + mCustomResolutions[i].Vertical;
            }
            mMenuItems[mMenuItems.Length - 1] = "back";
        }

        /// <inheritdoc />
        protected override void MenuItemSelected(int selectedIndex)
        {
            // Go back to upper menu.
            if (selectedIndex == mMenuItems.Length - 1)
            {
                OnPressBackButton();
            }
            else
            {
                var settings = mSettings.CurrentSettings;
                settings.Resolution = mCustomResolutions[selectedIndex];
                mSettings.CurrentSettings = settings;
            }
        }

        /// <inheritdoc/>
        protected override int SelectedIndex
        {
            get
            {
                {return base.SelectedIndex;}
            }
            set
            {
                if (value > mUpperItem + 4 && value <= mMenuItems.Length - 2)
                {
                    mUpperItem += 1;
                }
                else if (value < mUpperItem && value >= 0)
                {
                    mUpperItem -= 1;
                }
                mSelectedIndex = value;
                if (mSelectedIndex < 0)
                {
                    mSelectedIndex = mMenuItems.Length - 1;
                }
                else if (mSelectedIndex >= mMenuItems.Length)
                {
                    mSelectedIndex = 0;
                    mUpperItem = 0;
                }
                else if (mSelectedIndex == mMenuItems.Length - 2)
                {
                    mUpperItem = mMenuItems.Length - 2 - 4;
                }
            }
        }

        /// <inheritdoc/>
        protected override Color SelectColor(int i)
        {
            var tint = base.SelectColor(i);
            // Only proceed if i is not the index of the back button.
            if (i >= 0 && i < mCustomResolutions.Count)
            {
                // Highlight current resolution.
                if (mCustomResolutions[i].Horizontal == mScreens.Graphics.PreferredBackBufferWidth &&
                    mCustomResolutions[i].Vertical == mScreens.Graphics.PreferredBackBufferHeight)
                {
                    tint = Color.Yellow;
                }
            }
            return tint;
        }

        /// <inheritdoc/>
        protected override Point GetItemDrawBounds()
        {
            // Draw at most 5 of the resolution menu entries.
            if (mMenuItems.Length < 6)
            {
                return base.GetItemDrawBounds();
            }
            return new Point(mUpperItem, mUpperItem + 4);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            base.Draw(gameTime, spriteBatch, spriteFont);
            if (mMenuItems.Length > 6)
            {
                DrawScrollItems(spriteBatch, spriteFont);
            }
        }

        /// <summary>
        /// If there are more than 5 resolutions arrows for scrolling up and down will be shown.
        /// </summary>
        private void DrawScrollItems(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            // Measure size of any item.
            var size = (spriteFont.MeasureString(mMenuItems[0])) * mSize;
            // Determine size of arrows.
            var width = mWidth / 20;
            var height = mHeight / 20;
            // Determine positions of arrows.
            mScrollItemsPositions[0] = new Rectangle((mWidth - width) / 2, (int) mPosition.Y - height, width, height);
            mScrollItemsPositions[1] = new Rectangle((mWidth - width) / 2,
                (int) (mPosition.Y + mSize * (2 * size.Y + 5 * spriteFont.LineSpacing + 4 * LineSpace)),
                width,
                height);
            // Draw up arrow.
            spriteBatch.Begin();
            spriteBatch.Draw(mScrollItems[0], mScrollItemsPositions[0], mScrollItemsColor[0]);
            // Draw down arrow.
            spriteBatch.Draw(mScrollItems[1], mScrollItemsPositions[1], mScrollItemsColor[1]);
            spriteBatch.End();
        }

        /// <inheritdoc/>
        protected override void OnMousePress(MouseButtons button)
        {
            base.OnMousePress(button);
            var mouseState = mInput.GetMouse().GetState();
            var relativeLocation = new Point(mouseState.X, mouseState.Y);
            if (mScrollItemsPositions[0].Contains(relativeLocation))
            {
                mScrollItemsColor[0] = mScrollItemSelectedColor;
                SelectedIndex--;
            }
            else if (mScrollItemsPositions[1].Contains(relativeLocation))
            {
                mScrollItemsColor[1] = mScrollItemSelectedColor;
                SelectedIndex++;
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
            if (mScrollItemsPositions[0].Contains(new Point((int)x, (int)y)))
            {
                mScrollItemsColor[0] = mScrollItemSelectedColor;
            }
            else if (mScrollItemsPositions[1].Contains(new Point((int) x, (int) y)))
            {
                mScrollItemsColor[1] = mScrollItemSelectedColor;
            }
            else
            {
                mScrollItemsColor[0] = mScrollItemNormalColor;
                mScrollItemsColor[1] = mScrollItemNormalColor;
            }
        }

        /// <summary>
        /// Scroll down or up number of ticks.
        /// </summary>
        /// <param name="ticks">number of ticks.</param>
        private void OnMouseWheelRotated(float ticks)
        {
            if (mMenuItems.Length > 6)
            {
                SelectedIndex -= (int) ticks;
                if (mMenuItems[mSelectedIndex].Equals("back"))
                {
                    if (ticks < 0)
                    {
                        SelectedIndex = 0;
                        mUpperItem = 0;
                    }
                    else
                    {
                        SelectedIndex = mMenuItems.Length - 2;
                        mUpperItem = mSelectedIndex - 4;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void DisableEvents()
        {
            base.DisableEvents();
            mInput.GetMouse().MouseWheelRotated -= OnMouseWheelRotated;
        }
    }
}

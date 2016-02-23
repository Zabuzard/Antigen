using System;
using Antigen.Content;
using Antigen.Settings;
using Antigen.Sound;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// Abstract class for all menu screens. 
    /// </summary>
    abstract class MenuScreen : IScreen
    {
        protected readonly InputManager mInput;
        protected readonly ScreenManager mScreens;
        protected String[] mMenuItems;
        protected Rectangle[] mItemsRectangles;

        protected readonly Color mNormalColor;
        protected Color mHighlightColor;

        private Texture2D mMenuBackGround;
        protected int mSelectedIndex;
        protected Vector2 mPosition;
        protected int mWidth;
        protected int mHeight;
        private const int DefaultWidth = 800;
        private const int DefaultHeight = 480;
        protected const int LineSpace = 20;
        // Enable changing the size of the menu entries.
        protected float mSize;
        protected Texture2D mRectangle;

        /// <summary>
        /// Abstract class MenuScreen. Implements basic logic for all related screens.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        protected MenuScreen(InputManager input,
            GameWindow window,
            ContentLoader contentLoader,
            ScreenManager screenManager,
            SettingsManager settings)
        {
            ScreenEnabled = true;
            mScreens = screenManager;
            mInput = input;

            window.ClientSizeChanged += ChangeRatio;
            settings.OnResolutionChanged += ChangeRatio;
            EnableEvents();

            mNormalColor = Color.Black;
            mHighlightColor = Color.DarkBlue;

            ChangeSize();

            // This is necessary for being able to push screens on the stack without loading the content of the whole stack.
            LoadContent(contentLoader);
            mItemsRectangles = new Rectangle[11];
        }

        /// <summary>
        /// React on clientSizeChanged. Change size of menu background and entries.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">window change event</param>
        private void ChangeRatio(object sender, EventArgs e)
        {
            ChangeSize();
        }

        /// <summary>
        /// Determine the size of the menu.
        /// </summary>
        private void ChangeSize()
        {
            mWidth = mScreens.GraphicsDevice.Viewport.Width;
            mHeight = mScreens.GraphicsDevice.Viewport.Height; 
            if (mWidth > 0)
            {
                if (mHeight / mWidth <= DefaultHeight / DefaultWidth)
                {
                    mSize = (float) mHeight / DefaultHeight / 3;
                }
                else
                {
                    mSize = (float) mWidth / DefaultWidth / 3;
                }
            }
        }

        /// <summary>
        /// Handle key press.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>>
        protected virtual void OnKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Down:
                    SelectedIndex++;
                    break;
                case Keys.Up:
                    SelectedIndex--;
                    break;
                case Keys.Enter:
                    MenuItemSelected(SelectedIndex);
                    break;
                case Keys.Escape:
                    OnPressBackButton();
                    break;
            }
        }

        /// <summary>
        /// Handle Mouse clicks.
        /// </summary>
        protected virtual void OnMousePress(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                var mouseState = mInput.GetMouse().GetState();
                var relativeLocation = new Point(mouseState.X, mouseState.Y);
                for (int i = 0; i < mMenuItems.Length; i++)
                {
                    if (mItemsRectangles[i].Contains(relativeLocation))
                    {
                        mSelectedIndex = i;
                        MenuItemSelected(i);
                        break;
                    }
                }
            }
        }
        

        /// <summary>
        /// Handle mouse movement. Highlight menu entries when mouse is moved upon them.
        /// </summary>
        /// <param name="x">X coordinate of the movement end point.</param>
        /// <param name="y">Y coordinate of the movement end point.</param>
        protected virtual void OnMouseMove(float x, float y)
        {
            for (int i = 0; i < mItemsRectangles.Length; i++)
            {
                if (mItemsRectangles[i].Contains(new Point((int)x, (int)y)))
                {
                    mSelectedIndex = i;
                    return;
                }
            }
        }


        /// <summary>
        /// Index of the array of Strings. Determines which one is selected.
        /// </summary>
        protected virtual int SelectedIndex
        {
            get { return mSelectedIndex; }
            set
            {
                mSelectedIndex = value;
                if (mSelectedIndex < 0)
                {
                    mSelectedIndex = mMenuItems.Length - 1;
                }
                else if (mSelectedIndex >= mMenuItems.Length)
                {
                    mSelectedIndex = 0;
                }
            }
        }


        /// <summary>
        /// Each menuScreen must implement this method for specifying the behavior for the case that any button is selected.
        /// </summary>
        /// <param name="selectedIndex">Index of the menu entry that was selected.</param>
        protected abstract void MenuItemSelected(int selectedIndex);

        /// <summary>
        /// Tell soundwrapper to play menu music.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            mScreens.SoundWrapper.PlayMusic(Music.Menu);
        }

        /// <summary>
        /// Draw menu entries.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The spritebatch of the game.</param>
        /// <param name="spriteFont">The font of the game.</param>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            // Create array of the right size if the current array of length 11 is too small.
            if (mMenuItems.Length > 11)
            {
                mItemsRectangles = new Rectangle[mMenuItems.Length];
            }
            spriteBatch.Begin();
            DrawMenuEntries(spriteBatch, spriteFont, GetItemDrawBounds().X, GetItemDrawBounds().Y);
            spriteBatch.End();
        }

        /// <summary>
        /// Get the first and the last menu item to draw. Usually all items in mMenuItems.
        /// </summary>
        /// <returns>A Point with the indexes of the first and the last menu items to be drawn.</returns>
        protected virtual Point GetItemDrawBounds()
        {
            return new Point(0, mMenuItems.Length - 1);
        }

        /// <summary>
        /// Draw background and heading.
        /// </summary>
        protected virtual void DrawBackground(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Draw(mMenuBackGround, new Rectangle(0, 0, mWidth, mHeight), Color.White);
            // Draw heading.
            spriteBatch.DrawString(spriteFont,
                "Antigen",
                new Vector2(mPosition.X - 3 * spriteFont.MeasureString("Antigen").X * mSize / 2, mHeight / 10f),
                Color.GhostWhite * 1f,
                0.0f,
                new Vector2(0, 0),
                3 * mSize,
                new SpriteEffects(),
                0.0f);
            
        }

        /// <summary>
        /// Draw the entries of the current menu in the right postition.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch of the game.</param>
        /// <param name="spriteFont">The spritefont of the game.</param>
        /// <param name="firstItem">The index of the first menu item to be drawn.</param>
        /// <param name="lastItem">The index of the last menu item to be drawn.</param>
        private void DrawMenuEntries(SpriteBatch spriteBatch,
            SpriteFont spriteFont,
            int firstItem,
            int lastItem)
        {
            // Measure estimated height and width for all menu entries. Adapt size if it's too large.
            float height = 0;
            float maxWidth = 0;
            for (var i = firstItem; i < lastItem + 1; i++)
            {
                var currentWidth = (int)(spriteFont.MeasureString(mMenuItems[i]).X * mSize);
                if (currentWidth > maxWidth)
                {
                    maxWidth = currentWidth;
                }
                if (!mMenuItems[i].Equals("back"))
                {
                    height += (spriteFont.MeasureString(mMenuItems[i]).Y + LineSpace) * mSize;
                }
                else
                {
                    height -= (spriteFont.LineSpacing + LineSpace) * mSize;
                }
            }
            if (maxWidth > 0 && height > 0)
            {
                mSize *= maxWidth > mWidth ? mWidth / maxWidth : 1;
                mSize *= height > mHeight ? mHeight / height : 1;
            }
            mPosition = new Vector2(mWidth / 2f, (mHeight - height) / 2f);
            DrawBackground(spriteBatch, spriteFont);
            // Begin to draw.
            var location = mPosition;
            for (var i = firstItem; i < lastItem + 1; i++)
            {
                var size = (spriteFont.MeasureString(mMenuItems[i])) * mSize;
                var tint = SelectColor(i);

                var curPos = new Vector2(mPosition.X - size.X / 2f, location.Y + size.Y * mSize);

                // Last button is the "back to upper menu" button in the left lower corner.                
                if (i == mMenuItems.Length - 1 && mMenuItems[i].Equals("back"))
                {
                    curPos = new Vector2(mWidth / 10f, mHeight - mHeight / 8);
                }
                mItemsRectangles[i] = new Rectangle((int) curPos.X, (int) curPos.Y, (int) size.X, (int) size.Y);
                spriteBatch.Draw(mRectangle, mItemsRectangles[i], Color.White * 0.5f);
                spriteBatch.DrawString(spriteFont,
                    mMenuItems[i],
                    curPos,
                    tint,
                    0.0f,
                    new Vector2(0, 0),
                    mSize,
                    new SpriteEffects(),
                    0.0f);
                location.Y += (spriteFont.LineSpacing + LineSpace) * mSize;
            }
            // Draw back button if not drawn before.
            if (lastItem < mMenuItems.Length - 1 && mMenuItems[mMenuItems.Length - 1].Equals("back"))
            {
                var i = mMenuItems.Length - 1;
                var size = (spriteFont.MeasureString(mMenuItems[i])) * mSize;
                var tint = SelectColor(i);
                var curPos = new Vector2(mWidth / 10f, mHeight - mHeight / 8);
                mItemsRectangles[i] = new Rectangle((int)curPos.X, (int)curPos.Y, (int)size.X, (int)size.Y);
                spriteBatch.Draw(mRectangle, mItemsRectangles[i], Color.White * 0.5f);
                spriteBatch.DrawString(spriteFont,
                    mMenuItems[i],
                    curPos,
                    tint,
                    0.0f,
                    new Vector2(0, 0),
                    mSize,
                    new SpriteEffects(),
                    0.0f);
            }
        }

        /// <summary>
        /// Select the color of the menu entries depending on the screen and on availability.
        /// </summary>
        /// <param name="i">The index of the menu entry in its menuscreen.</param>
        /// <returns>Return the color depending on whether the entry has the chosen index or not.</returns>
        protected virtual Color SelectColor(int i)
        {
            return i == SelectedIndex ? mHighlightColor : mNormalColor;
        }

        /// <summary>
        /// Load the background image for the menu.
        /// </summary>
        /// <param name="contentLoader">The contentloader of the game.</param>
        public void LoadContent(ContentLoader contentLoader)
        {
            mMenuBackGround = contentLoader.LoadTexture("menubackground");
            mRectangle = contentLoader.LoadTexture("RoundedRectangle");
        }

        /// <inheritdoc />
        public bool ScreenEnabled 
        {
            get; private set;
        }

        /// <inheritdoc />
        public bool UpdateLower()
        {
            return false;
        }

        /// <inheritdoc />
        public bool DrawLower()
        {
            return GetType() == typeof (PauseScreen);
        }

        /// <summary>
        /// Register all events of the screen.
        /// </summary>
        public void EnableEvents()
        {
            mInput.GetKeyboard().KeyPressed += OnKeyPress;
            mInput.GetMouse().MouseButtonReleased += OnMousePress;
            mInput.GetMouse().MouseMoved += OnMouseMove;
            ChangeSize();
        }

        /// <summary>
        /// Deregister all events of the screen.
        /// </summary>
        public virtual void DisableEvents()
        {
            mInput.GetKeyboard().KeyPressed -= OnKeyPress;
            mInput.GetMouse().MouseButtonReleased -= OnMousePress;
            mInput.GetMouse().MouseMoved -= OnMouseMove;
        }

        /// <summary>
        /// Several menu screens provide a "back" button. Properly unload current screen and reregister events of lower screen.
        /// </summary>
        protected void OnPressBackButton()
        {
            DisableEvents();
            mScreens.Pop();
            var topScreen = mScreens.Peek() as MenuScreen;
            if (topScreen != null)
                topScreen.EnableEvents();
        }
    }
}

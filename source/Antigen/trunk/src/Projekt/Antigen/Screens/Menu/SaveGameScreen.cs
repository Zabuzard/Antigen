using System.IO;
using Antigen.Content;
using Antigen.Settings;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = Nuclex.Input.MouseButtons;
using System.Text.RegularExpressions;

namespace Antigen.Screens.Menu
{
    /// <summary>
    /// This screen provides a vision of the available slots.
    /// </summary>
    internal sealed class SaveGameScreen : MenuScreen
    {
        private readonly Color mMarkedColor;
        private readonly Texture2D[] mScrollItems;
        private readonly Rectangle[] mScrollItemsPositions;
        private readonly Color[] mScrollItemsColor;
        private readonly Color mScrollItemSelectedColor;
        private readonly Color mScrollItemNormalColor;
        private int mUpperItem;
        private Keys mLastKey = Keys.None;
        private string mInputText = "";
        private string[] mNames;
        private int mSelectedSlot;

        /// <summary>
        /// Initialize the menu items of the screen. Remember index of the slots that are already used.
        /// </summary>
        /// <param name="input">The input manager of the game.</param>
        /// <param name="window">The window of the game.</param>
        /// <param name="contentLoader">The content loader of the game.</param>
        /// <param name="screenManager">The screen manager of the game.</param>
        /// <param name="settings">The game's settings manager.</param>
        public SaveGameScreen(InputManager input,
            GameWindow window,
            ContentLoader contentLoader,
            ScreenManager screenManager,
            SettingsManager settings)
            : base(input, window, contentLoader, screenManager, settings)
        {
            mUpperItem = 0;
            mScrollItemsPositions = new Rectangle[2];
            mScrollItemNormalColor = Color.White;
            mScrollItemSelectedColor = Color.Yellow;
            mScrollItemsColor = new []{mScrollItemNormalColor, mScrollItemNormalColor};
            mScrollItems = new Texture2D[2];
            mScrollItems[0] = contentLoader.LoadTexture("UpArrow");
            mScrollItems[1] = contentLoader.LoadTexture("DownArrow");
            mMarkedColor = Color.Yellow;
            ReloadContent();
        }

        /// <summary>
        /// Save the game in the selected slot using the save game manager.
        /// </summary>
        /// <param name="selectedIndex">The currently selected index. Determines the slot to save in.</param>
        protected override void MenuItemSelected(int selectedIndex)
        {
            if (mMenuItems[selectedIndex].Equals("back"))
            {
                OnPressBackButton();
            }
            else if (selectedIndex < mMenuItems.Length - 4)
            {
                // press on any existing game save.
                DisableEvents();
                mScreens.PushLoadScreen(LoadScreen.Mode.Save, mNames[selectedIndex]);
            }
            else if (selectedIndex == mMenuItems.Length - 3)
            {
                // Press on save button.
                if (mInputText.Length < 1)
                {
                    return;
                }
                DisableEvents();
                mScreens.PushLoadScreen(LoadScreen.Mode.Save, mInputText);
            }
            else if (selectedIndex == mMenuItems.Length - 2)
            {
                // Press on Delete all button.
                DisableEvents();
                var files = Directory.GetFiles(Functions.GetFolderPath());
                for (var i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains("gameSave_"))
                    {
                        File.Delete(files[i]);
                    }
                }
                EnableEvents();
                ReloadContent();
            }
        }

        /// <inheritdoc/>
        protected override Color SelectColor(int i)
        {
            return i == mSelectedSlot ? mMarkedColor : i == mSelectedIndex ? mHighlightColor : mNormalColor;
        }

        /// <summary>
        /// Update the information whether a file exists or not.
        /// </summary>
        private void ReloadContent()
        {
            // First get number of existing gamesaves.
            var files = Directory.GetFiles(Functions.GetFolderPath());
            var numOfSlots = 0;
            foreach (var file in files)
            {
                if (file.Contains("gameSave_"))
                {
                    numOfSlots++;
                }
            }
            // Add a "Save" button, as well as a "Delete all" button. And a text field for input.
            mMenuItems = new string[numOfSlots + 4];
            mNames = new string[numOfSlots];
            var i = 0;
            while (i < numOfSlots)
            {
                foreach (var file in files)
                {
                    if (file.Contains("gameSave_"))
                    {
                        var r = new Regex(@"gameSave_(.*)\.bin", RegexOptions.IgnoreCase);
                        var match = r.Match(file);
                        var g = match.Groups[1];
                        mNames[i] = g.ToString();
                        mMenuItems[i] = mNames[i] + " - " + File.GetLastWriteTime(file);
                        i++;
                    }
                }
            }
            mMenuItems[mMenuItems.Length - 4] = mInputText;
            mMenuItems[mMenuItems.Length - 3] = "Save";
            mMenuItems[mMenuItems.Length - 2] = "Delete all";
            mMenuItems[mMenuItems.Length - 1] = "back";
            mSelectedSlot = mMenuItems.Length - 4;
            mInput.GetMouse().MouseWheelRotated += OnMouseWheelRotated;
        }


        /// <inheritdoc/>
        protected override int SelectedIndex
        {
            get
            {
                { return base.SelectedIndex; }
            }
            set
            {
                if (value > mUpperItem + 4 && value <= mMenuItems.Length - 5)
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
                else if (mSelectedIndex == mMenuItems.Length - 5)
                {
                    mUpperItem = mMenuItems.Length - 5 - 4;
                }
            }
        }

        /// <inheritdoc/>
        protected override Point GetItemDrawBounds()
        {
            // Draw at most 5 savegame menu entries.
            if (mMenuItems.Length < 5)
            {
                return new Point(mMenuItems.Length - 1, mMenuItems.Length - 1);
            }
            if (mMenuItems.Length < 9)
            {
                return new Point(0, mMenuItems.Length - 5);
            }
            return new Point(mUpperItem, mUpperItem + 4);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            base.Draw(gameTime, spriteBatch, spriteFont);
            Update(gameTime, spriteBatch, spriteFont);
            DrawSaveGameButtons(spriteBatch, spriteFont);
            if (mMenuItems.Length > 9)
            {
                DrawScrollItems(spriteBatch, spriteFont);
            }
        }

        /// <summary>
        /// Draw "Save" button, "Delete All" button and input line.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch of the game.</param>
        /// <param name="spriteFont">The sprite font of the game.</param>
        private void DrawSaveGameButtons(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Begin();
            // Draw "save" button.
            var i = mMenuItems.Length - 3;
            var size = (spriteFont.MeasureString(mMenuItems[i])) * mSize;
            var tint = SelectColor(i);
            var curPos = new Vector2((mWidth - size.X) / 2f, mHeight - mHeight / 8);
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
            // Draw "delete all" button.
            i = mMenuItems.Length - 2;
            size = (spriteFont.MeasureString(mMenuItems[i])) * mSize;
            tint = SelectColor(i);
            curPos = new Vector2(9 * mWidth / 10f - size.X, mHeight - mHeight / 8);
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
            spriteBatch.End();
        }

        /// <summary>
        /// Read input.
        /// </summary>
        private void Update(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (Keyboard.GetState().IsKeyUp(mLastKey))
            {
                mLastKey = Keys.None;
            }
            if (Keyboard.GetState().GetPressedKeys().Length > 0 && mLastKey == Keys.None)
            {
                mLastKey = Keyboard.GetState().GetPressedKeys()[0];
                if (mLastKey == Keys.Back)
                {
                    if (mInputText.Length != 0)
                        mInputText = mInputText.Substring(0, mInputText.Length - 1);
                }
                else if (mInputText.Length < 25)
                {
                    if (mLastKey.ToString().Length == 1)
                    {
                        mInputText += (char) mLastKey.GetHashCode();
                    }
                }
            }
            // Draw input line.
            var i = mMenuItems.Length - 4;
            mMenuItems[mMenuItems.Length - 4] = mInputText;
            var size = spriteFont.MeasureString("XXXXXXXXX") * mSize;
            if (size.X < spriteFont.MeasureString(mInputText + "_").X * mSize)
            {
                size = spriteFont.MeasureString(mInputText + "_") * mSize;
            }
            var curPos = new Vector2((mWidth - size.X) / 2f, mHeight - mHeight / 8 - size.Y - LineSpace * mSize);
            mItemsRectangles[mMenuItems.Length - 4] = new Rectangle((int)curPos.X,
                (int)curPos.Y,
                (int)size.X,
                (int)size.Y);
            var tint = SelectColor(i);
            curPos = new Vector2((mWidth - size.X) / 2f, mHeight - mHeight / 8 - size.Y - LineSpace * mSize);
            mItemsRectangles[i] = new Rectangle((int)curPos.X, (int)curPos.Y, (int)size.X, (int)size.Y);
            spriteBatch.Begin();
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
            var cursorPosition = new Vector2(
                (mWidth - size.X) / 2f + spriteFont.MeasureString(mMenuItems[i]).X * mSize,
                curPos.Y);
            if (gameTime != null)
            {
                // Blink cursor item twice per second.
                spriteBatch.DrawString(spriteFont,
                    "_",
                    cursorPosition,
                    tint * ((int) (gameTime.TotalGameTime.TotalMilliseconds % 1000 / 250 % 2)),
                    0.0f,
                    new Vector2(0, 0),
                    mSize,
                    new SpriteEffects(),
                    0.0f);
            }
            spriteBatch.End();
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
            mScrollItemsPositions[0] = new Rectangle((mWidth - width) / 2, (int)mPosition.Y - height, width, height);
            mScrollItemsPositions[1] = new Rectangle((mWidth - width) / 2,
                (int)(mPosition.Y + mSize * (2 * size.Y + 5 * spriteFont.LineSpacing + 4 * LineSpace)),
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
            else if (mScrollItemsPositions[1].Contains(new Point((int)x, (int)y)))
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
            if (mMenuItems.Length > 9)
            {
                SelectedIndex -= (int)ticks;
                if (mMenuItems[mSelectedIndex].Equals("back"))
                {
                    if (ticks < 0)
                    {
                        SelectedIndex = 0;
                        mUpperItem = 0;
                    }
                    else
                    {
                        SelectedIndex = mMenuItems.Length - 5;
                        mUpperItem = mSelectedIndex - 4;
                    }
                }
                else if (mSelectedIndex == mMenuItems.Length - 4)
                {
                    SelectedIndex = 0;
                    mUpperItem = 0;
                }
            }
        }

        /// <inheritdoc/>
        public override void DisableEvents()
        {
            base.DisableEvents();
            mInput.GetMouse().MouseWheelRotated -= OnMouseWheelRotated;
        }

        /// <inheritdoc/>
        protected override void OnKeyPress(Keys key)
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
                    MenuItemSelected(mMenuItems.Length - 3);
                    break;
                case Keys.Escape:
                    OnPressBackButton();
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Content;
using Antigen.Input;
using Antigen.Logic.Selection;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Antigen.HUD
{
    [Serializable]
    sealed class CellListPanel: HudItem
    {
        private readonly SelectionManager mSelectionManager;

        [NonSerialized]
        private ContentLoader mContentLoader;
        private List<CellInfoItem> mCellInfos;
        private readonly List<Button> mButtons;
        [NonSerialized]
        private Texture2D mPanelTexture;
        private readonly Button mUpButton;
        private readonly Button mDownButton;
        private readonly Button mSelectButton;
        private int mStartIndex;
        private int mDragStartIndex = -1;
        private int mDragStopIndex;       
        private const int SelectButtonWidth = 50;
        private const float AntigenBorder = 2;
        private const int CellInfoHeight = 15;
        private const int SortButtonWidth = 20;
        private const int SortButtonHeight = 10;
        private const int ScrollButtonWidth = 10;
        private const int ScrollButtonHeight = 10;
        private const int BarGap = 5;
        private const int ButtonGap = 5;
        private const int BarWidth = 30;
        private const int BarHeight = 5;
        private const int FontWidth = 100;

        /// <summary>
        /// Creates a new cell list panel
        /// </summary>
        /// <param name="input"></param>
        /// <param name="window"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="selectionManager"></param>
        public CellListPanel(InputDispatcher input, GameWindow window, Point position, int width, int height, SelectionManager selectionManager) : base(input, window, position, width, height)
        {
            mSelectionManager = selectionManager;
            Visible = false;
            mCellInfos = new List<CellInfoItem>();
            mButtons = new List<Button>();
            for (var j = 0; j < 2; j++)
            {
                for (var i = 0; i < 6; i++)
                {
                    var button = new SelectableButton(input,
                        window,
                        new Point(position.X + FontWidth + CellInfoHeight + BarWidth / 2 - SortButtonWidth / 2 + i * (BarWidth + BarGap),
                            position.Y + j * (SortButtonHeight + ButtonGap)),
                        SortButtonWidth,
                        SortButtonHeight,
                        SelectedStyle.FullSelectable);
                    mButtons.Add(button);
                    button.OnClick += OnSortButtonClick;
                }              
            }
            mSelectButton = new Button(input,
                window,
                new Point(position.X + FontWidth - SelectButtonWidth - ButtonGap, position.Y + ButtonGap),
                SelectButtonWidth,
                SortButtonHeight * 2) {Caption = "Select"};
            mSelectButton.OnClick += OnSelectButtonClick;
            mButtons.Add(mSelectButton);
            mUpButton = new Button(input, window, new Point(position.X + FontWidth + 6 * (BarWidth + BarGap), position.Y + 2 * (SortButtonHeight + ButtonGap)), ScrollButtonWidth, ScrollButtonHeight);
            mUpButton.OnClick += OnUpButtonClick;
            mButtons.Add(mUpButton);
            mDownButton = new Button(input, window, new Point(position.X + FontWidth + 6 * (BarWidth + BarGap), position.Y + height - ScrollButtonHeight ), ScrollButtonWidth, ScrollButtonHeight);
            mDownButton.OnClick += OnDownButtonClick;
            mButtons.Add(mDownButton);
        }

        private void OnSelectButtonClick(object sender, EventArgs eventArgs)
        {
            mSelectionManager.SelectExactly(mCellInfos.Where(cellInfoItem => cellInfoItem.Selected).Select(cellInfoItem => (ISelectable) cellInfoItem.TheUnit).ToList());
        }

        public override bool HandleDragStopped(ClickInfo info)
        {
            mDragStartIndex = -1;
            return false;
        }

        public override EventOrder EventOrder
        {
            get { return EventOrder.HudLower; }
        }

        private int MaxCount()
        {
            return (mHeight - SortButtonHeight * 2) / CellInfoHeight;
        }

        private int GetIndexFromPosition(int y)
        {
            return (y - mWindowPosition.Y - 2 * SortButtonHeight) / CellInfoHeight - 1 + mStartIndex;
        }

        public override bool HandleDragStarted(ClickInfo info)
        {
            mDragStartIndex = GetIndexFromPosition(info.Location.Relative.Y);
            
            return base.HandleDragStarted(info);
        }

        /// <inheritdoc />
        public override bool HandleDragging(MouseButtons button, Graphics.Coord<Point> location)
        {
            mDragStopIndex = GetIndexFromPosition(location.Relative.Y);
            if (mDragStartIndex >= 0 && location.Relative.X > mWindowPosition.X &&
                location.Relative.X < mWindowPosition.X + FontWidth + 6 * (BarWidth + BarGap))
            {
                for (var i = 0; i < mCellInfos.Count; i++)
                {
                    mCellInfos[i].Selected = ((i >= mDragStartIndex && i <= mDragStopIndex) || (i <= mDragStartIndex && i >= mDragStopIndex));
                }
            }
            return false;
        }

        /// <summary>
        /// Scrolls the list down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnDownButtonClick(object sender, EventArgs eventArgs)
        {
            if (mStartIndex + MaxCount() < mCellInfos.Count)
            {
                mStartIndex++;
                mUpButton.Color = Color.White;
            }
            if (mStartIndex + MaxCount() == mCellInfos.Count)
            {
                mDownButton.Color = Color.Gray;
            }
            
        }

        /// <summary>
        /// Scrolls the list up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnUpButtonClick(object sender, EventArgs eventArgs)
        {
            if (mStartIndex > 0) mStartIndex--;
            if (mStartIndex + MaxCount() < mCellInfos.Count)
            {
                mDownButton.Color = Color.White;
            }
            if (mStartIndex == 0) mUpButton.Color = Color.Gray;
        }

        /// <summary>
        /// Event that is called if a sort button is clicked.
        /// Sorts the cell list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnSortButtonClick(object sender, EventArgs eventArgs)
        {
            var index = mButtons.IndexOf((Button)sender);
            mCellInfos.Sort((item1, item2) => item1.GetBarValue(index % 6).CompareTo(item2.GetBarValue(index % 6)));
            if (index < 6) mCellInfos.Reverse();
            foreach (var selectableButton in mButtons.OfType<SelectableButton>())
            {
                selectableButton.Selected = false;
            }
            ((SelectableButton)sender).Selected = true;
        }

        /// <summary>
        /// Should be called if the unit list changes
        /// </summary>
        /// <param name="units"></param>
        public void UpdateList(IEnumerable<Unit> units)
        {
            foreach (var selectableButton in mButtons.OfType<SelectableButton>())
            {
                selectableButton.Selected = false;
            }
            mCellInfos = new List<CellInfoItem>();
            foreach (var unit in units)
            {
                mCellInfos.Add(new CellInfoItem(unit, BarGap, BarWidth, BarHeight, FontWidth, CellInfoHeight, AntigenBorder));
            }
            foreach (var cellInfoItem in mCellInfos)
            {
                cellInfoItem.LoadContent(mContentLoader);
            }
            mUpButton.Color = Color.Gray;
            mDownButton.Color = mStartIndex + MaxCount() < mCellInfos.Count ? Color.White : Color.Gray;
            mStartIndex = 0;
            Visible = mCellInfos.Any();
        }

        /// <inheritdoc/>
        public override void LoadGameState(GameWindow gameWindow, ContentLoader contentLoader)
        {
            foreach (var button in mButtons)
            {
                button.LoadGameState(gameWindow, contentLoader);
            }
            mUpButton.LoadGameState(gameWindow, contentLoader);
            mDownButton.LoadGameState(gameWindow, contentLoader);
            mSelectButton.LoadGameState(gameWindow, contentLoader);
            foreach (var item in mCellInfos)
            {
                item.LoadContent(contentLoader);
            }
            base.LoadGameState(gameWindow, contentLoader);
        }

        /// <inheritdoc />
        public override void LoadContent(ContentLoader contentLoader)
        {
            mContentLoader = contentLoader;
            mPanelTexture = contentLoader.LoadTexture("RoundedRectangle");
            for (var i = 0; i < 6; i++)
            {
                mButtons[i].ButtonTexture = contentLoader.LoadTexture("UpArrow");
            }
            for (var i = 6; i < 12; i++)
            {
                mButtons[i].ButtonTexture = contentLoader.LoadTexture("DownArrow");
            }
            mUpButton.ButtonTexture = contentLoader.LoadTexture("UpArrow");
            mDownButton.ButtonTexture = contentLoader.LoadTexture("DownArrow");
            mSelectButton.LoadContent(contentLoader);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (mCellInfos == null)
                    return;
            
            if (mCellInfos.Any())
            {
                spriteBatch.Draw(mPanelTexture,
                    new Rectangle(mWindowPosition.X, mWindowPosition.Y, mWidth, mHeight),
                    Color.Gray * 0.7f);
                foreach (var button in mButtons)
                {
                    button.Draw(gameTime, spriteBatch, spriteFont);
                }
                var maxCount = (mHeight - SortButtonHeight * 2) / CellInfoHeight;
                for (var i = 0; i < mCellInfos.Count - mStartIndex && i < maxCount; i++)
                {
                    mCellInfos[i + mStartIndex].Draw(spriteBatch,
                        spriteFont,
                        new Point(mWindowPosition.X, mWindowPosition.Y + SortButtonHeight * 2 + (i + 1) * CellInfoHeight));
                }
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            foreach (var button in mButtons)
            {
                button.Destroy();
            } 
        }
    }
}

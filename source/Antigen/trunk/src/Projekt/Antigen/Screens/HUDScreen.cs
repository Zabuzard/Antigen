using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antigen.Content;
using Antigen.HUD;
using Antigen.Input;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Screens.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Screens
{
    /// <summary>
    /// The HUD Screen width items to change the mode of the cells and items to display the properties of a cell
    /// </summary>
    [Serializable]
    internal sealed class HudScreen : IScreen, IActionListener, IDestroy
    {
        [NonSerialized]
        private GameWindow mWindow;
        private readonly SelectionManager mSelectionManager;
        private readonly ObjectCaches mObjectCaches;

        [NonSerialized]
        private ScreenManager mScreenManager;
        private readonly List<HudItem> mItems;
        [NonSerialized]
        private IEnumerable<Unit> mUnits;
        private readonly List<Bar> mBars;
        private const int PanelWidth = 200;
        private const int ListPanelWidth = 330;
        private const int PanelBorder = 10;
        private const int PanelHeight = 150;
        private const int ButtonSize = 30;
        private const int ButtonBorder = 5;
        private const int MaxLifepoints = (int) ValueStore.MaxLifepoints;
        private const int MaxLifespan = (int) ValueStore.MaxLifespan;
        private const int MaxSpeed = ValueStore.MaxBaseSpeed;
        private const int MaxStrength = (int) ValueStore.MaxAttackPower;
        private const int MaxDivisionSpeed = (int) ValueStore.MaxCellDivisionRate;
        private const int MaxResistance = ValueStore.MaxInfectionResistance;
        private const int BarWidth = 80;
        private const int BarHeight = 15;
        private const int CellCounterWidth = 200;
        private const int HintWidth = 200;
        private const int HintHeight = 30;

        private readonly SelectableButton mAttackButton;
        private readonly SelectableButton mStopButton;
        private readonly SelectableButton mFloatButton;
        private readonly SelectableButton mDivideButton;
        private readonly SelectableButton mStemcellButton;
        private readonly SelectableButton mTcellButton;
        private readonly SelectableButton mBcellButton;
        private readonly SelectableButton mMacrophageButton;
        private readonly SelectableButton mRedBloodcellButton;
        private readonly SelectableButton mAntibodyButton;
        private readonly Bar mLifepointsBar;
        private readonly Bar mLifespanBar;
        private readonly Bar mDivisionSpeedBar;
        private readonly Bar mSpeedBar;
        private readonly Bar mResistanceBar;
        private readonly Bar mStrengthBar;
        private readonly CellInfoPanel mCellInfoPanel;
        private readonly CellListPanel mCellListPanel;
        [NonSerialized]
        private ContentLoader mContentLoader;

        private readonly Button mCellCounter;
        private readonly Button mHintButton;


        /// <summary>
        /// Creates a new HUD Screen
        /// </summary>
        /// <param name="input">The input dispatcher</param>
        /// <param name="window">The window</param>
        /// <param name="selectionManager">The selection manager</param>
        /// <param name="objectCaches"></param>
        /// <param name="screenManager">Screen manager</param>
        public HudScreen(InputDispatcher input, GameWindow window, SelectionManager selectionManager,
            ObjectCaches objectCaches, ScreenManager screenManager)
        {
            mWindow = window;
            mSelectionManager = selectionManager;
            mObjectCaches = objectCaches;
            mScreenManager = screenManager;
            mUnits = mSelectionManager.SelectedObjects.Cast<Unit>();

            ScreenEnabled = true;

            selectionManager.SelectionChanged += OnSelectionChanged;

            mLifepointsBar = new Bar(BarWidth, BarHeight, Color.Green, MaxLifepoints, "Lifepoints: ");
            mLifespanBar = new Bar(BarWidth, BarHeight, Color.Blue, MaxLifespan, "Lifespan: ");
            mDivisionSpeedBar = new Bar(BarWidth, BarHeight, Color.Red, MaxDivisionSpeed, "Division rate: ");
            mSpeedBar = new Bar(BarWidth, BarHeight, Color.Red, MaxSpeed, "Speed: ");
            mStrengthBar = new Bar(BarWidth, BarHeight, Color.Red, MaxStrength, "Strength: ");
            mResistanceBar = new Bar(BarWidth, BarHeight, Color.Red, MaxResistance, "Virus resistance: ");

            mBars = new List<Bar>
            {
                mLifepointsBar,
                mLifespanBar,
                mSpeedBar,
                mStrengthBar,
                mDivisionSpeedBar,
                mResistanceBar
            };

            mHintButton = new Button(input,
                window,
                new Point(ButtonBorder, -ButtonSize * 3 - ButtonBorder * 5),
                HintWidth,
                HintHeight);
            mStemcellButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) {Hint = "Stemcell"};
            mRedBloodcellButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 3 + ButtonSize, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) {Hint = "Red blood cell"};
            mMacrophageButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 5 + ButtonSize * 2, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Macrophage" };
            mTcellButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 7 + ButtonSize * 3, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "T cell" };
            mBcellButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 9 + ButtonSize * 4, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "B cell" };
            mAntibodyButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 11 + ButtonSize * 5, -ButtonSize * 2 - ButtonBorder * 3),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Antibody" };
            mAttackButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder, -ButtonSize - ButtonBorder),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Attack" };
            mStopButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 3 + ButtonSize, -ButtonSize - ButtonBorder),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Defensive" };
            mFloatButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 5 + ButtonSize * 2, -ButtonSize - ButtonBorder),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Flow" };
            mDivideButton = new SelectableButton(input,
                window,
                new Point(ButtonBorder * 7 + ButtonSize * 3, -ButtonSize - ButtonBorder),
                ButtonSize,
                ButtonSize,
                SelectedStyle.NotSelectable) { Hint = "Divide" };
            mCellInfoPanel = new CellInfoPanel(input,
                window,
                new Point(-PanelWidth - PanelBorder, -PanelHeight - PanelBorder),
                PanelWidth,
                PanelHeight,
                mBars);
            mCellListPanel = new CellListPanel(input,
                window,
                new Point(-PanelWidth - ListPanelWidth - PanelBorder, -PanelHeight - PanelBorder),
                ListPanelWidth,
                PanelHeight, mSelectionManager);
            mCellCounter = new Button(input,
                window,
                new Point(-ButtonSize - CellCounterWidth - ButtonBorder, 0),
                CellCounterWidth,
                ButtonSize);
            var menuButton = new Button(input, window, new Point(-ButtonSize, 0), ButtonSize, ButtonSize)
            {
                Caption = "Menu"
            };
            mItems = new List<HudItem>
            {
                mStemcellButton,
                mRedBloodcellButton,
                mMacrophageButton,
                mTcellButton,
                mBcellButton,
                mAntibodyButton,
                mAttackButton,
                mStopButton,
                mFloatButton,
                mDivideButton,
                menuButton,
                mCellInfoPanel,
                mCellListPanel,
                mCellCounter,
                mHintButton
            };

            input.RegisterListener(this);
            menuButton.OnClick += OnMenuButtonClick;
            mStemcellButton.OnClick += OnDivisionResultButtonClick;
            mMacrophageButton.OnClick += OnDivisionResultButtonClick;
            mRedBloodcellButton.OnClick += OnDivisionResultButtonClick;
            mTcellButton.OnClick += OnDivisionResultButtonClick;
            mBcellButton.OnClick += OnDivisionResultButtonClick;
            mAntibodyButton.OnClick += OnDivisionResultButtonClick;
            mDivideButton.OnClick += OnDivideButtonClick;
            mStopButton.OnClick += OnStopButtonClick;
            mFloatButton.OnClick += OnFloatButtonClick;
            mAttackButton.OnClick += OnAttackButtonClick;
        }

        private void OnMenuButtonClick(object sender, EventArgs eventArgs)
        {
            mScreenManager.PushMenuScreen(typeof(PauseScreen));
        }

        private void OnDivisionResultButtonClick(object sender, EventArgs eventArgs)
        {
            if (sender == mStemcellButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Stemcell))
                {
                    ((Stemcell)unit).SetDivisionResult(Stemcell.DivisionResult.Stemcell);
                }
            }
            if (sender == mRedBloodcellButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Stemcell))
                {
                    ((Stemcell)unit).SetDivisionResult(Stemcell.DivisionResult.RedBloodcell);
                }
            }
            if (sender == mMacrophageButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Stemcell))
                {
                    ((Stemcell)unit).SetDivisionResult(Stemcell.DivisionResult.Macrophage);
                }
            }
            if (sender == mTcellButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Stemcell))
                {
                    ((Stemcell)unit).SetDivisionResult(Stemcell.DivisionResult.Tcell);
                }
            }
            if (sender == mBcellButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Stemcell))
                {
                    ((Stemcell)unit).SetDivisionResult(Stemcell.DivisionResult.Bcell);
                }
                foreach (var unit in mUnits.Where(unit => unit is Bcell))
                {
                    ((Bcell)unit).SetDivisionResult(Bcell.DivisionResult.Bcell);
                }
            }
            if (sender == mAntibodyButton)
            {
                foreach (var unit in mUnits.Where(unit => unit is Bcell).Where(unit => ((Bcell)unit).Antigen != ""))
                {
                    ((Bcell)unit).SetDivisionResult(Bcell.DivisionResult.Antibody);
                }
            }
        }

        /// <summary>
        /// Is executed when the attack button is clicked
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnAttackButtonClick(object sender, EventArgs eventArgs)
        {
            foreach (var unit in mUnits.Where(unit => unit is ICanAttack || unit is ICanDebuff || unit is ICanDeInfect))
            {
                unit.Mode = Unit.UnitMode.Offensive;
            }
            UpdateButtons();
        }

        /// <summary>
        /// Is executed when the float button is clicked
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnFloatButtonClick(object sender, EventArgs eventArgs)
        {
            foreach (var unit in mUnits)
            {
                unit.Mode = Unit.UnitMode.Drift;
            }
            UpdateButtons();
        }

        /// <summary>
        /// Is executed when the stop button is clicked
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnStopButtonClick(object sender, EventArgs eventArgs)
        {
            foreach (var unit in mUnits)
            {
                unit.Mode = Unit.UnitMode.Defensive;
            }
            UpdateButtons();
        }

        /// <summary>
        /// Is executed when the divide button is clicked
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnDivideButtonClick(object sender, EventArgs eventArgs)
        {
            foreach (var unit in mUnits.Where(unit => unit is ICanCellDivision))
            {
                unit.Mode = Unit.UnitMode.CellDivision;
            }
            UpdateButtons();
        }

        /// <summary>
        /// Is executed if the selection list changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="selectionChangeEventArgs">The event arguments</param>
        private void OnSelectionChanged(object sender, SelectionChangeEventArgs selectionChangeEventArgs)
        {
            mUnits = mSelectionManager.SelectedObjects.Cast<Unit>();
            UpdateButtons();
            UpdateBars();
            UpdateCellInfos();
        }

        private void UpdateCellInfos()
        {
            mCellListPanel.UpdateList(mUnits);
        }

        /// <summary>
        /// This method updates if the buttons are selected and selectable
        /// </summary>
        private void UpdateButtons()
        {
            if (mUnits.Any(unit => unit is Stemcell))
            {
                if (mUnits.All(unit => unit is Stemcell))
                {
                    mStemcellButton.Selectable = SelectedStyle.FullSelectable;
                    mRedBloodcellButton.Selectable = SelectedStyle.FullSelectable;
                    mMacrophageButton.Selectable = SelectedStyle.FullSelectable;
                }
                else
                {
                    mStemcellButton.Selectable = SelectedStyle.PartSelectable;
                    mRedBloodcellButton.Selectable = SelectedStyle.PartSelectable;
                    mMacrophageButton.Selectable = SelectedStyle.PartSelectable;
                }
            }
            else
            {
                mStemcellButton.Selectable = SelectedStyle.NotSelectable;
                mRedBloodcellButton.Selectable = SelectedStyle.NotSelectable;
                mMacrophageButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any(unit => unit is Tcell || unit is Stemcell))
            {
                mTcellButton.Selectable = mUnits.All(unit => unit is Tcell || unit is Stemcell)
                    ? SelectedStyle.FullSelectable
                    : SelectedStyle.PartSelectable;
            }
            else
            {
                mTcellButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any(unit => unit is Bcell || unit is Stemcell))
            {
                mBcellButton.Selectable = mUnits.All(unit => unit is Bcell || unit is Stemcell)
                    ? SelectedStyle.FullSelectable
                    : SelectedStyle.PartSelectable;
            }
            else
            {
                mBcellButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any(unit => unit is Bcell && ((Bcell)unit).Antigen != ""))
            {
                mAntibodyButton.Selectable = mUnits.All(unit => unit is Bcell && ((Bcell)unit).Antigen != "")
                    ? SelectedStyle.FullSelectable
                    : SelectedStyle.PartSelectable;
            }
            else
            {
                mAntibodyButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any(unit => unit is ICanCellDivision))
            {
                mDivideButton.Selectable = mUnits.All(unit => unit is ICanCellDivision)
                    ? SelectedStyle.FullSelectable
                    : SelectedStyle.PartSelectable;
            }
            else
            {
                mDivideButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any(unit => unit is ICanAttack || unit is ICanDebuff || unit is ICanDeInfect))
            {
                mAttackButton.Selectable = mUnits.All(unit => unit is ICanAttack || unit is ICanDebuff || unit is ICanDeInfect)
                    ? SelectedStyle.FullSelectable
                    : SelectedStyle.PartSelectable;
            }
            else
            {
                mAttackButton.Selectable = SelectedStyle.NotSelectable;
            }

            if (mUnits.Any())
            {
                mStopButton.Selectable = SelectedStyle.FullSelectable;
                mFloatButton.Selectable = SelectedStyle.FullSelectable;
            }
            else
            {
                mStopButton.Selectable = SelectedStyle.NotSelectable;
                mFloatButton.Selectable = SelectedStyle.NotSelectable;
            }

            mStemcellButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Stemcell && ((Stemcell) unit).GetDivisionResult() == Stemcell.DivisionResult.Stemcell);
            mRedBloodcellButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Stemcell && ((Stemcell)unit).GetDivisionResult() == Stemcell.DivisionResult.RedBloodcell);
            mTcellButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Stemcell && ((Stemcell) unit).GetDivisionResult() == Stemcell.DivisionResult.Tcell) |
                mUnits.Any(unit => unit is Tcell);
            mMacrophageButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Stemcell && ((Stemcell)unit).GetDivisionResult() == Stemcell.DivisionResult.Macrophage);
            mBcellButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Stemcell && ((Stemcell) unit).GetDivisionResult() == Stemcell.DivisionResult.Bcell) |
                mUnits.Any(
                    unit =>
                        unit is Bcell && ((Bcell) unit).GetDivisionResult() == Bcell.DivisionResult.Bcell);
            mAntibodyButton.Selected =
                mUnits.Any(
                    unit =>
                        unit is Bcell && ((Bcell)unit).GetDivisionResult() == Bcell.DivisionResult.Antibody);
            mStopButton.Selected = mUnits.Any(unit => unit.Mode == Unit.UnitMode.Defensive);
            mFloatButton.Selected = mUnits.Any(unit => unit.Mode == Unit.UnitMode.Drift);
            mAttackButton.Selected = mUnits.Any(unit => unit.Mode == Unit.UnitMode.Offensive);
            mDivideButton.Selected = mUnits.Any(unit => unit.Mode == Unit.UnitMode.CellDivision);
        }

        /// <summary>
        /// This mothod updates the property bars on the cell info panel
        /// </summary>
        private void UpdateBars()
        {
            if (mUnits.Any())
            {
                mCellInfoPanel.Visible = true;
                mLifepointsBar.MaxValue = mUnits.Average(unit => unit.GetMaxLifepoints());
                mLifespanBar.MaxValue = mUnits.Average(unit => unit.GetMaxLifespan());
                mLifepointsBar.SetWidth((int) (mLifepointsBar.MaxValue / ValueStore.MaxLifepoints * BarWidth));
                mLifespanBar.SetWidth((int) (mLifespanBar.MaxValue / ValueStore.MaxLifespan * BarWidth));
                mLifepointsBar.Value = (int) mUnits.Average(unit => unit.GetLifepoints());
                mLifespanBar.Value = (int) mUnits.Average(unit => unit.GetLifespan());             
                mSpeedBar.Value = (int) mUnits.Average(unit => unit.GetBaseSpeed());
                if (mUnits.Any(unit => unit is ICanAttack || unit is ICanDeInfect))
                {
                    mStrengthBar.Value =
                        Math.Round(mUnits.Where(unit => unit is ICanAttack || unit is ICanDeInfect)
                            .Average(
                                unit =>
                                    unit is ICanAttack
                                        ? ((ICanAttack) unit).GetAttackPower()
                                        : ((ICanDeInfect) unit).GetDeInfectionPower()),
                            1);
                    mStrengthBar.Visible = true;
                }
                else mStrengthBar.Visible = false;
                if (mUnits.Any(unit => unit is ICanCellDivision))
                {
                    mDivisionSpeedBar.Value =
                        (int) mUnits.Where(unit => unit is ICanCellDivision)
                            .Average(unit => ((ICanCellDivision) unit).GetCellDivisionRate());
                    mDivisionSpeedBar.Visible = true;
                }
                else mDivisionSpeedBar.Visible = false;

                if (mUnits.Any(unit => unit is IInfectable))
                {
                    mResistanceBar.Visible = true;
                    mResistanceBar.Value = (int) mUnits.Where(unit => unit is IInfectable).Average(unit => ((IInfectable)unit).GetInfectionResistance());
                }
                else mResistanceBar.Visible = false;
            }
            else
            {
                mCellInfoPanel.Visible = false;
            }
            
        }

        /// <inheritdoc />
        public bool ScreenEnabled { get; private set; }

        /// <inheritdoc />
        public bool UpdateLower()
        {
            return true;
        }

        /// <inheritdoc />
        public bool DrawLower()
        {
            return true;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            mCellCounter.Caption = "Red blood cells: " + mObjectCaches.ListRedBloodcell.Count.ToString(CultureInfo.InvariantCulture);
            mHintButton.Visible = false;
            foreach (var item in mItems.Where(item => item.ShowHint))
            {
                mHintButton.Visible = true;
                mHintButton.Caption = item.Hint;
            }
            UpdateButtons();
            UpdateBars();
            foreach (var hudItem in mItems)
            {
                hudItem.Update();
            }
        }

        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            mContentLoader = contentLoader;
            foreach (var item in mItems)
            {
                item.LoadContent(contentLoader);
            }
            foreach (var bar in mBars)
            {
                bar.LoadContent(contentLoader);
            }
            mAttackButton.ButtonTexture = contentLoader.LoadTexture("Attack");
            mStopButton.ButtonTexture = contentLoader.LoadTexture("Stop");
            mFloatButton.ButtonTexture = contentLoader.LoadTexture("Float");
            mDivideButton.ButtonTexture = contentLoader.LoadTexture("CellDivision");
            mStemcellButton.ButtonTexture = contentLoader.LoadTexture("Stemcell");
            mMacrophageButton.ButtonTexture = contentLoader.LoadTexture("Macrophage");
            mRedBloodcellButton.ButtonTexture = contentLoader.LoadTexture("RedBloodcell");
            mTcellButton.ButtonTexture = contentLoader.LoadTexture("Tcell");
            mBcellButton.ButtonTexture = contentLoader.LoadTexture("Bcell");
            mAntibodyButton.ButtonTexture = contentLoader.LoadTexture("Antibody");
        }

        /// <inheritdoc />
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Begin();
            foreach (var item in mItems)
            {
                item.Draw(gameTime, spriteBatch, spriteFont);
            }
            spriteBatch.End();
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }

        public bool HandleActionPerformed(UserAction action)
        {
            if (action == UserAction.SelectDefensiveMode) OnStopButtonClick(null, null);
            if (action == UserAction.SelectAttackMode) OnAttackButtonClick(null, null);
            if (action == UserAction.SelectCellDivisionMode) OnDivideButtonClick(null, null);
            if (action == UserAction.SelectFlowMode) OnFloatButtonClick(null, null);
            if (action == UserAction.BuildAntibody) OnDivisionResultButtonClick(mAntibodyButton, null);
            if (action == UserAction.BuildBcell) OnDivisionResultButtonClick(mBcellButton, null);
            if (action == UserAction.BuildMacrophage) OnDivisionResultButtonClick(mMacrophageButton, null);
            if (action == UserAction.BuildRedBloodcell) OnDivisionResultButtonClick(mRedBloodcellButton, null);
            if (action == UserAction.BuildStemcell) OnDivisionResultButtonClick(mStemcellButton, null);
            if (action == UserAction.BuildTCell) OnDivisionResultButtonClick(mTcellButton, null);
            return false;
        }

        /// <summary>
        /// Load contents after deserialization.
        /// </summary>
        /// <param name="screenManager">The screenmanager of the game.</param>
        /// <param name="gameWindow">The game window.</param>
        /// <param name="contentLoader">The contentloader of the game.</param>
        public void LoadGameState(ScreenManager screenManager, GameWindow gameWindow, ContentLoader contentLoader)
        {
            mScreenManager = screenManager;
            mWindow = gameWindow;
            mContentLoader = contentLoader;
            mUnits = mSelectionManager.SelectedObjects.Cast<Unit>();
            foreach (var item in mItems)
            {
                item.LoadGameState(mWindow, contentLoader);
            }
            foreach (var bar in mBars)
            {
                bar.LoadContent(mContentLoader);
            }
            LoadContent(contentLoader);
        }

        /// <inheritdoc />
        public void Destroy()
        {
            foreach (var item in mItems)
                item.Destroy();
        }
    }
}
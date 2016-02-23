using System;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive.Infection;
using Antigen.Logic.Selection;
using Antigen.Logic.UnitModes;
using Antigen.Objects.Units.Values;
using Antigen.Sound;
using Antigen.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// The stemmcell unit is the basic unit of the player. It can create other cells and walk.
    /// </summary>
    [Serializable]
    sealed class Stemcell : Unit, IMapCollidable, IObjectCollidable, ISelectable, ICanCellDivision, IInfectable
    {   
        private readonly DivisionBehavior mDivBehavior;
        private readonly UnitModeManager mModeManager;
        private readonly CellDivisionControl mCellDivisionControl;

        private bool Infected { get; set; }

        private DivisionResult mDivisionResult;

        /// <summary>
        /// All results that a cell division of this unit can bring.
        /// </summary>
        public enum DivisionResult
        {
            Stemcell,
            Bcell,
            Tcell,
            Macrophage,
            RedBloodcell
        }

        /// <summary>
        /// Creates a new Stemmcell with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        public Stemcell(ObjectCaches objectCaches, IDivisionContentLoader loader,
            InputDispatcher input, SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats)
            : this(objectCaches, loader, input, selectionManager, startPos, thatMap, soundWrapper, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new Stemmcell with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        /// <param name="thatInheritance">Inheritance table</param>
        public Stemcell(ObjectCaches objectCaches, IDivisionContentLoader loader,
            InputDispatcher input, SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Stemcell", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            mDivBehavior = new DivisionBehavior(this, input, thatMap, selectionManager,
                objectCaches, soundWrapper, stats);
            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            var defensiveControl = new DefensiveControl(this, MoveBehavior);
            var driftControl = new DriftControl(MoveBehavior);
            mCellDivisionControl = new CellDivisionControl(this, mDivBehavior, loader);
            mModeManager = new UnitModeManager(this, mCellDivisionControl, defensiveControl, driftControl, null, UnitMode.Defensive, input);

            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), (float)MaxLifespan / 100,
                0, Color.Black, Color.White, Color.White, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            mCellDraw.Update((int) mValues.GetLifepoints(), (int) mValues.GetLifespan(), false, mIsUnderAttack);
            base.Update(gameTime);
            mModeManager.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Die()
        {
            base.Die();
            Selected = false;
        }

        /// <inheritdoc />
        public override UnitMode Mode
        {
            get { return mModeManager.CurrentMode; }
            set
            {
                if (value == UnitMode.Offensive)
                    throw new InvalidUnitModeException("Attempt to activate offensive mode for stem cell.");
                mModeManager.CurrentMode = value;
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            mCellDraw.Draw(spriteBatch, spriteFont, Position, 1 - GetRemainingCellDivisionDuration() / GetCellDivisionRate(), Selected);
        }

        /// <inheritdoc />
        public bool IsVirtualCollidable
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsCellDividing()
        {
            return mDivBehavior.IsCellDividing;
        }

        /// <inheritdoc />
        public double GetCellDivisionRate()
        {
            return mValues.GetCellDivisionRate();
        }

        /// <inheritdoc />
        public void ChangeCellDivisionRate(double value)
        {
            mValues.ChangeCellDivisionRate(value);
        }

        /// <inheritdoc />
        public double GetRemainingCellDivisionDuration()
        {
            return mCellDivisionControl.GetRemainingCellDivisionDuration();
        }

        /// <inheritdoc />
        public bool IsInfected()
        {
            return Infected;
        }

        /// <inheritdoc />
        public void GettingInfected()
        {
            Infected = true;
        }

        /// <inheritdoc />
        public int GetInfectionResistance()
        {
            return mValues.GetInfectionResistance();
        }

        /// <inheritdoc />
        public void ChangeInfectionResistance(int amount)
        {
            mValues.ChangeInfectionResistance(amount);
        }

        /// <summary>
        /// Sets the current result for a division of this unit.
        /// </summary>
        /// <param name="thatResult">Resulting object of division</param>
        public void SetDivisionResult(DivisionResult thatResult)
        {
            mDivisionResult = thatResult;
        }

        /// <summary>
        /// Gets the current result for a division of this unit.
        /// </summary>
        /// <returns>Resulting object of division</returns>
        public DivisionResult GetDivisionResult()
        {
            return mDivisionResult;
        }

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            mDivisionResult = DivisionResult.Stemcell;
            Side = UnitSide.Friendly;

            mValues = new ValueStore(7, 7, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, 2, 9, 6, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool Selected { get; set; }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

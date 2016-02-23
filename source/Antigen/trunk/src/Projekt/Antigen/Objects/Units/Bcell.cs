using System;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.AntigenExchange;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive.Infection;
using Antigen.Logic.Selection;
using Antigen.Logic.UnitModes;
using Antigen.Objects.Properties;
using Antigen.Objects.Units.Values;
using Antigen.Sound;
using Antigen.Statistics;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// The Bcell unit can produce antibodies and collect antigens.
    /// </summary>
    [Serializable]
    sealed class Bcell : Unit, IMapCollidable, IObjectCollidable, ISelectable, ICanCellDivision, IInfectable, IHasAntigen
    {
        private readonly UnitModeManager mModeManager;
        private readonly DivisionBehavior mDivBehavior;
        private readonly CellDivisionControl mCellDivisionControl;
        private readonly AntigenExchangeControl mExchangeControl;

        private DivisionResult mDivisionResult;
        private DebuffTable mDebuffTable;
        private string mAntigen;

        private bool Infected { get; set; }

        /// <summary>
        /// All results that a cell division of this unit can bring.
        /// </summary>
        public enum DivisionResult
        {
            Bcell,
            Antibody
        }

        /// <inheritdoc />
        public string Antigen
        {
            get { return mAntigen; }
            set
            {
                mAntigen = value;
                if (mCellDraw != null) mCellDraw.NucleusColor = Functions.StrainToColor(value);
            }
        }

        /// <summary>
        /// Creates a new Bcell with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        /// <param name="thatDebuffTable">Buff table for with this unit created antibodies</param>
        //Resharper hint: Engine should support spawning this unit without cell division
// ReSharper disable UnusedMember.Global
        public Bcell(ObjectCaches objectCaches, IDivisionContentLoader loader, InputDispatcher input,
            SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap,
            SoundWrapper soundWrapper, IStatisticIncrementer stats, DebuffTable thatDebuffTable = null)
            : this(objectCaches, loader, input, selectionManager, startPos, thatMap, InheritanceTable.CreateEmptyTable(),
            soundWrapper, stats, thatDebuffTable)
// ReSharper restore UnusedMember.Global
        {
        }

        /// <summary>
        /// Creates a new Bcell with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="thatInheritance">Inheritance table</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        /// <param name="thatDebuffTable">Buff table for with this unit created antibodies</param>
        public Bcell(ObjectCaches objectCaches, IDivisionContentLoader loader, InputDispatcher input,
            SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap,
            InheritanceTable thatInheritance, SoundWrapper soundWrapper, IStatisticIncrementer stats, DebuffTable thatDebuffTable = null)
            : base(objectCaches, startPos, "B-Cell", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            mDebuffTable = thatDebuffTable ?? DebuffTable.CreateAverageBuff();
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            var exchangeBehavior = new AntigenExchangeBehavior(this);
            mDivBehavior = new DivisionBehavior(this, input, thatMap, selectionManager, objectCaches,
                soundWrapper, stats);

            var defensiveControl = new DefensiveControl(this, MoveBehavior);
            var driftControl = new DriftControl(MoveBehavior);
            mCellDivisionControl = new CellDivisionControl(this, mDivBehavior, loader);
            mExchangeControl = new AntigenExchangeControl(this, exchangeBehavior, input);
            mModeManager = new UnitModeManager(this, mCellDivisionControl, defensiveControl, driftControl, null, UnitMode.Defensive, input);

            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), (float)MaxLifespan / 100, 0,
                Color.Orange, Functions.StrainToColor(Antigen), Color.White, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            mCellDraw.Update((int)mValues.GetLifepoints(), (int)mValues.GetLifespan(), false, mIsUnderAttack);
            base.Update(gameTime);
            mModeManager.Update(gameTime);
            mExchangeControl.Update(gameTime);            
        }

        /// <inheritdoc />
        public override UnitMode Mode
        {
            get { return mModeManager.CurrentMode; }
            set
            {
                if (value == UnitMode.Offensive)
                    throw new InvalidUnitModeException("Attempt to activate offensive mode for B Cell.");
                mModeManager.CurrentMode = value;
            }
        }

        /// <inheritdoc />
        public override void Die()
        {
            base.Die();
            Selected = false;
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
        public bool Selected { get; set; }

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
            if (thatResult == DivisionResult.Antibody && Antigen.Equals(""))
            {
                throw new InvalidUnitModeException(Name + " can not produce antibodies without an antigen.");
            }
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

        /// <summary>
        /// Gets the buff table of with this unit created antibodies.
        /// </summary>
        /// <returns>Buff table of antibodies</returns>
        public DebuffTable GetDebuffTable()
        {
            return mDebuffTable;
        }

        /// <summary>
        /// Sets the buff table of with this unit created antibodies.
        /// </summary>
        /// <param name="thatDebuffTable">Table to set</param>
        public void SetDebuffTable(DebuffTable thatDebuffTable)
        {
            mDebuffTable = thatDebuffTable;
        }

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            Antigen = inheritance.Antigen;
            mDivisionResult = DivisionResult.Bcell;
            Side = UnitSide.Friendly;

            mValues = new ValueStore(3, 5, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, 4, 6, 3, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

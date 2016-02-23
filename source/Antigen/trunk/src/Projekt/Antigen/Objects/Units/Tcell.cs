using System;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.AntigenExchange;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive;
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
    /// The Tcell unit can free from virus infected cells and reproduce itself.
    /// </summary>
    [Serializable]
    sealed class Tcell : Unit, IMapCollidable, IObjectCollidable, ISelectable, ICanCellDivision, IInfectable, IHasAntigen, ICanDeInfect
    {
        private readonly UnitModeManager mModeManager;
        private readonly OffensiveBehavior mOffensiveBehavior;
        private readonly DivisionBehavior mDivBehavior;
        private readonly CellDivisionControl mCellDivisionControl;
        private readonly AntigenExchangeControl mExchangeControl;
        private string mAntigen;

        private bool Infected { get; set; }

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
        /// Creates a new Tcell with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="selectionManager">The game's selection manager.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        //Resharper hint: Engine should support spawning this unit without cell division
// ReSharper disable UnusedMember.Global
        public Tcell(ObjectCaches objectCaches, IDivisionContentLoader loader,
// ReSharper restore UnusedMember.Global
            InputDispatcher input, SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats)
            : this(objectCaches, loader, input, selectionManager, startPos, thatMap, soundWrapper, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new Tcell with a starting Position on the map.
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
        public Tcell(ObjectCaches objectCaches, IDivisionContentLoader loader,
            InputDispatcher input, SelectionManager selectionManager, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "T-Cell", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            mDivBehavior = new DivisionBehavior(this, input, thatMap, selectionManager,
                objectCaches, soundWrapper, stats);
            mOffensiveBehavior = new OffensiveBehavior(this, soundWrapper, stats);
            var exchangeBehavior = new AntigenExchangeBehavior(this);

            var offensiveControl = new OffensiveControl(this, MoveBehavior, mOffensiveBehavior, objectCaches.SpatialCache);
            var defensiveControl = new DefensiveControl(this, MoveBehavior, mOffensiveBehavior);
            var driftControl = new DriftControl(MoveBehavior);
            mCellDivisionControl = new CellDivisionControl(this, mDivBehavior, loader);
            mExchangeControl = new AntigenExchangeControl(this, exchangeBehavior, input);
            mModeManager = new UnitModeManager(this, mCellDivisionControl, defensiveControl, driftControl, offensiveControl, UnitMode.Defensive, input);

            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), (float)MaxLifespan / 100,
                0, Color.DeepPink, Functions.StrainToColor(Antigen), Color.White, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            mCellDraw.Update((int)mValues.GetLifepoints(), (int)mValues.GetLifespan(), mOffensiveBehavior.IsOffensive(), mIsUnderAttack);
            base.Update(gameTime);
            mModeManager.Update(gameTime);
            mExchangeControl.Update(gameTime);
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
            set { mModeManager.CurrentMode = value; }
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
        public float GetDeInfectionPower()
        {
            return mValues.GetDeInfectionPower();
        }

        /// <inheritdoc />
        public void ChangeDeInfectionPower(float value)
        {
            mValues.ChangeDeInfectionPower(value);
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

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            Antigen = inheritance.Antigen;
            Side = UnitSide.Friendly;

            mValues = new ValueStore(6, 5, ValueStore.NoValue, ValueStore.NoValue, 7, 5, 6, 5, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int) MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

using System;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.AntigenExchange;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive;
using Antigen.Logic.Offensive.Debuff;
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
    /// The Antibody unit can be produced from Bcells to debuff enemy cells.
    /// </summary>
    [Serializable]
    sealed class Antibody : Unit, IMapCollidable, IObjectCollidable, ISelectable, IInfectable, IHasAntigen, ICanDebuff
    {
        private readonly AntigenExchangeControl mExchangeControl;
        private readonly UnitModeManager mModeManager;

        private readonly DebuffTable mDebuffTable;

        private bool Infected { get; set; }
        /// <inheritdoc />
        public string Antigen { get; set; }

        /// <summary>
        /// Creates a new Antibody with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatDebuffTable">Buff table for this unit</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        //Resharper hint: Engine should support spawning this unit without cell division
// ReSharper disable UnusedMember.Global
        public Antibody(ObjectCaches objectCaches, InputDispatcher input,
// ReSharper restore UnusedMember.Global
            Vector2 startPos, DebuffTable thatDebuffTable, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats)
            : this(objectCaches, input, startPos, thatDebuffTable,
            thatMap, soundWrapper, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new Antibody with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatDebuffTable">Buff table for this unit</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        /// <param name="thatInheritance">Inheritance table</param>
        public Antibody(ObjectCaches objectCaches, InputDispatcher input,
            Vector2 startPos, DebuffTable thatDebuffTable, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Antibody", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            mDebuffTable = thatDebuffTable;
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            var exchangeBehavior = new AntigenExchangeBehavior(this);
            var offensiveBehavior = new OffensiveBehavior(this, soundWrapper, stats);

            var offensiveControl = new OffensiveControl(this, MoveBehavior, offensiveBehavior, objectCaches.SpatialCache);
            var defensiveControl = new DefensiveControl(this, MoveBehavior, offensiveBehavior);
            var driftControl = new DriftControl(MoveBehavior);
            mModeManager = new UnitModeManager(this, null, defensiveControl, driftControl, offensiveControl, UnitMode.Defensive, input);
            mExchangeControl = new AntigenExchangeControl(this, exchangeBehavior, input);

            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), Radius / 2f, 0, Color.Yellow,
                Functions.StrainToColor(Antigen), Color.White, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
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
                if (value == UnitMode.CellDivision)
                    throw new InvalidUnitModeException("Attempt to activate cell division mode for antibody.");
                mModeManager.CurrentMode = value;
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            mCellDraw.Draw(spriteBatch, spriteFont, Position, 0, Selected);
        }

        /// <inheritdoc />
        public bool IsVirtualCollidable
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Selected { get; set; }

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
        public DebuffTable GetDebuffTable()
        {
            return mDebuffTable;
        }

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            Antigen = inheritance.Antigen;
            Side = UnitSide.Friendly;

            mValues = new ValueStore(3, 4, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, 7, ValueStore.NoValue, 2, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

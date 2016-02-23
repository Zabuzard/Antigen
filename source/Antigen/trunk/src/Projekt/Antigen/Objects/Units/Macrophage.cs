using System;
using Antigen.Graphics;
using Antigen.Input;
using Antigen.Logic.AntigenExchange;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive;
using Antigen.Logic.Offensive.Attack;
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
    /// The macrophage unit is the basic combat unit of the player.
    /// It can create other attack and walk.
    /// </summary>
    [Serializable]
    sealed class Macrophage : Unit, IMapCollidable, IObjectCollidable, ISelectable, IHasAntigen, IInfectable, ICanAttack, IAntigenProvider
    {
        private readonly UnitModeManager mModeManager;
        private readonly OffensiveBehavior mOffensiveBehavior;
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
        /// Creates a new Macrophage with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        //Resharper hint: Engine should support spawning this unit without cell division
// ReSharper disable UnusedMember.Global
        public Macrophage(ObjectCaches objectCaches,
// ReSharper restore UnusedMember.Global
            InputDispatcher input, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats)
            : this(objectCaches, input, startPos, thatMap, soundWrapper, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new Macrophage with a starting Position on the map.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="input">Input manager which fires events</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        /// <param name="thatInheritance">Inheritance table</param>
        public Macrophage(ObjectCaches objectCaches,
            InputDispatcher input, Vector2 startPos, Map.Map thatMap, SoundWrapper soundWrapper,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Macrophage", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            mOffensiveBehavior = new OffensiveBehavior(this, soundWrapper, stats);
            var exchangeBehavior = new AntigenExchangeBehavior((IAntigenProvider) this);
            var offensiveControl = new OffensiveControl(this, MoveBehavior, mOffensiveBehavior, objectCaches.SpatialCache);
            var defensiveControl = new DefensiveControl(this, MoveBehavior, mOffensiveBehavior);
            var driftControl = new DriftControl(MoveBehavior);
            mExchangeControl = new AntigenExchangeControl((IAntigenProvider) this, exchangeBehavior, input);
            mModeManager = new UnitModeManager(this, null, defensiveControl, driftControl, offensiveControl, UnitMode.Defensive, input);

            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), (float)MaxLifespan / 100,
                Math.Min((int)mValues.GetAttackPower() * 5, 20), Color.Gray, Color.Gray, Color.White, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
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
            set
            {
                if (value == UnitMode.CellDivision)
                    throw new InvalidUnitModeException("Attempt to activate drift mode for Macrophage.");
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
        public float GetAttackPower()
        {
            return mValues.GetAttackPower();
        }

        /// <inheritdoc />
        public void ChangeAttackPower(float value)
        {
            mValues.ChangeAttackPower(value);
        }

        /// <inheritdoc />
        public string ProvideAntigen()
        {
            var result = Antigen;
            Antigen = "";
            return result;
        }

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            Antigen = inheritance.Antigen;
            Side = UnitSide.Friendly;

            mValues = new ValueStore(5, 5, 7, ValueStore.NoValue, ValueStore.NoValue, 6, ValueStore.NoValue, 3, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}
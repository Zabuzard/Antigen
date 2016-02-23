using System;
using Antigen.Graphics;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive.Infection;
using Antigen.Logic.UnitModes;
using Antigen.Objects.Units.Values;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// The red blood cell is the basic unit of the neutral player. It moves with the flow of the map.
    /// </summary>
    [Serializable]
    sealed class RedBloodcell : Unit, IMapCollidable, IObjectCollidable, IInfectable
    {
        private readonly DriftControl mDriftControl;

        private bool Infected { get; set; }

        /// <summary>
        /// Creates a new red blood cell with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        public RedBloodcell(ObjectCaches objectCaches, Vector2 startPos, Map.Map thatMap)
            : this(objectCaches, startPos, thatMap, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new red blood cell with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="thatInheritance">Inheritance table</param>
        public RedBloodcell(ObjectCaches objectCaches, Vector2 startPos, Map.Map thatMap,
            InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Red Bloodcell", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            mDriftControl = new DriftControl(MoveBehavior);
            Mode = UnitMode.Drift;
            mDriftControl.Activate();
            mCellDraw = new CellDraw(Radius, Math.Max(mValues.GetInfectionResistance() / 10, 1), 0,
                0, Color.Red, Color.Gray, Color.Gray, (int) mValues.GetLifepoints(), (int) mValues.GetLifespan());
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            mCellDraw.Update((int)mValues.GetLifepoints(), (int)mValues.GetLifespan(), false, mIsUnderAttack);
            base.Update(gameTime);
            mDriftControl.Update(gameTime);            
        }

        /// <inheritdoc />
        public override UnitMode Mode
        {
            set
            {
                if (value != UnitMode.Drift)
                    throw new InvalidUnitModeException("Attempt to activate non-drift mode for red bloodcell.");
                base.Mode = value;
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            mCellDraw.Draw(spriteBatch, spriteFont, Position, 0, false);
        }

        /// <inheritdoc />
        public bool IsVirtualCollidable
        {
            get { return false; }
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
            Side = UnitSide.Neutral;

            mValues = new ValueStore(3, 10, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, 4, ValueStore.NoValue, 2, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

using System;
using Antigen.Content;
using Antigen.Graphics;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Objects.Properties;
using Antigen.Objects.Units.Values;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = Antigen.Graphics.IDrawable;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// Abstract class for units.
    /// Units are moveable and provide there current and last position.
    /// </summary>
    [Serializable]
    abstract class Unit : GameObject, IDrawable, IUpdateable, ILoad, IHasSpeed, ILively, IAttackable, IHasSight, IDebuffable, IMutable, ICanMove
    {
        /// <summary>
        /// Identificators for units side.
        /// </summary>
        public enum UnitSide
        {
            Friendly,
            Enemy,
            Neutral
        };

        /// <summary>
        /// Mutually-exclusive unit modes.
        /// </summary>
        public enum UnitMode
        {
            Defensive,
            Drift,
            CellDivision,
            Offensive
        };

        /// <summary>
        /// Maximal lifepoints of the unit.
        /// </summary>
        protected float MaxLifepoints { get; set; }
        /// <summary>
        /// Maximal lifespan of the unit in seconds.
        /// </summary>
        protected double MaxLifespan { get; set; }
        /// <summary>
        /// Side of the unit.
        /// </summary>
        protected UnitSide Side { private get; set; }
        /// <summary>
        /// Drawing function for the unit.
        /// </summary>
        protected CellDraw mCellDraw;
        public CellDraw CellDraw { get { return mCellDraw; } }
        /// <summary>
        /// Values of this unit.
        /// </summary>
        protected ValueStore mValues;
        /// <summary>
        /// Name of the unit.
        /// </summary>
        public String Name { get; private set; }
        /// <summary>
        /// The unit's mutation table.
        /// </summary>
        public MutationTable Mutations { get; private set; }
        /// <summary>
        /// Behaviour used when moving the unit.
        /// </summary>
        protected IMoveBehavior MoveBehavior { get; set; }
        /// <summary>
        /// The game's central object caches.
        /// </summary>
        public readonly ObjectCaches mObjectCaches;

        /// <summary>
        /// Current mode of the unit.
        /// </summary>
        public virtual UnitMode Mode { get; set; }


        /// <summary>
        /// True if the unit is under attack
        /// </summary>
        protected bool mIsUnderAttack;

        /// <summary>
        /// Creates a new TestCell with a starting Position on the map.
        /// </summary>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatName">Name of the unit</param>
        /// <param name="thatMutations">Mutations of the unit</param>
        protected Unit(ObjectCaches thatObjectCaches, Vector2 startPos, String thatName, MutationTable thatMutations) : base(startPos)
        {
            mObjectCaches = thatObjectCaches;

            Radius = 20;
            Mutations = thatMutations;
            Name = thatName;
            MovementVector = Vector2.Zero;
        }

        /// <inheritdoc />
        public float GetLifepoints()
        {
            return mValues.GetLifepoints();
        }

        /// <inheritdoc />
        public float GetMaxLifepoints()
        {
            return MaxLifepoints;
        }

        /// <inheritdoc />
        public double GetLifespan()
        {
            return mValues.GetLifespan();
        }

        /// <inheritdoc />
        public double GetMaxLifespan()
        {
            return MaxLifespan;
        }

        /// <inheritdoc />
        public int GetBaseSpeed()
        {
            return mValues.GetBaseSpeed();
        }

        /// <inheritdoc />
        public void ChangeBaseSpeed(int value)
        {
            mValues.ChangeBaseSpeed(value);
            mIsUnderAttack = true;
        }

        /// <inheritdoc />
        public int GetSight()
        {
            return mValues.GetSight();
        }

        /// <inheritdoc />
        public void ChangeSight(int amount)
        {
            mValues.ChangeSight(amount);
            mIsUnderAttack = true;
        }

        /// <summary>
        /// Returns the identification of the units side specified by UnitSide.
        /// </summary>
        /// <returns>Identification of units side</returns>
        public UnitSide GetSide()
        {
            return Side;
        }

        /// <inheritdoc />
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont);

        /// <inheritdoc />
        public virtual void LoadContent(ContentLoader contentLoader)
        {
            mCellDraw.LoadContent(contentLoader);
        }

        /// <inheritdoc />
        public virtual bool IsAlive()
        {
            return GetLifepoints() > ValueStore.MinLifepoints && GetLifespan() > ValueStore.MinLifespan;
        }

        /// <inheritdoc />
        public virtual void ChangeLifepoints(float amount)
        {
            mValues.ChangeLifepoints(amount);
            mIsUnderAttack = true;
        }

        /// <inheritdoc />
        public virtual void ChangeLifespan(double amount)
        {
            mValues.ChangeLifespan(amount);
        }

        /// <inheritdoc />
        public virtual void Update(GameTime gameTime)
        {
            mIsUnderAttack = false;
            AvoidanceSteeringBehavior.AdjustAvoidanceHitbox(this, InternalAvoidanceHitbox);
            if (AvoidanceHitbox != null)
                mObjectCaches.ObjectCollision.Update(AvoidanceHitbox);

            var collidable = this as ICollidable;
            if (collidable != null)
                mObjectCaches.ObjectCollision.Update(collidable);

            ChangeLifespan(-gameTime.ElapsedGameTime.TotalSeconds);
            MoveBehavior.Update(gameTime);
            if (!IsAlive())
                Die();
        }

        /// <summary>
        /// Called during an update cycle if the unit has died.
        /// Should be overridden with unit-specific behaviour.
        /// </summary>
        public virtual void Die()
        {
            mObjectCaches.Remove(this);
            mObjectCaches.Remove(AvoidanceHitbox);
        }

        /// <inheritdoc />
        public Vector2 MovementVector { get; set; }

        /// <summary>
        /// Internal backing for the <see cref="AvoidanceHitbox"/>
        /// property. Classes inheriting from this class MUST set
        /// this to a non-<code>null</code> value in their constructor.
        /// </summary>
        protected AvoidanceSteeringBehavior.AvoidanceHitbox 
            InternalAvoidanceHitbox { private get; set; }

        /// <inheritdoc />
        public ICollidable AvoidanceHitbox
        {
            get { return InternalAvoidanceHitbox; }
        }

        /// <inheritdoc />
        public float MaxVelocity
        {
            get { return ValueStore.ConvertBaseSpeedToFloatFormat(GetBaseSpeed()); }
        }

        /// <inheritdoc />
        public float SightRange
        {
            get { return ValueStore.ConvertSightToPixel(mValues.GetSight()); }
        }

        /// <summary>
        /// Whether the unit is currently moving.
        /// </summary>
        /// <returns><code>true</code> if the unit is moving;
        /// <code>false</code> otherwise.</returns>
        public bool IsMoving()
        {
            return MoveBehavior.IsMoving;
        }
    }
}

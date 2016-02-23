using System;
using System.Collections.Generic;
using Antigen.AI;
using Antigen.Graphics;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Attack;
using Antigen.Objects.Properties;
using Antigen.Objects.Units.Values;
using Antigen.Statistics;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// The bacterium unit is one of the basic enemy units. It can walk, create other bacterium and attack cells.
    /// </summary>
    [Serializable]
    sealed class Bacterium : Unit, IMapCollidable, IObjectCollidable, ICanCellDivision, IHasStrain, ICanAttack
    {
        private readonly DivisionBehavior mDivBehavior;
        private readonly DivisionAction mDivAction;
        private readonly BasicAiControl mAiControl;

        /// <inheritdoc />
        public string Strain { get; private set; }

        /// <summary>
        /// Creates a new bacterium with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        public Bacterium(ObjectCaches objectCaches, IDivisionContentLoader loader, Vector2 startPos, Map.Map thatMap, AiBrain aiBrain,
            IStatisticIncrementer stats) : 
            this(objectCaches, loader, startPos, thatMap, aiBrain, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new bacterium with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="thatInheritance">Inheritance table</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        public Bacterium(ObjectCaches objectCaches, IDivisionContentLoader loader, Vector2 startPos, Map.Map thatMap, AiBrain aiBrain,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Bacterium", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            mDivBehavior = new DivisionBehavior(this, thatMap, objectCaches, aiBrain, stats);
            var attackBehavior = new AttackBehavior(this, null, stats);
            var points = aiBrain.GetDecisionPoints();
            mDivAction = new DivisionAction(this, mDivBehavior, loader, points, aiBrain);
            var actionList = new List<IAiAction>
            {
                new AttackAction(MoveBehavior, attackBehavior, this, points),
                mDivAction,
                new FlowAction(MoveBehavior, points),
                new EscapeAction(MoveBehavior, this, points)
            };
            mAiControl = new BasicAiControl(this, objectCaches.SpatialCache, actionList, aiBrain, points);

            mCellDraw = new CellDraw(Radius, 5, (float)MaxLifespan / 100, Math.Min((int)mValues.GetAttackPower() * 5, 20),
                Color.Green, Functions.StrainToColor(Strain), Color.Black, (int)mValues.GetLifepoints(), (int)mValues.GetLifespan());
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            mCellDraw.Draw(spriteBatch, spriteFont, Position, 1 - GetRemainingCellDivisionDuration() / GetCellDivisionRate(), false);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            mCellDraw.Update((int) mValues.GetLifepoints(), (int) mValues.GetLifespan(), false, mIsUnderAttack);
            base.Update(gameTime);
            mAiControl.Update(gameTime);
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
            return mAiControl.GetLastAction() is DivisionAction ? mDivAction.GetRemainingCellDivisionDuration() : GetCellDivisionRate();
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
        private void InitProperties(InheritanceTable inheritance)
        {
            Strain = inheritance.Antigen.Equals("") ? StrainGenerator.GetInstance().GenerateStrain() : inheritance.Antigen;
            Mode = UnitMode.Defensive;
            Side = UnitSide.Enemy;

            mValues = new ValueStore(5, 5, 7, ValueStore.NoValue, ValueStore.NoValue, 5, 8, ValueStore.NoValue, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int) MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Antigen.AI;
using Antigen.Graphics;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;
using Antigen.Objects.Units.Values;
using Antigen.Statistics;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// The Virus unit can infect other cells and then reproduce itself.
    /// </summary>
    [Serializable]
    sealed class Virus : Unit, IMapCollidable, IObjectCollidable, IHasStrain, ICanInfect, ICanCellDivision
    {
        private readonly InfectionBehavior mInfectionBehavior;
        private readonly DivisionBehavior mDivBehavior;
        private readonly VirusDivisionAction mVirusDivAction;
        private readonly BasicAiControl mAiControl;

        /// <inheritdoc />
        public string Strain { get; private set; }

        /// <summary>
        /// Creates a new virus with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        public Virus(ObjectCaches objectCaches, IDivisionContentLoader loader, Vector2 startPos, Map.Map thatMap, AiBrain aiBrain,
            IStatisticIncrementer stats)
            : this(objectCaches, loader, startPos, thatMap, aiBrain, stats, InheritanceTable.CreateEmptyTable())
        {
        }

        /// <summary>
        /// Creates a new virus with a given starting position.
        /// </summary>
        /// <param name="objectCaches">The game's object caches.</param>
        /// <param name="loader">Content loader for with division created units</param>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        /// <param name="thatInheritance">Inheritance table</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        public Virus(ObjectCaches objectCaches, IDivisionContentLoader loader, Vector2 startPos, Map.Map thatMap, AiBrain aiBrain,
            IStatisticIncrementer stats, InheritanceTable thatInheritance)
            : base(objectCaches, startPos, "Virus", thatInheritance.Mutations)
        {
            InitProperties(thatInheritance);
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            InternalAvoidanceHitbox = AvoidanceSteeringBehavior.MakeInitialAvoidanceHitbox(this);
            objectCaches.Add(AvoidanceHitbox);

            MoveBehavior = new MoveBehavior(this, thatMap, objectCaches.ObjectCollision, objectCaches.Pathfinder);
            mDivBehavior = new DivisionBehavior(this, thatMap, objectCaches, aiBrain, stats);
            mInfectionBehavior = new InfectionBehavior(this);
            var points = aiBrain.GetDecisionPoints();
            mVirusDivAction = new VirusDivisionAction(this, mDivBehavior, loader);
            var actionList = new List<IAiAction>
            {
                new InfectAction(mInfectionBehavior, MoveBehavior, this, points),
                mVirusDivAction,
                new FlowAction(MoveBehavior, points)
            };
            mAiControl = new BasicAiControl(this, objectCaches.SpatialCache, actionList, aiBrain, points);


            mCellDraw = new CellDraw(Radius, 5, (float)MaxLifespan / 100, Math.Min(GetInfectionPower() / 2, 10), Color.Blue, Functions.StrainToColor(Strain), Color.Black,
                (int) mValues.GetLifepoints(), (int) mValues.GetLifespan());
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            if (mInfectionBehavior.GetTarget() != null)
            {
                ((Unit) mInfectionBehavior.GetTarget()).CellDraw.Color = Color.Blue;
                ((Unit) mInfectionBehavior.GetTarget()).CellDraw.Draw(spriteBatch, spriteFont, Position, 1 - GetRemainingCellDivisionDuration() / GetCellDivisionRate(), false);
            } else mCellDraw.Draw(spriteBatch, spriteFont, Position, 0, false);
        }

        public override void LoadContent(Content.ContentLoader contentLoader)
        {
            base.LoadContent(contentLoader);
            if (mInfectionBehavior.GetTarget() != null)
            {
                mInfectionBehavior.GetTarget().LoadContent(contentLoader);
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (mInfectionBehavior.GetTarget() != null)
            {
                ((Unit)mInfectionBehavior.GetTarget()).CellDraw.Update((int)mValues.GetLifepoints(), (int)mValues.GetLifespan(), false, mIsUnderAttack);
            } else mCellDraw.Update((int)mValues.GetLifepoints(), (int)mValues.GetLifespan(), false, mIsUnderAttack);
            base.Update(gameTime);
            mAiControl.Update(gameTime);            
        }

        /// <inheritdoc />
        public override void Die()
        {
            base.Die();
            mInfectionBehavior.CutConnection();
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
            return mVirusDivAction.GetRemainingCellDivisionDuration();
        }

        /// <inheritdoc />
        public bool IsInfecting()
        {
            return mInfectionBehavior.IsInfecting;
        }

        /// <inheritdoc />
        public int GetInfectionPower()
        {
            return mValues.GetInfectionPower();
        }

        /// <inheritdoc />
        public void ChangeInfectionPower(int value)
        {
            mValues.ChangeInfectionPower(value);
        }

        public void HasInfected()
        {
            ((Unit)mInfectionBehavior.GetTarget()).CellDraw.SetMaxLifePoints((int)MaxLifepoints);
            ((Unit)mInfectionBehavior.GetTarget()).CellDraw.SetMaxLifeSpan((int)MaxLifespan);
            ((Unit) mInfectionBehavior.GetTarget()).CellDraw.NucleusColor = Functions.StrainToColor(Strain);
        }

        /// <inheritdoc />
        private void InitProperties(InheritanceTable inheritance)
        {
            Strain = inheritance.Antigen.Equals("") ? StrainGenerator.GetInstance().GenerateStrain() : inheritance.Antigen;
            Mode = UnitMode.Defensive;
            Side = UnitSide.Enemy;

            mValues = new ValueStore(4, 4, ValueStore.NoValue, 4, ValueStore.NoValue, 5, 6, ValueStore.NoValue, 5, GetSide());
            MutationTable.AffectObjectWithMutation(inheritance.Mutations, this, mValues);

            MaxLifepoints = mValues.GetLifepoints();
            MaxLifespan = mValues.GetLifespan();
            Radius = (int)MaxLifepoints / 2;
        }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

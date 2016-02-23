using System;
using Antigen.Content;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.UnitModes;
using Antigen.Objects.Properties;
using Antigen.Objects.Units.Values;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = Antigen.Graphics.IDrawable;
using IUpdateable = Antigen.Logic.IUpdateable;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// A simple flow particle. It moves with the flow of the map and is no attackable unit.
    /// </summary>
    [Serializable]
    sealed class FlowParticle : GameObject, IDrawable, IUpdateable, IMapCollidable, IHasSpeed, ILoad, ICanMove
    {
        [NonSerialized]
        private Texture2D mTexture;

        private readonly IMoveBehavior mMoveBehavior;
        private readonly DriftControl mDriftControl;

        private ValueStore mValues;

        /// <summary>
        /// Creates a new flow particle with a given starting position.
        /// </summary>
        /// <param name="startPos">Starting position of the unit on the map as absolute value</param>
        /// <param name="thatMap">Map that provides flow and collision data</param>
        public FlowParticle(Vector2 startPos, Map.Map thatMap) : base(startPos)
        {
            AvoidanceHitbox = null;

            InitProperties();
            //The following ugly hack triggers a recalculation of the object's
            //hitbox with the radius set by InitProperties().
            Position = Position;
            mMoveBehavior = new MoveBehavior(this, thatMap, null, null);
            mDriftControl = new DriftControl(mMoveBehavior);
            mDriftControl.Activate();
        }

        /// <inheritdoc />
        public void ChangeBaseSpeed(int value)
        {
            mValues.ChangeBaseSpeed(value);
        }

        /// <inheritdoc />
        public bool IsVirtualCollidable
        {
            get { return true; }
        }

        /// <inheritdoc />
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Draw(mTexture, new Vector2(Position.X, Position.Y), null, Color.DarkRed, 0, new Vector2(mTexture.Width / 2f, mTexture.Height / 2f), Radius * 2f / mTexture.Width, SpriteEffects.None, 0);
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            mDriftControl.Update(gameTime);
            mMoveBehavior.Update(gameTime);
        }

        public void LoadContent(ContentLoader contentLoader)
        {
            mTexture = contentLoader.LoadTexture("Circle");
        }

        /// <inheritdoc />
        private void InitProperties()
        {
            mValues = new ValueStore(ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue,
                ValueStore.NoValue, ValueStore.SpecialSpawnValue, ValueStore.NoValue, ValueStore.NoValue, ValueStore.NoValue, Unit.UnitSide.Neutral);

            Radius = 5;
        }

        /// <inheritdoc />
        public Vector2 MovementVector { get; set; }

        /// <inheritdoc />
        public float MaxVelocity
        {
            get { return ValueStore.ConvertBaseSpeedToFloatFormat(mValues.GetBaseSpeed()); }
        }

        /// <inheritdoc />
        public float SightRange
        {
            get { return ValueStore.ConvertSightToPixel(mValues.GetSight()); }
        }

        /// <summary>
        /// This property is not used by Flow Particles because they don't need to
        /// avoid collidable obstacles.
        /// </summary>
        public ICollidable AvoidanceHitbox { get; private set; }

        /// <inheritdoc />
        public bool CollisionInLastTick { get; set; }
    }
}

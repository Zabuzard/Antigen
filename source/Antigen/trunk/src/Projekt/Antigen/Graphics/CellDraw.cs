using System;
using Antigen.Content;
using Antigen.HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.Graphics
{
    /// <summary>
    /// Provides methods to draw cells.
    /// </summary>
    [Serializable]
    sealed class CellDraw : ILoad
    {
        private readonly int mRadius;
        private readonly int mBorderWidth;
        private readonly float mNucleusRadius;
        private readonly int mWeaponSize;
        private Color mColor;
        private Color mNucleusColor;
        private readonly Color mGroundColor;
        private readonly Bar mHealthBar;
        private readonly Bar mTimeBar;
        private float mUnderAttackValue;

        [NonSerialized]
        private Texture2D mCircleTexture, mWeaponTexture, mPlasmaTexture;

        private readonly Organelle mNucleus;
        private float mRotation;
        private float mFightValue;


        private const int BarHeight = 5;
        private const int MaxBarWidth = 100;
        private const int SelectionWidth = 5;
        private const float MaxUnderAttackValue = 0.8f;
        private const float UnderAttackDecrease = 0.1f;

        /// <summary>
        /// Creates a new object to draw a cell
        /// </summary>
        /// <param name="radius">Radius of the cell</param>
        /// <param name="borderWidth">Border width of the cell</param>
        /// <param name="nucleusRadius">The Radius of the cell nucleus</param>
        /// <param name="weaponSize">The size of the weapons</param>
        /// <param name="color">The color of the cell</param>
        /// <param name="nucleusColor"></param>
        /// <param name="groundColor"></param>
        /// <param name="maxLifePoints"></param>
        /// <param name="maxLifeSpan"></param>
        public CellDraw(int radius, int borderWidth, float nucleusRadius, int weaponSize, Color color, Color nucleusColor, Color groundColor, int maxLifePoints, int maxLifeSpan)
        {
            mRadius = radius;
            mBorderWidth = borderWidth;
            mNucleusRadius = nucleusRadius;
            mWeaponSize = weaponSize;
            mColor = color;
            mNucleusColor = nucleusColor;
            mGroundColor = groundColor;
            mNucleus = new Organelle();
            mHealthBar = new Bar(Math.Min(MaxBarWidth, maxLifePoints), BarHeight, Color.Green, maxLifePoints, "");
            mTimeBar = new Bar(Math.Min(MaxBarWidth, maxLifeSpan / 10), BarHeight, Color.Blue, maxLifeSpan, "");
        }

        public Color Color
        {
            set { mColor = value; }
        }

        public Color NucleusColor
        {
            set { mNucleusColor = value; }
        }

        public void SetMaxLifePoints(int lifePoints)
        {
            mHealthBar.SetWidth(Math.Min(MaxBarWidth, lifePoints));
            mHealthBar.MaxValue = lifePoints;
        }

        public void SetMaxLifeSpan(int lifeSpan)
        {
            mTimeBar.SetWidth(Math.Min(MaxBarWidth, lifeSpan / 10));
            mTimeBar.MaxValue = lifeSpan;
        }

        /// <summary>
        /// Draws a cell with given position and sprite batch on the graphic device.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw with</param>
        /// <param name="spriteFont">The sprite font</param>
        /// <param name="position">Position of the cell</param>
        /// <param name="divisionProgress"></param>
        /// <param name="isSelected">If the cell is selected</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont, Vector2 position, double divisionProgress, bool isSelected)
        {
            if (isSelected)
                spriteBatch.Draw(mCircleTexture,
                    new Vector2(position.X, position.Y),
                    null,
                    Color.Yellow,
                    0f,
                    new Vector2(mCircleTexture.Width / 2f, mCircleTexture.Height / 2f),
                    ((mRadius + SelectionWidth) * 2f) / mCircleTexture.Width,
                    SpriteEffects.None,
                    0);
            DrawCell(spriteBatch, position, 1);
            if (divisionProgress > 0) DrawCell(spriteBatch, new Vector2(position.X + (float)divisionProgress * mRadius, position.Y), (float)divisionProgress);
            mHealthBar.Draw(spriteBatch,
                spriteFont,
                new Vector2(position.X - mRadius, position.Y - mRadius - BarHeight - mWeaponSize), false);
            mTimeBar.Draw(spriteBatch,
                spriteFont,
                new Vector2(position.X - mRadius, position.Y - mRadius - BarHeight * 2 - mWeaponSize), false);
        }

        private void DrawCell(SpriteBatch spriteBatch, Vector2 position, float radius)
        {
            spriteBatch.Draw(mWeaponTexture,
                new Vector2(position.X, position.Y),
                null,
                Color.Blue,
                0f,
                new Vector2(mWeaponTexture.Width / 2f, mWeaponTexture.Height / 2f),
                ((mRadius + mWeaponSize * (float) (Math.Sin(mFightValue) + 1)) * 2f * radius) / mWeaponTexture.Width,
                SpriteEffects.None,
                0);
            spriteBatch.Draw(mCircleTexture,
                new Vector2(position.X, position.Y),
                null,
                mGroundColor,
                0f,
                new Vector2(mCircleTexture.Width / 2f, mCircleTexture.Height / 2f),
                (mRadius * 2f * radius) / mCircleTexture.Width,
                SpriteEffects.None,
                0);
            spriteBatch.Draw(mPlasmaTexture,
                new Vector2(position.X, position.Y),
                null,
                mColor,
                mRotation,
                new Vector2(mPlasmaTexture.Width / 2f, mPlasmaTexture.Height / 2f),
                ((mRadius - mBorderWidth) * 2f * radius) / mPlasmaTexture.Width,
                SpriteEffects.None,
                0);
            spriteBatch.Draw(mPlasmaTexture,
                new Vector2(position.X, position.Y),
                null,
                mColor,
                -mRotation,
                new Vector2(mPlasmaTexture.Width / 2f, mPlasmaTexture.Height / 2f),
                ((mRadius - mBorderWidth) * 2f * radius) / mPlasmaTexture.Width,
                SpriteEffects.None,
                0);
            spriteBatch.Draw(mCircleTexture,
                new Vector2(mNucleus.Position.X + position.X, mNucleus.Position.Y + position.Y),
                null,
                Color.Black,
                0f,
                new Vector2(mCircleTexture.Width / 2f, mCircleTexture.Height / 2f),
                (mNucleusRadius * 2f * radius) / mCircleTexture.Width,
                SpriteEffects.None,
                0);
            spriteBatch.Draw(mPlasmaTexture,
                new Vector2(mNucleus.Position.X + position.X, mNucleus.Position.Y + position.Y),
                null,
                mNucleusColor,
                mNucleus.Rotation,
                new Vector2(mPlasmaTexture.Width / 2f, mPlasmaTexture.Height / 2f),
                (mNucleusRadius * 2f * radius) / mPlasmaTexture.Width,
                SpriteEffects.None,
                0);
            if (mUnderAttackValue > 0)
            {
                spriteBatch.Draw(mCircleTexture,
                new Vector2(position.X, position.Y),
                null,
                Color.Red * mUnderAttackValue,
                0f,
                new Vector2(mCircleTexture.Width / 2f, mCircleTexture.Height / 2f),
                (mRadius * 2f * radius) / mCircleTexture.Width,
                SpriteEffects.None,
                0);
            }
        }

        /// <summary>
        /// Updates the drawing of the cell
        /// </summary>
        /// <param name="lifePoints">The life points of the cell</param>
        /// <param name="lifeSpan"></param>
        /// <param name="isFighting"></param>
        /// <param name="isUnderAttack"></param>
        public void Update(int lifePoints, int lifeSpan, bool isFighting, bool isUnderAttack)
        {
            mHealthBar.Value = lifePoints;
            mTimeBar.Value = lifeSpan;
            mRotation += 0.01f;
            if (mUnderAttackValue > 0) mUnderAttackValue -= UnderAttackDecrease;
            else if (isUnderAttack) mUnderAttackValue = MaxUnderAttackValue;
            if (isFighting) mFightValue = (float) ((mFightValue + 0.1f) % (2 * Math.PI));
            else mFightValue = 0;
            if (mRotation > 2 * Math.PI) mRotation = 0;
            mNucleus.Update(mRadius - mBorderWidth - mNucleusRadius);
        }

        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            mCircleTexture = contentLoader.LoadTexture("Circle");
            mWeaponTexture = contentLoader.LoadTexture("Weapon");
            mPlasmaTexture = contentLoader.LoadTexture("Plasma");
            mHealthBar.LoadContent(contentLoader);
            mTimeBar.LoadContent(contentLoader);
        }
    }
}

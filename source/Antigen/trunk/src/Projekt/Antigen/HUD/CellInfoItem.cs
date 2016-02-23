using System;
using System.Collections.Generic;
using Antigen.Content;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Antigen.HUD
{
    /// <summary>
    /// A cell info item with bars for the different properties
    /// </summary>
    [Serializable]
    sealed class CellInfoItem: ILoad
    {
        private readonly Unit mUnit;
        private readonly List<Bar> mBars;
        private readonly int mBarGap;
        private readonly int mBarWidth;
        private readonly int mBarHeight;
        private readonly int mFontWidth;
        private readonly int mFontHeight;
        private readonly float mAntigenBorder;
        [NonSerialized]
        private Texture2D mTexture;
        [NonSerialized]
        private Texture2D mCircleTexture;


        private const int MaxLifepoints = (int) ValueStore.MaxLifepoints;
        private const int MaxLifespan = (int)ValueStore.MaxLifespan;
        private const int MaxSpeed = ValueStore.MaxBaseSpeed;
        private const int MaxStrength = (int)ValueStore.MaxAttackPower;
        private const int MaxDivisionSpeed = (int)ValueStore.MaxCellDivisionRate;
        private const int MaxResistance = ValueStore.MaxInfectionResistance;

        /// <summary>
        /// Creates a new cell info item
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="barGap"></param>
        /// <param name="barWidth"></param>
        /// <param name="barHeight"></param>
        /// <param name="fontWidth"></param>
        /// <param name="fontHeight"></param>
        /// <param name="antigenBorder"></param>
        public CellInfoItem(Unit unit, int barGap, int barWidth, int barHeight, int fontWidth, int fontHeight, float antigenBorder)
        {
            mUnit = unit;
            mBarGap = barGap;
            mBarWidth = barWidth;
            mBarHeight = barHeight;
            mFontWidth = fontWidth;
            mFontHeight = fontHeight;
            mAntigenBorder = antigenBorder;

            mBars = new List<Bar>
            {
                new Bar(mBarWidth, mBarHeight, Color.Green, MaxLifepoints, "")
                {
                    Value = (int) unit.GetMaxLifepoints()
                },
                new Bar(mBarWidth, mBarHeight, Color.Blue, MaxLifespan, "")
                {
                    Value = (int) unit.GetMaxLifespan()
                },
                new Bar(mBarWidth, mBarHeight, Color.Red, MaxStrength, "")
                {
                    Value = (int) ((unit is ICanAttack) ? ((ICanAttack) unit).GetAttackPower()
                        : (unit is ICanDeInfect) ? ((ICanDeInfect)unit).GetDeInfectionPower() : 0)
                },
                new Bar(mBarWidth, mBarHeight, Color.Red, MaxDivisionSpeed, "")
                {
                    Value = (int) ((unit is ICanCellDivision) ? ((ICanCellDivision) unit).GetCellDivisionRate() : 0)
                },
                new Bar(mBarWidth, mBarHeight, Color.Red, MaxSpeed, "")
                {
                    Value = unit.GetBaseSpeed()
                },
                new Bar(mBarWidth, mBarHeight, Color.Red, MaxResistance, "")
                {
                    Value = (unit is IInfectable) ? ((IInfectable) unit).GetInfectionResistance() : 0
                },            
            };
            
            if (unit is ICanDebuff || unit is Bcell) 
            {
                var debuffTable = (unit is ICanDebuff) ? ((ICanDebuff) unit).GetDebuffTable() : ((Bcell) unit).GetDebuffTable();
                mBars.AddRange(new List<Bar>
                {
                    new Bar(mBarWidth, mBarHeight, Color.Green, DebuffTable.MaxSingleValue, "")
                    {
                        Value = debuffTable.GetValue(DebuffTable.DebuffValue.Lifepoints)
                    },
                    new Bar(mBarWidth, mBarHeight, Color.Blue, DebuffTable.MaxSingleValue, "")
                    {
                        Value = debuffTable.GetValue(DebuffTable.DebuffValue.Lifespan)
                    },
                    new Bar(mBarWidth, mBarHeight, Color.Red, DebuffTable.MaxSingleValue, "")
                    {
                        Value = debuffTable.GetValue(DebuffTable.DebuffValue.AttackPower)
                    },
                    new Bar(mBarWidth, mBarHeight, Color.Red, DebuffTable.MaxSingleValue, "")
                    {
                        Value = debuffTable.GetValue(DebuffTable.DebuffValue.CellDivisionRate)
                    },
                    new Bar(mBarWidth, mBarHeight, Color.Red, DebuffTable.MaxSingleValue, "")
                    {
                        Value = debuffTable.GetValue(DebuffTable.DebuffValue.MovementSpeed)
                    }
                });
            }
        }

        public double GetBarValue(int barIndex)
        {
            return mBars[barIndex].Value;
        }

        public bool Selected { get; set; }

        /// <summary>
        /// The Unit
        /// </summary>
        public Unit TheUnit {get { return mUnit; } }

        /// <inheritdoc />
        public void LoadContent(ContentLoader contentLoader)
        {
            foreach (var bar in mBars)
            {
                bar.LoadContent(contentLoader);
            }
            mTexture = contentLoader.LoadTexture("Point");
            mCircleTexture = contentLoader.LoadTexture("Circle");
        }

        /// <summary>
        /// Draws the item
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        /// <param name="position"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont, Point position)
        {
            if (Selected)
            {
                spriteBatch.Draw(mTexture, new Rectangle(position.X , position.Y, (mBarWidth + mBarGap) * 6 + mFontWidth + mFontHeight, mFontHeight), Color.Yellow);
            }

            var fontSize = spriteFont.MeasureString(mUnit.Name);
            spriteBatch.DrawString(spriteFont, mUnit.Name, new Vector2(position.X, position.Y), Color.White, 0, Vector2.Zero, mFontHeight / fontSize.Y, SpriteEffects.None, 0);

            var color = (mUnit is IHasAntigen)
                ? Functions.StrainToColor(((IHasAntigen)mUnit).Antigen)
                : Color.Black;
            spriteBatch.Draw(mCircleTexture, new Vector2(position.X + mFontWidth + mAntigenBorder, position.Y + mAntigenBorder), null, color, 0, Vector2.Zero, (mFontHeight - 2 * mAntigenBorder) / mCircleTexture.Width, SpriteEffects.None, 0);
            for (var i = 0; i < 6; i++)
            {
                mBars[i].Draw(spriteBatch,
                    spriteFont,
                    new Vector2(position.X + i * (mBarWidth + mBarGap) + mFontWidth + mFontHeight, position.Y),
                    false);
            }
            for (var i = 6; i < mBars.Count; i++)
            {
                mBars[i].Draw(spriteBatch,
                    spriteFont,
                    new Vector2(position.X + (i - 6) * (mBarWidth + mBarGap) + mFontWidth + mFontHeight, position.Y + mBarHeight),
                    false);
            }
        }
    }
}

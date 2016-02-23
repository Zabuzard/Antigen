using System;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Sound;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Implements a basic deinfection behavior for units.
    /// Can deinfect objects that are infected by others.
    /// </summary>
    [Serializable]
    sealed class DeInfectionBehavior : IDeInfectionBehavior
    {
        /// <summary>
        /// Object to deinfect with.
        /// </summary>
        private readonly ICanDeInfect mObj;
        [NonSerialized]
        private readonly SoundWrapper mSoundWrapper;

        /// <inheritdoc />
        public bool IsDeinfecting { get; private set; }

        /// <summary>
        /// Creates a new deinfection behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to deinfect with</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        public DeInfectionBehavior(ICanDeInfect obj, SoundWrapper soundWrapper)
        {
            mSoundWrapper = soundWrapper;
            mObj = obj;
            IsDeinfecting = false;
        }

        /// <inheritdoc />
        public void Deinfect(ICanInfect target, GameTime time)
        {
            if (!Functions.IsOffensiveConstellation((Unit)mObj, (Unit)target, true))
            {
                return;
            }
            IsDeinfecting = true;

            var lively = target as ILively;
            if (lively != null)
            {
                var lifepointLoss = (float)(mObj.GetDeInfectionPower() * time.ElapsedGameTime.TotalSeconds);
                //Attack boost if Antigen == Strain
                if (mObj is IHasAntigen && target is IHasStrain
                    && ((IHasAntigen)mObj).Antigen.Equals(((IHasStrain)target).Strain))
                {
                    lifepointLoss *= 2;
                }

                lively.ChangeLifepoints(-lifepointLoss);

                //Play sound when unit deinfects
                var unit = mObj as Unit;
                if (unit.GetSide() == Unit.UnitSide.Friendly && mSoundWrapper != null)
                {
                    mSoundWrapper.PlayEffect(Effect.DeInfection, unit.Position);
                }
            }

            IsDeinfecting = false;
        }
    }
}

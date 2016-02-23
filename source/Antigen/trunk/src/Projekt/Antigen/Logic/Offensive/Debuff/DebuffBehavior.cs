using System;
using Antigen.Logic.CellDivision;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Sound;
using Antigen.Util;

namespace Antigen.Logic.Offensive.Debuff
{
    /// <summary>
    /// Implements a basic debuff behavior for units.
    /// Can debuff untis with another.
    /// </summary>
    [Serializable]
    sealed class DebuffBehavior : IDebuffBehavior
    {
        /// <summary>
        /// Object to debuff with.
        /// </summary>
        private readonly ICanDebuff mObj;
        [NonSerialized]
        private readonly SoundWrapper mSoundWrapper;

        /// <inheritdoc />
        public bool IsDebuffing { get; private set; }

        /// <summary>
        /// Creates a new basic debuff behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to debuff with</param>
        /// <param name="soundWrapper">Wrapper to play sound with</param>
        public DebuffBehavior(ICanDebuff obj, SoundWrapper soundWrapper)
        {
            mSoundWrapper = soundWrapper;
            mObj = obj;
            IsDebuffing = false;
        }

        /// <inheritdoc />
        public void Debuff(IDebuffable target)
        {
            if (mObj is Unit && target is Unit)
            {
                if (Functions.IsFriendlySideConstellation((Unit)mObj, (Unit)target))
                {
                    return;
                }
            }
            //Resharper hint: Engine should support such property combinations for future units.
// ReSharper disable SuspiciousTypeConversion.Global
            if (mObj is ICanCellDivision && ((ICanCellDivision)mObj).IsCellDividing())
// ReSharper restore SuspiciousTypeConversion.Global
            {
                return;
            }
            //Resharper hint: Engine should support such property combinations for future units.
// ReSharper disable SuspiciousTypeConversion.Global
            if (mObj is ICanInfect && ((ICanInfect)mObj).IsInfecting())
// ReSharper restore SuspiciousTypeConversion.Global
            {
                return;
            }
            if (mObj is IInfectable && ((IInfectable)mObj).IsInfected())
            {
                return;
            }
            if (mObj is IHasAntigen && target is IHasStrain)
            {
                if (!((IHasAntigen) mObj).Antigen.Equals(((IHasStrain)target).Strain))
                {
                    return;
                }
            }
            //Resharper hint: Engine should support such property combinations for future units.
// ReSharper disable SuspiciousTypeConversion.Global
            if (mObj is IHasStrain && target is IHasAntigen)
            {
                if (!((IHasStrain)mObj).Strain.Equals(((IHasAntigen)target).Antigen))
// ReSharper restore SuspiciousTypeConversion.Global
                {
                    return;
                }
            }

            IsDebuffing = true;

            //Play sound when unit debuffs
            var unit = mObj as Unit;
            if (unit != null && unit.GetSide() == Unit.UnitSide.Friendly && mSoundWrapper != null)
            {
                mSoundWrapper.PlayEffect(Effect.Debuff, unit.Position);
            }

            DebuffTable.AffectTargetWithDebuff(mObj.GetDebuffTable(), target);

            IsDebuffing = false;

            //Destroy debuffing unit after debuff
            var obj = mObj as ILively;
            if (obj != null)
            {
                obj.ChangeLifespan(-obj.GetLifespan());
            }
        }
    }
}

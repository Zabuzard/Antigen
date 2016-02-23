using System;
using Antigen.Input;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Objects.Units;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.UnitModes
{
    /// <summary>
    /// Manages a unit's modes. Transitions between modes
    /// when necessary and forwards input events to the
    /// currently active mode.
    /// </summary>
    [Serializable]
    sealed class UnitModeManager : IUpdateable, IRightClickListener
    {
        /// <summary>
        /// Cell division mode control.
        /// </summary>
        private readonly IModeControl mCellDivision;
        /// <summary>
        /// Defensive mode control.
        /// </summary>
        private readonly IModeControl mDefensive;
        /// <summary>
        /// Drift mode control.
        /// </summary>
        private readonly IModeControl mDrift;
        /// <summary>
        /// Offensive mode control.
        /// </summary>
        private readonly IModeControl mOffensive;

        /// <summary>
        /// Reference to parent unit if it is
        /// ISelectable. <code>null</code> otherwise.
        /// </summary>
        private readonly ISelectable mSelectableParent;

        /// <summary>
        /// The parent unit's current mode.
        /// </summary>
        private Unit.UnitMode mCurrentMode;

        /// <summary>
        /// The parent unit's current mode. May be set
        /// to change the unit mode to the specified
        /// one. May only be set to values for which
        /// the corresponding behaviour is non-<code>null</code>.
        /// </summary>
        public Unit.UnitMode CurrentMode
        {
            get { return mCurrentMode; }
            set
            {
                CurrentModeControl().Terminate();
                mCurrentMode = value;
                CurrentModeControl().Activate();
            }
        }

        /// <summary>
        /// Creates a new mode manager for the given object,
        /// managing the given controls.
        /// </summary>
        /// <param name="parent">Parent object controlled
        /// by this manager.</param>
        /// <param name="cellDivision">Cell division mode control
        /// that is activated when activating cell division
        /// mode. May be <code>null</code> if the object does
        /// not support cell division.</param>
        /// <param name="defensive">Defensive mode control
        /// that is activated when activating defensive
        /// mode. May be <code>null</code> if the object does
        /// not support the defensive mode.</param>
        /// <param name="drift">Drift mode control
        /// that is activated when activating drift
        /// mode. May be <code>null</code> if the object does
        /// not support drifting.</param>
        /// <param name="offensive">Offensive mode control
        /// that is activated when activating offensive
        /// mode. May be <code>null</code> if the object does
        /// not support the offensive mode.</param>
        /// <param name="startMode">The mode that the unit starts
        /// in. The corresponding control object may not be
        /// <code>null</code>.</param>
        /// <param name="input">The game's input dispatcher.</param>
        public UnitModeManager(GameObject parent,
            IModeControl cellDivision,
            IModeControl defensive,
            IModeControl drift,
            IModeControl offensive,
            Unit.UnitMode startMode,
            InputDispatcher input)
        {
            mCellDivision = cellDivision;
            mDefensive = defensive;
            mDrift = drift;
            mOffensive = offensive;
            mSelectableParent = parent as ISelectable;

            mCurrentMode = startMode;
            CurrentModeControl().Activate();
            input.RegisterListener(this);
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            CurrentModeControl().Update(gameTime);
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            // Block the event if the parent is selectable but not
            // selected.
            if (mSelectableParent != null && ! mSelectableParent.Selected)
                return false;

            // Transition from drift/cell division to offensive or defensive mode,
            // depending on what the parent unit supports.
            if (CurrentMode == Unit.UnitMode.Drift ||
                CurrentMode == Unit.UnitMode.CellDivision)
            {
                if (mOffensive != null)
                    CurrentMode = Unit.UnitMode.Offensive;
                else if (mDefensive != null)
                    CurrentMode = Unit.UnitMode.Defensive;
            }

            return CurrentModeControl().HandleRightClick(info);
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return CurrentModeControl().EventOrder; }
        }

        /// <inheritdoc />
        private IModeControl CurrentModeControl()
        {
            switch (CurrentMode)
            {
                case Unit.UnitMode.CellDivision:
                    return mCellDivision;
                case Unit.UnitMode.Defensive:
                    return mDefensive;
                case Unit.UnitMode.Drift:
                    return mDrift;
                case Unit.UnitMode.Offensive:
                    return mOffensive;
                default:
                    throw new Exception("Unknown unit mode: " + mCurrentMode);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Input;

namespace Antigen.Logic.Selection
{
    /// <summary>
    /// Provides the user with the ability to select all units of a
    /// certain type by double-clicking on one such unit.
    /// </summary>
    [Serializable]
    sealed class DoubleClickSelector : IDoubleClickListener
    {
        /// <summary>
        /// Selection manager that selected objects are registered
        /// with.
        /// </summary>
        private readonly SelectionManager mSelectionManager;
        /// <summary>
        /// List of all units. May not be updated while
        /// <see cref="HandleDoubleLeftClick"/> is being executed.
        /// </summary>
        private readonly ICollection<ISelectable> mUnits;

        /// <summary>
        /// Creates a selector for double-click selection.
        /// </summary>
        /// <param name="selectableUnits">List of selectable units. May not be updated while
        /// <see cref="HandleDoubleLeftClick"/> is being executed.</param>
        /// <param name="manager">The selection manager that objects will
        /// be registered with.</param>
        public DoubleClickSelector(ICollection<ISelectable> selectableUnits, SelectionManager manager)
        {
            mUnits = selectableUnits;
            mSelectionManager = manager;
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.Selector; }
        }

        /// <inheritdoc />
        public bool HandleDoubleLeftClick(ClickInfo info)
        {
            if (!info.ObjectsUnderCursor.Any())
                return false;

            var type = info.ObjectsUnderCursor.First().GetType();
            var selectedUnits = mUnits.Where(u => u.GetType() == type).ToList();
            mSelectionManager.SelectExactly(selectedUnits);
            return true;
        }
    }
}

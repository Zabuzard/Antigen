using System;
using System.Collections.Generic;
using Antigen.Input;
using Microsoft.Xna.Framework.Input;

namespace Antigen.Logic.Selection
{
    /// <summary>
    /// Maintains a list of selected objects while the game is running.
    /// All methods of this class update both the set of selected game
    /// objects maintained by this class and the internal status of
    /// game objects affected by selection changes.
    /// </summary>
    [Serializable]
    sealed class SelectionManager : IKeyListener
    {
        private readonly ISelectable[][] mControlGroups;

        /// <summary>
        /// The set of objects currently selected.
        /// </summary>
        public ISet<ISelectable> SelectedObjects { get; private set; }
        /// <summary>
        /// Raised whenever the set of selected objects changes.
        /// </summary>
        public event EventHandler<SelectionChangeEventArgs> SelectionChanged;

        /// <summary>
        /// Creates a selection manager with an initially empty set
        /// of selected objects.
        /// </summary>
        public SelectionManager()
        {
            SelectedObjects = new HashSet<ISelectable>();
            mControlGroups = new ISelectable[9][];
        }

        /// <summary>
        /// Deselects all currently selected objects, clearing
        /// the set of selected game objects.
        /// 
        /// Raises a selection removal event.
        /// </summary>
        public void DeselectAll()
        {
            foreach (var obj in SelectedObjects)
                obj.Selected = false;
            SelectedObjects.Clear();

            RaiseChangeEvent();
        }

        /// <summary>
        /// Selects the given collection of objects and deselects all
        /// other objects that are currently selected.
        /// 
        /// Raises a removal event for all objects that are currently
        /// selected, followed by an addition event for the objects
        /// from <code>objs</code>.
        /// Note that the removal event may be inaccurate in that a
        /// removal event for objects from <code>objs</code> may be raised
        /// (followed by the corresponding addition event) although
        /// the selection status of objects in <code>objs</code> 
        /// effectively does not change.
        /// </summary>
        /// <param name="objs">Set of objects to be selected.</param>
        public void SelectExactly(ICollection<ISelectable> objs)
        {
            DeselectAll();

            SelectedObjects.UnionWith(objs);
            foreach (var obj in objs)
                obj.Selected = true;

            RaiseChangeEvent();
        }

        /// <summary>
        /// Removes the specified object from the list of
        /// selected objects and all control groups it
        /// may be present in. This method is to be used
        /// if an object dies.
        /// </summary>
        /// <param name="obj">Any selectable object.</param>
        public void Remove(ISelectable obj)
        {
            SelectedObjects.Remove(obj);
            for (var i = 0; i < mControlGroups.Length; i++)
            {
                var grp = mControlGroups[i];
                if (grp == null)
                    continue;

                var newgrp = new ISelectable[grp.Length];
                for (var j = 0; j < grp.Length; j++)
                    if (!grp[j].Equals(obj))
                        newgrp[j] = grp[j];
                mControlGroups[i] = newgrp;
            }
        }

        /// <summary>
        /// Raises a selection change event for one object.
        /// </summary>
        private void RaiseChangeEvent()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangeEventArgs());
        }

        /// <summary>
        /// Makes the currently selected objects a control
        /// group and assigns them to the specified index.
        /// The previous control group is overwritten.
        /// </summary>
        /// <param name="index">The 1-based control group
        /// index.</param>
        private void SetControlGroup(int index)
        {
            var grp = new ISelectable[SelectedObjects.Count];
            SelectedObjects.CopyTo(grp, 0);
            mControlGroups[index - 1] = grp;
        }

        /// <summary>
        /// Selects exactly the specified control group,
        /// if it exists.
        /// </summary>
        /// <param name="index">The 1-based control group
        /// index.</param>
        private void SelectControlGroup(int index)
        {
            var grp = mControlGroups[index - 1];
            if (grp != null)
                SelectExactly(grp);
        }

        /// <inheritdoc />
        public bool HandleKeyPress(Keys key)
        {
            return false;
        }

        /// <inheritdoc />
        public bool HandleKeyRelease(Keys key)
        {
            switch (key)
            {
                case Keys.F1:
                    SetControlGroup(1);
                    break;
                case Keys.F2:
                    SetControlGroup(2);
                    break;
                case Keys.F3:
                    SetControlGroup(3);
                    break;
                case Keys.F4:
                    SetControlGroup(4);
                    break;
                case Keys.F5:
                    SetControlGroup(5);
                    break;
                case Keys.F6:
                    SetControlGroup(6);
                    break;
                case Keys.F7:
                    SetControlGroup(7);
                    break;
                case Keys.F8:
                    SetControlGroup(8);
                    break;
                case Keys.F9:
                    SetControlGroup(9);
                    break;
                case Keys.NumPad1:
                case Keys.D1:
                    SelectControlGroup(1);
                    break;
                case Keys.NumPad2:
                case Keys.D2:
                    SelectControlGroup(2);
                    break;
                case Keys.NumPad3:
                case Keys.D3:
                    SelectControlGroup(3);
                    break;
                case Keys.NumPad4:
                case Keys.D4:
                    SelectControlGroup(4);
                    break;
                case Keys.NumPad5:
                case Keys.D5:
                    SelectControlGroup(5);
                    break;
                case Keys.NumPad6:
                case Keys.D6:
                    SelectControlGroup(6);
                    break;
                case Keys.NumPad7:
                case Keys.D7:
                    SelectControlGroup(7);
                    break;
                case Keys.NumPad8:
                case Keys.D8:
                    SelectControlGroup(8);
                    break;
                case Keys.NumPad9:
                case Keys.D9:
                    SelectControlGroup(9);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.Selector; }
        }
    }

    /// <summary>
    /// Event arguments for selection change events.
    /// </summary>
    sealed class SelectionChangeEventArgs : EventArgs
    {
    }
}

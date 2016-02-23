namespace Antigen.Logic.Selection
{
    /// <summary>
    /// Interface for selectable objects.
    /// </summary>
    interface ISelectable
    {
        /// <summary>
        /// Selection status: true if selected, false
        /// otherwise.
        /// 
        /// Note: Only the <see cref="SelectionManager"/> may
        /// write this property; everyone else may only read it.
        /// </summary>
        bool Selected { get; set; }
    }
}

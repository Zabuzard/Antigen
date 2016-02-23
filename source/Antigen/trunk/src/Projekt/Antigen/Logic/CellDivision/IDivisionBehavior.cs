namespace Antigen.Logic.CellDivision
{
    /// <summary>
    /// Interface for move behaviours that work with objects that can do cell division.
    /// </summary>
    interface IDivisionBehavior
    {
        /// <summary>
        /// Divides the given cell and creates a new one.
        /// </summary>
        /// <param name="cell">Cell to divide</param>
        /// <param name="loader">Content loader for units that where created with cell division</param>
        void DivideCell(ICanCellDivision cell, IDivisionContentLoader loader);
    }
}

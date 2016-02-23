using Antigen.Objects.Units;

namespace Antigen.Logic.CellDivision
{
    /// <summary>
    /// Interface for objects that load content of objects that are created by cell division.
    /// </summary>
    interface IDivisionContentLoader
    {
        /// <summary>
        /// Loads the content of the target unit which was created by cell division.
        /// </summary>
        /// <param name="target">Target to load content for</param>
        void LoadDivisionContent(Unit target);
    }
}

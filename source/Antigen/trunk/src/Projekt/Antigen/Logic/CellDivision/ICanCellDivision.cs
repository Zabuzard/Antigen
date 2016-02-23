namespace Antigen.Logic.CellDivision
{
    /// <summary>
    /// Interface for objects than can do cell divisions.
    /// </summary>
    interface ICanCellDivision
    {
        /// <summary>
        /// If the object is currently doing cell division.
        /// </summary>
        /// <returns>True if the object is currently doing cell division</returns>
        bool IsCellDividing();

        /// <summary>
        /// Gets the cell division rate of this object in seconds.
        /// </summary>
        /// <returns>Cell division rate of this object in seconds.</returns>
        double GetCellDivisionRate();

        /// <summary>
        /// Gets the remaining duration until the cell division is finished.
        /// If not cell dividing this is equals GetCellDivisionRate().
        /// Duration is in seconds.
        /// </summary>
        /// <returns>Remaining duration until the division is completed in seconds</returns>
        //Resharper hint: Method belongs to this interface and engine should support future requests of that method type.
// ReSharper disable UnusedMemberInSuper.Global
        double GetRemainingCellDivisionDuration();
// ReSharper restore UnusedMemberInSuper.Global

        /// <summary>
        /// Changes the cell division rate of this objects by the amount of value in seconds.
        /// </summary>
        /// <param name="value">Value to change division rate in seconds</param>
        void ChangeCellDivisionRate(double value);
    }
}

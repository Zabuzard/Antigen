namespace Antigen.Objects.Properties
{
    /// <summary>
    /// Interface for objects that have a visual range.
    /// </summary>
    interface IHasSight
    {
        /// <summary>
        /// Gets the sight of the object from 0 to 100
        /// </summary>
        /// <returns>Sight from 0 to 100</returns>
        int GetSight();

        /// <summary>
        /// Changes the sight of this object by given amount.
        /// </summary>
        /// <param name="amount">Amount to change sight</param>
        void ChangeSight(int amount);
    }
}

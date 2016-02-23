namespace Antigen.Statistics
{
    /// <summary>
    /// Interface for objects that can increment statistics.
    /// </summary>
    interface IStatisticIncrementer
    {

        /// <summary>
        /// Call this to increment the stat counter for the given stat name.
        /// </summary>
        /// <param name="name">The name of the stat counter.</param>
        void IncrementStatistic(StatName name);
    }
}

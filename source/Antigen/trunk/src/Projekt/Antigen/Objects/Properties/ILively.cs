namespace Antigen.Objects.Properties
{
    /// <summary>
    /// Interface for objects that have lifepoints and a lifespan.
    /// Provides methods to increase and decrease lifepoints and lifespan.
    /// </summary>
    interface ILively
    {
        /// <summary>
        /// If the object is alive.
        /// </summary>
        /// <returns>True if the object is alive</returns>
        bool IsAlive();

        /// <summary>
        /// Gets the current lifepoints of the object.
        /// </summary>
        /// <returns>Current lifepoints</returns>
        float GetLifepoints();

        /// <summary>
        /// Changes the lifepoints of the object by the given amount.
        /// Decreases if amount is negative and increases if positive.
        /// </summary>
        /// <param name="amount">Amount to change lifepoints</param>
        void ChangeLifepoints(float amount);

        /// <summary>
        /// Gets the current lifespan of the object in seconds.
        /// </summary>
        /// <returns>Current lifespan in seconds</returns>
        double GetLifespan();

        /// <summary>
        /// Changes the lifespan of the object by the given amount.
        /// Decreases if amount is negative and increases if positive.
        /// </summary>
        /// <param name="amount">Amount to change lifespan in seconds</param>
        void ChangeLifespan(double amount);
    }
}

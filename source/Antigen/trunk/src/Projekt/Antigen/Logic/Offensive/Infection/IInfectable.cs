using Antigen.Content;

namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Interface for objects than are able to become infected.
    /// </summary>
    interface IInfectable
    {
        /// <summary>
        /// If the object is currently infected.
        /// </summary>
        /// <returns>True if the object is currently infected</returns>
        bool IsInfected();

        /// <summary>
        /// Gets the current infection resistance of this object from 0 to 100.
        /// </summary>
        /// <returns>Infection resistance from 0 to 100</returns>
        int GetInfectionResistance();

        /// <summary>
        /// Changes the infection resistance of this object by given amount.
        /// </summary>
        /// <param name="amount">Amount to change infection resistance</param>
        void ChangeInfectionResistance(int amount);

        /// <summary>
        /// Called if object gets infected by an infecter.
        /// </summary>
        void GettingInfected();

        /// <summary>
        /// Call on non-existent dead infected cell for it's textures.
        /// </summary>
        /// <param name="contentLoader">Contentloader of the game.</param>
        void LoadContent(ContentLoader contentLoader);
    }
}

using Microsoft.Xna.Framework;

namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Interface for objects that can deinfect infected objects.
    /// </summary>
    interface IDeInfectionBehavior
    {
        /// <summary>
        /// Deinfects an object that is infected by the given target.
        /// </summary>
        /// <param name="target">Object to deinfect</param>
        /// <param name="time">GameTime object</param>
        void Deinfect(ICanInfect target, GameTime time);

        /// <summary>
        /// Returns if the object is currently deinfecting other objects.
        /// </summary>
        bool IsDeinfecting { get; }
    }
}

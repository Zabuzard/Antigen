
namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Interface for objects that can infect others.
    /// </summary>
    interface ICanInfect : IOffensiveable
    {
        /// <summary>
        /// If the object is currently infecting other objects.
        /// </summary>
        /// <returns>True if the object is currently infecting</returns>
        bool IsInfecting();

        /// <summary>
        /// Gets the infection power of this object from 0 to 100.
        /// </summary>
        /// <returns>Infection power from 0 to 100</returns>
        int GetInfectionPower();

        /// <summary>
        /// Changes the infection power by the amount of value.
        /// </summary>
        /// <param name="value">Value to change infection power</param>
        void ChangeInfectionPower(int value);

        /// <summary>
        /// Is called when the unit has currently infected the target
        /// </summary>
        void HasInfected();
    }
}

namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Interface for objects that can deinfect others.
    /// </summary>
    interface ICanDeInfect : ICanOffensive
    {
        /// <summary>
        /// Gets the deinfection power of this object in DPS.
        /// </summary>
        /// <returns>Deinfection power in DPS</returns>
        float GetDeInfectionPower();

        /// <summary>
        /// Changes the deinfection power by the amount of value in DPS.
        /// </summary>
        /// <param name="value">Value to change deinfection power in DPS</param>
        void ChangeDeInfectionPower(float value);
    }
}

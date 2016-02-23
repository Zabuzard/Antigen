using Antigen.Logic.Offensive.Infection;

namespace Antigen.Logic.Offensive.Attack
{
    /// <summary>
    /// Interface for objects that can attack others.
    /// </summary>
    interface ICanAttack : ICanOffensive
    {
        /// <summary>
        /// Gets the attack power of this object in DPS.
        /// </summary>
        /// <returns>Attack power in DPS</returns>
        float GetAttackPower();

        /// <summary>
        /// Changes the attack power by the amount of value in DPS
        /// </summary>
        /// <param name="value">Value to change attack power in DPS</param>
        void ChangeAttackPower(float value);
    }
}

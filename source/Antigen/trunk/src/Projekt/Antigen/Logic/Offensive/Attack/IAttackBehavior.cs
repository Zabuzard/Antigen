using Microsoft.Xna.Framework;

namespace Antigen.Logic.Offensive.Attack
{
    /// <summary>
    /// Interface for attack behaviours that work with objects that can attack others.
    /// </summary>
    interface IAttackBehavior
    {
        /// <summary>
        /// Attacks a given object.
        /// </summary>
        /// <param name="target">Object to attack</param>
        /// <param name="time">Current game time object</param>
        void Attack(IAttackable target, GameTime time);

        /// <summary>
        /// Returns if the object is currently attacking other objects.
        /// </summary>
        bool IsAttacking { get; }
    }
}

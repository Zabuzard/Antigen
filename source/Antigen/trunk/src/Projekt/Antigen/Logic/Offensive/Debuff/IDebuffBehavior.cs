namespace Antigen.Logic.Offensive.Debuff
{
    /// <summary>
    /// Interface for debuff behaviours that work with objects that can debuff others.
    /// </summary>
    interface IDebuffBehavior
    {
        /// <summary>
        /// Debuffs a given object.
        /// </summary>
        /// <param name="target">Object to debuff</param>
        void Debuff(IDebuffable target);

        /// <summary>
        /// Returns if the object is currently debuffing other objects.
        /// </summary>
        bool IsDebuffing { get; }
    }
}

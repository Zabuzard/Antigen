namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Interface for infect behaviours that work with objects that can infect others.
    /// </summary>
    interface IInfectionBehavior
    {
        /// <summary>
        /// Infects a given object.
        /// </summary>
        /// <param name="target">Object to infect</param>
        void Infect(IInfectable target);
    }
}

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Interface for steering behaviours. A steering behaviour
    /// applies a force to a host's movement vector in order
    /// to achieve some desired movement.
    /// </summary>
    interface ISteeringBehavior
    {
        /// <summary>
        /// Applies the steering force specific to
        /// this behaviour to the <code>host</code>.
        /// </summary>
        /// <param name="host">The behaviour's
        /// parent unit.</param>
        void ApplySteeringForce(ICanMove host);
    }
}

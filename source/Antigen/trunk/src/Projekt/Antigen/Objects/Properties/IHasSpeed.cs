namespace Antigen.Objects.Properties
{
    /// <summary>
    /// Interface for movable objects.
    /// </summary>
    interface IHasSpeed
    {
        /// <summary>
        /// Changes the base movement speed of the object by amount of value.
        /// </summary>
        /// <param name="value">Value to change speed</param>
        void ChangeBaseSpeed(int value);
    }
}

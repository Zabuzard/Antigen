using System;

namespace Antigen.Objects.Units.Values
{
    /// <summary>
    /// Thrown if buff value is invalid. E.g. an antibody with negative buff levels on attack.
    /// </summary>
    [Serializable]
    internal sealed class InvalidValueException : Exception
    {
        /// <summary>
        /// Creates a new InvalidUnitModeException with a custom message.
        /// </summary>
        /// <param name="message">Message for detailed explanation</param>
        internal InvalidValueException(String message)
            : base(message)
        {

        }
    }
}
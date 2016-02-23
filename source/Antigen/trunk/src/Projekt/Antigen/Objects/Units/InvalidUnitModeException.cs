using System;

namespace Antigen.Objects.Units
{
    /// <summary>
    /// Thrown if unit mode is invalid. E.g. an unit that can't attack can not be in an offensive mode.
    /// </summary>
    [Serializable]
    internal sealed class InvalidUnitModeException : Exception
    {
        /// <summary>
        /// Creates a new InvalidUnitModeException with a custom message.
        /// </summary>
        /// <param name="message">Message for detailed explanation</param>
        internal InvalidUnitModeException(String message)
            : base(message)
        {

        }
    }
}
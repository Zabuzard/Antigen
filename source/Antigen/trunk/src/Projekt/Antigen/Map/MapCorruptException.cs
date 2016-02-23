using System;

namespace Antigen.Map
{
    /// <summary>
    /// Thrown if map data is corrupt like wrong size or wrong colors.
    /// </summary>
    [Serializable]
    public sealed class MapCorruptException : Exception
    {
        /// <summary>
        /// Creates a new MapCorruptException with a custom message.
        /// </summary>
        /// <param name="message">Message for detailed explanation</param>
        public MapCorruptException(String message)
            : base(message)
        {

        }
    }
}
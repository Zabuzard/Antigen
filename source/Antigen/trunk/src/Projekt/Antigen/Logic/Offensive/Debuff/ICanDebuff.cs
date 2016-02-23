using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Units.Values;

namespace Antigen.Logic.Offensive.Debuff
{
    /// <summary>
    /// Interface for objects that can debuff others.
    /// </summary>
    interface ICanDebuff : ICanOffensive
    {

        /// <summary>
        /// Gets the debuff table of this object.
        /// </summary>
        /// <returns>Debuff table</returns>
        DebuffTable GetDebuffTable();
    }
}

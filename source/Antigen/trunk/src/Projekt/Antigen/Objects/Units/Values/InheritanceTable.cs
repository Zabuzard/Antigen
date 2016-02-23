using System;

namespace Antigen.Objects.Units.Values
{
    /// <summary>
    /// The InheritanceTable contains information about values that
    /// should be reused for cell division as inheritance.
    /// </summary>
    [Serializable]
    sealed class InheritanceTable
    {
        public String Antigen { get; private set; }
        public MutationTable Mutations { get; private set; }

        /// <summary>
        /// Creates a new InheritanceTable with given values.
        /// </summary>
        /// <param name="thatAntigen">Antigen for this table</param>
        /// <param name="thatMutations">Mutations for this table</param>
        public InheritanceTable(String thatAntigen, MutationTable thatMutations)
        {
            Antigen = thatAntigen;
            Mutations = thatMutations;
        }

        /// <summary>
        /// Creates an empty InheritanceTable where every entry is empty.
        /// </summary>
        /// <returns>Empty MutationTable</returns>
        public static InheritanceTable CreateEmptyTable()
        {
            return new InheritanceTable("", MutationTable.CreateEmptyTable());
        }
    }
}

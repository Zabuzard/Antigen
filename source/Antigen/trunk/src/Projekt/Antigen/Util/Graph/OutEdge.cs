namespace Antigen.Util.Graph
{
    /// <summary>
    /// Outgoing graph edge.
    /// </summary>
    /// <typeparam name="TNode">Type of nodes this edge connects.</typeparam>
    /// <typeparam name="TTag">Type of tag the edge holds.</typeparam>
    sealed class OutEdge<TNode, TTag>
    {
        /// <summary>
        /// The edge's successor.
        /// </summary>
        public TNode Successor { get; private set; }

        /// <summary>
        /// The edge's tag.
        /// </summary>
        public TTag Tag { get; private set; }
        
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        /// <param name="successor">The successor.</param>
        /// <param name="tag">The tag.</param>
        public OutEdge(TNode successor, TTag tag)
        {
            Successor = successor;
            Tag = tag;
        } 
    }
}

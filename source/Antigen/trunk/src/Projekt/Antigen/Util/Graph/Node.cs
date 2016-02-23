using System;

namespace Antigen.Util.Graph
{
    /// <summary>
    /// A graph node which may have outgoing edges
    /// to other nodes.
    /// </summary>
    /// <typeparam name="TValue">The type of values
    /// the node can hold.</typeparam>
    /// <typeparam name="TTag">The type of tag
    /// that edges going out from this node can hold.</typeparam>
    [Serializable]
    sealed class Node<TValue, TTag>
    {
        /// <summary>
        /// This node's value.
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// Edges going out from this node.
        /// </summary>
        public OutEdge<Node<TValue, TTag>, TTag>[] OutEdges { get; set; }

        /// <summary>
        /// Creates a node without any outgoing edges.
        /// </summary>
        /// <param name="value">The node's value.</param>
        public Node(TValue value)
        {
            Value = value;
            OutEdges = new OutEdge<Node<TValue, TTag>, TTag>[0];
        } 

        /// <summary>
        /// Creates a node with outgoing edges.
        /// </summary>
        /// <param name="value">The node's value.</param>
        /// <param name="edges">The outgoing edges. May not be
        /// <code>null</code>.</param>
        public Node(TValue value, OutEdge<Node<TValue, TTag>, TTag>[] edges)
        {
            Value = value;
            OutEdges = edges;
        } 

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}

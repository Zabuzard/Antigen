using System.Collections.Generic;
using System.Linq;

namespace Antigen.Util.Graph
{
    /// <summary>
    /// Functions for creating a graph from nodes.
    /// </summary>
    static class GraphBuilder
    {
        /// <summary>
        /// Connects the given nodes such that they form a graph.
        /// 
        /// This function replaces the outgoing edges of each given node
        /// such that it points to a node in <code>nodes</code>. For this
        /// purpose, nodes are identified with their values, so for every
        /// value of the target node of an outgoing edge, there must be
        /// at least one node with the same value in <code>nodes</code>.
        /// </summary>
        /// <typeparam name="TValue">Type of values that nodes hold.</typeparam>
        /// <typeparam name="TTag">Type of tags that edges hold.</typeparam>
        /// <param name="nodes">The source nodes. Every outgoing edge
        /// must lead to a node that is in turn present in this collection.
        /// Nodes are identified with their values, so if two nodes have
        /// the same value, one of them will be silently discarded.</param>
        /// <returns></returns>
        public static IDictionary<TValue, Node<TValue, TTag>> BuildGraph<TValue, TTag>(this IEnumerable<Node<TValue, TTag>> nodes)
        {
            var nodeSet = nodes.ToDictionary(node => node.Value);
            foreach (var node in nodeSet.Values)
                node.OutEdges = node.OutEdges.Select(edge =>
                    new OutEdge<Node<TValue, TTag>, TTag>(nodeSet[edge.Successor.Value], edge.Tag)).ToArray();

            return nodeSet;
        }
    }
}

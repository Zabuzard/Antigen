using System;
using System.Collections.Generic;
using System.Linq;

namespace Antigen.Util
{
    /// <summary>
    /// Custom priority queue wrapper.
    /// 
    /// This class's enumerator traverses the priority queue from the least to the
    /// greatest element.
    /// </summary>
    /// <typeparam name="T">The type of priority queue elements.</typeparam>
    [Serializable]
    sealed class SortedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// The priority queue.
        /// </summary>
        private readonly SortedDictionary<T, IList<T>> mItems; 

        /// <summary>
        /// Creates a new priority queue with a custom element comparison function.
        /// </summary>
        /// <param name="cmp">A comparer for priority queue elements.</param>
        public SortedList(IComparer<T> cmp)
        {
            mItems = new SortedDictionary<T, IList<T>>(cmp);
        }

        /// <summary>
        /// Enqueues an element. The priority queue allows duplicates, so
        /// an item that is enqueued multiple times will be present in the
        /// queue multiple times.
        /// </summary>
        /// <param name="elem">The element.</param>
        public void Add(T elem)
        {
            IList<T> bag;
            var bagExists = mItems.TryGetValue(elem, out bag);

            if (bagExists)
                bag.Add(elem);
            else
                mItems.Add(elem, new List<T>{elem});
        }

        /// <summary>
        /// Removes all items equal to <code>elem</code> from the queue.
        /// </summary>
        /// <param name="elem">An element.</param>
        /// <returns><code>true</code> if at least one item equal
        /// to <code>elem</code> was found in and removed from the queue; otherwise
        /// <code>false</code>.</returns>
        public void Remove(T elem)
        {
            IList<T> bag;
            var bagExists = mItems.TryGetValue(elem, out bag);

            if (!bagExists)
                return;

            bag.Remove(elem);
            if (!bag.Any())
                mItems.Remove(elem);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return mItems.SelectMany(items => items.Value).GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace ElectionGuard
{
    /// <summary>
    /// An ElectionguardEnumerator is an IReadonlyList implementation
    /// that can be used to enumerate various native collections on ElectionGuard objects
    /// </summary>
    public class ElectionGuardEnumerator<T>
        : DisposableBase, IReadOnlyList<T>
    {
        private readonly Func<int> _sizeResolver;
        private readonly Func<int, T> _itemResolver;

        /// <summary>
        /// Constructs a new ElectionGuardEnumerator
        /// </summary>
        /// <param name="sizeResolver">The function that resolves the collection size</param>
        /// <param name="itemResolver">The function that resolves the item at the specified index</param>
        public ElectionGuardEnumerator(
            Func<int> sizeResolver, Func<int, T> itemResolver)
        {
            _sizeResolver = sizeResolver;
            _itemResolver = itemResolver;
        }

        /// <summary>
        /// Gets the number of items in the collection
        /// </summary>
        public int Count => _sizeResolver();

        /// <summary>
        /// Gets the item at the specified index
        /// </summary>
        /// <param name="index">The index of the item to get</param>
        /// <returns>The item at the specified index</returns>
        public T this[int index] => _itemResolver(index);

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _itemResolver(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

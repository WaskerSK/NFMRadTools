using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    //Imported from WSystem
    public sealed class ReadOnlyIndexedHashSet<T> : IReadOnlyList<T>, IReadOnlySet<T>
    {
        private HashSet<T> hashSet;

        public ReadOnlyIndexedHashSet(HashSet<T> hashSet)
        {
            this.hashSet = hashSet;
        }

        public T this[int index] => hashSet.GetItemAt(index);

        public int Count => hashSet.Count;

        public bool Contains(T item) => hashSet.Contains(item);

        public HashSet<T>.Enumerator GetEnumerator() => hashSet.GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other) => hashSet.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => hashSet.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => hashSet.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => hashSet.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => hashSet.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => hashSet.SetEquals(other);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal HashSet<T> GetUnderlyingHashSet() => hashSet;
    }
}

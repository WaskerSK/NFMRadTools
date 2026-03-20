using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    public sealed class EmptyEnumerable<T> : IEnumerable<T>, IOrderedEnumerable<T>, IAsyncEnumerable<T>, IEnumerator<T>, IAsyncEnumerator<T>, ICollection<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
    {
        private static EmptyEnumerable<T> _empty = new EmptyEnumerable<T>();

        public T this[int index] { get => throw new IndexOutOfRangeException(); set => throw new IndexOutOfRangeException(); }

        public static EmptyEnumerable<T> Empty => _empty;

        public T Current => default;

        public int Count => 0;

        public bool IsReadOnly => true;

        object IEnumerator.Current => Current;

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            return;
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            return;
        }

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return this;
        }

        public void Dispose()
        {
            return;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool MoveNext()
        {
            return false;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(false);
        }

        public bool Remove(T item)
        {
            return false;
        }

        public void RemoveAt(int index)
        {
            throw new IndexOutOfRangeException();
        }

        public void Reset()
        {
            return;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public sealed class EmptyGrouping<TKey, TElement> : IGrouping<TKey, TElement>, IEnumerator<TElement>
    {
        private static EmptyGrouping<TKey, TElement> _empty = new EmptyGrouping<TKey, TElement>();
        public static EmptyGrouping<TKey, TElement> Empty => _empty;
        public TKey Key => default;

        public TElement Current => default;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            return;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
            return;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public sealed class EmptyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private static EmptyDictionary<TKey, TValue> _empty = new EmptyDictionary<TKey, TValue>();
        public static EmptyDictionary<TKey, TValue> Empty => _empty;
        public TValue this[TKey key] { get => default; set { return; } }

        public ICollection<TKey> Keys => EmptyEnumerable<TKey>.Empty;

        public ICollection<TValue> Values => EmptyEnumerable<TValue>.Empty;

        public int Count => 0;

        public bool IsReadOnly => true;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public KeyValuePair<TKey, TValue> Current => default;

        object IEnumerator.Current => Current;

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            return;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            return;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this;
        }

        public bool Remove(TKey key)
        {
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
            return;
        }

        public void Dispose()
        {
            return;
        }
    }
}

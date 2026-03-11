using NFMRadTools.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    //Imported from WSystem
    public static class QoLExtensions
    {
        #region HashSet
        /// <summary>
        /// Allows to retrieve items from a <see cref="HashSet{T}"/> with an index.
        /// </summary>
        /// <returns>The item at the given index.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static T GetItemAt<T>(this HashSet<T> self, int index)
        {
            ArgumentNullException.ThrowIfNull(self);
            if ((uint)index >= self.Count) throw new IndexOutOfRangeException();
            Array arr = Unsafe.As<byte, Array>(ref Unsafe.AddByteOffset(ref CLR.GetObjectDataReference(self), Unsafe.SizeOf<int[]>()));
            ref byte zeroIndexElement = ref MemoryMarshal.GetArrayDataReference(arr);
            return Unsafe.As<byte, T>(ref Unsafe.AddByteOffset(ref zeroIndexElement, (Unsafe.SizeOf<T>() + Unsafe.SizeOf<int>() * 2) * index));
        }
        #endregion

        #region CharacterSpan
        public static int GetLengthOfNumericCharactersFromIndex(this ReadOnlySpan<char> span, int index)
        {
            if (index < 0) return 0;
            if (span.Length <= 0 || index >= span.Length) return 0;
            int i = 0;
            if ((uint)index >= (uint)span.Length) return i;
            if (span[index] == '.') return i;
            if(span[index] == '-')
            {
                i++;
            }
            bool seenDecimal = false;
            for (; index + i < span.Length; i++)
            {
                if (char.IsNumber(span[index + i])) continue;
                if (span[index + i] == '.')
                {
                    if (seenDecimal) return i;
                    seenDecimal = true;
                    continue;
                }
                return i;
            }
            return i;
        }
        #endregion

        #region Enumerable
        public static IEnumerable<T> Interlace<T>(this IEnumerable<T> left, IEnumerable<T> right) => new InterlaceEnumerable<T>(left, right);

        private class InterlaceEnumerable<T> : IEnumerable<T>
        {
            private IEnumerable<T> left;
            private IEnumerable<T> right;

            public InterlaceEnumerable(IEnumerable<T> left, IEnumerable<T> right)
            {
                this.left = left;
                this.right = right;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new InterlaceEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class InterlaceEnumerator : IEnumerator<T>
            {
                private InterlaceEnumerable<T> owner;
                private IEnumerator<T> left;
                private IEnumerator<T> right;
                private IEnumerator<T> current;
                private IEnumerator<T> next;

                public T Current => current.Current;

                object IEnumerator.Current => Current;

                public InterlaceEnumerator(InterlaceEnumerable<T> interlace)
                {
                    owner = interlace;
                    if (owner is not null)
                    {
                        if (owner.left is not null) left = owner.left.GetEnumerator();
                        if (owner.right is not null) right = owner.right.GetEnumerator();
                    }
                    current = right;
                    next = left;
                }

                public bool MoveNext()
                {
                    if (next is null)
                    {
                        if (current is null) return false;
                        return current.MoveNext();
                    }
                    if (current is null)
                    {
                        if (next is null) return false;
                        current = next;
                        next = null;
                        return current.MoveNext();
                    }
                    SwapCurrent();
                    if (current.MoveNext()) return true;
                    SwapCurrent();
                    return current.MoveNext();
                }

                public void Dispose()
                {
                    if (left is not null) left.Dispose();
                    if (right is not null) right.Dispose();
                    owner = null;
                    right = null;
                    left = null;
                    current = null;
                    next = null;
                }

                public void Reset()
                {
                    if (left is not null) left.Reset();
                    if (right is not null) right.Reset();
                    current = right;
                    next = left;
                }

                private void SwapCurrent()
                {
                    var temp = current;
                    current = next;
                    next = temp;
                }
            }
        }
        #endregion


    }
}

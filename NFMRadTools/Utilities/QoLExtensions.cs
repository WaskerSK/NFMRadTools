using NFMRadTools.Utilities;
using System;
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
    }
}

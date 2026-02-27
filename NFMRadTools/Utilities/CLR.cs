using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
//Imported from WSystem, striped of all code that is not needed as it contains private code i dont wanna share.
namespace NFMRadTools.Utilities
{
    /// <summary>
    /// Contains function to interact with the CLR (Common Language Runtime).
    /// </summary>
    public static unsafe class CLR
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref byte GetObjectDataReference(object obj)
        {
            if (obj is null) return ref Unsafe.NullRef<byte>();
            return ref Unsafe.As<RawData>(obj).Data;
        }

    }
}

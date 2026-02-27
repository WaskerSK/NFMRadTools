using System;
using System.Runtime.CompilerServices;

namespace NFMRadTools.Utilities
{
    //Imported from WSystem
    public sealed class RawData
    {
        public byte Data;
        private RawData() { }
        public static unsafe ref byte GetRawData(object obj)
        {
            if(obj is null) throw new ArgumentNullException(nameof(obj));
            return ref Unsafe.As<RawData>(obj).Data;
        }
        public static unsafe RawData AsRawData(object obj)
        {
            return Unsafe.As<RawData>(obj);
        }
    }
}

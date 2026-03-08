using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Gearbox<T> where T : unmanaged, ISpanFormattable
    {
        public T Gear1 { get; set; }
        public T Gear2 { get; set; }
        public T Gear3 { get; set; }

        public override string ToString()
        {
            return $"{Gear1}, {Gear2}, {Gear3}";
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return $"{Gear1.ToString(format, provider)}";
        }
    }
}

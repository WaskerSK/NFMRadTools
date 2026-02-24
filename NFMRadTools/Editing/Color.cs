using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Color : IEquatable<Color>
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color() { }
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return $"{R},{G},{B}";
        }

        public override int GetHashCode()
        {
            return R ^ G ^ B;
        }
        public override bool Equals(object obj)
        {
            if (obj is Color c) return Equals(c);
            return false;
        }
        public bool Equals(Color other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public static bool operator ==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !a.Equals(b);
    }
}

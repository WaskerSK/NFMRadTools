using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class Wheel
    {
        public int GwGr { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public bool CanSteer { get; set; } // true 11; false 0
        public int Height { get; set; }
        public int Width { get; set; }

        public Color RimColor { get; set; }
        public int RimSize { get; set; }
        public int RimDepth { get; set; }

        public Wheel()
        {

        }

        public override string ToString()
        {
            //rims(r,g,b,size,depth)
            StringBuilder sb = new StringBuilder();
            sb.Append("gwgr(").Append(GwGr).AppendLine(")");
            sb.Append("rims(")
                .Append(RimColor.ToString())
                .Append(",").Append(RimSize)
                .Append(",").Append(RimDepth)
                .AppendLine(")");
            sb.AppendLine($"w({X},{Y},{Z},{(CanSteer ? "11" : "0")},{Width},{Height})");
            return sb.ToString();
        }

        public Definition GetDefinition() => new Definition(GwGr, RimColor, RimSize, RimDepth);

        public readonly struct Definition : IEquatable<Definition>
        {
            public int GwGr { get; }
            public Color RimColor { get; }
            public int RimSize { get; }
            public int RimDepth { get; }

            internal Definition(int gwgr, Color rimColor, int rimSize, int rimDepth)
            {
                GwGr = gwgr;
                RimColor = rimColor;
                RimSize = rimSize;
                RimDepth = rimDepth;
            }

            public override bool Equals(object obj)
            {
                if(obj is Definition other) return Equals(other); 
                return false;
            }

            public bool Equals(Definition other)
            {
                return GwGr == other.GwGr && RimColor == other.RimColor && RimSize == other.RimSize && RimDepth == other.RimDepth;
            }
            public static bool operator ==(Definition left, Definition right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Definition left, Definition right)
            {
                return !left.Equals(right);
            }

            public override int GetHashCode()
            {
                int hashcode = GwGr.GetHashCode();
                hashcode ^= RimColor.GetHashCode();
                hashcode ^= RimSize.GetHashCode();
                hashcode ^= RimDepth.GetHashCode();
                return hashcode;
            }
        }
    }
}

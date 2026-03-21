using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Vertex : IEquatable<Vertex>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Vertex(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex other) return Equals(other);
            return false;
        }

        public bool Equals(Vertex other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override string ToString()
        {
            return $"p({X},{Y},{Z})";
        }
        
        public static explicit operator Vector3D(Vertex vert) => new Vector3D(vert.X, vert.Y, vert.Z);
        public static explicit operator Vertex(Vector3D v) => new Vertex() { X = v.X.RoundToInt(), Y = v.Y.RoundToInt(), Z = v.Z.RoundToInt() };
        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return X ^ Y ^ Z;
        }
    }

    public sealed class VertexComparer : IComparer<Vertex>
    {
        public static VertexComparer Default { get; } = new VertexComparer();
        public int Compare(Vertex x, Vertex y)
        {
            int cmp = x.X.CompareTo(y.X);
            if (cmp != 0) return cmp;
            cmp = x.Y.CompareTo(y.Y);
            if (cmp != 0) return cmp;
            return x.Z.CompareTo(y.Z);
        }
    }
}

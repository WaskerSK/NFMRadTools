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
        public static explicit operator Vertex(Vector3D v) => new Vertex() { X = (int)v.X, Y = (int)v.Y, Z = (int)v.Z };
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
}

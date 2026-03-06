using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Vertex
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override string ToString()
        {
            return $"p({X},{Y},{Z})";
        }
        
        public static explicit operator Vector3D(Vertex vert) => new Vector3D(vert.X, vert.Y, vert.Z);
        public static explicit operator Vertex(Vector3D v) => new Vertex() { X = (int)v.X, Y = (int)v.Y, Z = (int)v.Z };
    }
}

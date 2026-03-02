using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    public readonly struct Vector3D
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector3D() { }
        public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }
    }
}

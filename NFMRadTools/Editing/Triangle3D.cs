using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public readonly struct Triangle3D
    {
        public Vector3D A { get; }
        public Vector3D B { get; }
        public Vector3D C { get; }
        public double SurfaceArea { get; }

        public Triangle3D(Vector3D a, Vector3D b, Vector3D c) 
        { 
            A = a; B = b; C = c;
            SurfaceArea = CalculateSurfaceArea();
        }


        private double CalculateSurfaceArea()
        {
            Vector3D ab = B - A;
            Vector3D ac = C - A;
            Vector3D cross = Vector3D.Cross(ab, ac);
            return 0.5 * Vector3D.Length(cross);
        }
    }
}

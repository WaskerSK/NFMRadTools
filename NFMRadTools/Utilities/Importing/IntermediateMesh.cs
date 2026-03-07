using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public class IntermediateMesh
    {
        public string Name { get; set; }
        public List<Vector3D> Vertices { get; } = new List<Vector3D>();
        public List<IntermediateFace> Faces { get; } = new List<IntermediateFace>();
        public IntermediateMeshMode Mode { get; internal set; }
        public IntermediateMeshWheelDefinition WheelDefinition { get; internal set; }
        

        public Cylinder GetBoundingCylinder()
        {
            if (Vertices.Count < 2) return new Cylinder();
            double minX = Vertices[0].X;
            double maxX = minX;
            double minY = Vertices[0].Y;
            double maxY = minY;
            double minZ = Vertices[0].Z;
            double maxZ = minZ;
            for(int i = 1; i < Vertices.Count; i++)
            {
                Vector3D v = Vertices[i];
                minX = double.Min(v.X, minX);
                maxX = double.Max(v.X, maxX);
                minY = double.Min(v.Y, minY);
                maxY = double.Max(v.Y, maxY);
                minZ = double.Min(v.Z, minZ);
                maxZ = double.Max(v.Z, maxZ);
            }
            Vector3D minV = new Vector3D(minX, minY, minZ);
            Vector3D maxV = new Vector3D(maxX, maxY, maxZ);
            Vector3D location = Vector3D.PerAxisMid(minV, maxV);
            Vector3D fminV = minV * Vector3D.VectorYZ;
            Vector3D fmaxV = maxV * Vector3D.VectorYZ;
            Vector3D fLoc = location * Vector3D.VectorYZ;
            double radius = double.Max(Vector3D.Length(Vector3D.Abs(Vector3D.Distance(fLoc, fminV))), Vector3D.Length(Vector3D.Abs(Vector3D.Distance(fLoc, fmaxV))));
            double width = double.Abs(maxV.X - minV.X);
            return new Cylinder(location, /*Vector3D.Zero,*/ radius, width);
        }
    }
}

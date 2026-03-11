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
        public IntermediateMeshMode Mode { get; set; }
        public IntermediateMeshWheelDefinition WheelDefinition { get; set; }
        public int? G6WheelIndex { get; set; }
        private IntermediateMeshInfo _info;
        private IntermediateMeshInfo MeshInfo
        {
            get
            {
                if(_info is null)
                {
                    _info = new IntermediateMeshInfo(this);
                }
                else
                    MeshInfo.Update();
                return _info;
            }
        }

        public Cylinder GetBoundingCylinder()
        {
            return MeshInfo.Cylinder;
        }

        public bool IsMeshIdenticalTo(IntermediateMesh other, double errorTolerance)
        {
            if(other is null) return false;
            if(other.Vertices.Count != this.Vertices.Count) return false;
            if(other.Faces.Count != this.Faces.Count) return false;
            List<Vector3D> A_orderedVerts = MeshInfo.OrderedLocationOffsetVertices;
            List<Vector3D> B_orderedVerts = other.MeshInfo.OrderedLocationOffsetVertices;
            for(int i = 0; i < this.Vertices.Count; i++)
            {
                double distance = Vector3D.Length(Vector3D.Distance(A_orderedVerts[i], B_orderedVerts[i]));
                if(distance > errorTolerance) return false;
            }
            return true;
        }

        private sealed class IntermediateMeshInfo
        {
            public IntermediateMesh Mesh { get; }
            public List<Vector3D> OrderedLocationOffsetVertices { get; }
            public Cylinder Cylinder { get; private set; }
            public Vector3D MeshCenterLocation { get; private set; }
            public IntermediateMeshInfo(IntermediateMesh mesh)
            {
                Mesh = mesh;
                OrderedLocationOffsetVertices = new List<Vector3D>();
                Update();
            }

            public void Update()
            {
                if(Mesh is null)
                {
                    OrderedLocationOffsetVertices.Clear();
                    Cylinder = new Cylinder();
                    return;
                }
                if (OrderedLocationOffsetVertices.Count == Mesh.Vertices.Count) return;
                CalculateBoundingCylinderAndLocation();
                OrderedLocationOffsetVertices.Clear();
                OrderedLocationOffsetVertices.EnsureCapacity(Mesh.Vertices.Count);
                for(int i = 0; i < Mesh.Vertices.Count; i++)
                {
                    OrderedLocationOffsetVertices.Add((Mesh.Vertices[i] - MeshCenterLocation) * (Mesh.Vertices[i].X < 0 ? new Vector3D(-1.0, 1.0, 1.0) : new Vector3D(1.0)));
                }
                OrderedLocationOffsetVertices.Sort(CompareVertexLocations);
            }

            private void CalculateBoundingCylinderAndLocation()
            {
                if (Mesh.Vertices.Count < 2)
                {
                    Cylinder = new Cylinder();
                    MeshCenterLocation = Vector3D.Mid(Mesh.Vertices[0], Mesh.Vertices[1]);
                    return;
                }
                double minX = Mesh.Vertices[0].X;
                double maxX = minX;
                double minY = Mesh.Vertices[0].Y;
                double maxY = minY;
                double minZ = Mesh.Vertices[0].Z;
                double maxZ = minZ;
                for (int i = 1; i < Mesh.Vertices.Count; i++)
                {
                    Vector3D v = Mesh.Vertices[i];
                    minX = double.Min(v.X, minX);
                    maxX = double.Max(v.X, maxX);
                    minY = double.Min(v.Y, minY);
                    maxY = double.Max(v.Y, maxY);
                    minZ = double.Min(v.Z, minZ);
                    maxZ = double.Max(v.Z, maxZ);
                }
                Vector3D minV = new Vector3D(minX, minY, minZ);
                Vector3D maxV = new Vector3D(maxX, maxY, maxZ);
                Vector3D location = Vector3D.Mid(minV, maxV);
                MeshCenterLocation = location;
                Vector3D fminV = minV * Vector3D.VectorYZ;
                Vector3D fmaxV = maxV * Vector3D.VectorYZ;
                Vector3D fLoc = location * Vector3D.VectorYZ;
                double radius = double.Max(Vector3D.Max(Vector3D.Abs(Vector3D.Distance(fLoc, fminV))), Vector3D.Max(Vector3D.Abs(Vector3D.Distance(fLoc, fmaxV))));
                double width = double.Abs(maxV.X - minV.X);
                Cylinder = new Cylinder(location, /*Vector3D.Zero,*/ radius, width);
            }

            private static int CompareVertexLocations(Vector3D x, Vector3D y)
            {
                int cmp = x.X.CompareTo(y.X);
                if(cmp != 0) return cmp;
                cmp = x.Y.CompareTo(y.Y);
                if(cmp != 0) return cmp;
                return x.Z.CompareTo(y.Z);
            }
        }
    }
}

using NFMRadTools.Utilities.Importing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public readonly struct Cylinder
    {
        public Vector3D Location { get; }
        //public Vector3D Origin { get; }
        public double Radius { get; }
        public double Width { get; }
        
        public double NFMVanillaCorrectedWheelRadius { get; }

        public double NFMCorrectedWidth { get; }

        public Cylinder(Vector3D location, /*Vector3D origin,*/ double radius, double width)
        {
            Location = location;
            //Origin = origin;
            Radius = radius;
            Width = width;
            NFMCorrectedWidth = (Width * 1.25) * (Location.X >= 0 ? -1 : 1);
            NFMVanillaCorrectedWheelRadius = Radius * (34.0 / 42.0);
        }

        public Wheel ConvertToNFMWheel(IntermediateMeshMode mode)
        {
            if (mode == IntermediateMeshMode.Normal) throw new InvalidOperationException();
            Wheel w = new Wheel();
            w.X = (int)Location.X;
            w.Y = (int)Location.Y;
            w.Z = (int)Location.Z;
            w.CanSteer = Location.Z >= 0;
            w.GwGr = 0;
            //w.Height = (int)NFMDsCorrectedWheelRadius;
            w.Width = (int)NFMCorrectedWidth;
            w.RimDepth = 0;
            //w.RimSize = (int)(NFMDsCorrectedWheelRadius * 0.7);
            switch(mode)
            {
                case IntermediateMeshMode.Normal: break;
                case IntermediateMeshMode.DragShotWheel:
                    w.Width = (int)Width * (Location.X < 0 ? -1 : 1);
                    goto case IntermediateMeshMode.VanillaWheel;
                case IntermediateMeshMode.G6Wheel:
                    //w.Width = (int)Width;
                    w.Width = 20;
                    w.Height = 20;
                    w.RimSize = 16;
                    //goto case IntermediateMeshMode.VanillaWheel;
                    break;
                case IntermediateMeshMode.PhyrexianWheel:
                case IntermediateMeshMode.VanillaWheel:
                    w.Height = (int)NFMVanillaCorrectedWheelRadius;
                    w.RimSize = (int)(NFMVanillaCorrectedWheelRadius * 0.7);
                    break;
            }
            return w;
        }

        public static Cylinder GetFromPolyGroups(IEnumerable<PolyGroup> polyGroups)
        {
            if (polyGroups is null) return new Cylinder();
            if(!polyGroups.Any()) return new Cylinder();
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            int minZ = int.MaxValue;
            int maxZ = int.MinValue;

            foreach(PolyGroup g in polyGroups)
            {
                foreach(Polygon p in g.Polygons)
                {
                    foreach(Vertex v in p.Vertices)
                    {
                        minX = int.Min(v.X, minX);
                        maxX = int.Max(v.X, maxX);
                        minY = int.Min(v.Y, minY);
                        maxY = int.Max(v.Y, maxY);
                        minZ = int.Min(v.Z, minZ);
                        maxZ = int.Max(v.Z, maxZ);
                    }
                }
            }

            Vector3D minV = new Vector3D(minX, minY, minZ);
            Vector3D maxV = new Vector3D(maxX, maxY, maxZ);
            Vector3D location = Vector3D.Mid(minV, maxV);
            Vector3D fminV = minV * Vector3D.VectorYZ;
            Vector3D fmaxV = maxV * Vector3D.VectorYZ;
            Vector3D fLoc = location * Vector3D.VectorYZ;
            double radius = double.Max(Vector3D.Max(Vector3D.Distance(fLoc, fminV)), Vector3D.Max(Vector3D.Distance(fLoc, fmaxV)));
            double width = double.Abs(maxV.X - minV.X);
            return new Cylinder(location, radius, width);
        }
    }
}

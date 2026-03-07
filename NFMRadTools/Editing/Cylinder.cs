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
        //Cylinders are only used for wheels currently hence no rotation is required.

        public Cylinder(Vector3D location, /*Vector3D origin,*/ double radius, double width)
        {
            Location = location;
            //Origin = origin;
            Radius = radius;
            Width = width;
        }

        public Wheel ConvertToNFMWheel()
        {
            Wheel w = new Wheel();
            w.X = (int)Location.X;
            w.Y = (int)Location.Y;
            w.Z = (int)Location.Z;
            w.CanSteer = Location.Z < 0;
            w.GwGr = 0;
            w.Height = NFMCorrectedRadius;
            w.Width = NFMCorrectedWidth;
            w.RimDepth = 0;
            w.RimSize = (int)(Radius / 2 * (40.0 / 36.0) * 0.7);
            return w;
        }

        public int NFMCorrectedRadius => (int)(Radius / 2 * (40.0 / 36.0));
        public int NFMCorrectedWidth => (int)(Width * 1.25) * ((int)Location.X >= 0 ? -1 : 1);
    }
}

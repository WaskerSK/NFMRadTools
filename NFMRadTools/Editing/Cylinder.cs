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
        
        public double NFMCorrectedRadius { get; }
        public double NFMCorrectedWheelRadius { get; }
        public double NFMCorrectedWidth { get; }

        public Cylinder(Vector3D location, /*Vector3D origin,*/ double radius, double width)
        {
            Location = location;
            //Origin = origin;
            Radius = radius;
            Width = width;
            NFMCorrectedWheelRadius = Radius * (50.0 / 60.0);
            double adjustScale = 1.0;
            if(NFMCorrectedWheelRadius > 46.0)
            {
                adjustScale = 46.0 / NFMCorrectedWheelRadius;
                NFMCorrectedWheelRadius = NFMCorrectedWheelRadius * adjustScale;
            }
            NFMCorrectedRadius = Radius * adjustScale;
            NFMCorrectedWidth = (Width * 1.25) * (Location.X >= 0 ? -1 : 1);
        }

        public Wheel ConvertToNFMWheel()
        {
            Wheel w = new Wheel();
            w.X = (int)Location.X;
            w.Y = (int)Location.Y;
            w.Z = (int)Location.Z;
            w.CanSteer = Location.Z >= 0;
            w.GwGr = 0;
            w.Height = (int)NFMCorrectedWheelRadius;
            w.Width = (int)NFMCorrectedWidth;
            w.RimDepth = 0;
            w.RimSize = (int)(NFMCorrectedWheelRadius * 0.7);
            return w;
        }
    }
}

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
        
        public double NFMDsCorrectedRadius { get; }
        public double NFMDsCorrectedWheelRadius { get; }
        
        public double NFMVanillaCorrectedWheelRadius { get; }

        public double NFMCorrectedWidth { get; }

        public Cylinder(Vector3D location, /*Vector3D origin,*/ double radius, double width)
        {
            Location = location;
            //Origin = origin;
            Radius = radius;
            Width = width;
            NFMDsCorrectedWheelRadius = Radius * (50.0 / 60.0);
            double adjustScale = 1.0;
            if(NFMDsCorrectedWheelRadius > 46.0)
            {
                adjustScale = 46.0 / NFMDsCorrectedWheelRadius;
                NFMDsCorrectedWheelRadius = NFMDsCorrectedWheelRadius * adjustScale;
            }
            NFMDsCorrectedRadius = Radius * adjustScale;
            NFMCorrectedWidth = (Width * 1.25) * (Location.X >= 0 ? -1 : 1);
            NFMVanillaCorrectedWheelRadius = Radius * (35.0 / 42.0); /// 2.0 * (35.0 / 30.0);
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
                    w.Height = (int)NFMDsCorrectedWheelRadius;
                    w.RimSize = (int)(NFMDsCorrectedWheelRadius * 0.7);
                    break;
                case IntermediateMeshMode.PhyrexianWheel:
                case IntermediateMeshMode.VanillaWheel:
                    w.Height = (int)NFMVanillaCorrectedWheelRadius;
                    w.RimSize = (int)(NFMVanillaCorrectedWheelRadius * 0.7);
                    break;
            }
            return w;
        }
    }
}

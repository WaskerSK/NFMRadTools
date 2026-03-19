using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class Polygon
    {
        public bool AlternativePolyMarkup { get; set; }
        public bool NoOutline { get; set; }
        public bool Glass { get; set; }
        public bool GlassTint { get; set; }
        public bool Light { get; set; }
        public bool LightFront { get; set; }
        public bool LightBack { get; set; }
        public bool LightBrake { get; set; }
        public bool LightReverse { get; set; }
        public bool DayOnly { get; set; }
        public bool NightOnly { get; set; }
        public Color Color { get; set; }
        public int? Fs { get; set; }
        public int Gr { get; set; }
        public List<Vertex> Vertices { get; }
        public List<string> Metadata { get; }
        public PolyGroup PolyGroup { get; set; }

        public Polygon()
        {
            Vertices = new List<Vertex>();
            Metadata = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (AlternativePolyMarkup)
                sb.AppendLine("[p]");
            else
                sb.AppendLine("<p>");
            sb.Append("c(").Append(Color.ToString()).AppendLine(")");
            if (Light)
                sb.AppendLine("light");
            if (LightFront)
                sb.AppendLine("lightF");
            if (LightBack)
                sb.AppendLine("lightB");
            if (LightReverse)
                sb.AppendLine("lightR");
            if (LightBrake)
                sb.AppendLine("lightBrake");
            if (DayOnly)
                sb.AppendLine("dayOnly");
            if (NightOnly)
                sb.AppendLine("nightOnly");
            if (NoOutline)
                sb.AppendLine("noOutline");
            if (Glass)
                sb.AppendLine("glass");
            if (GlassTint)
                sb.AppendLine("glassTint");
            if (Fs.HasValue)
                sb.Append("fs(").Append(Fs.Value).AppendLine(")");
            if (Gr != 0)
                sb.Append("gr(").Append(Gr).AppendLine(")");
            foreach (string metadata in Metadata)
            {
                sb.AppendLine(metadata);
            }
            if(PolyGroup.Mode == PolyGroupMode.PhyrexianWheel)
            {
                sb.Append("wheel(").Append(PolyGroup.CustomWheelIndex).AppendLine(")");
            }
            sb.AppendLine();
            foreach (Vertex v in Vertices)
            {
                sb.AppendLine(v.ToString());
            }
            if (AlternativePolyMarkup)
                sb.AppendLine("[/p]");
            else
                sb.AppendLine("</p>");
            sb.AppendLine();
            return sb.ToString();
        }

        //public 

        /*public double CalculateSurfaceArea()
        {

        }*/

        public Polygon Duplicate()
        {
            Polygon copy = new Polygon();
            copy.Vertices.AddRange(Vertices);
            copy.Metadata.AddRange(Metadata);
            copy.AlternativePolyMarkup = AlternativePolyMarkup;
            copy.NoOutline = NoOutline;
            copy.Glass = Glass;
            copy.GlassTint = GlassTint;
            copy.Light = Light;
            copy.LightFront = LightFront;
            copy.LightBack = LightBack;
            copy.LightBrake = LightBrake;
            copy.LightReverse = LightReverse;
            copy.DayOnly = DayOnly;
            copy.NightOnly = NightOnly;
            copy.Color = Color;
            copy.Fs = Fs;
            copy.Gr = Gr;
            copy.PolyGroup = PolyGroup;
            return copy;
        }

        public Polygon Mirror(Axis MirrorAxis, bool CreateCopy)
        {
            Polygon p = null;
            if (CreateCopy) p = this.Duplicate();
            else p = this;
            MirrorAxis = MirrorAxis.ToPositive();
            Vector3D v = default;
            switch(MirrorAxis)
            {
                case Axis.X:
                    v = new Vector3D(-1,1,1);
                    break;
                case Axis.Y:
                    v = new Vector3D(1, -1, 1);
                    break;
                case Axis.Z:
                    v = new Vector3D(1, 1, -1);
                    break;
            }
            for(int i = 0; i < p.Vertices.Count; i++)
            {
                p.Vertices[i] = (Vertex)((Vector3D)p.Vertices[i] * v);
            }
            return p;
        }
    }
}

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
                sb.Append("wheel(").Append(PolyGroup.PhyrexianWheelIndex).AppendLine(")");
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
    }
}

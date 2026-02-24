using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class Polygon
    {
        public bool NoOutline { get; set; }
        public Color Color { get; set; }
        public int? Fs { get; set; }
        public int Gr { get; set; }
        public List<Vertex> Vertices { get; }
        public List<string> Metadata { get; }

        public Polygon()
        {
            Vertices = new List<Vertex>();
            Metadata = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p>");
            if (NoOutline)
                sb.AppendLine("noOutline");
            sb.Append("c(").Append(Color.ToString()).AppendLine(")");
            if (Fs.HasValue)
                sb.Append("fs(").Append(Fs.Value).AppendLine(")");
            if (Gr != 0)
                sb.Append("gr(").Append(Gr).AppendLine(")");
            foreach (string metadata in Metadata)
            {
                sb.AppendLine(metadata);
            }
            sb.AppendLine();
            foreach (Vertex v in Vertices)
            {
                sb.AppendLine(v.ToString());
            }
            sb.AppendLine("</p>");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}

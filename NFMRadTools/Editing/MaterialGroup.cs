using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class MaterialGroup
    {
        public string Name { get; set; }
        public List<Polygon> Polygons { get; }

        public MaterialGroup()
        {
            Polygons = new List<Polygon>();
        }

        public void SetColor(Color color)
        {
            foreach (Polygon p in Polygons)
            {
                p.Color = color;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("//<m=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            foreach (Polygon poly in Polygons)
            {
                sb.AppendLine(poly.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("//</m=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            return sb.ToString();
        }

        public static string GetRandomGroupName()
        {
            Span<char> s = stackalloc char[8];
            for (int i = 0; i < s.Length; i++)
            {
                int random = Random.Shared.Next('A', 'Z' + 1);
                s[i] = (char)random;
            }
            return s.ToString();
        }
    }
}

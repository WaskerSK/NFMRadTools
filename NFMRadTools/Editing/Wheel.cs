using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class Wheel
    {
        public int GwGr { get; set; }

        public List<Instance> Instances { get; }

        public Color RimsColor { get; set; }
        public int RimSize { get; set; }
        public int RimDepth { get; set; }

        public Wheel()
        {
            Instances = new List<Instance>();
        }

        public override string ToString()
        {
            //rims(r,g,b,size,depth)
            StringBuilder sb = new StringBuilder();
            sb.Append("gwgr(").Append(GwGr).AppendLine(")");
            sb.Append("rims(")
                .Append(RimsColor.ToString())
                .Append(",").Append(RimSize)
                .Append(",").Append(RimDepth)
                .AppendLine(")");
            foreach(Instance instance in Instances)
            {
                sb.AppendLine(instance.ToString());
            }
            return sb.ToString();
        }

        public struct Instance
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public bool CanSteer { get; set; } // true 11; false 0
            public int Height { get; set; }
            public int Width { get; set; }

            public override string ToString()
            {
                return $"w({X},{Y},{Z},{(CanSteer ? "11" : "0")},{Width},{Height})";
            }
        }
    }
}

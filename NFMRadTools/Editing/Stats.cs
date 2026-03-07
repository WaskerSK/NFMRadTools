using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Stats
    {
        public int Speed { get; set; }
        public int Acceleration { get; set; }
        public int Stunts { get; set; }
        public int Strength { get; set; }
        public int Endurance { get; set; }

        public override string ToString()
        {
            return $"stat({Speed},{Acceleration},{Stunts},{Strength},{Endurance})";
        }
    }
}

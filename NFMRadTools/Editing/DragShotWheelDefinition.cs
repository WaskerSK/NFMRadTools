using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class DragShotWheelDefinition
    {
        public int Radius { get; set; }
        public int Depth { get; set; }
        public List<int> Targets { get; } = new List<int>();

        public DragShotWheelDefinition()
        {
        }
    }
}

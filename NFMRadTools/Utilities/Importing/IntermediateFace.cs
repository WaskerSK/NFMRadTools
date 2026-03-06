using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public class IntermediateFace
    {
        public List<int> Indexes { get; } = new List<int>();
        public IntermediateMaterial Material { get; set; }
    }
}

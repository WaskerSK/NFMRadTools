using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    [AlternativeNames]
    public enum VertexMergingRule
    {
        [EnumAlternativeName("n")]
        [EnumAlternativeName("null")]
        None = 0,
        [EnumAlternativeName("a")]
        All = 1,
        [EnumAlternativeName("w")]
        [EnumAlternativeName("wheel")]
        Wheels = 2
    }
}

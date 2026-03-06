using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    [AlternativeNames]
    public enum AutoColoringMode
    {
        [EnumAlternativeName("p")]
        [EnumAlternativeName("poly")]
        [EnumAlternativeName("polygon")]
        Polygons,
        [EnumAlternativeName("v")]
        [EnumAlternativeName("vert")]
        [EnumAlternativeName("vertex")]
        Vetrices,
        [EnumAlternativeName("b")]
        [EnumAlternativeName("box")]
        [EnumAlternativeName("bound")]
        [EnumAlternativeName("volume")]
        Bounds,
        [EnumAlternativeName("e")]
        [EnumAlternativeName("edge")]
        [EnumAlternativeName("length")]
        [EnumAlternativeName("len")]
        Edge,
        [EnumAlternativeName("s")]
        [EnumAlternativeName("sur")]
        [EnumAlternativeName("surf")]
        [EnumAlternativeName("surface")]
        [EnumAlternativeName("area")]
        SurfaceArea
    }
}

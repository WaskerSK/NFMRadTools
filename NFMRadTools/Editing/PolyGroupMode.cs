using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    [AlternativeNames]
    public enum PolyGroupMode
    {
        [EnumAlternativeName("n")]
        [EnumAlternativeName("norm")]
        Normal = 0,
        [EnumAlternativeName("dsw")]
        [EnumAlternativeName("dscw")]
        [EnumAlternativeName("ds-cw")]
        [EnumAlternativeName("ds")]
        [EnumAlternativeName("ds-w")]
        [EnumAlternativeName("dragshot")]
        [EnumAlternativeName("dragshotw")]
        DragShotWheel = 1,
        [EnumAlternativeName("phy")]
        [EnumAlternativeName("phycw")]
        [EnumAlternativeName("phy-cw")]
        [EnumAlternativeName("phyw")]
        [EnumAlternativeName("phy-w")]
        [EnumAlternativeName("phyrexian")]
        [EnumAlternativeName("phyrexianw")]
        PhyrexianWheel = 2,
        [EnumAlternativeName("g6")]
        [EnumAlternativeName("g6w")]
        [EnumAlternativeName("g6cw")]
        [EnumAlternativeName("g6-w")]
        [EnumAlternativeName("g6-cw")]
        G6Wheel = 3
    }
}

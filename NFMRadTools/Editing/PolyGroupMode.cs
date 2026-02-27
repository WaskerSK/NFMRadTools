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
        Normal,
        [EnumAlternativeName("dsw")]
        [EnumAlternativeName("dscw")]
        [EnumAlternativeName("ds-cw")]
        [EnumAlternativeName("ds")]
        [EnumAlternativeName("ds-w")]
        DragShotWheel,
        [EnumAlternativeName("phy")]
        [EnumAlternativeName("phycw")]
        [EnumAlternativeName("phy-cw")]
        [EnumAlternativeName("phyw")]
        [EnumAlternativeName("phy-w")]
        PhyrexianWheel
    }
}

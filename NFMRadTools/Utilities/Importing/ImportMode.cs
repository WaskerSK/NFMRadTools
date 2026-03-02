using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    [AlternativeNames]
    public enum ImportMode
    {
        [EnumAlternativeName("n")]
        New,
        [EnumAlternativeName("m")]
        Merge,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    public static class AlternativeEnumParser
    {
        public static bool TryParse(Type enumType, ReadOnlySpan<char> value, bool ignoreCase, out object result)
        {
            result = default;
            if (enumType is null) return false;
            if(!enumType.IsEnum) return false;
            if(enumType.IsDefined(typeof(AlternativeNamesAttribute)) && !(value.IsEmpty || value.IsWhiteSpace()))
            {
                FieldInfo[] fInfos = enumType.GetFields();
                foreach(FieldInfo fi in fInfos.Where(x => x.FieldType == enumType))
                {
                    if(fi.IsDefined(typeof(EnumAlternativeNameAttribute)))
                    {
                        foreach(EnumAlternativeNameAttribute altAtt in fi.GetCustomAttributes<EnumAlternativeNameAttribute>())
                        {
                            if(value.Equals(altAtt.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                result = fi.GetValue(null);
                                return result is not null;
                            }
                        }
                    }
                }
            }
            return Enum.TryParse(enumType, value, ignoreCase, out result);
        }
    }
}

using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    public static class ValueParser
    {
        public static bool TryParse(string value, Type type, out object result)
        {
            result = null;
            if (type is null) return false;
            if(type == typeof(string))
            {
                result = value;
                return true;
            }
            Type genericType = typeof(IParsable<>).MakeGenericType(type);
            if (type.GetInterfaces().Any(x => x == genericType))
            {
                try
                {
                    Type[] paramTypes = [typeof(string), typeof(IFormatProvider), type.MakeByRefType()];
                    MethodInfo tryparse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, paramTypes);
                    if(tryparse is null)
                    {
                        tryparse = type.GetMethod($"System.IParsable<{type.FullName}>.TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic, paramTypes);
                        if (tryparse is null) return false;
                    }
                    object[] args = [value, null, null];
                    object parseBool = tryparse.Invoke(null, args);
                    result = args[2];
                    return (bool)parseBool;
                }
                catch
                {
                }
                return false;
            }
            if(type.IsEnum)
            {
                return AlternativeEnumParser.TryParse(type, value, true, out result);
            }
            return false;
        }
    }
}

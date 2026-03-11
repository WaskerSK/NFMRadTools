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
            if (type.IsEnum)
            {
                return AlternativeEnumParser.TryParse(type, value, true, out result);
            }
            Type elementType = type;
            bool isNullable = false;
            if(type.IsArray)
            {
                elementType = type.GetElementType();
            }
            else
            {
                Type nullableT = Nullable.GetUnderlyingType(type);
                if(nullableT is not null)
                {
                    elementType = nullableT;
                    isNullable = true;
                }
            }
            if (elementType == typeof(string)) throw new NotSupportedException("String arrays are currently not supported.");
            Type genericType = typeof(IParsable<>).MakeGenericType(elementType);
            if (elementType.GetInterfaces().Any(x => x == genericType))
            {
                try
                {
                    Type[] paramTypes = [typeof(string), typeof(IFormatProvider), elementType.MakeByRefType()];
                    MethodInfo tryparse = elementType.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, paramTypes);
                    if(tryparse is null)
                    {
                        tryparse = elementType.GetMethod($"System.IParsable<{elementType.FullName}>.TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic, paramTypes);
                        if (tryparse is null) return false;
                    }
                    if(type.IsArray)
                    {
                        ArgumentEnumerator argEnum = new ArgumentEnumerator(value, ';', false);
                        int argCount = argEnum.ArgCount();
                        if (argCount <= 0)
                        {
                            result = Array.CreateInstance(elementType, 0);
                            return true;
                        }
                        Array resultArr = Array.CreateInstance(elementType, argCount);
                        object[] args = new object[3];
                        for (int i = 0; i < argCount; i++)
                        {
                            if(!argEnum.MoveNext()) return false;
                            args = [argEnum.Current.ToString(), null, null];
                            if(elementType.IsEnum)
                            {
                                if (!AlternativeEnumParser.TryParse(type, args[0] as string, true, out object resultEnum)) return false;
                                resultArr.SetValue(resultEnum, i);
                            }
                            else
                            {
                                object parseBool = tryparse.Invoke(null, args);
                                if (!(bool)parseBool) return false;
                                resultArr.SetValue(args[2], i);
                            }  
                        }
                        result = resultArr;
                        return true;
                    }
                    else
                    {
                        object[] args = [value, null, null];
                        object parseBool = tryparse.Invoke(null, args);
                        if(isNullable)
                        {
                            object parsedValue = args[2];
                            args[2] = Activator.CreateInstance(elementType, parsedValue);
                        }
                        result = args[2];
                        return (bool)parseBool;
                    }
                }
                catch
                {
                }
                return false;
            }
            return false;
        }
    }
}

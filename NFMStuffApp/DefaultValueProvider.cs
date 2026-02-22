using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMStuffApp
{
    public static class DefaultValueProvider
    {
        private static MethodInfo genericDefaultValueMethodInfo = typeof(DefaultValueProvider).GetMethod(nameof(GetDefaultValue), 1, BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());
        public static object GetDefaultValue(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (type.IsByRef || type.IsByRefLike) throw new ArgumentException("Refs and RefLikes are not allowed.");
            MethodInfo mi = genericDefaultValueMethodInfo.MakeGenericMethod(type);
            return mi.Invoke(null, null);
        }

        public static T GetDefaultValue<T>()
        {
            return default(T);
        }
    }
}

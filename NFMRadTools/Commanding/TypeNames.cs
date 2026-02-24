using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    public static class TypeNames
    {
        private static Dictionary<Type, string> NameDictionary = new Dictionary<Type, string>()
        {
            {typeof(byte), "byte" },
            {typeof(sbyte), "sbyte" },
            {typeof(bool), "bool" },
            {typeof(char), "char" },
            {typeof(short), "short" },
            {typeof(ushort), "ushort" },
            {typeof(int), "int" },
            {typeof(uint), "uint" },
            {typeof(float), "float" },
            {typeof(long), "long" },
            {typeof(ulong), "ulong" },
            {typeof(double), "double" },
            {typeof(decimal), "decimal" },
            {typeof(string), "string" },
        };

        public static void RegisterTypeName(Type type, string typeName)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
            if(!NameDictionary.TryAdd(type, typeName))
            {
                NameDictionary[type] = typeName;
            }
        }

        public static string GetTypeName(Type type)
        {
            if (type is null) return null;
            if(NameDictionary.TryGetValue(type, out var typeName)) return typeName;
            return type.Name;
        }
    }
}

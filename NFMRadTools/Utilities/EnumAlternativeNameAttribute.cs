using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public sealed class AlternativeNamesAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EnumAlternativeNameAttribute : Attribute
    {
        public string Name { get; }
        public EnumAlternativeNameAttribute(string name)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            this.Name = name;
        }
    }
}

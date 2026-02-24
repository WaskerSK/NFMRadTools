using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; }
        public DescriptionAttribute(string comment) 
        { 
            Description = comment;
        }
    }
}

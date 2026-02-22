using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string CommandName { get; init; }
        public bool VerifyCarLoaded { get; init; }
    }
}

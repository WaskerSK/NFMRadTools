using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class DevModeAttribute : Attribute
    {
        public bool DevMode { get; }
        public DevModeAttribute(bool devMode) { DevMode = devMode; }
    }
}

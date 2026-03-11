using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Configuration
{
    public interface IConfigurable
    {
        string EntryName { get; }

        static abstract void RegisterConfigEntry(OrderedDictionary<string, ConfigurableEntry> settings);
    }
}

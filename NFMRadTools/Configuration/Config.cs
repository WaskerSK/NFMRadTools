using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NFMRadTools.Configuration
{
    public class Config
    {
        public static readonly JsonSerializerOptions ConfigSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default) { WriteIndented = true };
        public string CarDirectory { get; set; }
        public string ImportDirectory { get; set; }

        public OrderedDictionary<string, ConfigurableEntry> Settings { get; set; } = new OrderedDictionary<string, ConfigurableEntry>();

        public void Save(string ConfigPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ConfigPath);
            if(!ConfigPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Config file must be a .json file.");
            }
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, ConfigSerializerOptions));
        }
    }

}

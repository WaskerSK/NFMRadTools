using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NFMRadTools
{
    public class Config
    {
        public string CarDirectory { get; set; }
        //public string ImportDirectory { get; set; }

        public void Save(string ConfigPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(ConfigPath);
            if(!ConfigPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Config file must be a .json file.");
            }
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize<Config>(this, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }

}

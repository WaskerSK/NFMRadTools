using NFMRadTools.Utilities.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [Command(DevCommand = true)]
    public static class DevCommands
    {
        [Command(CommandName = "dev.presetgen", DevCommand = true)]
        public static void GeneratePresetsCodeFromFile(string file)
        {
            PresetCodeConverter.RunPresetCreatorScript(file);
        }
    }
}

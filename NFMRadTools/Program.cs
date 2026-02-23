using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace NFMRadTools
{
    public class Program
    {
        public static string CarDirectory => Config.CarDirectory;
        public static Config Config { get; private set; }
        public const string NFMCarExtension = ".rad";
        public static SortedDictionary<string, Command> CommandList = new SortedDictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        public static NFMCar CurrentCar = null;
        static void Main(string[] args)
        {
            string configPath = $"{Environment.CurrentDirectory}\\Config.json";
            try
            {
                InitConfig(configPath);
            }
            catch { }
            if(Config is null)
            {
                Logger.Error("Fatal error: Failed to create or load config.");
                return;
            }
            InitCommands();
            Logger.Info("Type help to list commands.");
            Debug.Assert(Config is not null);
            while(true)
            {
                try
                {
                    string s = Console.ReadLine();
                    string cmd = GetCommandNameFromInputString(s);
                    if (!CommandList.TryGetValue(cmd, out Command command))
                    {
                        Logger.Error($"Invalid command \"{cmd}\".");
                        Logger.Info("Type hlep to list commands.");
                        continue;
                    }
                    if(command.VerifyCarLoaded)
                    {
                        if (!VerifyCarLoaded())
                            continue;
                    }
                    if (command.HasArgs)
                        command.Execute(s.AsSpan().Trim().Slice(cmd.Length).Trim().ToString());
                    else command.Execute(null);
                }
                catch (Exception e)
                {
                    if (e is null) throw new Exception("Unknown error.", e);
                    if (e is ExitException || e.InnerException is ExitException) return;
                    Logger.Error(e.ToString());
                }
            }
        }

        static void InitCommands()
        {
            CommandList.Clear();
            Logger.Info("Initializing commands.");
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsDefined(typeof(CommandAttribute)))
                {
                    foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (mi.IsDefined(typeof(CommandAttribute)))
                        {
                            CommandAttribute cmdAtt = mi.GetCustomAttribute<CommandAttribute>();
                            string cmdName = null;
                            if (string.IsNullOrWhiteSpace(cmdAtt.CommandName)) cmdName = mi.Name;
                            else cmdName = cmdAtt.CommandName;
                            Command cmd = new Command()
                            {
                                Name = cmdName,
                                Method = mi,
                                VerifyCarLoaded = cmdAtt.VerifyCarLoaded
                            };
                            CommandList.Add(cmdName, cmd);
                        }
                    }
                }
            }
            Logger.Info("Commands initialized.");
        }

        static string GetCommandNameFromInputString(string inputString)
        {
            ReadOnlySpan<char> span = inputString.AsSpan();
            span = span.TrimStart();
            int indexofWhiteSpace = -1;
            for(int i = 0; i < span.Length; i++)
            {
                if(char.IsWhiteSpace(span[i]))
                {
                    indexofWhiteSpace = i;
                    break;
                }
            }
            if(indexofWhiteSpace == -1 && span.Length == inputString.Length) return inputString;
            if (indexofWhiteSpace == -1) return span.ToString();
            return span.Slice(0, indexofWhiteSpace).ToString();
        }

        public static bool VerifyCarLoaded()
        {
            if (Program.CurrentCar is null)
            {
                Logger.Error("No car loaded.");
                return false;
            }
            return true;
        }

        static void InitConfig(string configPath)
        {
            if(File.Exists(configPath))
            {
                try
                {
                    using(Stream stream = File.OpenRead(configPath))
                    {
                        Config cfg = JsonSerializer.Deserialize<Config>(stream);
                        if (cfg is not null) Config = cfg;
                        else return;
                    }
                    if (Directory.Exists(Config.CarDirectory)) return;
                    throw null;
                }
                catch(Exception e)
                {
                    if(e is null)
                    {
                        Logger.Info("Config was loaded but CarDirectory was not found.");
                    }
                    else
                        Logger.Warning("Failed to load config.");
                }
            }
            Logger.Info("Initializing config.");
            if(Config is null)
                Config = new Config();
            while(true)
            {
                Console.Write("Enter your car folder path: ");
                string s = Console.ReadLine();
                if (Directory.Exists(s))
                {
                    Config.CarDirectory = s;
                    break;
                }
                Logger.Error($"Folder \"{s}\" was not found.");
            }
            string json = JsonSerializer.Serialize<Config>(Config, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
    }

    [Command]
    public static class CommandsLib
    {
        [Command(CommandName = "game.setcarfolder")]
        public static void SetCarFolder(string Folder)
        {
            if(!Directory.Exists(Folder))
            {
                Logger.Error("Directory not found.");
                return;
            }
            Program.Config.CarDirectory = Folder;
            Logger.Info($"Car directory was set to: \"{Folder}\".");
        }
        [Command(CommandName = "game.carfolder")]
        public static void PrintCarFolder()
        {
            if (string.IsNullOrWhiteSpace(Program.CarDirectory))
                Logger.Error("Car directory is not set.");
            else
                Logger.Info(Program.CarDirectory);
        }
        [Command(CommandName = "help")]
        public static void Help()
        {
            Logger.Info("List of commands:");
            StringBuilder sb = new StringBuilder();
            foreach (var cmdTuple in Program.CommandList)
            {
                sb.AppendLine(cmdTuple.Value.ToString());
            }
            Logger.Log(sb.ToString(), ConsoleColor.Cyan);
        }
        [Command(CommandName = "exit")]
        public static void Exit()
        {
            Logger.Info("Exiting.");
            throw new ExitException();
        }
        [Command(CommandName = "load")]
        public static void LoadCar(string CarName)
        {
            if(string.IsNullOrWhiteSpace(CarName))
            {
                Logger.Error("Invalid car file name.");
                return;
            }
            string filePath = $"{Program.CarDirectory}\\{CarName}{Program.NFMCarExtension}";
            string carData = null;
            try
            {
                carData = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return;
            }
            Program.CurrentCar = NFMCar.Parse(carData);
            if(Program.CurrentCar is null)
            {
                Logger.Error("Car failed to load.");
            }
            else
            {
                Logger.Info("Car loaded.");
            }
        }

        [Command(CommandName = "print", VerifyCarLoaded = true)]
        public static void PrintCar()
        {
            Logger.Info("Car code:");
            Console.WriteLine(Program.CurrentCar.ToString());
        }
        [Command(CommandName = "save", VerifyCarLoaded = true)]
        public static void Save(string CarName)
        {
            if (string.IsNullOrWhiteSpace(CarName))
            {
                Logger.Error("No car name specified.");
                return;
            }
            if (CarName.AsSpan().ContainsAny(Path.GetInvalidFileNameChars()))
            {
                Logger.Error("Invalid car file name.");
                return;
            }
            string filePath = $"{Program.CarDirectory}\\{CarName}{Program.NFMCarExtension}";
            File.WriteAllText(filePath, Program.CurrentCar.ToString());
            Logger.Info($"Car saved: \"{filePath}\".");
        }

        [Command(CommandName = "car.groups.list", VerifyCarLoaded = true)]
        public static void ListMaterialGroups()
        {
            Logger.Log("Material groups:", ConsoleColor.Green);
            for(int i = 0; i < Program.CurrentCar.MaterialGroups.Count; i++)
            {
                Console.Write("[");
                Console.Write(i);
                Console.Write("] - ");
                if (Program.CurrentCar.MaterialGroups[i].Name is null)
                {
                    Console.Write("Group_");
                    Console.Write(i + 1);
                }
                else Console.Write(Program.CurrentCar.MaterialGroups[i].Name);
                Console.Write(" - (");
                Console.Write(Program.CurrentCar.MaterialGroups[i].Polygons.Count);
                Console.WriteLine(" Polygons)");
            }
        }

        [Command(CommandName = "car.groups.new", VerifyCarLoaded = true)]
        public static void CreateNewMaterialGroup(string Name = null)
        {
            MaterialGroup mg = new MaterialGroup();
            if (string.IsNullOrWhiteSpace(Name)) Name = null;
            mg.Name = Name;
            Program.CurrentCar.MaterialGroups.Add(mg);
            Logger.Info($"New group created: [{Program.CurrentCar.MaterialGroups.Count-1}]");
        }

        [Command(CommandName = "car.groups.movepoly", VerifyCarLoaded = true)]
        public static void MovePolygonsToGroup(int SourceGroupIndex, int TargetGroupIndex, int PolyStartIndex, int PolyCount)
        {
            MaterialGroup source = Program.CurrentCar.MaterialGroups[SourceGroupIndex];
            MaterialGroup target = Program.CurrentCar.MaterialGroups[TargetGroupIndex];
            target.Polygons.AddRange(source.Polygons.Take(new Range(PolyStartIndex, PolyStartIndex + PolyCount)));
            source.Polygons.RemoveRange(PolyStartIndex, PolyCount);
            Logger.Info($"{PolyCount} polygons moved from group [{SourceGroupIndex}] to group [{TargetGroupIndex}].");
        }

        [Command(CommandName = "car.groups.setcolor", VerifyCarLoaded = true)]
        public static void SetGroupColor(int GroupIndex, byte R, byte G, byte B)
        {
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            Program.CurrentCar.MaterialGroups[GroupIndex].SetColor(c);
            Logger.Info($"Color of group [{GroupIndex}] was changed to {c.ToStringAlt()}.");
        }

        [Command(CommandName = "car.groups.removeempty", VerifyCarLoaded = true)]
        public static void RemoveEmptyGroups()
        {
            int countRemoved = 0;
            for(int i = Program.CurrentCar.MaterialGroups.Count - 1; i >= 0; i--)
            {
                if(Program.CurrentCar.MaterialGroups[i].Polygons.Count <= 0)
                {
                    Program.CurrentCar.MaterialGroups.RemoveAt(i);
                    countRemoved++;
                }
            }
            Logger.Info($"Empty groups removed: {countRemoved}.");
        }

        [Command(CommandName = "car.groups.setfs", VerifyCarLoaded = true)]
        public static void SetGroupFs(int GroupIndex, int FsValue)
        {
            foreach(Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = FsValue;
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
        }

        [Command(CommandName = "car.setfs", VerifyCarLoaded = true)]
        public static void SetCarFs(int FsValue)
        {
            int polycount = 0;
            foreach(MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach(Polygon p in mg.Polygons)
                {
                    p.Fs = FsValue;
                }
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.removefs", VerifyCarLoaded = true)]
        public static void RemoveGroupFs(int GroupIndex)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = null;
            }
            Logger.Info($"Value of \'fs\' has been removed to from {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
        }

        [Command(CommandName = "car.removefs", VerifyCarLoaded = true)]
        public static void RemoveCarFs()
        {
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.Fs = null;
                }
            }
            Logger.Info($"Value of \'fs\' has been removed from {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
        }

        [Command(CommandName = "car.setoutline", VerifyCarLoaded = true)]
        public static void SetCarOutline(bool Value)
        {
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.NoOutline = Value;
                }
            }
            Logger.Info($"Value of \'noOutline\' has been {(Value ? "added to" : "removed from")} {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.setoutline", VerifyCarLoaded = true)]
        public static void SetGroupOutline(int GroupIndex, bool Value)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.NoOutline = Value;
            }
            Logger.Info($"Value of \'noOutline\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
        }

        [Command(CommandName = "car.groups.setgr", VerifyCarLoaded = true)]
        public static void SetGroupGr(int GroupIndex, int Value)
        {
            foreach(Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Gr = Value;
            }
            Logger.Info($"Value of \'gr\' has been {(Value == 0 ? "removed from" : $"set to {Value} on")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
        }
    }

    public class ExitException : Exception
    {
        public ExitException() { }
    }
}

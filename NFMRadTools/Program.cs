using NFMRadTools.Commanding;
using NFMRadTools.Editing;
using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;

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
                    if (e is ExitException || e.InnerException is ExitException)
                    {
                        int exitCode = 0;
                        ExitException exitException = e as ExitException;
                        exitException ??= e.InnerException as ExitException;
                        exitCode = exitException.ExitCode;
                        Environment.Exit(exitCode);
                        return;
                    }
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
                            DescriptionAttribute descriptionAtt = mi.GetCustomAttribute<DescriptionAttribute>();
                            string description = null;
                            if(descriptionAtt is not null)
                            {
                                description = descriptionAtt.Description;
                            }
                            Command cmd = new Command()
                            {
                                Name = cmdName,
                                Method = mi,
                                VerifyCarLoaded = cmdAtt.VerifyCarLoaded,
                                Description = description
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
}

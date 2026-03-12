using NFMRadTools.Commanding;
using NFMRadTools.Configuration;
using NFMRadTools.Editing;
using NFMRadTools.Utilities;
using NFMRadTools.Utilities.Importing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;

[assembly: DevMode(false)]

namespace NFMRadTools
{
    public class Program
    {
        public const string NFMDemicalFormat = "0.0###############";
        public static string CarDirectory => Config.CarDirectory;
        public static string ImportDirectory => Config.ImportDirectory;
        public static Config Config { get; private set; }
        public const string NFMCarExtension = ".rad";
        public static SortedDictionary<string, Command> CommandList = new SortedDictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        public static NFMCar CurrentCar = null;
        public static readonly CoordinateSystem NFMCoordinates = new CoordinateSystem(Direction.Right, Direction.Down, Direction.Back);
        
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            string configPath = Path.Combine(Environment.CurrentDirectory, "Config.json");
            Init:
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
            InitConfigurables();
            Config.Save(configPath);
            InitCommands();
            Logger.Info("Type help to list commands.");
            Debug.Assert(Config is not null);
            while(true)
            {
                try
                {
                    string s = Console.ReadLine();
                    string cmd = GetCommandNameFromInputString(s);
                    if(cmd.EndsWith('?'))
                    {
                        cmd = "?";
                    }
                    if (!CommandList.TryGetValue(cmd, out Command command))
                    {
                        Logger.Error($"Invalid command \"{cmd}\".");
                        Logger.Info("Type help to list commands.");
                        continue;
                    }
                    if(command.VerifyCarLoaded)
                    {
                        if (!VerifyCarLoaded())
                            continue;
                    }
                    if (command.HasArgs)
                    {
                        if(command.Name == "?" && s.AsSpan().TrimStart()[0] != '?')
                        {
                            command.Execute(s.AsSpan().Trim().Slice(0, s.Length - 1).Trim().ToString());
                        }
                        else
                        {
                            command.Execute(s.AsSpan().Trim().Slice(cmd.Length).Trim().ToString());
                        }
                    }
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
                    if(e is RestartException || e.InnerException is RestartException)
                    {
                        goto Init;
                    }
                    Logger.Error(e.ToString());
                }
            }
        }

        static void InitConfigurables()
        {
            if (Config is null) throw new InvalidOperationException();
            if (Config.Settings is null) throw new InvalidOperationException();
            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(t.IsAssignableTo(typeof(IConfigurable)) && !t.IsAbstract)
                {
                    t.GetMethod(nameof(IConfigurable.RegisterConfigEntry), BindingFlags.Static | BindingFlags.Public).Invoke(null, [Config.Settings]);
                }
            }
        }

        static void InitCommands()
        {
            CommandList.Clear();
            Logger.Info("Initializing commands.");
            bool devMode = false;
            DevModeAttribute devAtt = Assembly.GetExecutingAssembly().GetCustomAttribute<DevModeAttribute>();
            if(devAtt is not null)
            {
                devMode = devAtt.DevMode;
            }
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                CommandAttribute typeCmdAtt = t.GetCustomAttribute<CommandAttribute>();
                bool canProceesToMethods = false;
                if(typeCmdAtt is not null)
                {
                    canProceesToMethods = typeCmdAtt.DevCommand.ToInt() <= devMode.ToInt();
                }
                if (canProceesToMethods)
                {
                    foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (mi.IsDefined(typeof(CommandAttribute)))
                        {
                            CommandAttribute cmdAtt = mi.GetCustomAttribute<CommandAttribute>();
                            if (cmdAtt.DevCommand.ToInt() > devMode.ToInt()) continue;
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
            Config.Save(configPath);
        }

    }
}

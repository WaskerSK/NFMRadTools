using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NFMRadTools
{
    public class Program
    {
        public static string CarDirectory = "F:\\NFM\\data\\mycars";
        public static string NFMCarExtension = ".rad";
        public static SortedDictionary<string, Command> CommandList = new SortedDictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        public static NFMCar CurrentCar = null;
        static void Main(string[] args)
        {
            InitCommands();
            Console.WriteLine("Type help to list commands.");
            while(true)
            {
                try
                {
                    string s = Console.ReadLine();
                    string cmd = GetCommandNameFromInputString(s);
                    if (!CommandList.TryGetValue(cmd, out Command command))
                    {
                        Console.WriteLine("Invalid command.");
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
                    if (e is ExitException) return;
                    Console.WriteLine(e.ToString());
                }
            }
        }

        static void InitCommands()
        {
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

        static bool VerifyCarLoaded()
        {
            if (Program.CurrentCar is null)
            {
                Console.WriteLine("No car loaded.");
                return false;
            }
            return true;
        }
    }

    [Command]
    public static class CommandsLib
    {
        [Command(CommandName = "help")]
        public static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("List of commands:");
            StringBuilder sb = new StringBuilder();
            foreach (var cmdTuple in Program.CommandList)
            {
                sb.AppendLine(cmdTuple.Value.ToString());
            }
            Console.WriteLine(sb.ToString());
        }
        [Command(CommandName = "exit")]
        public static void Exit()
        {
            throw new ExitException();
        }
        [Command(CommandName = "load")]
        public static void LoadCar(string CarName)
        {
            if(string.IsNullOrWhiteSpace(CarName))
            {
                Console.WriteLine("Invalid car file name.");
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
                Console.WriteLine(ex.Message);
                return;
            }
            Program.CurrentCar = NFMCar.Parse(carData);
            if(Program.CurrentCar is null)
            {
                Console.WriteLine("Car failed to load.");
            }
            else
            {
                Console.WriteLine("Car loaded.");
            }
        }

        [Command(CommandName = "print", VerifyCarLoaded = true)]
        public static void PrintCar()
        {
            Console.WriteLine(Program.CurrentCar.ToString());
        }
        [Command(CommandName = "save", VerifyCarLoaded = true)]
        public static void Save(string CarName)
        {
            if (string.IsNullOrWhiteSpace(CarName))
            {
                Console.WriteLine("No car name specified.");
                return;
            }
            if (CarName.AsSpan().ContainsAny(Path.GetInvalidFileNameChars()))
            {
                Console.WriteLine("Invalid car filename.");
                return;
            }
            string filePath = $"{Program.CarDirectory}\\{CarName}{Program.NFMCarExtension}";
            File.WriteAllText(filePath, Program.CurrentCar.ToString());
            Console.Write("Car saved: ");
            Console.WriteLine(filePath);
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.list", VerifyCarLoaded = true)]
        public static void ListMaterialGroups()
        {
            Console.WriteLine();
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
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.new", VerifyCarLoaded = true)]
        public static void CreateNewMaterialGroup(string Name = null)
        {
            Console.WriteLine();
            MaterialGroup mg = new MaterialGroup();
            if (string.IsNullOrWhiteSpace(Name)) Name = null;
            mg.Name = Name;
            Program.CurrentCar.MaterialGroups.Add(mg);
            Console.WriteLine($"New group created: [{Program.CurrentCar.MaterialGroups.Count-1}]");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.movepoly", VerifyCarLoaded = true)]
        public static void MovePolygonsToGroup(int SourceGroupIndex, int TargetGroupIndex, int PolyStartIndex, int PolyCount)
        {
            Console.WriteLine();
            MaterialGroup source = Program.CurrentCar.MaterialGroups[SourceGroupIndex];
            MaterialGroup target = Program.CurrentCar.MaterialGroups[TargetGroupIndex];
            target.Polygons.AddRange(source.Polygons.Take(new Range(PolyStartIndex, PolyStartIndex + PolyCount)));
            source.Polygons.RemoveRange(PolyStartIndex, PolyCount);
            Console.WriteLine($"{PolyCount} polygons moved from group [{SourceGroupIndex}] to group [{TargetGroupIndex}].");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.setcolor", VerifyCarLoaded = true)]
        public static void SetGroupColor(int GroupIndex, byte R, byte G, byte B)
        {
            Console.WriteLine();
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            Program.CurrentCar.MaterialGroups[GroupIndex].SetColor(c);
            Console.WriteLine($"Color of group [{GroupIndex}] was changed to {c.ToStringAlt()}.");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.removeempty", VerifyCarLoaded = true)]
        public static void RemoveEmptyGroups()
        {
            Console.WriteLine();
            int countRemoved = 0;
            for(int i = Program.CurrentCar.MaterialGroups.Count - 1; i >= 0; i--)
            {
                if(Program.CurrentCar.MaterialGroups[i].Polygons.Count <= 0)
                {
                    Program.CurrentCar.MaterialGroups.RemoveAt(i);
                    countRemoved++;
                }
            }
            Console.WriteLine($"Empty groups removed: {countRemoved}.");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.setfs", VerifyCarLoaded = true)]
        public static void SetGroupFs(int GroupIndex, int FsValue)
        {
            Console.WriteLine();
            foreach(Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = FsValue;
            }
            Console.WriteLine($"Value of \'fs\' has been set to {FsValue} for {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
            Console.WriteLine();
        }

        [Command(CommandName = "car.setfs", VerifyCarLoaded = true)]
        public static void SetCarFs(int FsValue)
        {
            Console.WriteLine();
            int polycount = 0;
            foreach(MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach(Polygon p in mg.Polygons)
                {
                    p.Fs = FsValue;
                }
            }
            Console.WriteLine($"Value of \'fs\' has been set to {FsValue} for {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.removefs", VerifyCarLoaded = true)]
        public static void RemoveGroupFs(int GroupIndex)
        {
            Console.WriteLine();
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = null;
            }
            Console.WriteLine($"Value of \'fs\' has been removed to from {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
            Console.WriteLine();
        }

        [Command(CommandName = "car.removefs", VerifyCarLoaded = true)]
        public static void RemoveCarFs()
        {
            Console.WriteLine();
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.Fs = null;
                }
            }
            Console.WriteLine($"Value of \'fs\' has been removed from {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
            Console.WriteLine();
        }

        [Command(CommandName = "car.setoutline", VerifyCarLoaded = true)]
        public static void SetCarOutline(bool Value)
        {
            Console.WriteLine();
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.NoOutline = Value;
                }
            }
            Console.WriteLine($"Value of \'noOutline\' has been {(Value ? "added to" : "removed from")} {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.setoutline", VerifyCarLoaded = true)]
        public static void SetGroupOutline(int GroupIndex, bool Value)
        {
            Console.WriteLine();
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.NoOutline = Value;
            }
            Console.WriteLine($"Value of \'noOutline\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            Console.WriteLine();
        }

        [Command(CommandName = "car.groups.setgr", VerifyCarLoaded = true)]
        public static void SetGroupGr(int GroupIndex, int Value)
        {
            Console.WriteLine();
            foreach(Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Gr = Value;
            }
            Console.WriteLine($"Value of \'gr\' has been {(Value == 0 ? "removed from" : $"set to {Value} on")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            Console.WriteLine();
        }
    }

    public class ExitException : Exception
    {
        public ExitException() { }
    }
}

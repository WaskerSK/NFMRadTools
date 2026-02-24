using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [Command]
    public static class Commands
    {
        [Command(CommandName = "game.setcarfolder")]
        [Description("""
            Sets the folder from which to load and save cars.
            =Inputs=
            string Folder - The car folder path.
            """)]
        public static void SetCarFolder(string Folder)
        {
            if (!Directory.Exists(Folder))
            {
                Logger.Error("Directory not found.");
                return;
            }
            Program.Config.CarDirectory = Folder;
            Logger.Info($"Car directory was set to: \"{Folder}\".");
            string configPath = $"{Environment.CurrentDirectory}\\Config.json";
            try
            {
                Program.Config.Save(configPath);
            }
            catch(Exception e)
            {
                Logger.Warning("Failed to save config.");
                Logger.Error(e.ToString());
            }
        }

        [Command(CommandName = "game.carfolder")]
        [Description("Prints the current car folder from which cars are loaded and saved to.")]
        public static void PrintCarFolder()
        {
            if (string.IsNullOrWhiteSpace(Program.CarDirectory))
                Logger.Error("Car directory is not set.");
            else
                Logger.Info(Program.CarDirectory);
        }

        [Command(CommandName = "help")]
        [Description("""
            Lits all the available commands.
            =Inputs=
            int Page (optional) - The page number to display.
            """)]
        public static void Help(int Page = 1)
        {
            int commandsPerPage = 20;
            int commandCount = Program.CommandList.Count;
            double dPages = (double)commandCount / commandsPerPage;
            int pages = (int)double.Ceiling(dPages);
            if(Page < 1 || Page > pages)
            {
                Logger.Error($"Invalid page number: {Page}.");
            }
            Logger.Info("List of commands:");
            string pageMsg = $"[Page {Page}/{pages}]";
            Logger.Log(pageMsg, ConsoleColor.White);
            Page -= 1;
            StringBuilder sb = new StringBuilder();
            foreach (var cmdTuple in Program.CommandList.Skip(Page * commandsPerPage).Take(commandsPerPage))
            {
                sb.AppendLine(cmdTuple.Value.ToString());
            }
            if (sb[sb.Length-1] == '\n')
            {
                sb.Remove(sb.Length - 1, 1);
            }
            Logger.Log(sb.ToString(), ConsoleColor.Cyan);
            Logger.Log(pageMsg, ConsoleColor.White);
            Logger.Info("Type help <PageNumber> to view other pages.");
        }

        [Command(CommandName = "help.command")]
        [Description("""
            Shows help for a specific command.
            =Inputs=
            string Command (optional) - The command to show help for. If not provided shows help for itself.
            """)]
        public static void CommandHelp(string Command = "help.command")
        {
            if(string.IsNullOrWhiteSpace(Command))
            {
                Logger.Error("Missing command name.");
                return;
            }
            if(!Program.CommandList.TryGetValue(Command, out Command cmd))
            {
                Logger.Error($"Command \"{Command}\" was not found.");
                return;
            }
            if(string.IsNullOrWhiteSpace(cmd.Description))
            {
                Logger.Warning($"The command \"{Command}\" has no description.");
                return;
            }
            Logger.Info($"Description of command \"{Command}\":");
            Logger.Log(cmd.Description, ConsoleColor.Cyan);
        }

        [Command(CommandName = "exit")]
        [Description("Exits the application.")]
        public static void Exit()
        {
            Logger.Info("Exiting.");
            throw new ExitException();
        }

        [Command(CommandName = "load")]
        [Description("""
            Loads a car from the current car folder.
            =Inputs=
            string CarName - The name of the car to load. Note: Do not include a file extension.
            """)]
        public static void LoadCar(string CarName)
        {
            if (string.IsNullOrWhiteSpace(CarName))
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
            if (Program.CurrentCar is null)
            {
                Logger.Error("Car failed to load.");
            }
            else
            {
                Logger.Info("Car loaded.");
            }
        }

        [Command(CommandName = "print", VerifyCarLoaded = true)]
        [Description("Prints the code of the currently loaded car.")]
        public static void PrintCar()
        {
            Logger.Info("Car code:");
            Logger.Log(Program.CurrentCar.ToString(), ConsoleColor.White);
        }

        [Command(CommandName = "save", VerifyCarLoaded = true)]
        [Description("""
            Saves the currently loaded car into the current car folder.
            =Inputs=
            string CarName - The name of the car file name. Note: Do not include a file extension.
            """)]
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
        [Description("Lists all the material groups in the currently loaded car, displaying indexes and polycounts of the groups.")]
        public static void ListMaterialGroups()
        {
            Logger.Log("Material groups:", ConsoleColor.Green);
            for (int i = 0; i < Program.CurrentCar.MaterialGroups.Count; i++)
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
        [Description("""
            Creates a new material group.
            =Inputs=
            string Name (optional) - The name of the new material group. If not provided, a random name is generated.
            """)]
        public static void CreateNewMaterialGroup(string Name = null)
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                Logger.Warning("Invalid name provided. Generating random name.");
                Name = MaterialGroup.GetRandomGroupName();
            }
            MaterialGroup mg = new MaterialGroup();
            mg.Name = Name;
            Program.CurrentCar.MaterialGroups.Add(mg);
            Logger.Info($"New group created: [{Program.CurrentCar.MaterialGroups.Count - 1}] - {mg.Name}");
        }

        [Command(CommandName = "car.groups.movepoly", VerifyCarLoaded = true)]
        [Description("""
            Moves polygons from one material group to another.
            =Inputs=
            int SourceGroupIndex - Zero based index of the material group from which the polygons are being moved.
            int TargetGroupIndex - Zero based index of the material group to which the polygons are being moved.
            int PolyStartIndex - Zero based index of the first polygon to take.
            int PolyCount - Number of polygons to move.
            =Remarks=
            Use "car.groups.list" command to see group indexes and polycounts. 
            """)]
        public static void MovePolygonsToGroup(int SourceGroupIndex, int TargetGroupIndex, int PolyStartIndex, int PolyCount)
        {
            MaterialGroup source = Program.CurrentCar.MaterialGroups[SourceGroupIndex];
            MaterialGroup target = Program.CurrentCar.MaterialGroups[TargetGroupIndex];
            target.Polygons.AddRange(source.Polygons.Take(new Range(PolyStartIndex, PolyStartIndex + PolyCount)));
            source.Polygons.RemoveRange(PolyStartIndex, PolyCount);
            Logger.Info($"{PolyCount} polygons moved from group [{SourceGroupIndex}] to group [{TargetGroupIndex}].");
        }

        [Command(CommandName = "car.groups.setcolor", VerifyCarLoaded = true)]
        [Description("""
            Sets the color of all polygons in the given material group.
            =Inputs=
            int GroupIndex - Zero based index of the material group to which to apply the color.
            byte R - The red channel color value.
            byte G - The green channel color value.
            byte B - The blue channel color value.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void SetGroupColor(int GroupIndex, byte R, byte G, byte B)
        {
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            Program.CurrentCar.MaterialGroups[GroupIndex].SetColor(c);
            Logger.Info($"Color of group [{GroupIndex}] was changed to {c.ToString()}.");
        }

        [Command(CommandName = "car.groups.removeempty", VerifyCarLoaded = true)]
        [Description("Removes material groups containing 0 polygons.")]
        public static void RemoveEmptyGroups()
        {
            int countRemoved = 0;
            for (int i = Program.CurrentCar.MaterialGroups.Count - 1; i >= 0; i--)
            {
                if (Program.CurrentCar.MaterialGroups[i].Polygons.Count <= 0)
                {
                    Program.CurrentCar.MaterialGroups.RemoveAt(i);
                    countRemoved++;
                }
            }
            Logger.Info($"Empty groups removed: {countRemoved}.");
        }

        [Command(CommandName = "car.groups.setfs", VerifyCarLoaded = true)]
        [Description("""
            Sets the fs(x) value for all polygons within a material group.
            =Inputs=
            int GroupIndex - Zero based index of the group to which to apply the value.
            int FsValue - The x value of fs(x).
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void SetGroupFs(int GroupIndex, int FsValue)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = FsValue;
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
        }

        [Command(CommandName = "car.setfs", VerifyCarLoaded = true)]
        [Description("""
            Sets the fs(x) value for all polygons of the car.
            =Inputs=
            int FsValue - The x value of fs(x).
            """)]
        public static void SetCarFs(int FsValue)
        {
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.Fs = FsValue;
                }
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.removefs", VerifyCarLoaded = true)]
        [Description("""
            Removes the fs(x) value from all polygons within a material group.
            =Inputs=
            int GroupIndex - Zero based index of the group from which to remove the value.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void RemoveGroupFs(int GroupIndex)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = null;
            }
            Logger.Info($"Value of \'fs\' has been removed to from {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
        }

        [Command(CommandName = "car.removefs", VerifyCarLoaded = true)]
        [Description("Removes the fs(x) value from all polygons of the car.")]
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
        [Description("""
            Removes or adds noOutline attribute to all polygons of the car.
            =Inputs=
            bool Value - The bool value that indicates wether the car will have outlines.
            =Remarks=
            Use true if u want outline, otherwise false for no outline.
            """)]
        public static void SetCarOutline(bool Value)
        {
            int polycount = 0;
            foreach (MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.NoOutline = !Value;
                }
            }
            Logger.Info($"Value of \'noOutline\' has been {(!Value ? "added to" : "removed from")} {polycount} - Polygons across {Program.CurrentCar.MaterialGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.setoutline", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds noOutline attribute to all polygons within the given material group.
            =Input=
            int GroupIndex - Zero based index of the group.
            bool Value - The bool value that indicates wether the car will have outlines.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want outline, otherwise false for no outline.
            """)]
        public static void SetGroupOutline(int GroupIndex, bool Value)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.NoOutline = !Value;
            }
            Logger.Info($"Value of \'noOutline\' has been {(!Value ? "added to" : "removed from")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
        }

        [Command(CommandName = "car.groups.setgr", VerifyCarLoaded = true)]
        [Description("""
            Sets or removes the gr(x) value from a material group.
            =Inputs=
            int GroupIndex = Zero based index of the group.
            int Value - The x value of the gr(x).
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use value of 0 to remove gr(x) attribute.
            """)]
        public static void SetGroupGr(int GroupIndex, int Value)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Gr = Value;
            }
            Logger.Info($"Value of \'gr\' has been {(Value == 0 ? "removed from" : $"set to {Value} on")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
        }

        [Command(CommandName = "car.colors.get", VerifyCarLoaded = true)]
        [Description("""
            Prints the given car color.
            =Inputs=
            int CarColorNumber - The number of color which to print.
            =Remarks=
            Use value of 1 or 2 to print 1st color or 2nd color.
            """)]
        public static void GetCarColor(int CarColorNumber)
        {
            if(CarColorNumber < 1 ||  CarColorNumber > 2)
            {
                Logger.Error($"Invalid color number \'{CarColorNumber}\'.");
                Logger.Info("NFM car has 2 colors, 1stColor '1' and 2ndColor '2'.");
                return;
            }
            if(CarColorNumber == 1)
            {
                if (!Program.CurrentCar.FirstColor.HasValue)
                {
                    Logger.Log("1st color is not defined.", ConsoleColor.DarkYellow);
                    return;
                }
                Logger.Log($"1stColor({Program.CurrentCar.FirstColor.Value})", ConsoleColor.Cyan);
                return;
            }
            else
            {
                if(!Program.CurrentCar.SecondColor.HasValue)
                {
                    Logger.Log("2nd color is not defined.", ConsoleColor.DarkYellow);
                    return;
                }
                Logger.Log($"2ndColor({Program.CurrentCar.SecondColor.Value})", ConsoleColor.Cyan);
                return;
            }
        }

        [Command(CommandName = "car.colors.set", VerifyCarLoaded = true)]
        [Description("""
            Sets the color of the given dynamic car color.
            =Inputs=
            int CarColorNumber = The number of color which to change.
            byte R - The red channel color value.
            byte G - The green channel color value.
            byte B - The blue channel color value.
            =Remarks=
            Use value of 1 or 2 to change 1st color or 2nd color.
            """)]
        public static void SetCarColor(int CarColorNumber, byte R, byte G, byte B)
        {
            if (CarColorNumber < 1 || CarColorNumber > 2)
            {
                Logger.Error($"Invalid color number \'{CarColorNumber}\'.");
                Logger.Info("NFM car has 2 colors, 1stColor '1' and 2ndColor '2'.");
                return;
            }
            if (CarColorNumber == 1)
            {
                Program.CurrentCar.FirstColor = new Color(R, G, B);
                Logger.Info($"1stColor was changed to {Program.CurrentCar.FirstColor.Value}");
                return;
            }
            else
            {
                Program.CurrentCar.SecondColor = new Color(R, G, B);
                Logger.Info($"2ndColor was changed to {Program.CurrentCar.SecondColor.Value}");
                return;
            }
        }

        [Command(CommandName = "car.colors.auto", VerifyCarLoaded = true)]
        [Description("Scans the colors of all polygons and sets 1st and 2nd color based on the 2 most common colors.")]
        public static void AutoSetCarColors()
        {
            Logger.Info("Calculating colors.");
            Dictionary<Color, int> d = new Dictionary<Color, int>();
            foreach(MaterialGroup mg in Program.CurrentCar.MaterialGroups)
            {
                foreach(Polygon p in mg.Polygons)
                {
                    Color c = p.Color;
                    if(d.TryGetValue(c, out int count))
                    {
                        d[c] = count + 1;
                    }
                    else
                    {
                        d.Add(c, 1);
                    }
                }
            }
            Color c1 = new Color();
            int c1Count = 0;
            Color c2 = new Color();
            int c2Count = 0;
            foreach(KeyValuePair<Color, int> entry in d)
            {
                if(entry.Value > c1Count)
                {
                    c2 = c1;
                    c2Count = c1Count;
                    c1 = entry.Key;
                    c1Count = entry.Value;
                    continue;
                }
                if(entry.Value > c2Count)
                {
                    c2 = entry.Key;
                    c2Count = entry.Value;
                    continue;
                }
            }
            Program.CurrentCar.FirstColor = c1;
            Program.CurrentCar.SecondColor = c2;
            Logger.Info($"First color was set to ({c1}) - {c1Count} polygons, Second color was set to ({c2}) - {c2Count} polygons.");
        }
        //public static void ImportObj()
    }
}

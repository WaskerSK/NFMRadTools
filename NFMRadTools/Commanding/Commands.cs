using NFMRadTools.Editing;
using NFMRadTools.Editing.Presets;
using NFMRadTools.Utilities;
using NFMRadTools.Utilities.Importing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [Command]
    public static class Commands
    {
        [Command(CommandName = "game.carfolder")]
        [Description("""
            Prints or sets the folder from which cars are loaded or saved to.
            =Inputs=
            string Folder (optional) - The new car folder path.
            =Remarks=
            Do not provide a folder to print the current car folder path.
            """)]
        public static void GameCarFolder(string Folder = null)
        {
            if(string.IsNullOrWhiteSpace(Folder))
            {
                if (string.IsNullOrWhiteSpace(Program.CarDirectory))
                    Logger.Error("Car directory is not set.");
                else
                    Logger.Info(Program.CarDirectory);
                return;
            }
            if (!Directory.Exists(Folder))
            {
                Logger.Error("Directory not found.");
                return;
            }
            Program.Config.CarDirectory = Folder;
            Logger.Info($"Car directory was set to: \"{Folder}\".");
            string configPath = Path.Combine(Environment.CurrentDirectory, "Config.json");
            try
            {
                Program.Config.Save(configPath);
            }
            catch (Exception e)
            {
                Logger.Warning("Failed to save config.");
                Logger.Error(e.ToString());
            }
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
            Logger.Info("Optional parameters can be skipped with \'-\'");
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
            Logger.Info("Type help PageNumber to view other pages.");
        }

        [Command(CommandName = "?")]
        [Description("""
            Shows help for a specific command.
            =Inputs=
            string Command (optional) - The command to show help for. If not provided shows help for itself.
            =Usage=
            car.groups.list? - shows help for the command car.groups.list.
            """)]
        public static void CommandHelp(string Command = "?")
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
            =Remarks=
            You can put full path to a car file as the car name if u wanna load from external folder.
            Note: if using full path to car file you need to include an extension.
            """)]
        public static void LoadCar(string CarName)
        {
            if (string.IsNullOrWhiteSpace(CarName))
            {
                Logger.Error("Invalid car file name.");
                return;
            }
            string filePath = null;
            if (!File.Exists(CarName))
                filePath = Path.Combine(Program.CarDirectory, $"{CarName}{Program.NFMCarExtension}");
            else
                filePath = CarName;
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
            Program.CurrentCar.LoadedFromFile = filePath;
            Logger.Info("Car loaded.");
        }

        [Command(CommandName = "reload")]
        [Description("Reloads the currently loaded car.")]
        public static void Reload()
        {
            if(!File.Exists(Program.CurrentCar.LoadedFromFile))
            {
                Logger.Error($"Failed to reload car, the file \"{Program.CurrentCar.LoadedFromFile}\" no longer exists.");
                return;
            }
            LoadCar(Program.CurrentCar.LoadedFromFile);
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
            string CarName (optional) - The name of the car file name. Note: Do not include a file extension.
            """)]
        public static void Save(string CarName = null)
        {
            bool carNameIsPath = false;
            if (string.IsNullOrWhiteSpace(CarName))
            {
                if(string.IsNullOrWhiteSpace(Program.CurrentCar.LoadedFromFile))
                {
                    Logger.Error("No car name specified.");
                    return;
                }
                CarName = Program.CurrentCar.LoadedFromFile;
                if(Path.HasExtension(CarName))
                {
                    CarName = Path.ChangeExtension(CarName, "rad");
                    carNameIsPath = true;
                }
                else carNameIsPath = false;
            }
            if(!carNameIsPath)
            {
                if (CarName.AsSpan().ContainsAny(Path.GetInvalidFileNameChars()))
                {
                    Logger.Error("Invalid car file name.");
                    return;
                }
            }
            string filePath = null;
            if(carNameIsPath) filePath = CarName;
            else filePath = Path.Combine(Program.CarDirectory, $"{CarName}{Program.NFMCarExtension}");
            File.WriteAllText(filePath, Program.CurrentCar.ToString());
            Program.CurrentCar.LoadedFromFile = filePath;
            Logger.Info($"Car saved: \"{filePath}\".");
        }

        [Command(CommandName = "car.groups.list", VerifyCarLoaded = true)]
        [Description("Lists all the poly groups in the currently loaded car, displaying indexes and polycounts of the groups.")]
        public static void ListPolyGroups()
        {
            Logger.Log("Poly groups:", ConsoleColor.Green);
            for (int i = 0; i < Program.CurrentCar.PolyGroups.Count; i++)
            {
                Console.Write("[");
                Console.Write(i);
                Console.Write("] - ");
                Console.Write(Program.CurrentCar.PolyGroups[i].Name);
                Console.Write(" - (");
                Console.Write(Program.CurrentCar.PolyGroups[i].Polygons.Count);
                Console.Write(" Polygons) | Mode: ");
                Console.WriteLine(Program.CurrentCar.PolyGroups[i].Mode);
            }
        }

        [Command(CommandName = "car.groups.new", VerifyCarLoaded = true)]
        [Description("""
            Creates a new poly group.
            =Inputs=
            string Name (optional) - The name of the new poly group. If not provided, a random name is generated.
            PolyGroupMode Mode (optional) - The mode of the poly group that dictates wether the polygons represent a car body or a custom wheel.
            """)]
        public static void CreateNewPolyGroup(string Name = null, PolyGroupMode Mode = PolyGroupMode.Normal)
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                Logger.Warning("Invalid name provided. Generating random name.");
                Name = RandomName.Get();
            }
            PolyGroup g = new PolyGroup();
            g.Name = Name;
            g.Mode = Mode;
            Program.CurrentCar.PolyGroups.Add(g);
            Logger.Info($"New group created: [{Program.CurrentCar.PolyGroups.Count - 1}] - {g.Name} - Mode: {Mode}");
        }

        [Command(CommandName = "car.groups.cwconvert", VerifyCarLoaded = true)]
        [Description("""
            Changes current custom wheels to a different type of custom wheels.
            =Inputs=
            PolyGroupMode NewMode - The new custom wheel mode.
            """)]
        public static void ChangeCustomWheelMode(PolyGroupMode NewMode)
        {
            if(NewMode > PolyGroupMode.G6Wheel)
            {
                Logger.Error("Invalid mode value.");
                return;
            }
            if(NewMode == PolyGroupMode.Normal)
            {
                Logger.Error("Normal mode is not valid for custom wheels.");
                return;
            }
            NFMCar car = Program.CurrentCar;
            if(!car.PolyGroups.Any(x => x.Mode != PolyGroupMode.Normal))
            {
                Logger.Warning("No custom wheel groups found. No changes were made.");
                return;
            }
            PolyGroupMode oldMode = car.PolyGroups.First(x => x.Mode != PolyGroupMode.Normal).Mode;
            if(car.PolyGroups.Where(x => x.Mode != PolyGroupMode.Normal).Any(x => x.Mode != oldMode))
            {
                Logger.Error("Changing between multiple wheel modes is not supported.");
                return;
            }
            List<PolyGroup> groups = new List<PolyGroup>(car.PolyGroups.Where(x => x.Mode != PolyGroupMode.Normal));
            Debug.Assert(groups.Count > 0);
            foreach(PolyGroup g in groups)
            {
                car.PolyGroups.Remove(g);
            }
            switch(oldMode)
            {
                case PolyGroupMode.DragShotWheel:
                    {
                        double offset = car.DragShotWheelDefinition.Depth;
                        offset /= 2.0;
                        foreach(PolyGroup g in groups)
                        {
                            foreach(Polygon p in g.Polygons)
                            {
                                for(int i = 0; i < p.Vertices.Count; i++)
                                {
                                    p.Vertices[i] = (Vertex)((Vector3D)p.Vertices[i] - new Vector3D(offset, 0.0, 0.0));
                                }
                            }
                        }
                        switch(NewMode)
                        {
                            case PolyGroupMode.PhyrexianWheel:
                                {
                                    foreach(PolyGroup g in groups)
                                    {
                                        g.Mode = PolyGroupMode.PhyrexianWheel;
                                        g.CustomWheelIndex = 0;
                                    }
                                    IEnumerable<PolyGroup> groupsToDuplicate = groups.Take(groups.Count);
                                    int i = 1;
                                    foreach(Wheel w in car.Wheels.Skip(1))
                                    {
                                        foreach(PolyGroup g in groupsToDuplicate)
                                        {
                                            PolyGroup clone = g.Duplicate();
                                            clone.CustomWheelIndex = i;
                                            if(w.X < 0)
                                                clone.Mirror(Axis.X, false);
                                            groups.Add(clone);
                                        }
                                        i++;
                                    }
                                    if (car.Wheels[0].X < 0)
                                    {
                                        foreach (PolyGroup g in groupsToDuplicate)
                                        {
                                            g.Mirror(Axis.X, false);
                                        }
                                    }
                                    break;
                                }
                            case PolyGroupMode.G6Wheel:
                                {
                                    foreach (PolyGroup g in groups)
                                    {
                                        g.Mode = PolyGroupMode.G6Wheel;
                                        g.CustomWheelIndex = 0;
                                        g.Mirror(Axis.X, false);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case PolyGroupMode.PhyrexianWheel:
                    {
                        switch(NewMode)
                        {
                            case PolyGroupMode.DragShotWheel:
                                {
                                    groups.RemoveAll(x => x.CustomWheelIndex != 0);
                                    Cylinder c = Cylinder.GetFromPolyGroups(groups);
                                    car.DragShotWheelDefinition.Depth = int.Abs(c.Width.RoundToInt());
                                    car.DragShotWheelDefinition.Radius = c.Radius.RoundToInt();
                                    double offset = car.DragShotWheelDefinition.Depth;
                                    offset /= 2.0;
                                    bool mirror = car.Wheels[0].X < 0;
                                    foreach (PolyGroup g in groups)
                                    {
                                        g.Mode = PolyGroupMode.DragShotWheel;
                                        if(mirror) g.Mirror(Axis.X, false);

                                        foreach (Polygon p in g.Polygons)
                                        {
                                            for (int i = 0; i < p.Vertices.Count; i++)
                                            {
                                                Vector3D v = (Vector3D)p.Vertices[i];
                                                p.Vertices[i] = (Vertex)(v + new Vector3D(offset, 0.0, 0.0));
                                            }
                                        }
                                    }
                                    foreach(Wheel w in car.Wheels)
                                    {
                                        if (w.X < 0) w.Width *= -1;
                                    }
                                    break;
                                }
                            case PolyGroupMode.G6Wheel:
                                {
                                    IEnumerable<IGrouping<int, PolyGroup>> wheelGroups = groups.GroupBy(x => x.CustomWheelIndex);
                                    List<IGrouping<int, PolyGroup>> uniqueModels = new List<IGrouping<int, PolyGroup>>();
                                    Dictionary<int, List<int>> wheelMap = new Dictionary<int, List<int>>();
                                    foreach(IGrouping<int, PolyGroup> wheelModel in wheelGroups)
                                    {
                                        IGrouping<int, PolyGroup> match = uniqueModels.FirstOrDefault(x =>
                                        {
                                            if (x.Sum(g => g.Polygons.Sum(p => p.Vertices.Count)) != wheelModel.Sum(g => g.Polygons.Sum(p => p.Vertices.Count)))
                                                return false;
                                            Cylinder cA = Cylinder.GetFromPolyGroups(wheelModel);
                                            Cylinder cB = Cylinder.GetFromPolyGroups(x);
                                            IOrderedEnumerable<Vertex> Unique = x.SelectMany(g => g.Polygons).SelectMany(p => p.Vertices).Order(VertexComparer.Default);
                                            IEnumerable<Vertex> newVert = wheelModel.SelectMany(g => g.Polygons).SelectMany(p => p.Vertices);
                                            if (int.Sign(car.Wheels[x.Key].X) != int.Sign(car.Wheels[wheelModel.Key].X))
                                                newVert = newVert.Convert(x => (Vertex)((Vector3D)x * new Vector3D(-1.0, 1.0, 1.0)));
                                            IOrderedEnumerable<Vertex> New = newVert.Order(VertexComparer.Default);
                                            Wheel aWheel = car.Wheels[x.Key];
                                            Wheel bWheel = car.Wheels[wheelModel.Key];
                                            return Unique.SequenceEqual(New, (a, b) =>
                                            {
                                                int Aradius = cA.Radius.RoundToInt();
                                                int Awidth = cA.Width.RoundToInt();
                                                int Bradius = cB.Radius.RoundToInt();
                                                int Bwidth = cB.Width.RoundToInt();
                                                double RadiusRatio = (double)Aradius / Bradius;
                                                double WidthRatio = (double)Awidth / Bwidth;
                                                return a == (Vertex)((Vector3D)b * new Vector3D(WidthRatio, RadiusRatio, RadiusRatio));
                                            });
                                        });
                                        if (match is null)
                                        {
                                            uniqueModels.Add(wheelModel);
                                            wheelMap.Add(wheelModel.Key, new List<int>() { wheelModel.Key});
                                            continue;
                                        }
                                        wheelMap[match.Key].Add(wheelModel.Key);
                                    }
                                    groups.Clear();
                                    for(int i = 0; i < uniqueModels.Count; i++)
                                    {
                                        foreach(PolyGroup g in uniqueModels[i])
                                        {
                                            g.Mode = PolyGroupMode.G6Wheel;
                                            g.CustomWheelIndex = i;
                                            foreach(int wheelIndex in wheelMap[uniqueModels[i].Key])
                                            {
                                                car.Wheels[wheelIndex].WheelModel = i;
                                            }
                                        }
                                    }
                                    foreach (IGrouping<int, PolyGroup> grouping in uniqueModels)
                                    {
                                        if (car.Wheels[grouping.Key].X > 0)
                                        {
                                            foreach (PolyGroup g in grouping)
                                                g.Mirror(Axis.X, false);
                                        }
                                        groups.AddRange(grouping);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case PolyGroupMode.G6Wheel: throw new NotImplementedException();
            }
            car.PolyGroups.AddRange(groups);
            Logger.Info($"Custom wheels were changed from {oldMode} to {NewMode}.");
        }

        [Command(CommandName = "car.groups.movepoly", VerifyCarLoaded = true)]
        [Description("""
            Moves polygons from one poly group to another.
            =Inputs=
            int SourceGroupIndex - Zero based index of the poly group from which the polygons are being moved.
            int TargetGroupIndex - Zero based index of the poly group to which the polygons are being moved.
            int PolyStartIndex - Zero based index of the first polygon to take.
            int PolyCount - Number of polygons to move.
            =Remarks=
            Use "car.groups.list" command to see group indexes and polycounts. 
            """)]
        public static void MovePolygonsToGroup(int SourceGroupIndex, int TargetGroupIndex, int PolyStartIndex, int PolyCount)
        {
            if(SourceGroupIndex < 0 || SourceGroupIndex >= Program.CurrentCar.PolyGroups.Count)
            {
                Logger.Error("Invalid source group index.");
                return;
            }
            if(TargetGroupIndex < 0 || TargetGroupIndex >= Program.CurrentCar.PolyGroups.Count)
            {
                Logger.Error("Invalid target group index.");
                return;
            }
            PolyGroup source = Program.CurrentCar.PolyGroups[SourceGroupIndex];
            PolyGroup target = Program.CurrentCar.PolyGroups[TargetGroupIndex];
            if (PolyStartIndex < 0 || PolyStartIndex >= source.Polygons.Count)
            {
                Logger.Error("Invalid poly start index.");
                return;
            }
            PolyCount = (int)uint.Clamp((uint)PolyCount, 0, (uint)source.Polygons.Count - (uint)PolyStartIndex);
            target.AddPolygons(source.Polygons.Take(new Range(PolyStartIndex, PolyStartIndex + PolyCount)));
            source.RemoveRange(PolyStartIndex, PolyCount);
            Logger.Info($"{PolyCount} polygons moved from group [{SourceGroupIndex}] - {source.Name} to group [{TargetGroupIndex}] {target.Name}.");
        }

        [Command(CommandName = "car.groups.setcolor", VerifyCarLoaded = true)]
        [Description("""
            Sets the color of all polygons in the given poly group.
            =Inputs=
            int[] GroupIndexes - Zero based indexes of the poly groups to which to apply the color separated by ;.
            byte R - The red channel color value.
            byte G - The green channel color value.
            byte B - The blue channel color value.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void SetGroupColor(int[] GroupIndexes, byte R, byte G, byte B)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            foreach(int GroupIndex in GroupIndexes)
            {
                Program.CurrentCar.PolyGroups[GroupIndex].SetColor(c);
                Logger.Info($"Color of group [{GroupIndex}] was changed to {c.ToString()}.");
            }
        }

        [Command(CommandName = "car.groups.removeempty", VerifyCarLoaded = true)]
        [Description("Removes poly groups containing 0 polygons.")]
        public static void RemoveEmptyGroups()
        {
            int countRemoved = 0;
            for (int i = Program.CurrentCar.PolyGroups.Count - 1; i >= 0; i--)
            {
                if (Program.CurrentCar.PolyGroups[i].Polygons.Count <= 0)
                {
                    Program.CurrentCar.PolyGroups.RemoveAt(i);
                    countRemoved++;
                }
            }
            Logger.Info($"Empty groups removed: {countRemoved}.");
        }

        [Command(CommandName = "car.groups.setfs", VerifyCarLoaded = true)]
        [Description("""
            Sets the fs(x) value for all polygons within a poly group.
            =Inputs=
            int[] GroupIndexes - Zero based indexes of the groups to which to apply the value separated by ;.
            int FsValue - The x value of fs(x).
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void SetGroupFs(int[] GroupIndexes, int FsValue)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach(int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Fs = FsValue;
                }
                Logger.Info($"Value of \'fs\' has been set to {FsValue} for {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
            }
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
            foreach (PolyGroup mg in Program.CurrentCar.PolyGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.Fs = FsValue;
                }
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {polycount} - Polygons across {Program.CurrentCar.PolyGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.removefs", VerifyCarLoaded = true)]
        [Description("""
            Removes the fs(x) value from all polygons within a poly group.
            =Inputs=
            int[] GroupIndexes - Zero based indexes of the groups from which to remove the value separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void RemoveGroupFs(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Fs = null;
                }
                Logger.Info($"Value of \'fs\' has been removed to from {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
            }
        }

        [Command(CommandName = "car.removefs", VerifyCarLoaded = true)]
        [Description("Removes the fs(x) value from all polygons of the car.")]
        public static void RemoveCarFs()
        {
            int polycount = 0;
            foreach (PolyGroup mg in Program.CurrentCar.PolyGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.Fs = null;
                }
            }
            Logger.Info($"Value of \'fs\' has been removed from {polycount} - Polygons across {Program.CurrentCar.PolyGroups.Count} groups.");
        }

        [Command(CommandName = "car.setoutline", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds noOutline attribute to all polygons of the car.
            =Inputs=
            bool Value - The bool value that indicates wether the car will have outlines.
            =Remarks=
            Use true if you want outline, otherwise false for no outline.
            """)]
        public static void SetCarOutline(bool Value)
        {
            int polycount = 0;
            foreach (PolyGroup mg in Program.CurrentCar.PolyGroups)
            {
                polycount += mg.Polygons.Count;
                foreach (Polygon p in mg.Polygons)
                {
                    p.NoOutline = !Value;
                }
            }
            Logger.Info($"Value of \'noOutline\' has been {(!Value ? "added to" : "removed from")} {polycount} - Polygons across {Program.CurrentCar.PolyGroups.Count} groups.");
        }

        [Command(CommandName = "car.groups.setoutline", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds noOutline attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have outlines.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want outline, otherwise false for no outline.
            """)]
        public static void SetGroupOutline(int[] GroupIndexes, bool Value)
        {
            if(GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach(int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.NoOutline = !Value;
                }
                Logger.Info($"Value of \'noOutline\' has been {(!Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setglass", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds glass attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have glass attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want glass attribute, otherwise false for no glass.
            """)]
        public static void SetGroupGlass(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Glass = Value;
                }
                Logger.Info($"Value of \'glass\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setglasstint", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds glassTint attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have glassTint attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want glassTint attribute, otherwise false for no glassTint.
            """)]
        public static void SetGroupGlassTint(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.GlassTint = Value;
                }
                Logger.Info($"Value of \'glassTint\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setlight", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds light attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have light attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want light attribute, otherwise false for no light.
            """)]
        public static void SetGroupLight(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Light = Value;
                }
                Logger.Info($"Value of \'light\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setlightF", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds lightF attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have lightF attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want lightF attribute, otherwise false for no lightF.
            """)]
        public static void SetGroupLightFront(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.LightFront = Value;
                }
                Logger.Info($"Value of \'lightF\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setlightB", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds lightB attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have lightB attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want lightB attribute, otherwise false for no lightB.
            """)]
        public static void SetGroupLightBack(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.LightBack = Value;
                }
                Logger.Info($"Value of \'lightB\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setlightR", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds lightR attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have lightR attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want lightR attribute, otherwise false for no lightR.
            """)]
        public static void SetGroupLightReverse(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.LightReverse = Value;
                }
                Logger.Info($"Value of \'lightR\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setlightbrake", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds lightBrake attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have lightBrake attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want lightBrake attribute, otherwise false for no lightBrake.
            """)]
        public static void SetGroupLightBrake(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.LightBrake = Value;
                }
                Logger.Info($"Value of \'lightBrake\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setdayonly", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds dayOnly attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have dayOnly attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want dayOnly attribute, otherwise false for no dayOnly.
            """)]
        public static void SetGroupDayOnly(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.DayOnly = Value;
                }
                Logger.Info($"Value of \'dayOnly\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setnightonly", VerifyCarLoaded = true)]
        [Description("""
            Removes or adds nightOnly attribute to all polygons within the given poly group.
            =Input=
            int[] GroupIndexes - Zero based array of index of the groups separated by ;.
            bool Value - The bool value that indicates wether the polygons will have nightOnly attribute.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use true if u want nightOnly attribute, otherwise false for no nightOnly.
            """)]
        public static void SetGroupNightOnly(int[] GroupIndexes, bool Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.NightOnly = Value;
                }
                Logger.Info($"Value of \'nightOnly\' has been {(Value ? "added to" : "removed from")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.setgr", VerifyCarLoaded = true)]
        [Description("""
            Sets or removes the gr(x) value from a poly group.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            int Value - The x value of the gr(x).
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            Use value of 0 to remove gr(x) attribute.
            """)]
        public static void SetGroupGr(int[] GroupIndexes, int Value)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = Value;
                }
                Logger.Info($"Value of \'gr\' has been {(Value == 0 ? "removed from" : $"set to {Value} on")} {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            } 
        }

        [Command(CommandName = "car.groups.makewheelwell", VerifyCarLoaded = true)]
        [Description("""
            Sets gr(40) and fs(0) value on a poly group.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void MakeGroupWheelWell(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = 40;
                    p.Fs = 0;
                }
                Logger.Info($"Following values \"gr(40) fs(0)\" have been set on {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.makecoloredoutline", VerifyCarLoaded = true)]
        [Description("""
            Sets gr(-10) value on a poly group.
            =Effect=
            Renders the polygon's outline in the same colour as the polygon.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void MakeGroupColoredOutlines(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = -10;
                }
                Logger.Info($"Following value \"gr(-10)\" have been set on {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.makeinvisible", VerifyCarLoaded = true)]
        [Description("""
            Sets gr(-13) value on a poly group.
            =Effect=
            Renders the polygon invisible, just showing it's shadow.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void MakeGroupInvisible(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = -13;
                }
                Logger.Info($"Following value \"gr(-13)\" have been set on {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.makedamaged", VerifyCarLoaded = true)]
        [Description("""
            Sets gr(-15) value on a poly group.
            =Effect=
            Renders the polygon as if it has been damaged.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void MakeGroupDamaged(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = -15;
                }
                Logger.Info($"Following value \"gr(-15)\" have been set on {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
        }

        [Command(CommandName = "car.groups.makeelectricoutline", VerifyCarLoaded = true)]
        [Description("""
            Sets gr(-18) value on a poly group.
            =Effect=
            Renders the polygon's outline with an electrifying effect.
            =Inputs=
            int[] GroupIndexes = Zero based indexes of the groups separated by ;.
            =Remarks=
            Use "car.groups.list" command to see group indexes.
            """)]
        public static void MakeGroupElectrictOutline(int[] GroupIndexes)
        {
            if (GroupIndexes.Length <= 0)
            {
                Logger.Warning("No indexes provided. No groups were affected.");
                return;
            }
            foreach (int GroupIndex in GroupIndexes)
            {
                foreach (Polygon p in Program.CurrentCar.PolyGroups[GroupIndex].Polygons)
                {
                    p.Gr = -18;
                }
                Logger.Info($"Following value \"gr(-18)\" have been set on {Program.CurrentCar.PolyGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
            }
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
        [Description("""
            Scans the colors of all polygons and sets 1st and 2nd color based on the 2 most common colors based on given mode.
            =Inputs=
            AutoColoringMode Mode (optional) - Mode which determins how the algorythm decides what colors are used the most.
            =Remarks=
            Available coloring modes:
            Polygons - Scans colors based on number of polygons.
            Vertices - Scans colors based on number of vertices.
            Bounds - Scans colors based on the volume of the bounding box that encapsulates all polygons of given color.
            Edge - Scanes colors based on length of edges.
            Surface - Currently not implemented. Scans colors based on surface area of polygons.
            """)]
        public static void AutoSetCarColors(AutoColoringMode Mode = AutoColoringMode.Polygons)
        {
            Logger.Info("Calculating colors.");
            IEnumerable<PolyGroup> groups = Program.CurrentCar.PolyGroups.Where(x => x.Mode == PolyGroupMode.Normal);
            if(!groups.Any()) groups = Program.CurrentCar.PolyGroups;
            switch(Mode)
            {
                case AutoColoringMode.Polygons:
                case AutoColoringMode.Vetrices:
                    {
                        Dictionary<Color, int> d = new Dictionary<Color, int>();
                        foreach (PolyGroup g in groups)
                        {
                            foreach (Polygon p in g.Polygons)
                            {
                                Color c = p.Color;
                                if (d.TryGetValue(c, out int count))
                                {
                                    d[c] = count + (Mode == AutoColoringMode.Polygons ? 1 : p.Vertices.Count);
                                }
                                else
                                {
                                    d.Add(c, (Mode == AutoColoringMode.Polygons ? 1 : p.Vertices.Count));
                                }
                            }
                        }
                        Color c1 = new Color();
                        int c1Count = 0;
                        Color c2 = new Color();
                        int c2Count = 0;
                        foreach (KeyValuePair<Color, int> entry in d)
                        {
                            if (entry.Value > c1Count)
                            {
                                c2 = c1;
                                c2Count = c1Count;
                                c1 = entry.Key;
                                c1Count = entry.Value;
                                continue;
                            }
                            if (entry.Value > c2Count)
                            {
                                c2 = entry.Key;
                                c2Count = entry.Value;
                                continue;
                            }
                        }
                        Program.CurrentCar.FirstColor = c1;
                        Program.CurrentCar.SecondColor = c2;
                        Logger.Info($"First color was set to ({c1}) - {c1Count} {(Mode == AutoColoringMode.Polygons ? "polygons" : "vertices")}, Second color was set to ({c2}) - {c2Count} {(Mode == AutoColoringMode.Polygons ? "polygons" : "vertices")}.");
                        return;
                    }
                case AutoColoringMode.Bounds:
                    {
                        Dictionary<Color, List<BoundingBox>> d = new Dictionary<Color, List<BoundingBox>>();
                        foreach(PolyGroup g in groups)
                        {
                            foreach(Polygon p in g.Polygons)
                            {
                                BoundingBox b = BoundingBox.Make(p);
                                if(d.TryGetValue(p.Color, out List<BoundingBox> bl))
                                {
                                    bl.Add(b);
                                }
                                else
                                {
                                    List<BoundingBox> l = new List<BoundingBox>();
                                    l.Add(b);
                                    d.Add(p.Color, l);
                                }
                            }
                        }
                        IEnumerable<KeyValuePair<Color, ulong>> volumes = d.Select(x =>
                        {
                            List<BoundingBox> boxes = x.Value;
                            ulong volume = 0;
                            if (boxes is null) volume = 0;
                            else
                            {
                                for (int i = 0; i < boxes.Count; i++)
                                {
                                    BoundingBox box = boxes[i];
                                    volume = box.Volume;
                                    for(int j = i - 1;  j >= 0; j--)
                                    {
                                        BoundingBox other = boxes[j];
                                        volume -= box.GetIntersectingVolume(other);
                                    }
                                }
                            }
                            return new KeyValuePair<Color, ulong>(x.Key, volume);
                        });
                        Color c1 = new Color();
                        ulong c1Volume = 0;
                        Color c2 = new Color();
                        ulong c2Volume = 0;
                        foreach (KeyValuePair<Color, ulong> entry in volumes)
                        {
                            if (entry.Value > c1Volume)
                            {
                                c2 = c1;
                                c2Volume = c1Volume;
                                c1 = entry.Key;
                                c1Volume = entry.Value;
                                continue;
                            }
                            if (entry.Value > c2Volume)
                            {
                                c2 = entry.Key;
                                c2Volume = entry.Value;
                                continue;
                            }
                        }
                        Program.CurrentCar.FirstColor = c1;
                        Program.CurrentCar.SecondColor = c2;
                        Logger.Info($"First color was set to ({c1}) - {c1Volume} units of volume, Second color was set to ({c2}) - {c2Volume} units of volume.");
                        return;
                    }
                case AutoColoringMode.Edge:
                    {
                        Dictionary<Color, double> d = new Dictionary<Color, double>();
                        foreach (PolyGroup g in groups)
                        {
                            foreach (Polygon p in g.Polygons)
                            {
                                Color c = p.Color;
                                double edgeLength = 0.0;
                                for(int i = 0; i < p.Vertices.Count; i++)
                                {
                                    Vector3D vCurrent = (Vector3D)p.Vertices[i];
                                    Vector3D vNext = (Vector3D)p.Vertices[(i + 1) % p.Vertices.Count];
                                    edgeLength += Vector3D.Length(Vector3D.Distance(vCurrent, vNext));
                                }
                                if (d.TryGetValue(c, out double len))
                                {
                                    d[c] = len + edgeLength;
                                }
                                else
                                {
                                    d.Add(c, edgeLength);
                                }
                            }
                        }
                        Color c1 = new Color();
                        double c1Len = 0;
                        Color c2 = new Color();
                        double c2Len = 0;
                        foreach (KeyValuePair<Color, double> entry in d)
                        {
                            if (entry.Value > c1Len)
                            {
                                c2 = c1;
                                c2Len = c1Len;
                                c1 = entry.Key;
                                c1Len = entry.Value;
                                continue;
                            }
                            if (entry.Value > c2Len)
                            {
                                c2 = entry.Key;
                                c2Len = entry.Value;
                                continue;
                            }
                        }
                        Program.CurrentCar.FirstColor = c1;
                        Program.CurrentCar.SecondColor = c2;
                        Logger.Info($"First color was set to ({c1}) - {c1Len} units, Second color was set to ({c2}) - {c2Len} units.");
                        return;
                    }
                case AutoColoringMode.SurfaceArea:
                    {
                        /*Dictionary<Color, double> d = new Dictionary<Color, double>();
                        foreach(PolyGroup g in groups)
                        {
                            foreach(Polygon p in g.Polygons)
                            {

                            }
                        }*/
                        throw new NotImplementedException("Surface area calculation is not yet implemented.");
                    }
            }
        }
        [Command(CommandName = "game.importfolder")]
        [Description("""
            Prints or sets the folder from which cars are imported.
            =Inputs=
            string Folder (optional) - The new import folder path.
            =Remarks=
            Do not provide a folder to print the current import folder path.
            """)]
        public static void GameImportFolder(string Folder = null)
        {
            if (string.IsNullOrWhiteSpace(Folder))
            {
                if (string.IsNullOrWhiteSpace(Program.ImportDirectory))
                    Logger.Error("Import directory is not set.");
                else
                    Logger.Info(Program.ImportDirectory);
                return;
            }
            if (!Directory.Exists(Folder))
            {
                Logger.Error("Directory not found.");
                return;
            }
            Program.Config.ImportDirectory = Folder;
            Logger.Info($"Import directory was set to: \"{Folder}\".");
            string configPath = Path.Combine(Environment.CurrentDirectory, "Config.json");
            try
            {
                Program.Config.Save(configPath);
            }
            catch (Exception e)
            {
                Logger.Warning("Failed to save config.");
                Logger.Error(e.ToString());
            }
        }
        [Command(CommandName = "car.stats.recharged.set", VerifyCarLoaded = true)]
        [Description("""
            Sets the rechargeds stats to given preset or removes them entirely.
            =Inputs=
            string RechargedStatsPreset (optional) - Name of the preset to apply.
            =Remakrs=
            Leave preset name empty or type "null" or "none" to remove recharged stats.
            """)]
        public static void SetRechargedStatsPreset(string RechargedStatsPreset = null)
        {
            if(string.IsNullOrWhiteSpace(RechargedStatsPreset) 
                || "null".Equals(RechargedStatsPreset, StringComparison.OrdinalIgnoreCase)
                || "none".Equals(RechargedStatsPreset, StringComparison.OrdinalIgnoreCase))
            {
                Program.CurrentCar.RechargedStats = null;
                Logger.Info("Recharged stats of the car were removed.");
                return;
            }
            string presetName = RechargedStatsPreset;
            if (!RechargedStatPresets.GetPreset(RechargedStatsPreset, out RechargedStatsPreset result))
            {
                Logger.Warning($"Preset \"{RechargedStatsPreset}\" was not found, using default \"High Rider\" preset instead.");
                presetName = "High Rider";
            }
            if(Program.CurrentCar.RechargedStats is null)
            {
                Program.CurrentCar.RechargedStats = new RechargedStats(result);
            }
            else
            {
                Program.CurrentCar.RechargedStats.ApplyPreset(result);
            }
            Logger.Info($"Recharged stats were set to \"{presetName.Replace('_', ' ')}\" preset.");
        }
        [Command(CommandName = "car.stats.recharged.presets")]
        [Description("Lists all available recharged stats presets.")]
        public static void ListRechargedStatPresets()
        {
            IEnumerable<string> names = RechargedStatPresets.PresetNames();
            if (!names.Any())
            {
                Logger.Info("There are no presets currently available.");
                return;
            }
            Logger.Info("List of presets:");
            foreach (string name in names)
            {
                Logger.Log(name, ConsoleColor.Cyan);
            }
        }

        [Command(CommandName = "import")]
        [Description("""
            Imports a model and create a new car or merges the model with an already loaded car.
            =Inputs=
            string File - A full path to a file or a file name of the model.
            ImportMode Mode (optional) - An import mode method to use. New to create a new car or Merge to combine with currently loaded car.
            double Scale (optional) - The scale of the car when imported.
            CoordinateSystem Coordinates (optional) - The coordinate system the imported car was made in/exported with.
            string RechargedStatsPreset (optional) - Name of the recharged stats preset to use.
            AutoColoringMode ColoringMode (optional) - The auto coloring mode to use.
            VertexMergingRule VertexMergingRule (optional) - Rule that decides on what polygons to merge vertices removing duplicate points.
            =Remarks=
            If a file name is provided without a full path, the import folder will be used to look for the file.
            Leave preset name empty or type "null" or "none" to remove recharged stats.
            Leave coordinates empty to use default coordinate system of given importer,
            otherwise specify in a Forward/Right/Up axis format. Example: XYZ.
            Available merging rules:
            None - Do not merge verts.
            All - Merges verts on all polygons.
            Wheels - Merges verts only on custom wheels.
            Available coloring modes:
            Polygons - Scans colors based on number of polygons.
            Vertices - Scans colors based on number of vertices.
            Bounds - Scans colors based on the volume of the bounding box that encapsulates all polygons of given color.
            Edge - Scanes colors based on length of edges.
            Surface - Currently not implemented. Scans colors based on surface area of polygons.
            """)]
        public static void Import(string File, ImportMode Mode = ImportMode.New, double Scale = 1.0, CoordinateSystem? Coordinates = default, string RechargedStatsPreset = nameof(RechargedStatPresets.High_Rider), AutoColoringMode ColoringMode = AutoColoringMode.Polygons, VertexMergingRule VertexMergingRule = VertexMergingRule.All)
        {
            if(Mode < 0 || Mode > ImportMode.Merge)
            {
                Logger.Error("Invalid import mode.");
                return;
            }
            if(string.IsNullOrWhiteSpace(File))
            {
                Logger.Error("Invalid file name.");
                return;
            }
            if(!System.IO.File.Exists(File))
            {
                if(!Directory.Exists(Program.ImportDirectory))
                {
                    Logger.Error("Import directory is not defined.");
                    return;
                }
                string localPath = Path.Combine(Program.ImportDirectory, File);
                if(!System.IO.File.Exists(localPath))
                {
                    Logger.Error($"File: {localPath} was not found.");
                    return;
                }
                File = localPath;
            }
            Importer imp = ImportRegistry.GetImporter(File);
            if(imp is null)
            {
                Logger.Error($"File: \"{File}\" is not supported for importing.");
                return;
            }
            IntermediateCarModel importedCar = null;
            try
            {
                importedCar = imp.ImportCar(File, Scale, Coordinates);
            }
            catch(Exception ex)
            {
                Logger.Error("Failed to import car.");
                Logger.Error(ex.ToString());
                return;
            }
            if(importedCar is null)
            {
                Logger.Error("Failed to import car. Unknown error."); //unkown error.
                return;
            }
            Logger.Info("Car imported.");
            switch(Mode)
            {
                case ImportMode.Merge:
                    if(Program.CurrentCar is null)
                    {
                        Logger.Warning("There is no currently loaded car. Importing as new car instead.");
                        goto case ImportMode.New;
                    }
                    Logger.Info("Merging cars.");
                    importedCar.MergeWithNFMCar(Program.CurrentCar, VertexMergingRule);
                    Logger.Info("Merging complete.");
                    break;
                case ImportMode.New:
                    Logger.Info("Converting to NFM car.");
                    Program.CurrentCar = importedCar.ConvertToNFMCar(VertexMergingRule);
                    Program.CurrentCar.LoadedFromFile = Path.GetFileNameWithoutExtension(File);
                    break;
            }
            AutoSetCarColors(ColoringMode);
            SetRechargedStatsPreset(RechargedStatsPreset);
            Logger.Info("Finished importing.");
            return;
        }

        [Command(CommandName = "clipboard", VerifyCarLoaded = true)]
        [Description("""
            Copies the current car code into the clipboard.
            =Remarks=
            Requires privilidge to start external processes.
            On linux xclip is required aswell for clipboard to work.
            Otherwise use print command to get the car code or save to save car code to file.
            """)]
        public static void Clipboard()
        {
            ProcessStartInfo psi = null;
            if(OperatingSystem.IsWindows())
            {
                psi = new ProcessStartInfo("cmd", "/c clip");
            }
            else if(OperatingSystem.IsLinux())
            {
                psi = new ProcessStartInfo("xclip", "-selection clipboard");
            }
            if (psi is null)
            {
                Logger.Error($"Clipboard is currently not supported on \"{Environment.OSVersion.ToString()}\"");
                return;
            }
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            string code = Program.CurrentCar.ToString();
            using (Process proc = Process.Start(psi))
            {
                if(proc is null)
                {
                    Logger.Error($"Failed to start application \"{psi.FileName}\".");
                    return;
                }
                proc.StandardInput.Write(code);
                proc.StandardInput.Close();
                proc.WaitForExit();
            }
            Logger.Info("Car code was copied to clipboard.");
        }
        [Command(CommandName = "restart")]
        [Description("""Restarts the tools. Usefull if you have made changes in config outside of the tool.""")]
        public static void Restart()
        {
            throw RestartException.Instance;
        }
    }
}

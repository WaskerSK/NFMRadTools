using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    [Command]
    public static class Commands
    {
        [Command(CommandName = "game.setcarfolder")]
        public static void SetCarFolder(string Folder)
        {
            if (!Directory.Exists(Folder))
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
        public static void PrintCar()
        {
            Logger.Info("Car code:");
            Logger.Log(Program.CurrentCar.ToString(), ConsoleColor.White);
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
        public static void CreateNewMaterialGroup(string Name = null)
        {
            MaterialGroup mg = new MaterialGroup();
            if (string.IsNullOrWhiteSpace(Name)) Name = null;
            mg.Name = Name;
            Program.CurrentCar.MaterialGroups.Add(mg);
            Logger.Info($"New group created: [{Program.CurrentCar.MaterialGroups.Count - 1}]");
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
            Logger.Info($"Color of group [{GroupIndex}] was changed to {c.ToString()}.");
        }

        [Command(CommandName = "car.groups.removeempty", VerifyCarLoaded = true)]
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
        public static void SetGroupFs(int GroupIndex, int FsValue)
        {
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Fs = FsValue;
            }
            Logger.Info($"Value of \'fs\' has been set to {FsValue} for {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}].");
        }

        [Command(CommandName = "car.setfs", VerifyCarLoaded = true)]
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
            foreach (Polygon p in Program.CurrentCar.MaterialGroups[GroupIndex].Polygons)
            {
                p.Gr = Value;
            }
            Logger.Info($"Value of \'gr\' has been {(Value == 0 ? "removed from" : $"set to {Value} on")} {Program.CurrentCar.MaterialGroups[GroupIndex].Polygons.Count} - Polygons in group [{GroupIndex}]");
        }
        [Command(CommandName = "car.colors.get", VerifyCarLoaded = true)]
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
    }
}

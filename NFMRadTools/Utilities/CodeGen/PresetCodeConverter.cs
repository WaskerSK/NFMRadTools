using NFMRadTools.Editing;
using NFMRadTools.Editing.Presets;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.CodeGen
{
    public static class PresetCodeConverter
    {
        public static void RunPresetCreatorScript(string file)
        {
            List<(string, RechargedStats)> presets = new List<(string, RechargedStats)>();
            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    string presetName = null;
                    while (true)
                    {
                        presetName = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(presetName)) break;
                    }
                    presetName = presetName.Trim().Replace(' ', '_');
                    string data = null;
                    RechargedStats stats = new RechargedStats();
                    while (true)
                    {
                        data = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(data)) break;
                        ReadOnlySpan<char> line = data.AsSpan().Trim();
                        if (line.StartsWith("swits("))
                        {
                            line = line.Slice("swits(".Length).TrimStart();
                            int[] arr = ArrayPool<int>.Shared.Rent(3);
                            try
                            {
                                int indexOfComma = -1;
                                for (int i = 0; i < 3; i++)
                                {
                                    line = line.Slice(indexOfComma + 1).TrimStart();
                                    arr[i] = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                                    indexOfComma = line.IndexOf(',');
                                }
                                Gearbox<int> swits = new Gearbox<int>();
                                swits.Gear1 = arr[0];
                                swits.Gear2 = arr[1];
                                swits.Gear3 = arr[2];
                                stats.Swits = swits;
                            }
                            finally
                            {
                                ArrayPool<int>.Shared.Return(arr);
                            }
                            continue;
                        }
                        if (line.StartsWith("acelf("))
                        {
                            line = line.Slice("acelf(".Length).TrimStart();
                            double[] arr = ArrayPool<double>.Shared.Rent(3);
                            try
                            {
                                int indexOfComma = -1;
                                for (int i = 0; i < 3; i++)
                                {
                                    line = line.Slice(indexOfComma + 1).TrimStart();
                                    arr[i] = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                                    indexOfComma = line.IndexOf(',');
                                }
                                Gearbox<double> acelf = new Gearbox<double>();
                                acelf.Gear1 = arr[0];
                                acelf.Gear2 = arr[1];
                                acelf.Gear3 = arr[2];
                                stats.Acelf = acelf;
                            }
                            finally
                            {
                                ArrayPool<double>.Shared.Return(arr);
                            }
                            continue;
                        }
                        if (line.StartsWith("handb("))
                        {
                            line = line.Slice("handb(".Length).TrimStart();
                            stats.Handbrake = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("airs("))
                        {
                            line = line.Slice("airs(".Length).TrimStart();
                            stats.Airspeed = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("airc("))
                        {
                            line = line.Slice("airc(".Length).TrimStart();
                            stats.Aircontrol = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("turn("))
                        {
                            line = line.Slice("turn(".Length).TrimStart();
                            stats.Turnspeed = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("grip("))
                        {
                            line = line.Slice("grip(".Length).TrimStart();
                            stats.Grip = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("bounce("))
                        {
                            line = line.Slice("bounce(".Length).TrimStart();
                            stats.Bounce = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("simag("))
                        {
                            line = line.Slice("simag(".Length).TrimStart();
                            stats.Simag = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("moment("))
                        {
                            line = line.Slice("moment(".Length).TrimStart();
                            stats.Moment = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("comprad("))
                        {
                            line = line.Slice("comprad(".Length).TrimStart();
                            stats.Comprad = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("push("))
                        {
                            line = line.Slice("push(".Length).TrimStart();
                            stats.PushesOthers = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("revpush("))
                        {
                            line = line.Slice("revpush(".Length).TrimStart();
                            stats.GetsPushed = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("lift("))
                        {
                            line = line.Slice("lift(".Length).TrimStart();
                            stats.LiftsOthers = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("revlift("))
                        {
                            line = line.Slice("revlift(".Length).TrimStart();
                            stats.GetsLifted = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("powerloss("))
                        {
                            line = line.Slice("powerloss(".Length).TrimStart();
                            stats.Powerloss = long.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("flipy("))
                        {
                            line = line.Slice("flipy(".Length).TrimStart();
                            stats.Flipy = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("msquash("))
                        {
                            line = line.Slice("msquash(".Length).TrimStart();
                            stats.Msquash = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("clrad("))
                        {
                            line = line.Slice("clrad(".Length).TrimStart();
                            stats.Clrad = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("dammult("))
                        {
                            line = line.Slice("dammult(".Length).TrimStart();
                            stats.DamageMultiplier = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("maxmag("))
                        {
                            line = line.Slice("maxmag(".Length).TrimStart();
                            stats.Maxmag = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("dishandle("))
                        {
                            line = line.Slice("dishandle(".Length).TrimStart();
                            stats.Dishandle = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                        if (line.StartsWith("outdam("))
                        {
                            line = line.Slice("outdam(".Length).TrimStart();
                            stats.Outdam = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            continue;
                        }
                    }
                    ValueTuple<string, RechargedStats> tuple = (presetName, stats);
                    presets.Add(tuple);
                }
            }
            if (!presets.Any()) return;
            string outFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}-code.txt");
            StringBuilder sb = new StringBuilder();
            foreach (var preset in presets)
            {
                sb.AppendLine($"public static {nameof(RechargedStatsPreset)} {preset.Item1} =>");
                sb.AppendLine($"\tnew {nameof(RechargedStatsPreset)}()");
                sb.AppendLine("\t{");
                RechargedStats stats = preset.Item2;
                sb.AppendLine($"\t\tSwits = new Gearbox<int>() {{ Gear1 = {stats.Swits.Gear1}, Gear2 = {stats.Swits.Gear2}, Gear3 = {stats.Swits.Gear3}}},");
                sb.AppendLine($"\t\tAcelf = new Gearbox<double>() {{ Gear1 = {stats.Acelf.Gear1.ToString(Program.NFMDemicalFormat)}, Gear2 = {stats.Acelf.Gear2.ToString(Program.NFMDemicalFormat)}, Gear3 = {stats.Acelf.Gear3.ToString(Program.NFMDemicalFormat)}}},");
                sb.AppendLine($"\t\tHandbrake = {stats.Handbrake},");
                sb.AppendLine($"\t\tAirspeed = {stats.Airspeed.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tAircontrol = {stats.Aircontrol},");
                sb.AppendLine($"\t\tTurnspeed = {stats.Turnspeed},");
                sb.AppendLine($"\t\tGrip = {stats.Grip.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tBounce = {stats.Bounce.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tSimag = {stats.Simag.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tMoment = {stats.Moment.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tComprad = {stats.Comprad.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tPushesOthers = {stats.PushesOthers},");
                sb.AppendLine($"\t\tGetsPushed = {stats.GetsPushed},");
                sb.AppendLine($"\t\tLiftsOthers = {stats.LiftsOthers},");
                sb.AppendLine($"\t\tGetsLifted = {stats.GetsLifted},");
                sb.AppendLine($"\t\tPowerloss = {stats.Powerloss},");
                sb.AppendLine($"\t\tFlipy = {stats.Flipy},");
                sb.AppendLine($"\t\tMsquash = {stats.Msquash},");
                sb.AppendLine($"\t\tClrad = {stats.Clrad},");
                sb.AppendLine($"\t\tDamageMultiplier = {stats.DamageMultiplier.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tMaxmag = {stats.Maxmag},");
                sb.AppendLine($"\t\tDishandle = {stats.Dishandle.ToString(Program.NFMDemicalFormat)},");
                sb.AppendLine($"\t\tOutdam = {stats.Outdam.ToString(Program.NFMDemicalFormat)}");
                sb.AppendLine("\t};");
                sb.AppendLine();
            }
            File.WriteAllText(outFile, sb.ToString());
        }
    }
}

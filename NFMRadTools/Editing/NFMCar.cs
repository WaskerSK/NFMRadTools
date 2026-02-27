using NFMRadTools.Utilities.Macros;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class NFMCar
    {
        public Color? FirstColor { get; set; }
        public Color? SecondColor { get; set; }
        public List<string> Metadata { get; }
        public List<PolyGroup> PolyGroups { get; }
        public List<Wheel> Wheels { get; }

        public DragShotWheelDefinition DragShotWheelDefinition { get; }

        public NFMCar()
        {
            DragShotWheelDefinition = new DragShotWheelDefinition();
            DragShotWheelDefinition.Radius = 53;
            DragShotWheelDefinition.Depth = 40;
            PolyGroups = new List<PolyGroup>();
            Metadata = new List<string>();
            Wheels = new List<Wheel>();
        }

        public static NFMCar Parse(string carData)
        {
            if(string.IsNullOrWhiteSpace(carData)) return null;
            OptimizedStringReader sr = new OptimizedStringReader(carData);
            NFMCar car = new NFMCar();
            int? phyIndex = null;
            PolyGroupMode currentMode = PolyGroupMode.Normal;
            PolyGroup currentGroup = null;
            Polygon currentPoly = null;
            Wheel wheel = null;
            while(!sr.EndOfString())
            {
                ReadOnlySpan<char> line = sr.ReadLine();
                if(line.IsWhiteSpace()) continue;
                line = line.TrimStart();
                if(line.StartsWith("//")) continue;
                if(line.StartsWith("<p>"))
                {
                    if(currentGroup is null)
                    {
                        currentGroup = new PolyGroup();
                        currentGroup.Mode = currentMode;
                        currentGroup.Name = PolyGroup.GetRandomGroupName();
                        car.PolyGroups.Add(currentGroup);
                    }
                    currentPoly = new Polygon();
                    continue;
                }
                if(line.StartsWith("[p]"))
                {
                    if(currentGroup is null)
                    {
                        currentGroup = new PolyGroup();
                        currentGroup.Mode = currentMode;
                        currentGroup.Name = PolyGroup.GetRandomGroupName();
                        car.PolyGroups.Add(currentGroup);
                    }
                    currentPoly = new Polygon();
                    currentPoly.AlternativePolyMarkup = true;
                    continue;
                }
                if(line.StartsWith("</p>"))
                {
                    currentGroup.AddPolygon(currentPoly);
                    currentPoly = null;
                    continue;
                }
                if(line.StartsWith("[/p]"))
                {
                    currentGroup.AddPolygon(currentPoly);
                    currentPoly = null;
                    continue;
                }
                if(line.StartsWith("<g="))
                {
                    line = line.Slice("<g=".Length);
                    line = line.Slice(0, line.IndexOf('>'));
                    currentGroup = new PolyGroup();
                    if (line.IsEmpty || line.IsWhiteSpace())
                    {
                        currentGroup.Name = PolyGroup.GetRandomGroupName();
                    }
                    else
                    {
                        currentGroup.Name = line.ToString();
                    }
                    currentGroup.Mode = currentMode;
                    if(currentMode == PolyGroupMode.PhyrexianWheel)
                    {
                        currentGroup.PhyrexianWheelIndex = phyIndex.Value;
                    }
                    car.PolyGroups.Add(currentGroup);
                    continue;
                }
                if(line.StartsWith("</g="))
                {
                    currentGroup = null;
                    continue;
                }
                if(line.StartsWith("c("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(2, GetLengthOfNumericCharactersFromIndex(line, 2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : iR > byte.MaxValue ? byte.MaxValue : (byte)iR;
                    c.G = g.IsWhiteSpace() ? (byte)0 : iG > byte.MaxValue ? byte.MaxValue : (byte)iG;
                    c.B = b.IsWhiteSpace() ? (byte)0 : iB > byte.MaxValue ? byte.MaxValue : (byte)iB;
                    currentPoly.Color = c;
                    continue;
                }
                if(line.StartsWith("p("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    Vertex v = new Vertex();
                    ReadOnlySpan<char> x = line.Slice(2, GetLengthOfNumericCharactersFromIndex(line, 2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> y = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> z = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    v.X = x.IsWhiteSpace() ? 0 : int.Parse(x);
                    v.Y = y.IsWhiteSpace() ? 0 : int.Parse(y);
                    v.Z = z.IsWhiteSpace() ? 0 : int.Parse(z);
                    currentPoly.Vertices.Add(v);
                    continue;
                }
                if(line.StartsWith("fs("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    int value = int.Parse(line.Slice(3, GetLengthOfNumericCharactersFromIndex(line, 3)));
                    currentPoly.Fs = value;
                    continue;
                }
                if(line.StartsWith("gr("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    int value = int.Parse(line.Slice(3, GetLengthOfNumericCharactersFromIndex(line, 3)));
                    currentPoly.Gr = value;
                    continue;
                }
                if(line.StartsWith("noOutline"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.NoOutline = true;
                    continue;
                }
                if(line.StartsWith("1stColor("))
                {
                    line = line.Slice("1stColor(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : iR > byte.MaxValue ? byte.MaxValue : (byte)iR;
                    c.G = g.IsWhiteSpace() ? (byte)0 : iG > byte.MaxValue ? byte.MaxValue : (byte)iG;
                    c.B = b.IsWhiteSpace() ? (byte)0 : iB > byte.MaxValue ? byte.MaxValue : (byte)iB;
                    car.FirstColor = c;
                    continue;
                }
                if(line.StartsWith("2ndColor("))
                {
                    line = line.Slice("2ndColor(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : iR > byte.MaxValue ? byte.MaxValue : (byte)iR;
                    c.G = g.IsWhiteSpace() ? (byte)0 : iG > byte.MaxValue ? byte.MaxValue : (byte)iG;
                    c.B = b.IsWhiteSpace() ? (byte)0 : iB > byte.MaxValue ? byte.MaxValue : (byte)iB;
                    car.SecondColor = c;
                    continue;
                }
                if(line.StartsWith("<wheel")) //begin DragShot wheel
                {
                    currentGroup = null;
                    currentMode = PolyGroupMode.DragShotWheel;
                    line = line.Slice("<wheel".Length).TrimStart();
                    int choiceIndex = SpanStartsWithChoiceIndex(line, "radius=\"", "depth=\"");
                    if (choiceIndex == -1) throw new FormatException($"Invalid tag found at line [{sr.Line}]: {sr.LastReadLine.ToString()}");
                    int radius = 0;
                    int depth = 0;
                    DoNTimes doNTimes = new DoNTimes(2);
                    while(doNTimes.Next())
                    {
                        if (choiceIndex == 0) //radius
                        {
                            line = line.Slice("radius=\"".Length);
                            ReadOnlySpan<char> radiusChars = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                            radius = int.Parse(radiusChars);
                            line = line.Slice(radiusChars.Length + 1).TrimStart();
                        }
                        else //depth
                        {
                            line = line.Slice("depth=\"".Length);
                            ReadOnlySpan<char> depthChars = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                            depth = int.Parse(depthChars);
                            line = line.Slice(depthChars.Length + 1).TrimStart();
                        }
                    }
                    car.DragShotWheelDefinition.Radius = radius;
                    car.DragShotWheelDefinition.Depth = depth;
                    continue;
                }
                if(line.StartsWith("</wheel>"))
                {
                    currentMode = PolyGroupMode.Normal;
                    currentGroup = null;
                    continue;
                }
                if(line.StartsWith("wheel("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    if (currentGroup.Mode != PolyGroupMode.PhyrexianWheel) throw new FormatException("Reading unmarked Phyrexian Wheels are currenty not supported.");
                    continue;
                }
                if(line.StartsWith("<phy-wheel-"))
                {
                    currentMode = PolyGroupMode.PhyrexianWheel;
                    line = line.Slice("<phy-wheel-".Length);
                    phyIndex = int.Parse(line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0)));
                    currentGroup = null;
                    continue;
                }
                if(line.StartsWith("</phy-wheel-"))
                {
                    currentMode = PolyGroupMode.Normal;
                    currentGroup = null;
                    phyIndex = null;
                    continue;
                }
                if(line.StartsWith("gwgr("))
                {
                    line = line.Slice("gwgr(".Length);
                    line = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                    int gwgr = int.Parse(line);
                    wheel = new Wheel();
                    wheel.GwGr = gwgr;
                    car.Wheels.Add(wheel);
                    continue;
                }
                if(line.StartsWith("rims("))
                {
                    //rims(r,g,b,size,depth)
                    if (wheel is null) throw new FormatException();
                    line = line.Slice("rims(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : iR > byte.MaxValue ? byte.MaxValue : (byte)iR;
                    c.G = g.IsWhiteSpace() ? (byte)0 : iG > byte.MaxValue ? byte.MaxValue : (byte)iG;
                    c.B = b.IsWhiteSpace() ? (byte)0 : iB > byte.MaxValue ? byte.MaxValue : (byte)iB;
                    wheel.RimsColor = c;
                    line = line.Slice(indexOfSecondComma + 1);
                    int indexOfThirdComma = line.IndexOf(',');
                    ReadOnlySpan<char> size = line.Slice(indexOfThirdComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfThirdComma + 1));
                    int iSize = int.Parse(size);
                    wheel.RimSize = iSize;
                    line = line.Slice(indexOfThirdComma + 1);
                    int indexOfFourthComma = line.IndexOf(",");
                    ReadOnlySpan<char> depth = line.Slice(indexOfFourthComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFourthComma + 1));
                    wheel.RimDepth = int.Parse(depth);
                    continue;
                }
                if(line.StartsWith("w("))
                {
                    if (wheel is null) throw new FormatException();
                    //w(x,y,z,steer,width,height)
                    line = line.Slice("w(".Length);
                    ReadOnlySpan<char> x = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> y = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> z = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iX = int.Parse(x);
                    int iY = int.Parse(y);
                    int iZ = int.Parse(z);
                    line = line.Slice(indexOfSecondComma + 1);
                    int indexOfThirdComma = line.IndexOf(',');
                    ReadOnlySpan<char> steer = line.Slice(indexOfThirdComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfThirdComma + 1));
                    int iSteer = int.Parse(steer);
                    line = line.Slice(indexOfThirdComma + 1);
                    int indexOfFourthComma = line.IndexOf(",");
                    ReadOnlySpan<char> width = line.Slice(indexOfFourthComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFourthComma + 1));
                    int iWidth = int.Parse(width);
                    int indexOfFifthComma = line.IndexOf(',');
                    ReadOnlySpan<char> height = line.Slice(indexOfFifthComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFifthComma + 1));
                    int iHeight = int.Parse(height);
                    Wheel.Instance wheelInstance = new Wheel.Instance();
                    wheelInstance.X = iX;
                    wheelInstance.Y = iY;
                    wheelInstance.Z = iZ;
                    wheelInstance.CanSteer = iSteer != 0;
                    wheelInstance.Width = iWidth;
                    wheelInstance.Height = iHeight;
                    wheel.Instances.Add(wheelInstance);
                    continue;
                }
                if(currentGroup is null || currentPoly is null)
                {
                    car.Metadata.Add(line.ToString());
                    continue;
                }
                currentPoly.Metadata.Add(line.ToString());
            }
            return car;
        }

        private static int SpanStartsWithChoiceIndex(ReadOnlySpan<char> line, params ReadOnlySpan<string> choices)
        {
            for(int i = 0; i < choices.Length; i++)
            {
                if (line.StartsWith(choices[i], StringComparison.Ordinal)) return i;
            }
            return -1;
        }

        private static int GetLengthOfNumericCharactersFromIndex(ReadOnlySpan<char> span, int index)
        {
            if(index < 0) return 0;
            if(span.Length <= 0 || index >= span.Length) return 0;
            int i = 0;
            for(; index + i < span.Length; i++)
            {
                if (char.IsNumber(span[index + i])) continue;
                if (span[index + i] == '-') continue;
                return i;
            }
            return i;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(FirstColor.HasValue)
            {
                sb.Append("1stColor(").Append(FirstColor.Value.ToString()).AppendLine(")");
            }
            if(SecondColor.HasValue)
            {
                sb.Append("2ndColor(").Append(SecondColor.Value.ToString()).AppendLine(")");
            }
            foreach(PolyGroup g in PolyGroups.Where(x => x.Mode == PolyGroupMode.Normal))
            {
                sb.AppendLine(g.ToString());
            }
            IEnumerable<PolyGroup> enumerable = PolyGroups.Where(x => x.Mode == PolyGroupMode.DragShotWheel);
            if (enumerable.Any())
            {
                sb.Append("<wheel radius=\"")
                    .Append(DragShotWheelDefinition.Radius)
                    .Append("\" depth=\"")
                    .Append(DragShotWheelDefinition.Depth)
                    .AppendLine("\">");
                foreach (PolyGroup g in enumerable)
                {
                    sb.AppendLine(g.ToString());
                }
                sb.AppendLine("</wheel>");
            }
            
            foreach (string s in Metadata)
            {
                sb.AppendLine(s);
            }

            enumerable = PolyGroups.Where(x => x.Mode == PolyGroupMode.PhyrexianWheel);
            bool hasPhyWheels = enumerable.Any();
            foreach (Wheel wheel in Wheels)
            {
                sb.Append(wheel.ToString());
                if (hasPhyWheels) sb.Append("c");
                sb.AppendLine();
            }

            
            if(hasPhyWheels)
            {
                var groups = enumerable.GroupBy(x => x.PhyrexianWheelIndex).OrderBy(x => x.Key);
                foreach (var group in groups)
                {
                    sb.Append("<phy-wheel-").Append(group.Key).AppendLine(">");
                    foreach(PolyGroup g in group)
                    {
                        sb.AppendLine(g.ToString());
                    }
                    sb.Append("</phy-wheel-").Append(group.Key).AppendLine(">");
                }
            }
            return sb.ToString();
        }
    }
}

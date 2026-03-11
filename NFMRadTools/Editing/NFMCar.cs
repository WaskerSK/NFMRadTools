using NFMRadTools.Utilities;
using NFMRadTools.Utilities.Macros;
using System;
using System.Buffers;
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
        public string LoadedFromFile { get; set; }
        public Color? FirstColor { get; set; }
        public Color? SecondColor { get; set; }
        public List<string> Metadata { get; }
        public List<PolyGroup> PolyGroups { get; }
        public List<Wheel> Wheels { get; }
        public Stats? Stats { get; set; }
        public Physics? Physics { get; set; }
        public int? Handling { get; set; }
        public RechargedStats RechargedStats { get; set; }

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
            int? customWheelIndex = null;
            bool hasG6Wheels = false;
            PolyGroupMode currentMode = PolyGroupMode.Normal;
            PolyGroup currentGroup = null;
            Polygon currentPoly = null;
            //Wheel wheel = null;
            int gwgr = 0;
            Color rimColor = new Color(120,120,120);
            int rimSize = 20;
            int rimDepth = 10;
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
                        currentGroup.Name = RandomName.Get();
                        if (customWheelIndex.HasValue)
                        {
                            currentGroup.CustomWheelIndex = customWheelIndex.Value;
                        }
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
                        currentGroup.Name = RandomName.Get();
                        if (customWheelIndex.HasValue)
                        {
                            currentGroup.CustomWheelIndex = customWheelIndex.Value;
                        }
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
                        currentGroup.Name = RandomName.Get();
                    }
                    else
                    {
                        currentGroup.Name = line.ToString();
                    }
                    currentGroup.Mode = currentMode;
                    if(currentMode == PolyGroupMode.PhyrexianWheel || currentMode == PolyGroupMode.G6Wheel)
                    {
                        currentGroup.CustomWheelIndex = customWheelIndex.Value;
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
                    ReadOnlySpan<char> r = line.Slice(2, line.GetLengthOfNumericCharactersFromIndex(2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
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
                    ReadOnlySpan<char> x = line.Slice(2, line.GetLengthOfNumericCharactersFromIndex(2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> y = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> z = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
                    v.X = x.IsWhiteSpace() ? 0 : int.Parse(x);
                    v.Y = y.IsWhiteSpace() ? 0 : int.Parse(y);
                    v.Z = z.IsWhiteSpace() ? 0 : int.Parse(z);
                    currentPoly.Vertices.Add(v);
                    continue;
                }
                if(line.StartsWith("fs("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    int value = int.Parse(line.Slice(3, line.GetLengthOfNumericCharactersFromIndex(3)));
                    currentPoly.Fs = value;
                    continue;
                }
                if(line.StartsWith("gr("))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    int value = int.Parse(line.Slice(3, line.GetLengthOfNumericCharactersFromIndex(3)));
                    currentPoly.Gr = value;
                    continue;
                }
                if(line.StartsWith("noOutline"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.NoOutline = true;
                    continue;
                }
                if(line.StartsWith("glass"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.Glass = true;
                    continue;
                }
                if (line.StartsWith("glassTint"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.GlassTint = true;
                    continue;
                }
                if (line.StartsWith("light"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.Light = true;
                    continue;
                }
                if(line.StartsWith("lightF"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.LightFront = true;
                    continue;
                }
                if (line.StartsWith("lightB"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.LightBack = true;
                    continue;
                }
                if (line.StartsWith("lightR"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.LightReverse = true;
                    continue;
                }
                if (line.StartsWith("lightBrake"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.LightBrake = true;
                    continue;
                }
                if (line.StartsWith("dayOnly"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.DayOnly = true;
                    continue;
                }
                if (line.StartsWith("nightOnly"))
                {
                    if (currentGroup is null || currentPoly is null) throw new FormatException();
                    currentPoly.NightOnly = true;
                    continue;
                }
                if (line.StartsWith("1stColor("))
                {
                    line = line.Slice("1stColor(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
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
                    ReadOnlySpan<char> r = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
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
                            ReadOnlySpan<char> radiusChars = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                            radius = int.Parse(radiusChars);
                            line = line.Slice(radiusChars.Length + 1).TrimStart();
                        }
                        else //depth
                        {
                            line = line.Slice("depth=\"".Length);
                            ReadOnlySpan<char> depthChars = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
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
                if(line.StartsWith("<wheelModel(")) // begin G6 wheel
                {
                    currentMode = PolyGroupMode.G6Wheel;
                    hasG6Wheels = true;
                    currentGroup = null;
                    line = line.Slice("<wheelModel(".Length);
                    customWheelIndex = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if(line.StartsWith("</wheelModel>"))
                {
                    currentMode = PolyGroupMode.Normal;
                    currentGroup = null;
                    customWheelIndex = null;
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
                    customWheelIndex = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    currentGroup = null;
                    continue;
                }
                if(line.StartsWith("</phy-wheel-"))
                {
                    currentMode = PolyGroupMode.Normal;
                    currentGroup = null;
                    customWheelIndex = null;
                    continue;
                }
                if(line.StartsWith("gwgr("))
                {
                    line = line.Slice("gwgr(".Length);
                    line = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                    gwgr = int.Parse(line);
                    continue;
                }
                if(line.StartsWith("rims("))
                {
                    //rims(r,g,b,size,depth)
                    line = line.Slice("rims(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : iR > byte.MaxValue ? byte.MaxValue : (byte)iR;
                    c.G = g.IsWhiteSpace() ? (byte)0 : iG > byte.MaxValue ? byte.MaxValue : (byte)iG;
                    c.B = b.IsWhiteSpace() ? (byte)0 : iB > byte.MaxValue ? byte.MaxValue : (byte)iB;
                    rimColor = c;
                    line = line.Slice(indexOfSecondComma + 1);
                    int indexOfThirdComma = line.IndexOf(',');
                    ReadOnlySpan<char> size = line.Slice(indexOfThirdComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfThirdComma + 1));
                    int iSize = int.Parse(size);
                    rimSize = iSize;
                    line = line.Slice(indexOfThirdComma + 1);
                    int indexOfFourthComma = line.IndexOf(",");
                    ReadOnlySpan<char> depth = line.Slice(indexOfFourthComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFourthComma + 1));
                    rimDepth = int.Parse(depth);
                    continue;
                }
                if(line.StartsWith("w("))
                {
                    //w(x,y,z,steer,width,height)
                    line = line.Slice("w(".Length);
                    ReadOnlySpan<char> x = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> y = line.Slice(indexOfFirstComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> z = line.Slice(indexOfSecondComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfSecondComma + 1));
                    int iX = int.Parse(x);
                    int iY = int.Parse(y);
                    int iZ = int.Parse(z);
                    line = line.Slice(indexOfSecondComma + 1);
                    int indexOfThirdComma = line.IndexOf(',');
                    ReadOnlySpan<char> steer = line.Slice(indexOfThirdComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfThirdComma + 1));
                    int iSteer = int.Parse(steer);
                    line = line.Slice(indexOfThirdComma + 1);
                    int indexOfFourthComma = line.IndexOf(",");
                    ReadOnlySpan<char> width = line.Slice(indexOfFourthComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFourthComma + 1));
                    int iWidth = int.Parse(width);
                    int indexOfFifthComma = line.IndexOf(',');
                    ReadOnlySpan<char> height = line.Slice(indexOfFifthComma + 1, line.GetLengthOfNumericCharactersFromIndex(indexOfFifthComma + 1));
                    int iHeight = int.Parse(height);
                    int? wheelModel = null;
                    if(hasG6Wheels)
                    {
                        line = line.Slice(indexOfFifthComma + 1);
                        int indexOfSixthComma = line.IndexOf(",");
                        ReadOnlySpan<char> model = line.Slice(indexOfSixthComma + 1);
                        model = model.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                        wheelModel = int.Parse(model);
                    }
                    Wheel wheel = new Wheel();
                    wheel.X = iX;
                    wheel.Y = iY;
                    wheel.Z = iZ;
                    wheel.CanSteer = iSteer != 0;
                    wheel.Width = iWidth;
                    wheel.Height = iHeight;
                    wheel.GwGr = gwgr;
                    wheel.RimColor = rimColor;
                    wheel.RimDepth = rimDepth;
                    wheel.RimSize = rimSize;
                    wheel.WheelModel = wheelModel;
                    car.Wheels.Add(wheel);
                    continue;
                }
                if(line.StartsWith("stat("))
                {
                    Stats stats = new Stats();
                    line = line.Slice("stat(".Length);
                    int[] arr = ArrayPool<int>.Shared.Rent(5);
                    try
                    {
                        int indexOfComma = -1;
                        for (int i = 0; i < 5; i++)
                        {
                            line = line.Slice(indexOfComma + 1);
                            int value = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            arr[i] = value;
                            indexOfComma = line.IndexOf(',');
                        }
                        stats.Speed = arr[0];
                        stats.Acceleration = arr[1];
                        stats.Stunts = arr[2];
                        stats.Strength = arr[3];
                        stats.Endurance = arr[4];
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                    car.Stats = stats;
                    continue;
                }
                if(line.StartsWith("physics("))
                {
                    Physics phys = new Physics();
                    line = line.Slice("physics(".Length);
                    int[] arr = ArrayPool<int>.Shared.Rent(16);
                    try
                    {
                        int indexOfComma = -1;
                        for (int i = 0; i < 16; i++)
                        {
                            line = line.Slice(indexOfComma + 1);
                            int value = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                            arr[i] = value;
                            indexOfComma = line.IndexOf(',');
                        }
                        phys.Handbrake = arr[0];
                        phys.TurningSensitivity = arr[1];
                        phys.TireGrip = arr[2];
                        phys.Bouncing = arr[3];
                        phys.Unknown = arr[4];
                        phys.LiftsOthers = arr[5];
                        phys.GetsLifted = arr[6];
                        phys.PushesOthers = arr[7];
                        phys.GetsPushed = arr[8];
                        phys.AerialRotationSpeed = arr[9];
                        phys.AerialControlGliding = arr[10];
                        phys.Radius = arr[11];
                        phys.Magnitude = arr[12];
                        phys.RoofDestruction = arr[13];
                        phys.Engine = (Engine)arr[14];
                        phys.Unknown2 = arr[15];
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                    car.Physics = phys;
                    continue;
                }
                if(line.StartsWith("handling("))
                {
                    line = line.Slice("handling(".Length);
                    int handling = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    car.Handling = handling;
                    continue;
                }
                if(line.StartsWith("swits("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("swits(".Length).TrimStart();
                    int[] arr = ArrayPool<int>.Shared.Rent(3);
                    try
                    {
                        int indexOfComma = -1;
                        for(int i = 0; i < 3; i++)
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
                if(line.StartsWith("acelf("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
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
                if(line.StartsWith("handb("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("handb(".Length).TrimStart();
                    stats.Handbrake = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if(line.StartsWith("airs("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("airs(".Length).TrimStart();
                    stats.Airspeed = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("airc("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("airc(".Length).TrimStart();
                    stats.Aircontrol = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("turn("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("turn(".Length).TrimStart();
                    stats.Turnspeed = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("grip("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("grip(".Length).TrimStart();
                    stats.Grip = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("bounce("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("bounce(".Length).TrimStart();
                    stats.Bounce = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("simag("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("simag(".Length).TrimStart();
                    stats.Simag = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("moment("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("moment(".Length).TrimStart();
                    stats.Moment = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("comprad("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("comprad(".Length).TrimStart();
                    stats.Comprad = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("push("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("push(".Length).TrimStart();
                    stats.PushesOthers = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("revpush("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("revpush(".Length).TrimStart();
                    stats.GetsPushed = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("lift("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("lift(".Length).TrimStart();
                    stats.LiftsOthers = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("revlift("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("revlift(".Length).TrimStart();
                    stats.GetsLifted = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("powerloss("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("powerloss(".Length).TrimStart();
                    stats.Powerloss = long.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("flipy("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("flipy(".Length).TrimStart();
                    stats.Flipy = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("msquash("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("msquash(".Length).TrimStart();
                    stats.Msquash = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("clrad("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("clrad(".Length).TrimStart();
                    stats.Clrad = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("dammult("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("dammult(".Length).TrimStart();
                    stats.DamageMultiplier = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("maxmag("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("maxmag(".Length).TrimStart();
                    stats.Maxmag = int.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("dishandle("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("dishandle(".Length).TrimStart();
                    stats.Dishandle = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (line.StartsWith("outdam("))
                {
                    if (car.RechargedStats is null) car.RechargedStats = new RechargedStats();
                    RechargedStats stats = car.RechargedStats;
                    line = line.Slice("outdam(".Length).TrimStart();
                    stats.Outdam = double.Parse(line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0)));
                    continue;
                }
                if (currentGroup is null || currentPoly is null)
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

        public void SetDefaultCarPhysicProperties()
        {
            Stats = new Stats()
            {
                Speed = 87,
                Acceleration = 123,
                Stunts = 120,
                Strength = 197,
                Endurance = 153
            };
            Physics = new Physics()
            {
                Handbrake = 100,
                TurningSensitivity = 78,
                TireGrip = 90,
                Bouncing = 24,
                Unknown = 50,
                LiftsOthers = 90,
                GetsLifted = 0,
                PushesOthers = 12,
                GetsPushed = 0,
                AerialRotationSpeed = 68,
                AerialControlGliding = 24,
                Radius = 100,
                Magnitude = 78,
                RoofDestruction = 0,
                Engine = Engine.Normal,
                Unknown2 = 86423
            };
            Handling = 100;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (FirstColor.HasValue)
            {
                sb.Append("1stColor(").Append(FirstColor.Value.ToString()).AppendLine(")");
            }
            if(SecondColor.HasValue)
            {
                sb.Append("2ndColor(").Append(SecondColor.Value.ToString()).AppendLine(")");
            }
            sb.AppendLine();
            if(RechargedStats is not null)
            {
                sb.AppendLine(RechargedStats.ToString());
            }
            if (Metadata.Any()) sb.AppendLine();

            foreach (string s in Metadata)
            {
                sb.AppendLine(s);
            }

            if (Metadata.Any()) sb.AppendLine();

            foreach (PolyGroup g in PolyGroups.Where(x => x.Mode == PolyGroupMode.Normal))
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

            enumerable = PolyGroups.Where(x => x.Mode == PolyGroupMode.G6Wheel);

            foreach(PolyGroup g in enumerable.OrderBy(x => x.CustomWheelIndex))
            {
                sb.Append("<wheelModel(").Append(g.CustomWheelIndex).AppendLine(")>");
                sb.AppendLine();
                sb.AppendLine(g.ToString());
                sb.AppendLine("</wheelModel>");
            }
            
            enumerable = PolyGroups.Where(x => x.Mode == PolyGroupMode.PhyrexianWheel);
            bool hasPhyWheels = enumerable.Any();
            Dictionary<int, int> phyWheelIndexMap = null;
            if (hasPhyWheels)
            {
                phyWheelIndexMap = new Dictionary<int, int>(Wheels.Count);
            }
            int wheelIndex = 0;
            foreach (IGrouping<Wheel.Definition, Wheel> wheelGroup in Wheels.GroupBy(x => x.GetDefinition()))
            {
                Wheel.Definition def = wheelGroup.Key;
                sb.Append("gwgr(").Append(def.GwGr).AppendLine(")");
                sb.Append("rims(")
                    .Append(def.RimColor.ToString())
                    .Append(",").Append(def.RimSize)
                    .Append(",").Append(def.RimDepth)
                    .AppendLine(")");
                var sideGroups = wheelGroup.GroupBy(x => x.X >= 0);
                IGrouping<bool, Wheel> leftWheels = sideGroups.FirstOrDefault(x => x.Key == false);
                IGrouping<bool, Wheel> rightWheels = sideGroups.FirstOrDefault(x => x.Key == true);
                if (leftWheels.Count() != rightWheels.Count()) throw new InvalidDataException("Wheel counts on left and right side do not match.");
                
                foreach(Wheel wheel in leftWheels.OrderByDescending(x => x.Z).Interlace(rightWheels.OrderByDescending(x => x.Z)))
                {
                    sb.Append("w(")
                        .Append(wheel.X)
                        .Append(",").Append(wheel.Y)
                        .Append(",").Append(wheel.Z)
                        .Append(",").Append(wheel.CanSteer ? "11" : "0")
                        .Append(",").Append(wheel.Width)
                        .Append(",").Append(wheel.Height);
                    if(wheel.WheelModel.HasValue)
                    {
                        sb.Append(",").Append(wheel.WheelModel.Value);
                    }
                    sb.Append(")");
                    if (hasPhyWheels)
                    {
                        sb.Append("c");
                        int originalIndex = Wheels.IndexOf(wheel);
                        phyWheelIndexMap.Add(originalIndex, wheelIndex);
                    }
                    sb.AppendLine();
                    wheelIndex++;
                }
            }

            if (Stats.HasValue || Physics.HasValue || Handling.HasValue)
                sb.AppendLine();
            if (Stats.HasValue) sb.AppendLine(Stats.Value.ToString());
            if (Physics.HasValue) sb.AppendLine(Physics.Value.ToString());
            if (Handling.HasValue) sb.AppendLine($"handling({Handling.Value})");
            
            if (hasPhyWheels)
            {
                sb.AppendLine();
                var groups = enumerable.GroupBy(x => phyWheelIndexMap[x.CustomWheelIndex]).OrderBy(x => x.Key);
                foreach (var group in groups)
                {
                    sb.Append("<phy-wheel-").Append(group.Key).AppendLine(">");
                    foreach(PolyGroup g in group)
                    {
                        int originalIndex = g.CustomWheelIndex;
                        g.CustomWheelIndex = group.Key;
                        sb.AppendLine(g.ToString());
                        g.CustomWheelIndex = originalIndex;
                    }
                    sb.Append("</phy-wheel-").Append(group.Key).AppendLine(">");
                }
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    public class NFMCar
    {
        public Color? FirstColor { get; set; }
        public Color? SecondColor { get; set; }
        public List<string> Metadata { get; }
        public List<MaterialGroup> MaterialGroups { get; }

        public NFMCar()
        {
            MaterialGroups = new List<MaterialGroup>();
            Metadata = new List<string>();
        }

        public static NFMCar Parse(string carData)
        {
            if(string.IsNullOrWhiteSpace(carData)) return null;
            OptimizedStringReader sr = new OptimizedStringReader(carData);
            NFMCar car = new NFMCar();
            MaterialGroup currentGroup = null;
            Polygon currentPoly = null;
            while(!sr.EndOfString())
            {
                ReadOnlySpan<char> line = sr.ReadLine();
                if(line.IsWhiteSpace()) continue;
                line = line.TrimStart();
                if(!char.IsAsciiLetter(line[0])) continue;
                if(line.StartsWith("//"))
                {
                    line = line.Slice(2).TrimStart();
                    if(line.StartsWith("<m="))
                    {
                        line = line.Slice("<m=".Length);
                        line = line.Slice(0, line.IndexOf('>'));
                        currentGroup = new MaterialGroup();
                        currentGroup.Name = line.ToString();
                        car.MaterialGroups.Add(currentGroup);
                        continue;
                    }
                    if(line.StartsWith("</m="))
                    {
                        currentGroup = null;
                        continue;
                    }
                    continue;
                }
                if(line.StartsWith("<p>"))
                {
                    if(currentGroup is null)
                    {
                        currentGroup = new MaterialGroup();
                        car.MaterialGroups.Add(currentGroup);
                    }
                    currentPoly = new Polygon();
                    continue;
                }
                if(line.StartsWith("</p>"))
                {
                    currentGroup.Polygons.Add(currentPoly);
                    currentPoly = null;
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
                    c.R = r.IsWhiteSpace() ? (byte)0 : (iR > byte.MaxValue ? byte.MaxValue : (byte)iR);
                    c.G = g.IsWhiteSpace() ? (byte)0 : (iG > byte.MaxValue ? byte.MaxValue : (byte)iG);
                    c.B = b.IsWhiteSpace() ? (byte)0 : (iB > byte.MaxValue ? byte.MaxValue : (byte)iB);
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
                    ReadOnlySpan<char> r = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : (iR > byte.MaxValue ? byte.MaxValue : (byte)iR);
                    c.G = g.IsWhiteSpace() ? (byte)0 : (iG > byte.MaxValue ? byte.MaxValue : (byte)iG);
                    c.B = b.IsWhiteSpace() ? (byte)0 : (iB > byte.MaxValue ? byte.MaxValue : (byte)iB);
                    car.FirstColor = c;
                    continue;
                }
                if(line.StartsWith("2ndColor("))
                {
                    line = line.Slice("2ndColor(".Length);
                    Color c = new Color();
                    ReadOnlySpan<char> r = line.Slice(0, GetLengthOfNumericCharactersFromIndex(line, 2));
                    int indexOfFirstComma = line.IndexOf(',');
                    ReadOnlySpan<char> g = line.Slice(indexOfFirstComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfFirstComma + 1));
                    line = line.Slice(indexOfFirstComma + 1);
                    int indexOfSecondComma = line.IndexOf(',');
                    ReadOnlySpan<char> b = line.Slice(indexOfSecondComma + 1, GetLengthOfNumericCharactersFromIndex(line, indexOfSecondComma + 1));
                    int iR = int.Parse(r);
                    int iG = int.Parse(g);
                    int iB = int.Parse(b);
                    c.R = r.IsWhiteSpace() ? (byte)0 : (iR > byte.MaxValue ? byte.MaxValue : (byte)iR);
                    c.G = g.IsWhiteSpace() ? (byte)0 : (iG > byte.MaxValue ? byte.MaxValue : (byte)iG);
                    c.B = b.IsWhiteSpace() ? (byte)0 : (iB > byte.MaxValue ? byte.MaxValue : (byte)iB);
                    car.SecondColor = c;
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
            foreach(MaterialGroup mg in MaterialGroups)
            {
                sb.AppendLine(mg.ToString());
            }
            foreach (string s in Metadata)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }
    }

    public class MaterialGroup
    {
        public string Name { get; set; }
        public List<Polygon> Polygons { get; }

        public MaterialGroup()
        {
            Polygons = new List<Polygon>();
        }

        public void SetColor(Color color)
        {
            foreach(Polygon p in Polygons)
            {
                p.Color = color;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("//<m=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            foreach(Polygon poly in Polygons)
            {
                sb.AppendLine(poly.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("//</m=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            return sb.ToString();
        }
    }

    public struct Color : IEquatable<Color>
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color() { }
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return $"{R},{G},{B}";
        }

        public override int GetHashCode()
        {
            return R ^ G ^ B;
        }
        public override bool Equals(object obj)
        {
            if(obj is Color c) return Equals(c);
            return false;
        }
        public bool Equals(Color other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public static bool operator==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !a.Equals(b);
    }

    public class Polygon
    {
        public bool NoOutline { get; set; }
        public Color Color { get; set; }
        public int? Fs { get; set; }
        public int Gr { get; set; }
        public List<Vertex> Vertices { get; }
        public List<string> Metadata { get; }

        public Polygon()
        {
            Vertices = new List<Vertex>();
            Metadata = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<p>");
            if (NoOutline)
                sb.AppendLine("noOutline");
            sb.Append("c(").Append(Color.ToString()).AppendLine(")");
            if(Fs.HasValue)
                sb.Append("fs(").Append(Fs.Value).AppendLine(")");
            if(Gr != 0)
                sb.Append("gr(").Append(Gr).AppendLine(")");
            foreach(string metadata in Metadata)
            {
                sb.AppendLine(metadata);
            }
            sb.AppendLine();
            foreach(Vertex v in Vertices)
            {
                sb.AppendLine(v.ToString());
            }
            sb.AppendLine("</p>");
            sb.AppendLine();
            return sb.ToString();
        }
    }

    public struct Vertex
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override string ToString()
        {
            return $"p({X},{Y},{Z})";
        }
    }
}

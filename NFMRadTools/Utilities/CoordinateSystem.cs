using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace NFMRadTools.Utilities
{
    public readonly struct CoordinateSystem : ISpanParsable<CoordinateSystem>, IEquatable<CoordinateSystem>, ISpanFormattable
    {
        private readonly int value;
        public Direction XAxis => (Direction)(value & 0x000000FF);
        public Direction YAxis => (Direction)((value & 0x0000FF00) >> 8);
        public Direction ZAxis => (Direction)((value & 0x00FF0000) >> 16);

        public bool IsValid
        {
            get
            {
                Direction x = XAxis;
                Direction y = YAxis;
                Direction z = ZAxis;
                if ((x > Direction.Down || y > Direction.Down || z > Direction.Down)) return false;
                return !(x == y || x == z || y == z);
            }
        }

        public CoordinateSystem(Direction xAxis, Direction yAxis, Direction zAxis)
        {
            int x = (int)xAxis;
            int y = (int)yAxis;
            int z = (int)zAxis;
            uint max = (uint)Direction.Down;
            if ((uint)x > max || (uint)y > max || (uint)z > max)
                throw new ArgumentException("Invalid coordinates");
            if (x == y || x == z || y == z)
                throw new ArgumentException("Cannot have identical direction on multiple axis.");
            value = x;
            value |= y << 8;
            value |= z << 16;
        }

        public Axis GetDirectionAxis(Direction dir)
        {
            dir = dir.ToPositive();
            if (XAxis.ToPositive() == dir) return Axis.X;
            if(YAxis.ToPositive() == dir) return Axis.Y;
            return Axis.Z;
        }
        public Direction GetAxisDirection(Axis axis)
        {
            axis = axis.ToPositive();
            switch(axis)
            {
                case Axis.X: return XAxis;
                case Axis.Y: return YAxis;
                case Axis.Z: return ZAxis;
            }
            throw new ArgumentException("Invalid axis value.");
        }
        public bool Equals(CoordinateSystem other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            if(obj is CoordinateSystem other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return value;
        }

        public override string ToString()
        {
            string x = null;
            string y = null;
            string z = null;
            FormatAxisStrings(ref x, ref y, ref z);
            return $"{x}{y}{z}";
        }

        public static CoordinateSystem Parse(string s)
        {
            ArgumentNullException.ThrowIfNull(s);
            return Parse(s.AsSpan());
        }

        public static CoordinateSystem Parse(ReadOnlySpan<char> s)
        {
            if (TryParse(s, out CoordinateSystem result)) return result;
            throw new ArgumentException("Incorrect input format.");
        }

        public static bool TryParse(string s, out CoordinateSystem result)
        {
            result = default;
            if(s is null) return false;
            return TryParse(s.AsSpan(), out result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, out CoordinateSystem result)
        {
            result = default;
            if(s.Length < 3) return false;
            if(!ReadAxis(ref s, out Axis x)) return false;
            if(!ReadAxis(ref s, out Axis y)) return false;
            if(!ReadAxis(ref s, out Axis z)) return false;
            if(x == y || x == z || y == z) return false;
            Span<Axis> axisSpan = stackalloc Axis[3];
            axisSpan[0] = x;
            axisSpan[1] = y;
            axisSpan[2] = z;
            Span<Direction> resultDirs = stackalloc Direction[3];
            Direction dir = Direction.Front;
            foreach(Axis axis in axisSpan)
            {
                int i = 0;
                Direction vDir = dir;
                switch(axis)
                {
                    case Axis.X:
                        i = 0;
                        break;
                    case Axis.NX:
                        i = 0;
                        vDir = vDir.ToNegative();
                        break;
                    case Axis.Y:
                        i = 1;
                        break;
                    case Axis.NY:
                        i = 1;
                        vDir = vDir.ToNegative();
                        break;
                    case Axis.Z:
                        i = 2;
                        break;
                    case Axis.NZ:
                        i = 2;
                        vDir = vDir.ToNegative();
                        break;
                }
                resultDirs[i] = vDir;
                dir += 2;
            }
            result = new CoordinateSystem(resultDirs[0], resultDirs[1], resultDirs[2]);
            return true;
        }

        private static bool ReadAxis(ref ReadOnlySpan<char> s, out Axis axis)
        {
            axis = default;
            if(s.Length < 1) return false;
            bool negative = false;
            int charIndex = 0;
            if(s[0] == '-')
            {
                if(s.Length < 2) return false;
                negative = true;
                charIndex = 1;
            }
            char c = s[charIndex];
            if(c == 'x' || c == 'X') axis = negative ? Axis.NX : Axis.X;
            else if(c == 'y' || c == 'Y') axis = negative ? Axis.NY : Axis.Y;
            else if(c == 'z' || c == 'Z') axis = negative ? Axis.NZ : Axis.Z;
            else return false;
            s = s.Slice(charIndex + 1);
            return true;
        }

        public bool TryFormat(Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            string x = null;
            string y = null;
            string z = null;
            FormatAxisStrings(ref x, ref y, ref z);
            int charsNeeded = x.Length + y.Length + z.Length;
            if (charsNeeded > destination.Length) return false;
            for(int i = 0; i < 3; i++)
            {
                string s = null;
                switch(i)
                {
                    case 0: s = x; break;
                    case 1: s = y; break;
                    case 2: s = z; break;
                }
                foreach(char c in s)
                {
                    destination[charsWritten] = c;
                    charsWritten++;
                }
            }
            return true;
        }
        private unsafe void FormatAxisStrings(ref string x, ref string y, ref string z)
        {
            Direction d = XAxis;
            void* v = null;
            switch (d)
            {
                case Direction.Front:
                case Direction.Back:
                    v = Unsafe.AsPointer(ref x);
                    break;
                case Direction.Right:
                case Direction.Left:
                    v = Unsafe.AsPointer(ref y);
                    break;
                case Direction.Up:
                case Direction.Down:
                    v = Unsafe.AsPointer(ref z);
                    break;
            }
            Unsafe.AsRef<string>(v) = d.IsPositive() ? "X" : "-X";
            d = YAxis;
            v = null;
            switch (d)
            {
                case Direction.Front:
                case Direction.Back:
                    v = Unsafe.AsPointer(ref x);
                    break;
                case Direction.Right:
                case Direction.Left:
                    v = Unsafe.AsPointer(ref y);
                    break;
                case Direction.Up:
                case Direction.Down:
                    v = Unsafe.AsPointer(ref z);
                    break;
            }
            Unsafe.AsRef<string>(v) = d.IsPositive() ? "Y" : "-Y";
            d = ZAxis;
            v = null;
            switch (d)
            {
                case Direction.Front:
                case Direction.Back:
                    v = Unsafe.AsPointer(ref x);
                    break;
                case Direction.Right:
                case Direction.Left:
                    v = Unsafe.AsPointer(ref y);
                    break;
                case Direction.Up:
                case Direction.Down:
                    v = Unsafe.AsPointer(ref z);
                    break;
            }
            Unsafe.AsRef<string>(v) = d.IsPositive() ? "Z" : "-Z";
        }

        static CoordinateSystem ISpanParsable<CoordinateSystem>.Parse(ReadOnlySpan<char> s, IFormatProvider provider)
        {
            return Parse(s);
        }

        static bool ISpanParsable<CoordinateSystem>.TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out CoordinateSystem result)
        {
            return TryParse(s, out result);
        }

        static CoordinateSystem IParsable<CoordinateSystem>.Parse(string s, IFormatProvider provider)
        {
            return Parse(s);
        }

        static bool IParsable<CoordinateSystem>.TryParse(string s, IFormatProvider provider, out CoordinateSystem result)
        {
            return TryParse(s, out result);
        }

        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            return TryFormat(destination, out charsWritten);
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }
        public static bool operator ==(CoordinateSystem left, CoordinateSystem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoordinateSystem left, CoordinateSystem right)
        {
            return !left.Equals(right);
        }
    }

    public enum Direction : byte
    {
        Front = 0,
        Back = 1,
        Right = 2,
        Left = 3,
        Up = 4,
        Down = 5
    }

    public enum Axis : byte
    {
        X = 0,
        NX = 1,
        Y = 2,
        NY = 3,
        Z = 4,
        NZ = 5
    }

    public static class DirectionExtensions
    {
        public static bool IsPositive(this Direction coordinate) => int.IsEvenInteger((int)coordinate);
        public static bool IsNegative(this Direction coordinate) => int.IsOddInteger((int)coordinate);
        public static Direction ToPositive(this Direction coordinate)
        {
            if(IsPositive(coordinate)) return coordinate;
            return (Direction)((int)coordinate - 1);
        }
        public static Direction ToNegative(this Direction coordinate)
        {
            if(IsNegative(coordinate)) return coordinate;
            return (Direction)((int)coordinate + 1);
        }
        public static bool EqualSign(this Direction coordinate1, Direction coordinate2)
        {
            return IsPositive(coordinate1) == IsPositive(coordinate2);
        }
    }

    public static class AxisExtensions
    {
        public static bool IsPositive(this Axis axis) => int.IsEvenInteger((int)axis);
        public static bool IsNegative(this Axis axis) => int.IsOddInteger((int)axis);
        public static Axis ToPositive(this Axis axis)
        {
            if (IsPositive(axis)) return axis;
            return (Axis)((int)axis - 1);
        }
        public static Axis ToNegative(this Axis axis)
        {
            if (IsNegative(axis)) return axis;
            return (Axis)((int)axis + 1);
        }
        public static bool EqualSign(this Axis axis1, Axis axis2)
        {
            return IsPositive(axis1) == IsPositive(axis2);
        }
        public static string ToAxisString(this Axis axis)
        {
            switch(axis)
            {
                case Axis.X: return "X";
                case Axis.NX: return "-X";
                case Axis.Y: return "Y";
                case Axis.NY: return "-Y";
                case Axis.Z: return "Z";
                case Axis.NZ: return "-Z";
            }
            throw new ArgumentException("Invalid axis value.");
        }
        public static bool IsX(this Axis axis) => ToPositive(axis) == Axis.X;
        public static bool IsY(this Axis axis) => ToPositive(axis) == Axis.Y;
        public static bool IsZ(this Axis axis) => ToPositive(axis) == Axis.Z;
    }
}

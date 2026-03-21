using NFMRadTools.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Vector3D() { }
        public Vector3D(double xyz) { X = xyz; Y = xyz; Z = xyz; }
        public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static Vector3D Zero => new Vector3D(0.0);
        public static Vector3D VectorX => new Vector3D(1.0, 0.0, 0.0);
        public static Vector3D VectorXY => new Vector3D(1.0, 1.0, 0.0);
        public static Vector3D VectorXZ => new Vector3D(1.0, 0.0, 1.0);
        public static Vector3D VectorXYZ => new Vector3D(1.0, 1.0, 1.0);
        public static Vector3D VectorY => new Vector3D(0.0, 1.0, 0.0);
        public static Vector3D VectorYZ => new Vector3D(0.0, 1.0, 1.0);
        public static Vector3D VectorZ => new Vector3D(0.0, 0.0, 1.0);

        public override bool Equals(object obj)
        {
            if (obj is Vector3D other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public bool NearlyEquals(Vector3D other, double Tolerance = 0.00001)
        {
            bool bX = double.Abs(X - other.X) <= Tolerance;
            bool bY = double.Abs(Y - other.Y) <= Tolerance;
            bool bZ = double.Abs(Z - other.Z) <= Tolerance;
            return bX && bY && bZ;
        }

        public bool Equals(Vector3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }
        public static Vector3D operator+(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3D operator-(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3D operator*(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        public static Vector3D operator/(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        public static bool Equals(Vector3D A, Vector3D B) => A.Equals(B);
        public static bool NearlyEquals(Vector3D A, Vector3D B, double Tolerance = 0.00001) => A.NearlyEquals(B, Tolerance);
        
        public static Vector3D Add(Vector3D A, Vector3D B) => A + B;
        public static Vector3D Subtract(Vector3D A, Vector3D B) => A - B;
        public static Vector3D Multiply(Vector3D A, Vector3D B) => A * B;
        public static Vector3D Divide(Vector3D A, Vector3D B) => A / B;
        public static Vector3D Normalize(Vector3D Value)
        {
            double max = Max(Abs(Value));
            if (max == 0)
            {
                double normalizedZero = 1.0 / 3.0;
                return new Vector3D(normalizedZero);
            }
            return Value / new Vector3D(max);
        }
        public static Vector3D Abs(Vector3D Value) => new Vector3D(double.Abs(Value.X), double.Abs(Value.Y), double.Abs(Value.Z));
        public static Vector3D Negate(Vector3D Value) => Value * new Vector3D(-1.0);
        public static Vector3D Distance(Vector3D A, Vector3D B) => Abs(A - B);
        public static Vector3D MinComponents(Vector3D A, Vector3D B) => new Vector3D(double.Min(A.X, B.X), double.Min(A.Y, B.Y), double.Min(A.Z, B.Z));
        public static Vector3D MaxComponents(Vector3D A, Vector3D B) => new Vector3D(double.Max(A.X, B.X), double.Max(A.Y, B.Y), double.Max(A.Z, B.Z));
        public static double Sum(Vector3D Value) => Value.X + Value.Y + Value.Z;
        public static double Max(Vector3D Value) => double.Max(Value.X, double.Max(Value.Y, Value.Z));
        public static double Min(Vector3D Value) => double.Min(Value.X, double.Min(Value.Y, Value.Z));
        public static double Length(Vector3D Value) => double.Sqrt(Sum(Value * Value));
        public static Vector3D Mid(Vector3D A, Vector3D B)
        {
            Vector3D minComps = Vector3D.MinComponents(A, B);
            Vector3D maxComps = Vector3D.MaxComponents(A, B);
            return minComps + (maxComps - minComps) / new Vector3D(2.0);
            //A + (A - B) / new Vector3D(2.0);
        }
        public static Vector3D Cross(Vector3D A, Vector3D B) => new Vector3D(A.Y * B.Z - A.Z * B.Y, A.Z * B.X - A.X * B.Z, A.X * B.Y - A.Y * B.X);
        public static Vector3D Swizzle(Vector3D Value, Axis XAxis = Axis.X, Axis YAxis = Axis.Y, Axis ZAxis = Axis.Z)
        {
            double x = GetSwizzledValue(Value, XAxis);
            double y = GetSwizzledValue(Value, YAxis);
            double z = GetSwizzledValue(Value, ZAxis);
            return new Vector3D(x, y, z);
        }
        public static Vector3D SwizzleCoordsBound(Vector3D Value, CoordinateSystem ValueCoordinates, CoordinateSystem DesiredCoordinates)
        {
            if(ValueCoordinates == DesiredCoordinates) return Value;
            Direction cX = DesiredCoordinates.XAxis;
            Direction cY = DesiredCoordinates.YAxis;
            Direction cZ = DesiredCoordinates.ZAxis;

            double x = GetBoundSwizzledValue(Value, ValueCoordinates, cX);
            double y = GetBoundSwizzledValue(Value, ValueCoordinates, cY);
            double z = GetBoundSwizzledValue(Value, ValueCoordinates, cZ);

            return new Vector3D(x, y, z);
        }
        private static double GetSwizzledValue(Vector3D value, Axis axis)
        {
            switch(axis)
            {
                case Axis.X: return value.X;
                case Axis.Y: return value.Y;
                case Axis.Z: return value.Z;
                case Axis.NX: return value.X * -1.0;
                case Axis.NY: return value.Y * -1.0;
                case Axis.NZ: return value.Z * -1.0;
            }
            throw new ArgumentException();
        }
        private static double GetBoundSwizzledValue(Vector3D value, CoordinateSystem boundCoords, Direction direction)
        {
            Axis axis = boundCoords.GetDirectionAxis(direction);
            if (!boundCoords.GetAxisDirection(axis).EqualSign(direction))
                axis = axis.ToNegative();
            return GetSwizzledValue(value, axis);
        }
    }
}

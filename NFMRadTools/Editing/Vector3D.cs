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

        public static Vector3D Abs(Vector3D Value) => new Vector3D(double.Abs(Value.X), double.Abs(Value.Y), double.Abs(Value.Z));
        public static Vector3D Negate(Vector3D Value) => Value * new Vector3D(-1.0);
        public static Vector3D Distance(Vector3D A, Vector3D B) => Abs(A - B);
        public static double Sum(Vector3D Value) => Value.X + Value.Y + Value.Z;
        public static double Max(Vector3D Value) => double.Max(Value.X, double.Max(Value.Y, Value.Z));
        public static double Min(Vector3D Value) => double.Min(Value.X, double.Min(Value.Y, Value.Z));
        public static double Length(Vector3D Value) => double.Sqrt(Sum(Value * Value));
        public static Vector3D Mid(Vector3D A, Vector3D B) => A + Distance(A, B) / new Vector3D(2.0);
        public static Vector3D Cross(Vector3D A, Vector3D B) => new Vector3D(A.Y * B.Z - A.Z * B.Y, A.Z * B.X - A.X * B.Z, A.X * B.Y - A.Y * B.X);
    }
}

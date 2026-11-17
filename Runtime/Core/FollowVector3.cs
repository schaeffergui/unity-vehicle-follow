using System;

namespace VehicleFollow.Core
{
    /// <summary>
    /// Lightweight, engine-independent 3D vector for pure C# steering and pose math.
    /// </summary>
    public readonly struct FollowVector3 : IEquatable<FollowVector3>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public FollowVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static FollowVector3 Zero => new FollowVector3(0f, 0f, 0f);
        public static FollowVector3 Up => new FollowVector3(0f, 1f, 0f);
        public static FollowVector3 Forward => new FollowVector3(0f, 0f, 1f);
        public static FollowVector3 Right => new FollowVector3(1f, 0f, 0f);

        public float LengthSquared => X * X + Y * Y + Z * Z;
        public float Length => (float)Math.Sqrt(LengthSquared);

        public FollowVector3 Normalized
        {
            get
            {
                float len = Length;
                return len > 1e-6f ? new FollowVector3(X / len, Y / len, Z / len) : Zero;
            }
        }

        public static FollowVector3 operator +(FollowVector3 a, FollowVector3 b) =>
            new FollowVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static FollowVector3 operator -(FollowVector3 a, FollowVector3 b) =>
            new FollowVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static FollowVector3 operator *(FollowVector3 v, float scalar) =>
            new FollowVector3(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static FollowVector3 operator *(float scalar, FollowVector3 v) =>
            new FollowVector3(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static float Dot(FollowVector3 a, FollowVector3 b) =>
            a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static FollowVector3 Cross(FollowVector3 a, FollowVector3 b) =>
            new FollowVector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);

        public static float Distance(FollowVector3 a, FollowVector3 b) =>
            (a - b).Length;

        public bool Equals(FollowVector3 other) =>
            X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        public override bool Equals(object obj) =>
            obj is FollowVector3 other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(X, Y, Z);

        public override string ToString() =>
            $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}

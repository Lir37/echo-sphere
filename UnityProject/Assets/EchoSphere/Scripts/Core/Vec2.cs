using System;

namespace EchoSphere.Core
{
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public static readonly Vec2 Zero = new Vec2(0f, 0f);
        public readonly float X;
        public readonly float Y;
        public Vec2(float x, float y) { X = x; Y = y; }
        public float LengthSquared => X * X + Y * Y;
        public float Length => (float)Math.Sqrt(LengthSquared);
        public Vec2 Normalized { get { var length = Length; return length <= 0.0001f ? Zero : this / length; } }
        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
        public static float Distance(Vec2 a, Vec2 b) => (a - b).Length;
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float value) => new Vec2(a.X * value, a.Y * value);
        public static Vec2 operator /(Vec2 a, float value) => value == 0f ? Zero : new Vec2(a.X / value, a.Y / value);
        public bool Equals(Vec2 other) => Math.Abs(X - other.X) <= 0.0001f && Math.Abs(Y - other.Y) <= 0.0001f;
        public override bool Equals(object obj) => obj is Vec2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public readonly struct FormationNode
    {
        public readonly Vec2 Position;
        public readonly bool Alive;
        public readonly float NetworkDisabledTimer;
        public FormationNode(Vec2 position, bool alive, float networkDisabledTimer = 0f)
        { Position = position; Alive = alive; NetworkDisabledTimer = networkDisabledTimer; }
        public bool IsActive => Alive && NetworkDisabledTimer <= 0f;
    }
}

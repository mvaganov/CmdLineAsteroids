using System;
using System.Drawing;

namespace asteroids {
	public struct Vec2 {
		public float x, y;
		public float X { get => x; set => x = value; }
		public float Y { get => y; set => y = value; }
		public Vec2(float x, float y) { this.x = x; this.y = y; }
		public static implicit operator Vec2((float X, float Y) tuple) => new Vec2(tuple.X, tuple.Y);
		public static implicit operator Point(Vec2 v) => new Point((int)v.X, (int)v.Y);
		public static implicit operator Vec2(Point v) => new Vec2(v.X, v.Y);
		public static Vec2 operator +(Vec2 a, Vec2 b) => (a.X + b.X, a.Y + b.Y);
		public static Vec2 operator -(Vec2 a, Vec2 b) => (a.X - b.X, a.Y - b.Y);
		public static Vec2 operator *(Vec2 a, float scalar) => (a.X * scalar, a.Y * scalar);
		public static Vec2 operator /(Vec2 a, float scalar) => (a.X / scalar, a.Y / scalar);
		public override string ToString() => $"({x},{y})";
		public Vec2 Scaled(Vec2 scale) => (x * scale.X, y * scale.Y);
		public Vec2 InverseScaled(Vec2 scale) => (x / scale.X, y / scale.Y);
		public void Scale(Vec2 scale) { x *= scale.X; y *= scale.Y; }
		public void InverseScale(Vec2 scale) { x /= scale.X; y /= scale.Y; }
		public void Floor() { x = MathF.Floor(x); y = MathF.Floor(y); }
		public void Ceil() { x = MathF.Ceiling(x); y = MathF.Ceiling(y); }
		public static float DegreesToRadians(float degrees) => degrees * MathF.PI / 180;
		public float UnitVectorToRadians() => MathF.Atan2(y, x);
		public float UnitVectorToDegrees() => UnitVectorToRadians() * 180 / MathF.PI;
		public static Vec2 UnitVectorFromRadians(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));
		public static Vec2 UnitVectorFromDegrees(float degrees) => UnitVectorFromRadians(DegreesToRadians(degrees));
		public Vec2 RotatedRadians(float radians) {
			Vec2 rot = UnitVectorFromRadians(radians);
			return new Vec2(rot.X * X - rot.Y * Y, rot.Y * X + rot.X * Y);
		}
		public Vec2 RotatedDegrees(float degrees) => RotatedRadians(DegreesToRadians(degrees));
		public static Vec2 Zero = (0, 0);
		public static Vec2 One = (1, 1);
		public static Vec2 Half = (1f / 2, 1f / 2);
		public static Vec2 Max = (float.MaxValue, float.MaxValue);
		public static Vec2 Min = (float.MinValue, float.MinValue);
		public static Vec2 DirectionMinX = (-1, 0);
		public static Vec2 DirectionMaxX = (1, 0);
		public static Vec2 DirectionMinY = (0, -1);
		public static Vec2 DirectionMaxY = (0, 1);

		public static Vec2 Random => ((float)randomGenerator.NextDouble(), (float)randomGenerator.NextDouble());
		public static System.Random randomGenerator = new System.Random();
	}
}

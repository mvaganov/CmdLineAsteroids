using System;
using System.Drawing;

namespace MathMrV {
	public struct Vec2 {
		public float x, y;
		public float X { get => x; set => x = value; }
		public float Y { get => y; set => y = value; }
		public Vec2(float x, float y) { this.x = x; this.y = y; }
		public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
		public static implicit operator Point(Vec2 v) => new Point((int)v.x, (int)v.y);
		public static implicit operator Vec2(Point v) => new Vec2(v.X, v.Y);
		public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
		public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
		public static Vec2 operator -(Vec2 a) => new Vec2(-a.x, -a.y);
		public static Vec2 operator *(Vec2 a, float scalar) => new Vec2(a.x * scalar, a.y * scalar);
		public static Vec2 operator *(float scalar, Vec2 a) => new Vec2(a.x * scalar, a.y * scalar);
		public static Vec2 operator /(Vec2 a, float scalar) => new Vec2(a.x / scalar, a.y / scalar);
		public static bool operator ==(Vec2 a, Vec2 b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(Vec2 a, Vec2 b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) => obj is Vec2 v && this == v;
		public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
		public override string ToString() => $"({x},{y})";
		public float MagnitudeSqr => x * x + y * y;
		public float Magnitude => MathF.Sqrt(MagnitudeSqr);
		public Vec2 Normal => this / Magnitude;
		public Vec2 Scaled(Vec2 scale) => new Vec2(x * scale.x, y * scale.y);
		public Vec2 InverseScaled(Vec2 scale) => new Vec2(x / scale.x, y / scale.y);
		public void Scale(Vec2 scale) { x *= scale.x; y *= scale.y; }
		public void InverseScale(Vec2 scale) { x /= scale.x; y /= scale.y; }
		public void Floor() { x = MathF.Floor(x); y = MathF.Floor(y); }
		public void Ceil() { x = MathF.Ceiling(x); y = MathF.Ceiling(y); }
		public float Distance(Vec2 other) => (this - other).Magnitude;
		internal static float Distance(Vec2 a, Vec2 b) => a.Distance(b);

		public static float DegreesToRadians(float degrees) => degrees * MathF.PI / 180;
		internal Vec2 ToUnitVector() {
			if (x == 0 && y == 0) return DirectionMaxX;
			float mag = Magnitude;
			return new Vec2(x / mag, y / mag);
		}
		public static float WrapRadian(float f) {
			while (f > MathF.PI) { f -= 2*MathF.PI; }
			while (f <= -MathF.PI) { f += 2*MathF.PI; }
			return f;
		}
		public float UnitVectorToRadians() => WrapRadian(MathF.Atan2(y, x));
		public float UnitVectorToDegrees() => UnitVectorToRadians() * 180 / MathF.PI;
		public static Vec2 UnitVectorFromRadians(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));
		public static Vec2 UnitVectorFromDegrees(float degrees) => UnitVectorFromRadians(DegreesToRadians(degrees));
		public Vec2 RotatedRadians(float radians) => Rotated(UnitVectorFromRadians(radians));

		public Vec2 Rotated(Vec2 dir) => new Vec2(dir.x * x - dir.y * y, dir.y * x + dir.x * y);
		public void Rotate(Vec2 dir) => this = Rotated(dir);

		public void RotateRadians(float radians) => this = RotatedRadians(radians);
		public Vec2 RotatedDegrees(float degrees) => RotatedRadians(DegreesToRadians(degrees));
		public void RotateDegrees(float degrees) => this = RotatedDegrees(degrees);
		public bool IsWithin(Vec2 minInclusive, Vec2 maxExclusive) {
			return x >= minInclusive.x && y >= minInclusive.y && x < maxExclusive.x && y < maxExclusive.y;
		}
		public Vec2 FlippedXY() => new Vec2(y, x);
		public Vec2 Perpendicular() => new Vec2(-y, x);
		public void ClampToInt() { x = (int)x; y = (int)y; }
		public void RoundToInt() { x = MathF.Round(x); y = MathF.Round(y); }
		public static float Dot(Vec2 a, Vec2 b) {
			return a.x * b.x + a.y * b.y;
		}
		public static Vec2 Reflect(Vec2 incomingVector, Vec2 normalVector) {
			//normalVector = normalVector.FlippedXY();
			return incomingVector - (normalVector * 2 * Vec2.Dot(incomingVector, normalVector));
		}

		public static Vec2 Zero = (0, 0);
		public static Vec2 One = (1, 1);
		public static Vec2 Half = (1f / 2, 1f / 2);
		public static Vec2 Max = (float.MaxValue, float.MaxValue);
		public static Vec2 Min = (float.MinValue, float.MinValue);
		public static Vec2 DirectionMinX = (-1, 0);
		public static Vec2 DirectionMaxX = (1, 0);
		public static Vec2 DirectionMinY = (0, -1);
		public static Vec2 DirectionMaxY = (0, 1);
		public static Vec2 NaN = (float.NaN, float.NaN);
		public bool IsNaN() => float.IsNaN(x) || float.IsNaN(y);

		public static Vec2 RandomDirection => UnitVectorFromDegrees((float)randomGenerator.NextDouble()*360);
		public static System.Random randomGenerator = new System.Random();
	}
}

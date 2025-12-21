using System;
using System.Drawing;

namespace MathMrV {
	// TODO replace usage of Vec2 with Vector2, for optimization purposes.
	// extend the Vector2 class with `Rotated` and other methods.
	//public static class Vector2Extention {
	//	public static Vector2 Rotated(this Vector2 self, Vector2 dir) =>
	//		new Vector2(dir.X * self.X - dir.Y * self.Y, dir.Y * self.X + dir.X * self.Y);
	//}
	public struct Vec2 {
		public float X, Y;
		public float Magnitude => MathF.Sqrt(MagnitudeSqr);
		public float MagnitudeSqr => X * X + Y * Y;
		public Vec2 Normal {
			get {
				if (X == 0 && Y == 0) { return DirectionMaxX; }
				float mag = Magnitude;
				return new Vec2(X / mag, Y / mag);
			}
		}
		public Vec2(float x, float y) { this.X = x; this.Y = y; }
		public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
		public static implicit operator Point(Vec2 v) => new Point((int)v.X, (int)v.Y);
		public static implicit operator Vec2(Point v) => new Vec2(v.X, v.Y);
		public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
		public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
		public static Vec2 operator -(Vec2 a) => new Vec2(-a.X, -a.Y);
		public static Vec2 operator *(Vec2 a, float scalar) => new Vec2(a.X * scalar, a.Y * scalar);
		public static Vec2 operator *(float scalar, Vec2 a) => new Vec2(a.X * scalar, a.Y * scalar);
		public static Vec2 operator /(Vec2 a, float scalar) => new Vec2(a.X / scalar, a.Y / scalar);
		public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
		public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);
		public bool Equals(Vec2 other) => other.X == X && other.Y == Y;
		public override bool Equals(object obj) => obj is Vec2 vec2 && Equals(vec2);
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
		public override string ToString() => $"({X},{Y})";
		public float Distance(Vec2 other) => (this - other).Magnitude;
		public float DistanceSquared(Vec2 other) => (this - other).MagnitudeSqr;
		internal static float Distance(Vec2 a, Vec2 b) => a.Distance(b);
		internal static float DistanceSquared(Vec2 a, Vec2 b) => a.DistanceSquared(b);
		public Vec2 Scaled(Vec2 scale) => new Vec2(X * scale.X, Y * scale.Y);
		public Vec2 InverseScaled(Vec2 scale) => new Vec2(X / scale.X, Y / scale.Y);
		public void Scale(Vec2 scale) { X *= scale.X; Y *= scale.Y; }
		public void InverseScale(Vec2 scale) { X /= scale.X; Y /= scale.Y; }
		public void Floor() { X = MathF.Floor(X); Y = MathF.Floor(Y); }
		public void Ceil() { X = MathF.Ceiling(X); Y = MathF.Ceiling(Y); }
		public static float DegreesToRadians(float degrees) => degrees * MathF.PI / 180;
		public void Normalize() { float mag = Magnitude; X /= mag; Y /= mag; }
		public static Vec2 NormalFromRadians(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));
		public static Vec2 NormalFromDegrees(float degrees) => NormalFromRadians(DegreesToRadians(degrees));
		public float NormalToRadians() => WrapRadian(MathF.Atan2(Y, X));
		public float NormalToDegrees() => NormalToRadians() * 180 / MathF.PI;
		public static float WrapRadian(float radian) {
			while (radian > MathF.PI) { radian -= 2 * MathF.PI; }
			while (radian <= -MathF.PI) { radian += 2 * MathF.PI; }
			return radian;
		}
		public static float WrapDegrees(float degree) {
			while (degree > 180) { degree -= 2 * 180; }
			while (degree <= -MathF.PI) { degree += 2 * 180; }
			return degree;
		}
		public Vec2 RotatedRadians(float radians) => Rotated(NormalFromRadians(radians));
		public Vec2 Rotated(Vec2 dir) => new Vec2(dir.X * X - dir.Y * Y, dir.Y * X + dir.X * Y);
		public Vec2 Unrotated(Vec2 dir) => new Vec2(dir.X * X - -dir.Y * Y,-dir.Y * X + dir.X * Y);
		public void Rotate(Vec2 dir) => this = Rotated(dir);
		public void Unrotate(Vec2 dir) => this = Unrotated(dir);
		public void RotateRadians(float radians) => this = RotatedRadians(radians);
		public Vec2 RotatedDegrees(float degrees) => RotatedRadians(DegreesToRadians(degrees));
		public void RotateDegrees(float degrees) => this = RotatedDegrees(degrees);
		public bool IsWithin(Vec2 minInclusive, Vec2 maxExclusive) {
			return X >= minInclusive.X && Y >= minInclusive.Y && X < maxExclusive.X && Y < maxExclusive.Y;
		}
		public Vec2 FlippedXY() => new Vec2(Y, X);
		public Vec2 Perpendicular() => new Vec2(-Y, X);
		public void ClampToInt() { X = (int)X; Y = (int)Y; }
		public void RoundToInt() { X = MathF.Round(X); Y = MathF.Round(Y); }
		public static float Dot(Vec2 a, Vec2 b) {
			return a.X * b.X + a.Y * b.Y;
		}
		public static Vec2 Reflect(Vec2 incomingVector, Vec2 surfaceNormal) {
			return incomingVector - (surfaceNormal * 2 * Vec2.Dot(incomingVector, surfaceNormal));
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
		public bool IsNaN() => IsNaN(this);
		public static bool IsNaN(Vec2 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y);
		public bool IsZero() => IsZero(this);
		public static bool IsZero(Vec2 vec) => vec.X == 0 && vec.Y == 0;
		public static float Cross(Vec2 a, Vec2 b) => (a.X * b.Y) - (a.Y * b.X);
		public static float Cross(Vec2 a, Vec2 b, Vec2 c) => (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
		public static bool TryGetLineSegmentIntersection(Vec2 startA, Vec2 endA, Vec2 startB, Vec2 endB, out Vec2 point) {
			Vec2 deltaA = endA - startA;
			Vec2 deltaB = endB - startB;
			float orthogonalityOfLines = Cross(deltaA, deltaB);
			bool isParallel = Math.Abs(orthogonalityOfLines) < 1e-5f;
			if (isParallel) {
				point = Vec2.NaN;
				return false;
			}
			Vec2 linesDelta = startB - startA;
			float orthogonalityOfLineA = Cross(linesDelta, deltaA);
			float orthogonalityOfLineB = Cross(linesDelta, deltaB);
			float whereLineBCrossesOnLineA = orthogonalityOfLineB / orthogonalityOfLines;
			float whereLineACrossesOnLineB = orthogonalityOfLineA / orthogonalityOfLines;
			point = startA + (deltaA * whereLineBCrossesOnLineA);
			bool doesLineBCollideLineA = whereLineBCrossesOnLineA >= 0f && whereLineBCrossesOnLineA <= 1f;
			bool doesLineACollideLineB = whereLineACrossesOnLineB >= 0f && whereLineACrossesOnLineB <= 1f;
			return (doesLineBCollideLineA && doesLineACollideLineB);
		}
	}
}

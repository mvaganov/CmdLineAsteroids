using System;
using System.Drawing;

namespace MathMrV {
	public struct Vec2 {
		public float x, y;
		public float X { get => x; set => x = value; }
		public float Y { get => y; set => y = value; }
		public float Magnitude => MathF.Sqrt(MagnitudeSqr);
		public float MagnitudeSqr => x * x + y * y;
		public Vec2 Normal {
			get {
				if (x == 0 && y == 0) { return DirectionMaxX; }
				float mag = Magnitude;
				return new Vec2(x / mag, y / mag);
			}
		}
		public Vec2(float x, float y) { this.x = x; this.y = y; }
		public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
		public static implicit operator Point(Vec2 v) => new Point((int)v.x, (int)v.y);
		public static implicit operator Vec2(Point v) => new Vec2(v.X, v.Y);
		public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
		public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
		public static Vec2 operator -(Vec2 a) => new Vec2(-a.x, -a.y);
		public static Vec2 operator *(Vec2 a, float scalar) => new Vec2(a.x * scalar, a.y * scalar);
		public static Vec2 operator *(float scalar, Vec2 a) => new Vec2(a.x * scalar, a.y * scalar);
		public static Vec2 operator /(Vec2 a, float scalar) => new Vec2(a.x / scalar, a.y / scalar);
		public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
		public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);
		public bool Equals(Vec2 other) => other.x == x && other.y == y;
		public override bool Equals(object obj) => obj is Vec2 vec2 && Equals(vec2);
		public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
		public override string ToString() => $"({x},{y})";
		public float Distance(Vec2 other) => (this - other).Magnitude;
		public float DistanceSquared(Vec2 other) => (this - other).MagnitudeSqr;
		internal static float Distance(Vec2 a, Vec2 b) => a.Distance(b);
		internal static float DistanceSquared(Vec2 a, Vec2 b) => a.DistanceSquared(b);
		public Vec2 Scaled(Vec2 scale) => new Vec2(x * scale.x, y * scale.y);
		public Vec2 InverseScaled(Vec2 scale) => new Vec2(x / scale.x, y / scale.y);
		public void Scale(Vec2 scale) { x *= scale.x; y *= scale.y; }
		public void InverseScale(Vec2 scale) { x /= scale.x; y /= scale.y; }
		public void Floor() { x = MathF.Floor(x); y = MathF.Floor(y); }
		public void Ceil() { x = MathF.Ceiling(x); y = MathF.Ceiling(y); }
		public static float DegreesToRadians(float degrees) => degrees * MathF.PI / 180;
		public void Normalize() { float mag = Magnitude; x /= mag; y /= mag; }
		public static Vec2 NormalFromRadians(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));
		public static Vec2 NormalFromDegrees(float degrees) => NormalFromRadians(DegreesToRadians(degrees));
		public float NormalToRadians() => WrapRadian(MathF.Atan2(y, x));
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
		public Vec2 Rotated(Vec2 dir) => new Vec2(dir.x * x - dir.y * y, dir.y * x + dir.x * y);
		public Vec2 Unrotated(Vec2 dir) => new Vec2(dir.x * x - -dir.y * y,-dir.y * x + dir.x * y);
		public void Rotate(Vec2 dir) => this = Rotated(dir);
		public void Unrotate(Vec2 dir) => this = Unrotated(dir);
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
		public static bool IsNaN(Vec2 vec) => float.IsNaN(vec.x) || float.IsNaN(vec.y);
		public bool IsZero() => IsZero(this);
		public static bool IsZero(Vec2 vec) => vec.x == 0 && vec.y == 0;
		public static bool TryGetLineSegmentIntersection(Vec2 startA, Vec2 endA, Vec2 startB, Vec2 endB, out Vec2 point) {
			Vec2 deltaA = endA - startA;
			Vec2 deltaB = endB - startB;
			// cross-product can be intuited as value for "how different are the directions of lines"
			float orthogonalityOfLines = (deltaA.X * deltaB.Y) - (deltaA.Y * deltaB.X);
			bool isParallel = Math.Abs(orthogonalityOfLines) < 1e-5f;
			if (isParallel) {
				point = Vec2.NaN;
				return false;
			}
			Vec2 lineDelta = startB - startA;
			float orthogonalityOfLineA = (lineDelta.X * deltaA.Y) - (lineDelta.Y * deltaA.X);
			float orthogonalityOfLineB = (lineDelta.X * deltaB.Y) - (lineDelta.Y * deltaB.X);
			float whereLineBCrossesOnLineA = orthogonalityOfLineB / orthogonalityOfLines;
			float whereLineACrossesOnLineB = orthogonalityOfLineA / orthogonalityOfLines;
			point = startA + (deltaA * whereLineBCrossesOnLineA);
			bool doesLineBCollideLineA = whereLineBCrossesOnLineA >= 0f && whereLineBCrossesOnLineA <= 1f;
			bool doesLineACollideLineB = whereLineACrossesOnLineB >= 0f && whereLineACrossesOnLineB <= 1f;
			return (doesLineBCollideLineA && doesLineACollideLineB);
		}
	}
}

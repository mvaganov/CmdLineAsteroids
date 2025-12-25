using ConsoleMrV;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MathMrV {
	public struct Circle {
		public Vec2 Center;
		public float Radius;
		public float Area => Radius * Radius * MathF.PI;
		public float InertiaWithoutDensity => 0.5f * Area * Radius * Radius;

		public static Circle NaN = new Circle(Vec2.NaN, float.NaN);

		public Circle(Vec2 center, float radius) { Center = center; Radius = radius; }
		public override string ToString() => $"({Center}, r:{Radius})";
		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.X - position.X, dy = point.Y - position.Y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(Center, Radius, point);
		public void Draw(CommandLineCanvas canvas) => Draw(canvas, Center, Radius);
		public static void Draw(CommandLineCanvas canvas, Vec2 pos, float radius) {
			if (!TryGetAABB(pos, radius, out Vec2 min, out Vec2 max)) {
				return;
			}
			canvas.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) => Circle.IsInsideCircle(pos, radius, point);
		}
		public bool IsColliding(Circle other) => IsColliding(Center, Radius, other.Center, other.Radius);
		public bool IsColliding(Vec2 otherCenter, float otherRadius) =>
			IsColliding(Center, Radius, otherCenter, otherRadius);
		public static bool IsColliding(Vec2 centerA, float radiusA, Vec2 centerB, float radiusB) {
			float dx = centerA.X - centerB.X;
			float dy = centerA.Y - centerB.Y;
			float r = radiusA + radiusB;
			return dx * dx + dy * dy < r * r;
		}
		public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(Center, Radius, out min, out max);
		public static bool TryGetCircleCollision(Circle a, Circle b, out Vec2 delta, out float depth) {
			delta = b.Center - a.Center;
			float dist = delta.Length();
			float totalRad = a.Radius + b.Radius;
			depth = totalRad - dist;
			return depth > 0;
		}
		public static bool TryGetCircleCollision(Circle a, Circle b, out Vec2 overlapCenter) {
			bool collision = TryGetCircleCollision(a, b, out Vec2 delta, out float depth);
			Vec2 dir = delta.Normal;
			overlapCenter = a.Center + dir * (a.Radius - (depth/2));
			return collision;
		}
		public static bool TryGetAABB(Vec2 center, float radius, out Vec2 min, out Vec2 max) {
			Vec2 extent = (radius, radius);
			min = center - extent;
			max = center + extent;
			return radius > 0;
		}
		public bool IntersectsAABB(AABB aabb) => IntersectsAABB(aabb.Min, aabb.Max);
		public bool IntersectsAABB(Vec2 min, Vec2 max) {
			if (AABB.Contains(Center, min, max)) { return true; }
			Vec2 radSize = new Vec2(Radius, Radius);
			Vec2 expandedMin = min - radSize;
			Vec2 expandedMax = max + radSize;
			if (AABB.Contains(Center, expandedMin, expandedMax)) {
				Vec2 cornerCase = Vec2.NaN;
				if (Center.X < min.X) {
					if (Center.Y < min.Y) {
						cornerCase = min;
					} else if (Center.Y > max.Y) {
						cornerCase = new Vec2(min.X, max.Y);
					}
				} else if (Center.X > max.X) {
					if (Center.Y < min.Y) {
						cornerCase = new Vec2(max.X, min.Y);
					} else if (Center.Y > max.Y) {
						cornerCase = max;
					}
				}
				if (!cornerCase.IsNaN()) {
					float distanceSqr = (cornerCase - Center).LengthSquared();
					return distanceSqr <= Radius * Radius;
				}
				return true;
			}
			return false;
		}

		public static IList<Vec2> FindCircleCircleIntersections(Circle c1, Circle c2) {
			Vec2[] intersections = null;
			Vec2 delta = c2.Center - c1.Center;
			float quadrance = delta.LengthSquared();
			float d = MathF.Sqrt(quadrance);
			bool circlesTooFarApart = d > c1.Radius + c2.Radius;
			if (circlesTooFarApart) { return intersections; }
			bool oneCircleInsideTheOther = d < Math.Abs(c1.Radius - c2.Radius);
			if (oneCircleInsideTheOther) { return intersections = Array.Empty<Vec2>(); }
			bool circlesAreCoincident = (d == 0 && c1.Radius == c2.Radius);
			if (circlesAreCoincident) { return intersections; }
			float c1r2 = c1.Radius * c1.Radius;
			float c2r2 = c2.Radius * c2.Radius;
			float distanceC1ToRadicalLine = (c1r2 - c2r2 + quadrance) / (2 * d);
			float distanceBetweenCollisionPoints = MathF.Sqrt(c1r2 - distanceC1ToRadicalLine * distanceC1ToRadicalLine);
			Vec2 dir = delta / d;
			Vec2 radicalCenter = c1.Center + dir * distanceC1ToRadicalLine;
			Vec2 perpendicularDistanceBetweenPoints = dir.Perpendicular() * distanceBetweenCollisionPoints;
			Vec2 intersection = radicalCenter + perpendicularDistanceBetweenPoints;
			bool isTangent = d == c1.Radius + c2.Radius || d == Math.Abs(c1.Radius - c2.Radius);
			intersections = new Vec2[!isTangent ? 2 : 1];
			intersections[0] = intersection;
			if (!isTangent) {
				intersection = radicalCenter - perpendicularDistanceBetweenPoints;
				intersections[1] = intersection;
			}
			return intersections;
		}
	}
}

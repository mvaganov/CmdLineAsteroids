using ConsoleMrV;
using System;

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
			float dx = point.x - position.x, dy = point.y - position.y;
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
			float dx = centerA.x - centerB.x;
			float dy = centerA.y - centerB.y;
			float r = radiusA + radiusB;
			return dx * dx + dy * dy < r * r;
		}
		public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(Center, Radius, out min, out max);
		public static bool TryGetCircleCollision(Circle a, Circle b, out Vec2 delta, out float depth) {
			delta = b.Center - a.Center;
			float dist = delta.Magnitude;
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
				if (Center.x < min.x) {
					if (Center.y < min.y) {
						cornerCase = min;
					} else if (Center.y > max.y) {
						cornerCase = new Vec2(min.x, max.y);
					}
				} else if (Center.x > max.x) {
					if (Center.y < min.y) {
						cornerCase = new Vec2(max.x, min.y);
					} else if (Center.y > max.y) {
						cornerCase = max;
					}
				}
				if (!cornerCase.IsNaN()) {
					float distanceSqr = (cornerCase - Center).MagnitudeSqr;
					return distanceSqr <= Radius * Radius;
				}
				return true;
			}
			return false;
		}
	}
}

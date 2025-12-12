using ConsoleMrV;
using System;

namespace MathMrV {
	public struct Circle {
		public Vec2 center;
		public float radius;
		public Vec2 Position { get => center; set => center = value; }
		public float Radius { get => radius; set => radius = value; }
		public float Area => radius * radius * MathF.PI;
		public float InertiaWithoutDensity => 0.5f * Area * radius * radius;

		public static Circle NaN = new Circle(Vec2.NaN, float.NaN);

		public Circle(Vec2 position, float radius) { this.center = position; this.radius = radius; }
		public override string ToString() => $"({center}, r:{radius})";
		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.x - position.x, dy = point.y - position.y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(center, radius, point);
		public void Draw(CommandLineCanvas canvas) => Draw(canvas, center, radius);
		public static void Draw(CommandLineCanvas canvas, Vec2 pos, float radius) {
			if (!TryGetAABB(pos, radius, out Vec2 min, out Vec2 max)) {
				return;
			}
			canvas.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) => Circle.IsInsideCircle(pos, radius, point);
		}
		public bool IsColliding(Circle other) => IsColliding(center, radius, other.center, other.radius);
		public bool IsColliding(Vec2 otherCenter, float otherRadius) =>
			IsColliding(center, radius, otherCenter, otherRadius);
		public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(center, radius, out min, out max);
		public static bool TryGetCircleCollision(Circle a, Circle b, out Vec2 delta, out float depth) {
			delta = b.center - a.center;
			float dist = delta.Magnitude;
			float totalRad = a.radius + b.radius;
			depth = totalRad - dist;
			return depth > 0;
		}
		public static bool TryGetCircleCollision(Circle a, Circle b, out Vec2 overlapCenter) {
			bool collision = TryGetCircleCollision(a, b, out Vec2 delta, out float depth);
			Vec2 dir = delta.Normal;
			overlapCenter = a.center + dir * (a.radius - (depth/2));
			return collision;
		}
		public static bool IsColliding(Vec2 centerA, float radiusA, Vec2 centerB, float radiusB) {
			float dx = centerA.x - centerB.x;
			float dy = centerA.y - centerB.y;
			float r = radiusA + radiusB;
			return dx * dx + dy * dy < r * r;
		}
		public static bool TryGetAABB(Vec2 center, float radius, out Vec2 min, out Vec2 max) {
			Vec2 extent = (radius, radius);
			min = center - extent;
			max = center + extent;
			return radius > 0;
		}
		public bool IntersectsAABB(AABB aabb) => IntersectsAABB(aabb.Min, aabb.Max);
		public bool IntersectsAABB(Vec2 min, Vec2 max) {
			if (AABB.Contains(center, min, max)) {
				return true;
			}
			Vec2 radSize = new Vec2(radius, radius);
			Vec2 expandedMin = min - radSize;
			Vec2 expandedMax = max + radSize;
			if (AABB.Contains(center, expandedMin, expandedMax)) {
				Vec2 cornerCase = Vec2.NaN;
				if (center.x < min.x) {
					if (center.y < min.y) {
						cornerCase = min;
					} else if (center.y > max.y) {
						cornerCase = new Vec2(min.x, max.y);
					}
				} else if (center.x > max.x) {
					if (center.y < min.y) {
						cornerCase = new Vec2(max.x, min.y);
					} else if (center.y > max.y) {
						cornerCase = max;
					}
				}
				if (!cornerCase.IsNaN()) {
					float distanceSqr = (cornerCase - center).MagnitudeSqr;
					return distanceSqr <= radius * radius;
				}
				return true;
			}
			return false;
		}
	}
}

using ConsoleMrV;

namespace MathMrV {
	public struct Circle {
		public Vec2 center;
		public float radius;
		public Vec2 Position { get => center; set => center = value; }
		public float Radius { get => radius; set => radius = value; }
		public Circle(Vec2 position, float radius) {
			this.center = position;
			this.radius = radius;
		}
		public static void Draw(CommandLineCanvas canvas, Vec2 pos, float radius) {
			if (!TryGetAABB(pos, radius, out Vec2 min, out Vec2 max)) {
				return;
			}
			canvas.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) => Circle.IsInsideCircle(pos, radius, point);
		}
		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.x - position.x;
			float dy = point.y - position.y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(center, radius, point);

		public void Draw(CommandLineCanvas canvas) => Draw(canvas, center, radius);
		public bool IsColliding(Circle other) => IsColliding(center, radius, other.center, other.radius);
		public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(center, radius, out min, out max);
		public static bool TryGetCircleCollisionPoints(Circle a, Circle b, out Vec2 pointA, out Vec2 pointB) {
			Vec2 delta = b.center - a.center;
			float dist = delta.Magnitude;
			float totalRad = a.radius + b.radius;
			if (dist > totalRad) {
				pointA = a.center;
				pointB = b.center;
				return false;
			}
			Vec2 dir = delta / dist;
			pointA = a.center + dir * a.radius;
			pointB = b.center - dir * b.radius;
			return true;
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
	}
}

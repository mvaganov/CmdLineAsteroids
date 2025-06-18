
namespace asteroids {
	public struct Circle {
		public Vec2 position;
		public float radius;
		public Circle(Vec2 position, float radius) {
			this.position = position;
			this.radius = radius;
		}
		public static bool TryGetAABB(Vec2 center, float radius, out Vec2 min, out Vec2 max) {
			Vec2 extent = (radius, radius);
			min = center - extent;
			max = center + extent;
			return radius > 0;
		}
		public static void Draw(CommandLineGraphicsContext g, Vec2 pos, float radius) {
			if (!TryGetAABB(pos, radius, out Vec2 min, out Vec2 max)) {
				return;
			}
			float dx, dy;
			g.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) {
				dx = point.x - pos.x;
				dy = point.y - pos.y;
				return (dx * dx + dy * dy <= radius * radius);
			}
		}
		public void Draw(CommandLineGraphicsContext g) => Draw(g, position, radius);
		public static bool IsColliding(Vec2 centerA, float radiusA, Vec2 centerB, float radiusB) {
			float dx = centerA.x - centerB.x;
			float dy = centerA.y - centerB.y;
			float r = radiusA + radiusB;
			return dx * dx + dy * dy < r * r;
		}
		public bool IsColliding(Circle other) => IsColliding(position, radius, other.position, other.radius);
	}
}

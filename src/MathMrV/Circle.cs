using ConsoleMrV;

namespace MathMrV {
	public struct Circle {
		public Vec2 position;
		public float radius;
		public Vec2 Position { get => position; set => position = value; }
		public float Radius { get => radius; set => radius = value; }
		public Circle(Vec2 position, float radius) {
			this.position = position;
			this.radius = radius;
		}
		public static void Draw(CommandLineGraphicsContext g, Vec2 pos, float radius) {
			if (!TryGetAABB(pos, radius, out Vec2 min, out Vec2 max)) {
				return;
			}
			g.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) => Circle.IsInsideCircle(pos, radius, point);
		}
		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.x - position.x;
			float dy = point.y - position.y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(position, radius, point);

		public void Draw(CommandLineGraphicsContext g) => Draw(g, position, radius);
		public bool IsColliding(Circle other) => IsColliding(position, radius, other.position, other.radius);
		public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(position, radius, out min, out max);
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

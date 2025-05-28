using System;
using System.Collections.Generic;
using System.Text;
using static asteroids.Program;

namespace asteroids {
	public struct Circle {
		public Vec2 position;
		public float radius;
		public Circle(Vec2 position, float radius) {
			this.position = position;
			this.radius = radius;
		}
		public static void Draw(CmdLineBufferGraphicsContext g, Vec2 pos, float radius) {
			Vec2 extent = (radius, radius);
			Vec2 min = pos - extent;
			Vec2 max = pos + extent;
			float r2 = radius * radius, dx, dy;
			g.DrawSupersampledShape(IsInsideCircle, min, max);
			bool IsInsideCircle(Vec2 point) {
				dx = point.X - pos.X;
				dy = point.Y - pos.Y;
				return (dx * dx + dy * dy <= r2);
			}
		}
		public void Draw(CmdLineBufferGraphicsContext g) => Draw(g, position, radius);
	}
}

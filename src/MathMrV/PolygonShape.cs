using System;
using System.Collections.Generic;

namespace MathMrV {
	public struct PolygonShape {
		private Vec2[] points;

		public PolygonShape(Vec2[] points) {
			this.points = points;
		}
		public override string ToString() => $"(polygon: {string.Join(", ", points)})";
		public Vec2[] Points { get => points; set { points = value; } }
		public int Count => points.Length;
		public Vec2 GetPoint(int index) => points[index];
		public void SetPoint(int index, Vec2 point) => points[index] = point;
		public static bool IsInPolygon(IList<Vec2> poly, Vec2 pt) {
			bool inside = false;
			for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++) {
				Vec2 pi = poly[i], pj = poly[j];
				bool intersect = pi.y > pt.y != pj.y > pt.y && pt.x < (pj.x - pi.x) * (pt.y - pi.y) / (pj.y - pi.y) + pi.x;
				if (intersect) {
					inside = !inside;
				}
			}
			return inside;
		}
		public static bool TryGetAABB(IList<Vec2> points, out Vec2 min, out Vec2 max) {
			min = Vec2.Max;
			max = Vec2.Min;
			if (points.Count == 0) { return false; }
			for (int i = 0; i < points.Count; ++i) {
				Vec2 p = points[i];
				if (p.x < min.x) { min.x = p.x; }
				if (p.y < min.y) { min.y = p.y; }
				if (p.x > max.x) { max.x = p.x; }
				if (p.y > max.y) { max.y = p.y; }
			}
			return true;
		}
		public static Vec2[] CreateRegular(int sides, Vec2 startingPoint = default) {
			if (startingPoint == default) {
				startingPoint = Vec2.DirectionMaxX;
			}
			Vec2[] points = new Vec2[sides];
			Vec2 point = startingPoint;
			float radianTurn = MathF.PI * 2 / sides;
			for (int i = 0; i < sides; ++i) {
				points[i] = point;
				point.RotateRadians(radianTurn);
			}
			return points;
		}
	}
}

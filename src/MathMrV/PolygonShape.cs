using System;
using System.Collections.Generic;

namespace MathMrV {
	public struct PolygonShape {
		private Vec2[] points;

		public PolygonShape(Vec2[] points) {
			this.points = points;
			float area = CalculateArea(points);
			if (area < 0) {
				throw new ArgumentException("points must be CCW (counter-clockwise order)");
			}
		}
		public override string ToString() => $"(polygon: {string.Join(", ", points)})";
		public Vec2[] Points { get => points; set { points = value; } }
		public int Count => points.Length;
		public Vec2 GetPoint(int index) => points[index];
		public void SetPoint(int index, Vec2 point) => points[index] = point;
		public static float CalculateArea(IList<Vec2> vertices) {
			float totalArea = 0f;
			for (int i = 0; i < vertices.Count; i++) {
				Vec2 a = vertices[i], b = vertices[(i + 1) % vertices.Count];
				float areaOfParallelogramDefinedByABZero = Vec2.Cross(a, b);
				float triangleAreaSigned = 0.5f * areaOfParallelogramDefinedByABZero;
				totalArea += triangleAreaSigned; //negative values from concave crossproduct subtracts correctly
				float longnessTerm = Vec2.Dot(a, a) + Vec2.Dot(a, b) + Vec2.Dot(b, b);
			}
			return totalArea;
		}
		public static bool IsInPolygon(IList<Vec2> poly, Vec2 pt) {
			bool inside = false;
			for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++) {
				Vec2 pi = poly[i], pj = poly[j];
				bool intersect = pi.Y > pt.Y != pj.Y > pt.Y && pt.X < (pj.X - pi.X) * (pt.Y - pi.Y) / (pj.Y - pi.Y) + pi.X;
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
				if (p.X < min.X) { min.X = p.X; }
				if (p.Y < min.Y) { min.Y = p.Y; }
				if (p.X > max.X) { max.X = p.X; }
				if (p.Y > max.Y) { max.Y = p.Y; }
			}
			return true;
		}
		public static Vec2[] CreateRegular(int sides, Vec2 startingPoint = default) {
			if (startingPoint == default) {
				startingPoint = Vec2.UnitX;
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

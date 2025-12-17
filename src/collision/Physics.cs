using MathMrV;
using System;

namespace MrV.Physics {

	public static class InertiaCalculator {
		public static void CalculatePolygonAreaAndInertia(Vec2[] vertices, out float area, out float inertiaWithoutDensity) {
			float totalArea = 0f;
			float inertiaMomentNumeratorSum = 0f;
			for (int i = 0; i < vertices.Length; i++) {
				Vec2 a = vertices[i], b = vertices[(i + 1) % vertices.Length];
				float areaOfParallelogramDefinedByAZeroB = Vec2.Cross(a, b);
				float triangleAreaSigned = 0.5f * areaOfParallelogramDefinedByAZeroB;
				totalArea += triangleAreaSigned; //negative values from concave crossproduct subtracts correctly
				float longnessTerm = Vec2.Dot(a, a) + Vec2.Dot(a, b) + Vec2.Dot(b, b);
				inertiaMomentNumeratorSum += triangleAreaSigned * longnessTerm;
			}
			area = totalArea;
			float secondMomentOfArea = inertiaMomentNumeratorSum / 6.0f;
			inertiaWithoutDensity = Math.Abs(secondMomentOfArea);
		}

		public static void CalculateCircleAreaAndInertia(float radius, out float area, out float inertiaWithoutDensity) {
			area = MathF.PI * radius * radius;
			inertiaWithoutDensity = 0.5f * area * radius * radius;
		}

		/// <summary>
		/// Helper: Calculates the geometric center (centroid) of a polygon.
		/// You should subtract this from every vertex so the object rotates around its real center.
		/// </summary>
		public static Vec2 CalculateCentroid(Vec2[] points) {
			if (points.Length < 3) { return Vec2.Zero; }
			Vec2 centerSum = Vec2.Zero;
			float totalArea = 0f;
			for (int i = 0; i < points.Length; i++) {
				Vec2 v1 = points[i];
				Vec2 v2 = points[(i + 1) % points.Length];
				float cross = Vec2.Cross(v1, v2);
				float triangleArea = 0.5f * cross;
				totalArea += triangleArea;
				centerSum += (v1 + v2) * triangleArea; // Weighted by area
			}

			// Centroid formula involves dividing by 3x the area due to triangle centers being at 1/3 height
			// But since we weighted by triangleArea (which is 0.5 * cross), the math simplifies:
			// Centroid = (Sum of (v1 + v2) * cross) / (3 * Sum of cross)
			if (totalArea == 0) { return Vec2.Zero; }

			// The standard simplified centroid formula using cross product weights:
			return centerSum / (3f * totalArea);
		}
	}
}

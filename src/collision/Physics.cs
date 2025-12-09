using MathMrV;
using System;

namespace MrV.Physics {

	public static class InertiaCalculator {
		/// <summary>Works for convex and concave polygons</summary>
		/// <param name="inertiaWithoutDensity">If `mass != area`, multiply by `(mass/area)`, aka density</param>
		public static void CalculatePolygonAreaAndInertia(Vec2[] vertices, out float area, out float inertiaWithoutDensity) {
			float totalArea = 0f;
			float inertiaMomentNumeratorSum = 0f;
			for (int i = 0; i < vertices.Length; i++) {
				//Vec2 0 = Vec2.Zero;
				Vec2 a = vertices[i];
				Vec2 b = vertices[(i + 1) % vertices.Length];
				// 2D Cross Product to get 2x the signed area of the triangle (a, b, z)
				float crossProductABZ = a.X * b.Y - a.Y * b.X;//+(b.X*0.Y-b.Y*0.X)+(0.X*a.Y-0.Y*a.X);
				float triangleAreaSigned = 0.5f * crossProductABZ;
				totalArea += triangleAreaSigned;//negative values from concave cross prod subtracts correctly
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
		public static Vec2 CalculateCentroid(Vec2[] vertices) {
			if (vertices.Length < 3) return Vec2.Zero;

			Vec2 centerSum = Vec2.Zero;
			float totalArea = 0f;

			for (int i = 0; i < vertices.Length; i++) {
				Vec2 v1 = vertices[i];
				Vec2 v2 = vertices[(i + 1) % vertices.Length];

				float cross = (v1.X * v2.Y) - (v1.Y * v2.X);
				float triangleArea = 0.5f * cross;

				totalArea += triangleArea;
				centerSum += (v1 + v2) * triangleArea; // Weighted by area
			}

			// Centroid formula involves dividing by 3x the area due to triangle centers being at 1/3 height
			// But since we weighted by triangleArea (which is 0.5 * cross), the math simplifies:
			// Centroid = (Sum of (v1 + v2) * cross) / (3 * Sum of cross)

			if (totalArea == 0) return Vec2.Zero;

			// The standard simplified centroid formula using cross product weights:
			return centerSum / (3f * totalArea);
		}
	}
}

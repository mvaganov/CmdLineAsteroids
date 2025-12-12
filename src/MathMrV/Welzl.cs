using MrV;
using System;
using System.Collections.Generic;

namespace MathMrV {
	public static class Welzl {
		public static Circle GetMinimumCircle(IList<Vec2> points) {
			List<Vec2> shuffled = GetShuffled(points);
			return Calculate(shuffled, new List<Vec2>(), shuffled.Count);
		}

		public static List<Vec2> GetShuffled(IList<Vec2> points) {
			List<Vec2> shuffled = new List<Vec2>(points);
			Random rand = new Random();
			for (int i = shuffled.Count - 1; i > 0; i--) {
				int j = rand.Next(i + 1);
				var temp = shuffled[i];
				shuffled[i] = shuffled[j];
				shuffled[j] = temp;
			}
			return shuffled;
		}

		private static Circle Calculate(IList<Vec2> allPoints, List<Vec2> pointsUsedForCircle, int pointsToConsider) {
			if (pointsToConsider == 0 || pointsUsedForCircle.Count == 3) {
				return MakeCircleTrivial(pointsUsedForCircle);
			}
			Vec2 currentPoint = allPoints[pointsToConsider - 1];
			Circle bestCircleNotUsingThisPoint = Calculate(allPoints, pointsUsedForCircle, pointsToConsider - 1);
			if (bestCircleNotUsingThisPoint.Contains(currentPoint)) {
				return bestCircleNotUsingThisPoint;
			}
			pointsUsedForCircle.Add(currentPoint);
			Circle bestCircleUsingThisPoint = Calculate(allPoints, pointsUsedForCircle, pointsToConsider - 1);
			pointsUsedForCircle.RemoveAt(pointsUsedForCircle.Count - 1);
			return bestCircleUsingThisPoint;
		}

		private static Circle MakeCircleTrivial(IList<Vec2> points) {
			if (points.Count == 0) {
				return new Circle(new Vec2(0, 0), 0);
			} else if (points.Count == 1) {
				return new Circle(points[0], 0);
			} else if (points.Count == 2) {
				return MakeDiameter(points[0], points[1]);
			}
			// Check if one of the 2-point circles encloses the 3rd
			Circle c1 = MakeDiameter(points[0], points[1]);
			if (c1.Contains(points[2])) return c1;
			Circle c2 = MakeDiameter(points[0], points[2]);
			if (c2.Contains(points[1])) return c2;
			Circle c3 = MakeDiameter(points[1], points[2]);
			if (c3.Contains(points[0])) return c3;
			return Circumcircle(points[0], points[1], points[2]);
		}

		public static Circle MakeDiameter(Vec2 a, Vec2 b) {
			Vec2 center = new Vec2((a.x + b.x) / 2, (a.y + b.y) / 2);
			float radius = center.Distance(a);
			return new Circle(center, radius);
		}

		public static Circle Circumcircle(Vec2 a, Vec2 b, Vec2 c) {
			float determinant = 2 * (a.x * (b.y - c.y) +
			                         b.x * (c.y - a.y) +
			                         c.x * (a.y - b.y));
			const float epsilon = 1e-9f;
			if (MathF.Abs(determinant) < epsilon) {
				return new Circle(new Vec2(0, 0), float.PositiveInfinity);
			}
			float x = ((a.x * a.x + a.y * a.y) * (b.y - c.y) +
			           (b.x * b.x + b.y * b.y) * (c.y - a.y) +
			           (c.x * c.x + c.y * c.y) * (a.y - b.y)) / determinant;
			float y = ((a.x * a.x + a.y * a.y) * (c.x - b.x) +
			           (b.x * b.x + b.y * b.y) * (a.x - c.x) +
			           (c.x * c.x + c.y * c.y) * (b.x - a.x)) / determinant;
			Vec2 center = new Vec2(x, y);
			return new Circle(center, center.Distance(a));
		}

		public static Circle GetMinimumCircle(IList<Vec2> points, int[] indexList) {
			VectorListFromIndexList vecList = new VectorListFromIndexList(points, indexList);
			return GetMinimumCircle(vecList);
		}
	}
}

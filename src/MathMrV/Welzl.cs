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
			Vec2 center = new Vec2((a.X + b.X) / 2, (a.Y + b.Y) / 2);
			float radius = center.Distance(a);
			return new Circle(center, radius);
		}

		public static Circle Circumcircle(Vec2 a, Vec2 b, Vec2 c) {
			float determinant = 2 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
			const float epsilon = 1e-6f;
			bool pointsAreColinear = MathF.Abs(determinant) < epsilon;
			if (pointsAreColinear) { return Circle.NaN; }
			float aMagSqr = a.MagnitudeSqr, bMagSqr = b.MagnitudeSqr, cMagSqr = c.MagnitudeSqr;
			float x = (aMagSqr * (b.Y - c.Y) + bMagSqr * (c.Y - a.Y) + cMagSqr * (a.Y - b.Y)) / determinant;
			float y = (aMagSqr * (c.X - b.X) + bMagSqr * (a.X - c.X) + cMagSqr * (b.X - a.X)) / determinant;
			Vec2 center = new Vec2(x, y);
			return new Circle(center, center.Distance(a));
		}

		public static Circle GetMinimumCircle(IList<Vec2> points, int[] indexList) {
			VectorListFromIndexList vecList = new VectorListFromIndexList(points, indexList);
			return GetMinimumCircle(vecList);
		}
	}
}

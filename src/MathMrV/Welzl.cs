using System;
using System.Collections.Generic;

namespace MathMrV {
	public static class Welzl {
		public static Circle MakeCircle(IList<Vec2> points) {
			List<Vec2> shuffled = new List<Vec2>(points);
			Random rand = new Random();
			for (int i = shuffled.Count - 1; i > 0; i--) {
				int j = rand.Next(i + 1);
				var temp = shuffled[i];
				shuffled[i] = shuffled[j];
				shuffled[j] = temp;
			}
			return Calculate(shuffled, new List<Vec2>(), shuffled.Count);
		}
		private static Circle Calculate(List<Vec2> allPoints, List<Vec2> pointsBeingUsedForCircle, int pointsToConsider) {
			if (pointsToConsider == 0 || pointsBeingUsedForCircle.Count == 3) {
				return MakeCircleTrivial(pointsBeingUsedForCircle);
			}
			Vec2 currentPoint = allPoints[pointsToConsider - 1];
			Circle bestCircleNotUsingThisPoint = Calculate(allPoints, pointsBeingUsedForCircle, pointsToConsider - 1);
			if (bestCircleNotUsingThisPoint.Contains(currentPoint)) {
				return bestCircleNotUsingThisPoint;
			}
			pointsBeingUsedForCircle.Add(currentPoint);
			Circle bestCircleUsingThisPoint = Calculate(allPoints, pointsBeingUsedForCircle, pointsToConsider - 1);
			pointsBeingUsedForCircle.RemoveAt(pointsBeingUsedForCircle.Count - 1);
			return bestCircleUsingThisPoint;
		}

		private static Circle MakeCircleTrivial(List<Vec2> points) {
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
			return MakeCircumcircle(points[0], points[1], points[2]);
		}

		private static Circle MakeDiameter(Vec2 a, Vec2 b) {
			Vec2 center = new Vec2((a.X + b.X) / 2, (a.Y + b.Y) / 2);
			float radius = center.Distance(a);
			return new Circle(center, radius);
		}

		private static Circle MakeCircumcircle(Vec2 a, Vec2 b, Vec2 c) {
			float d = 2 * (a.X * (b.Y - c.Y) +
										 b.X * (c.Y - a.Y) +
										 c.X * (a.Y - b.Y));
			if (Math.Abs(d) < 1e-8) {
				return new Circle(new Vec2(0, 0), float.PositiveInfinity);
			}
			float ux = ((a.X * a.X + a.Y * a.Y) * (b.Y - c.Y) +
									(b.X * b.X + b.Y * b.Y) * (c.Y - a.Y) +
									(c.X * c.X + c.Y * c.Y) * (a.Y - b.Y)) / d;
			float uy = ((a.X * a.X + a.Y * a.Y) * (c.X - b.X) +
									(b.X * b.X + b.Y * b.Y) * (a.X - c.X) +
									(c.X * c.X + c.Y * c.Y) * (b.X - a.X)) / d;
			Vec2 center = new Vec2(ux, uy);
			float radius = center.Distance(a);
			return new Circle(center, radius);
		}
	}
}

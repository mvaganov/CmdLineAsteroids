using ConsoleMrV;
using System;
using System.Collections.Generic;

namespace MathMrV {
	public struct Polygon {
		private Vec2[] originalPoints;
		private Vec2 directionUnitVector;
		private Vec2 position;

		private Vec2[] cachedPoints;
		private bool cacheValid;
		private Vec2 cachedBoundBoxMin, cachedBoundBoxMax;
		public Vec2 Position { get => position; set { position = value; SetDirty(); } }
		public Vec2 Direction { get => directionUnitVector; set { directionUnitVector = value; SetDirty(); } }
		public float RotationRadians {
			get => directionUnitVector.UnitVectorToRadians();
			set { directionUnitVector = Vec2.UnitVectorFromRadians(value); SetDirty(); }
		}
		public float RotationDegrees {
			get => directionUnitVector.UnitVectorToDegrees();
			set { directionUnitVector = Vec2.UnitVectorFromDegrees(value); SetDirty(); }
		}
		public Vec2[] OriginalPoints { get => originalPoints; set { originalPoints = value; SetDirty(); } }
		public int Count => originalPoints.Length;

		public bool IsReadOnly => false;
		public void SetDirty() => cacheValid = false;

		public void Draw(CommandLineCanvas canvas) {
			UpdateCacheAsNeeded();
			canvas.DrawSupersampledShape(IsInsidePolygon, cachedBoundBoxMin, cachedBoundBoxMax);
		}
		bool IsInsidePolygon(Vec2 point) => IsInPolygon(cachedPoints, point);
		public Polygon(Vec2[] points) {
			originalPoints = points;
			directionUnitVector = Vec2.DirectionMaxX;
			cachedBoundBoxMax = cachedBoundBoxMin = position = Vec2.Zero;
			cacheValid = false;
			cachedPoints = null;
		}
		private void UpdateCacheAsNeeded() {
			if (cacheValid) { return; }
			if (cachedPoints == null || cachedPoints.Length != originalPoints.Length) {
				cachedPoints = new Vec2[originalPoints.Length];
			}
			float ca = directionUnitVector.x;
			float sa = directionUnitVector.y;
			for (int i = 0; i < originalPoints.Length; i++) {
				Vec2 v = originalPoints[i];
				cachedPoints[i] = new Vec2(ca * v.x - sa * v.y + position.x, sa * v.x + ca * v.y + position.y);
			}
			if (!TryGetAABB(cachedPoints, out cachedBoundBoxMin, out cachedBoundBoxMax)) {
				throw new Exception("failed to calculate bounding box for polygon");
			}
			cachedBoundBoxMin.Floor();
			cachedBoundBoxMax.Ceil();
			cacheValid = true;
		}
		public Vec2 GetPoint(int index) {
			UpdateCacheAsNeeded();
			return cachedPoints[index];
		}
		public void SetPoint(int index, Vec2 point) {
			cachedPoints[index] = point;
			float ca = directionUnitVector.x;
			float sa = directionUnitVector.y;
			originalPoints[index] = new Vec2(ca * point.x - sa * point.y, sa * point.x + ca * point.y);
		}

		public static bool IsInPolygon(IList<Vec2> poly, Vec2 p) {
			bool inside = false;
			for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++) {
				Vec2 pi = poly[i], pj = poly[j];
				bool intersect = pi.y > p.y != pj.y > p.y && p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x;
				if (intersect) inside = !inside;
			}
			return inside;
		}

		public static bool TryGetAABB(IList<Vec2> points, out Vec2 min, out Vec2 max) {
			min = Vec2.Max;
			max = Vec2.Min;
			if (points.Count == 0) {
				return false;
			}
			for (int i = 0; i < points.Count; ++i) {
				Vec2 p = points[i];
				if (p.x < min.x) { min.x = p.x; }
				if (p.y < min.y) { min.y = p.y; }
				if (p.x > max.x) { max.x = p.x; }
				if (p.y > max.y) { max.y = p.y; }
			}
			return true;
		}
		public bool Contains(Vec2 point) => IsInPolygon(cachedPoints, point);

		public void CopyTo(Vec2[] array, int arrayIndex) => Array.Copy(originalPoints, 0, array, arrayIndex, Count);
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

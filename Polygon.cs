using System;
using System.Collections;
using System.Collections.Generic;

namespace asteroids {
	public struct Polygon : IList<Vec2> {
		private Vec2[] originalPoints;
		private Vec2 directionUnitVector;
		private Vec2 originOffset;

		private Vec2[] cachedPoints;
		private bool cacheValid;
		private Vec2 cachedBoundBoxMin, cachedBoundBoxMax;
		public Vec2 OriginOffset { get => originOffset; set { originOffset = value; cacheValid = false; } }
		public Vec2 Direction { get => directionUnitVector; set => directionUnitVector = value; }
		public float RotationRadians {
			get => directionUnitVector.UnitVectorToRadians();
			set { directionUnitVector = Vec2.UnitVectorFromRadians(value); cacheValid = false; }
		}
		public float RotationDegrees {
			get => directionUnitVector.UnitVectorToDegrees();
			set { directionUnitVector = Vec2.UnitVectorFromDegrees(value); cacheValid = false; }
		}
		public int Count => originalPoints.Length;

		public bool IsReadOnly => false;

		public void Draw(CmdLineBufferGraphicsContext g) {
			UpdateCacheAsNeeded();
			g.DrawSupersampledShape(IsInsidePolygon, cachedBoundBoxMin, cachedBoundBoxMax);
		}
		bool IsInsidePolygon(Vec2 point) => IsInPolygon(cachedPoints, point);
		Vec2 IList<Vec2>.this[int index] { get => GetPoint(index); set => SetPoint(index, value); }
		public Polygon(IList<Vec2> points) {
			originalPoints = new Vec2[points.Count];
			for (int i = 0; i < points.Count; ++i) {
				originalPoints[i] = points[i];
			}
			directionUnitVector = Vec2.DirectionMaxX;
			cachedBoundBoxMax = cachedBoundBoxMin = originOffset = Vec2.Zero;
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
				cachedPoints[i] = new Vec2(ca * v.X - sa * v.Y + originOffset.X, sa * v.X + ca * v.Y + originOffset.Y);
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
			originalPoints[index] = new Vec2(ca * point.X - sa * point.Y, sa * point.X + ca * point.Y);
		}
		public static bool IsInPolygon(IList<Vec2> poly, Vec2 p) {
			Vec2 p1, p2;
			bool inside = false;
			if (poly.Count < 3) {
				return inside;
			}
			Vec2 oldPoint = poly[poly.Count - 1];
			for (int i = 0; i < poly.Count; i++) {
				Vec2 newPoint = poly[i];
				if (newPoint.X > oldPoint.X) {
					p1 = oldPoint;
					p2 = newPoint;
				} else {
					p1 = newPoint;
					p2 = oldPoint;
				}
				if ((newPoint.X <= p.X) == (p.X <= oldPoint.X) &&
				(p.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (p.X - p1.X)) {
					inside = !inside;
				}
				oldPoint = newPoint;
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

		int IList<Vec2>.IndexOf(Vec2 item) => Array.IndexOf(cachedPoints, item);

		void IList<Vec2>.Insert(int index, Vec2 item) => throw new NotImplementedException();

		public void RemoveAt(int index) => throw new NotImplementedException();

		void ICollection<Vec2>.Add(Vec2 item) => throw new NotImplementedException();

		public void Clear() => throw new NotImplementedException();

		public void CopyTo(Vec2[] array, int arrayIndex) => Array.Copy(originalPoints, 0, array, arrayIndex, Count);

		bool ICollection<Vec2>.Remove(Vec2 item) => throw new NotImplementedException();

		IEnumerator<Vec2> IEnumerable<Vec2>.GetEnumerator() {
			throw new NotImplementedException(); // TODO
		}

		public IEnumerator GetEnumerator() {
			throw new NotImplementedException(); // TODO
		}
	}
}

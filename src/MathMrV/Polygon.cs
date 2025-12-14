using ConsoleMrV;
using System;

namespace MathMrV {
	public partial class Polygon {
		public PolygonShapeDetailed model;
		private Vec2 directionUnitVector;
		private Vec2 position;
		private Vec2[] cachedPoints;
		private bool cacheValid;
		private Vec2 cachedBoundBoxMin, cachedBoundBoxMax;
		public Circle BoundingCircleInLocalSpace;
		public Vec2 Position { get => position; set { position = value; SetDirty(); } }
		public Vec2 Direction { get => directionUnitVector; set { directionUnitVector = value; SetDirty(); } }
		public float RotationRadians {
			get => directionUnitVector.NormalToRadians();
			set { directionUnitVector = Vec2.NormalFromRadians(value); SetDirty(); }
		}
		public float RotationDegrees {
			get => directionUnitVector.NormalToDegrees();
			set { directionUnitVector = Vec2.NormalFromDegrees(value); SetDirty(); }
		}
		public Polygon(Vec2[] points) {
			model = new PolygonShapeDetailed(points);
			directionUnitVector = Vec2.DirectionMaxX;
			cachedBoundBoxMax = cachedBoundBoxMin = position = Vec2.Zero;
			cacheValid = false;
			cachedPoints = null;
			ConvexHullIndexLists = null;
			UpdateCacheAsNeeded();
		}
		public void SetDirty() => cacheValid = false;
		public Vec2 GetPoint(int index) {
			UpdateCacheAsNeeded();
			return cachedPoints[index];
		}
		public void SetPointGlobalSpace(int index, Vec2 point) {
			cachedPoints[index] = point;
			float cos = directionUnitVector.x, sin = -directionUnitVector.y;
			point -= position;
			model.Points[index] = new Vec2(cos * point.x - sin * point.y, sin * point.x + cos * point.y);
			cacheValid = false;
		}
		public Circle GetCollisionBoundingCircle() {
			Circle circle = BoundingCircleInLocalSpace;
			circle.Center = Position + circle.Center.RotatedRadians(RotationRadians);
			return circle;
		}

		public void Draw(CommandLineCanvas canvas) {
			UpdateCacheAsNeeded();
			canvas.DrawSupersampledShape(IsInsidePolygon, cachedBoundBoxMin, cachedBoundBoxMax);
		}
		public bool IsInsidePolygon(Vec2 point) => PolygonShape.IsInPolygon(cachedPoints, point);
		private void UpdateCacheAsNeeded() {
			if (cacheValid) { return; }
			ForceUpdateCache();
		}
		private void ForceUpdateCache() {
			if (cachedPoints == null || cachedPoints.Length != model.Points.Length) {
				cachedPoints = new Vec2[model.Points.Length];
			}
			BoundingCircleInLocalSpace = Welzl.GetMinimumCircle(model.Points);
			float cos = directionUnitVector.x, sin = directionUnitVector.y;
			for (int i = 0; i < model.Points.Length; i++) {
				Vec2 v = model.Points[i];
				cachedPoints[i] = new Vec2(cos * v.x - sin * v.y + position.x, sin * v.x + cos * v.y + position.y);
			}
			if (!PolygonShape.TryGetAABB(cachedPoints, out cachedBoundBoxMin, out cachedBoundBoxMax)) {
				throw new Exception("failed to calculate bounding box for polygon");
			}
			UpdateConvexHullIndexLists();
			cacheValid = true;
		}
	}
}

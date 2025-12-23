using ConsoleMrV;
using System;

namespace MathMrV {
	public partial class Polygon {
		public Geometry2D model;
		private Vec2 _directionUnitVector;
		private Vec2 _position;
		private Vec2[] _cachedPoints;
		private bool _cacheValid;
		private Vec2 _cachedBoundBoxMin, _cachedBoundBoxMax;
		private Circle _boundingCircleInGlobalSpace;
		public Circle BoundingCircleInLocalSpace => model.BoundingCircle;
		public Vec2 Position { get => _position; set { _position = value; SetDirty(); } }
		public Vec2 Direction { get => _directionUnitVector; set { _directionUnitVector = value; SetDirty(); } }
		public float RotationRadians {
			get => _directionUnitVector.NormalToRadians();
			set { _directionUnitVector = Vec2.NormalFromRadians(value); SetDirty(); }
		}
		public float RotationDegrees {
			get => _directionUnitVector.NormalToDegrees();
			set { _directionUnitVector = Vec2.NormalFromDegrees(value); SetDirty(); }
		}
		public Polygon(Geometry2D polygonShape) {
			model = polygonShape;
			_directionUnitVector = Vec2.UnitX;
			_cachedBoundBoxMax = _cachedBoundBoxMin = _position = Vec2.Zero;
			_cacheValid = false;
			_cachedPoints = null;
			UpdateCacheAsNeeded();
		}
		public Polygon(Vec2[] points) : this (new Geometry2D(points)) {}
		public void SetDirty() => _cacheValid = false;
		public Vec2 GetPoint(int index) {
			UpdateCacheAsNeeded();
			return _cachedPoints[index];
		}
		public Circle GetCollisionBoundingCircle() => _boundingCircleInGlobalSpace;

		public void Draw(CommandLineCanvas canvas) {
			UpdateCacheAsNeeded();
			canvas.DrawSupersampledShape(IsInsidePolygon, _cachedBoundBoxMin, _cachedBoundBoxMax);
			canvas.SetColor(ConsoleColor.White);
			//DrawConvex(canvas, (int)(MrV.Time.TimeSeconds*2) % model.ConvexHullIndexLists.Length);
		}
		public bool IsInsidePolygon(Vec2 point) => PolygonShape.IsInPolygon(_cachedPoints, point);
		private void UpdateCacheAsNeeded() {
			if (_cacheValid) { return; }
			ForceUpdateCache();
		}
		private void ForceUpdateCache() {
			if (_cachedPoints == null || _cachedPoints.Length != model.Points.Length) {
				_cachedPoints = new Vec2[model.Points.Length];
			}
			_boundingCircleInGlobalSpace = BoundingCircleInLocalSpace;
			//_boundingCircleInGlobalSpace.Center.Rotate(_directionUnitVector);
			//_boundingCircleInGlobalSpace.Center += _position;
			_boundingCircleInGlobalSpace.Center = ConvertLocalPositionToWorldSpace(_boundingCircleInGlobalSpace.Center);
			float cos = _directionUnitVector.X, sin = _directionUnitVector.Y;
			for (int i = 0; i < model.Points.Length; i++) {
				_cachedPoints[i] = ConvertLocalPositionToWorldSpace(model.Points[i]);
			}
			if (!PolygonShape.TryGetAABB(_cachedPoints, out _cachedBoundBoxMin, out _cachedBoundBoxMax)) {
				throw new Exception("failed to calculate bounding box for polygon");
			}
			_cacheValid = true;
		}
	}
}

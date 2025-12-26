using collision;
using ConsoleMrV;
using System;
using System.Collections.Generic;

namespace MathMrV {
	public partial class Polygon {
		public float Area => model.Area;
		public float InertiaWithoutDensity => model.InertiaWithoutDensity;

		public Vec2 ConvertLocalPositionToWorldSpace(Vec2 point) {
			point -= model.GeometricCenterOffset;
			point.Rotate(Direction);
			point += _position;
			return point;
		}

		public Vec2 ConvertLocalDirectionToWorldSpace(Vec2 delta) {
			delta.Rotate(Direction);
			return delta;
		}

		public Vec2 ConvertPointToLocalSpace(Vec2 point) {
			point -= _position;
			point.Unrotate(Direction);
			return point;
		}

		public Circle GetCollisionBoundingCircle(int index) {
			Circle c = model.GetCollisionBoundingCircle(index);
			c.Center = ConvertLocalPositionToWorldSpace(c.Center);
			return c;
		}

		public void DrawConvex(CommandLineCanvas canvas, int convexIndex) {
			UpdateCacheAsNeeded();
			IList<Vec2> verts = model.ConvexHull.Points[convexIndex];
			canvas.DrawSupersampledShape(IsInsidePolygon, _cachedBoundBoxMin, _cachedBoundBoxMax);
			bool IsInsidePolygon(Vec2 point) => PolygonShape.IsInPolygon(verts, ConvertPointToLocalSpace(point));
		}
		public bool TryGetCircleCollision(Vec2 center, float radius,
		out Vec2 closestPoint, out Vec2 circleToPointDelta, out float distanceSq) {
			center = ConvertPointToLocalSpace(center);
			bool result = model.TryGetCircleCollision(center, radius, out closestPoint, out circleToPointDelta, out distanceSq);
			closestPoint = ConvertLocalPositionToWorldSpace(closestPoint);
			circleToPointDelta = ConvertLocalDirectionToWorldSpace(circleToPointDelta);
			return result;
		}
		private bool IsIndexPairConsecutive(int indexA, int indexB) {
			return indexA + 1 == indexB || (indexB == 0 && indexA == (_cachedPoints.Length-1));
		}

		public bool TryGetPolyCollision(Polygon other, ref List<CollisionData> collisionDatas) {
			UpdateCacheAsNeeded();
			other.UpdateCacheAsNeeded();
			return TryGetPolygonCollision(this, other, ref collisionDatas);
		}

		private static bool TryGetPolygonCollision(Polygon mainPoly, Polygon otherPoly, ref List<CollisionData> collisionDatas) {
			bool foundCollision = false;
			CollisionData result = null;
			for (int mainConvex = 0; mainConvex < mainPoly.model.ConvexHull.IndexLists.Length; mainConvex++) {
				for (int otherConvex = 0; otherConvex < otherPoly.model.ConvexHull.IndexLists.Length; otherConvex++) {
					bool collisionJustHappened = CheckCollisionOfSubMeshes(mainPoly, otherPoly, mainConvex, otherConvex, ref result)
					&& CheckCollisionOfSubMeshes(otherPoly, mainPoly, otherConvex, mainConvex, ref result);
					if (collisionJustHappened) {
						result.IsColliding = true;
						if (collisionDatas == null) { collisionDatas = new List<CollisionData>(); }
						Vec2 direction = otherPoly._position - mainPoly._position;
						if (Vec2.Dot(direction, result.Normal) < 0) {
							result.Normal = -result.Normal;
						}
						collisionDatas.Add(result);
						foundCollision |= collisionJustHappened;
					}
				}
			}
			return foundCollision;
		}

		public static IList<Vec2> CalculateIntersections(CollisionData data) {
			Polygon self = data.Self as Polygon;
			if (self == null) return null;
			Polygon other = data.Other as Polygon;
			if (other == null) return null;
			return CalculateIntersections(self, other, data.ColliderIndexSelf, data.ColliderIndexOther);
		}
		public static IList<Vec2> CalculateIntersections(Polygon self, Polygon other, int colliderIndexSelf, int colliderIndexOther) {
			List<Vec2> intersections = new List<Vec2>();
			int[] subColliderIndexes = self.model.ConvexHull.IndexLists[colliderIndexSelf];
			for (int i = 0; i < subColliderIndexes.Length; ++i) {
				int indexA = subColliderIndexes[i];
				int indexB = subColliderIndexes[(i+1) % subColliderIndexes.Length];
				bool segmentIsOnEdgeOfMainPolygon = self.IsIndexPairConsecutive(indexA, indexB);
				if (!segmentIsOnEdgeOfMainPolygon) { continue; }
				Vec2 lineStart = other.ConvertPointToLocalSpace(self.GetPoint(indexA));
				Vec2 lineEnd = other.ConvertPointToLocalSpace(self.GetPoint(indexB));
				if (other.model.TryGetCrossingSegment(lineStart, lineEnd, colliderIndexOther, intersections)) {
				}
			}
			for (int i = 0; i < intersections.Count; ++i) {
				intersections[i] = other.ConvertLocalPositionToWorldSpace(intersections[i]);
			}
			return intersections;
		}
		public static Vec2 CalculateEstimateCollisionPoint(CollisionData data) {
			Polygon self = data.Self as Polygon;
			if (self == null) return Vec2.NaN;
			Polygon other = data.Other as Polygon;
			if (other == null) return Vec2.NaN;
			return CalculateEstimateCollisionPoint(self, other, data.ColliderIndexSelf, data.ColliderIndexOther);
		}
		public static Vec2 CalculateEstimateCollisionPoint(Polygon self, Polygon other, int colliderIndexSelf, int colliderIndexOther) {
			Circle circleA = self.GetCollisionBoundingCircle(colliderIndexSelf);
			Circle circleB = other.GetCollisionBoundingCircle(colliderIndexOther);
			Circle.TryGetCircleCollision(circleA, circleB, out Vec2 estimatedCollisionPoint);
			return estimatedCollisionPoint;
		}

		private static bool CheckCollisionOfSubMeshes(Polygon mainPoly, Polygon otherPoly, int mainConvex, int otherConvex, ref CollisionData result) {
			int[] mainIndexList = mainPoly.model.ConvexHull.IndexLists[mainConvex];
			for (int i = 0; i < mainIndexList.Length; i++) {
				int index0 = mainIndexList[i], index1 = mainIndexList[(i + 1) % mainIndexList.Length];
				Vec2 p0 = mainPoly.GetPoint(index0), p1 = mainPoly.GetPoint(index1);
				Vec2 edge = p1 - p0;
				Vec2 edgeNormal = new Vec2(-edge.Y, edge.X);
				edgeNormal.Normalize(); // normalize for accurate depth measurement
				mainPoly.ProjectPolygon(edgeNormal, mainConvex, out float minA, out float maxA);
				otherPoly.ProjectPolygon(edgeNormal, otherConvex, out float minB, out float maxB);
				bool noOverlapCollisionImpossible = minA >= maxB || minB >= maxA;
				if (noOverlapCollisionImpossible) { return false; }
				float collisionDepthOnAxis = Math.Min(maxB - minA, maxA - minB);
				bool foundBestCollisionOptionSoFar = result == null || collisionDepthOnAxis < result.Depth;
				if (foundBestCollisionOptionSoFar && mainPoly.IsIndexPairConsecutive(index0, index1)) {
					if (result == null) {
						result = CollisionData.Commission();
					}
					result.Init(mainPoly, otherPoly, normal:edgeNormal, depth:collisionDepthOnAxis,
						colliderIndexSelf: mainConvex, colliderIndexOther: otherConvex); // TODO make sure this gets decommissioned...
				}
			}
			return true;
		}
		public void ProjectPolygon(Vec2 axis, int convexIndex, out float min, out float max) {
			min = float.MaxValue; max = float.MinValue;
			int[] indexList = model.ConvexHull.IndexLists[convexIndex];
			for (int i = 0; i < indexList.Length; i++) {
				float projection = Vec2.Dot(GetPoint(indexList[i]), axis);
				if (projection < min) { min = projection; }
				if (projection > max) { max = projection; }
			}
		}
	}
}

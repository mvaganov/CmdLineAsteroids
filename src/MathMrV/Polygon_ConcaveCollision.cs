using ConsoleMrV;
using System;
using System.Collections.Generic;

namespace MathMrV {
	public partial class Polygon {
		public float Area => model.Area;
		public float InertiaWithoutDensity => model.InertiaWithoutDensity;

		public Vec2 ConvertLocalPositionToWorldSpace(Vec2 point) {
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

		public struct CollisionData {
			public Polygon objectA, objectB;
			public bool IsColliding;
			public Vec2 Normal; // direction to push B out of A
			public int ObjectAConvexIndex;
			public int ObjectBConvexIndex;
			public float Depth; // How far to push
			public void Init() {
				IsColliding = false;
				Depth = float.MaxValue;
				Normal = Vec2.Zero;
			}
			public IList<Vec2> CalculateContacts() {
				return Polygon.GetIntersections(objectA, ObjectAConvexIndex, objectB, ObjectBConvexIndex);
			}
			public Vec2 CalculateEstimateCollisionPoint() {
				Circle circleA = objectA.model.ConvexHull.BoundingCircles[ObjectAConvexIndex];
				Circle circleB = objectB.model.ConvexHull.BoundingCircles[ObjectBConvexIndex];
				Circle.TryGetCircleCollision(circleA, circleB, out Vec2 estimatedCollisionPoint);
				return estimatedCollisionPoint;
			}
		}
		public bool TryGetPolyCollision(Polygon other, ref List<CollisionData> collisionDatas) {
			UpdateCacheAsNeeded();
			other.UpdateCacheAsNeeded();
			return TryGetPolygonCollision(this, other, ref collisionDatas);
		}

		private static bool TryGetPolygonCollision(Polygon mainPoly, Polygon otherPoly, ref List<CollisionData> collisionDatas) {
			bool foundCollision = false;
			for (int mainConvex = 0; mainConvex < mainPoly.model.ConvexHull.IndexLists.Length; mainConvex++) {
				for (int otherConvex = 0; otherConvex < otherPoly.model.ConvexHull.IndexLists.Length; otherConvex++) {
					CollisionData result = new CollisionData();
					result.Init();
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

		public static IList<Vec2> GetIntersections(Polygon a, int convexHullA, Polygon b, int convexHullB) {
			List<Vec2> intersections = null;
			int[] subMeshA = a.model.ConvexHull.IndexLists[convexHullA];
			int[] subMeshB = b.model.ConvexHull.IndexLists[convexHullB];
			for (int i = 0; i < subMeshA.Length; ++i) {
				int nextIndex = (i+1) % subMeshA.Length;
				if (!a.IsIndexPairConsecutive(i, nextIndex)) { continue; }
				Vec2 lineStart = b.ConvertPointToLocalSpace(a.GetPoint(subMeshA[i]));
				Vec2 lineEnd = b.ConvertPointToLocalSpace(a.GetPoint(subMeshA[nextIndex]));
				if (b.model.TryGetCrossingSegment(lineStart, lineEnd, convexHullB, out _, out Vec2 result)) {
					if (intersections == null) { intersections = new List<Vec2>(); }
					intersections.Add(b.ConvertLocalPositionToWorldSpace(result));
				}
			}
			return intersections;
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
				bool foundBestCollisionOptionSoFar = collisionDepthOnAxis < result.Depth;
				if (foundBestCollisionOptionSoFar && mainPoly.IsIndexPairConsecutive(index0, index1)) {
					result.Depth = collisionDepthOnAxis;
					result.Normal = edgeNormal;
					result.objectA = mainPoly;
					result.objectB = otherPoly;
					// specific contact points can be calculated later with relevant convex polygon IDs
					result.ObjectAConvexIndex = mainConvex;
					result.ObjectBConvexIndex = otherConvex;
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

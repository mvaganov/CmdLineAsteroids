using MrV;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathMrV {
	public class PolygonShapeDetailed {
		public PolygonShape Shape;
		public Circle BoundingCircleInLocalSpace; // TODO rename BoundingCircle
		public int[][] ConvexHullIndexLists;
		public Circle[] ConvexHullCircles;
		public float Area, InertiaWithoutDensity;
		public VectorListFromIndexList[] ConvexHullPoints;
		public PolygonShapeDetailed(Vec2[] points) {
			Shape = new PolygonShape(points);
			Initialize();
		}
		public override string ToString() => $"(polygon: {string.Join(", ", Points)})";
		public Vec2[] Points { get => Shape.Points; set { Shape.Points = value; } }
		public int Count => Points.Length;
		public Vec2 GetPoint(int index) => Points[index];
		public void SetPoint(int index, Vec2 point) => Points[index] = point;
		public Circle GetCollisionBoundingCircle() => BoundingCircleInLocalSpace;
		public Circle GetCollisionBoundingCircle(int convexHullIndex) => ConvexHullCircles[convexHullIndex];
		public void Initialize() {
			BoundingCircleInLocalSpace = Welzl.GetMinimumCircle(Points);
			int[][] triangleIndexes = DecomposePolygonIntoTriangles_HertelMehlhorn(Points);
			MrV.Physics.InertiaCalculator.CalculatePolygonAreaAndInertia(Points, out Area, out InertiaWithoutDensity);
			ConvexHullIndexLists = ConvertTriangleListIntoConvexHulls(triangleIndexes, Points);
			ConvexHullCircles = new Circle[ConvexHullIndexLists.Length];
			ConvexHullPoints = new VectorListFromIndexList[ConvexHullIndexLists.Length];
			for (int i = 0; i < ConvexHullCircles.Length; ++i) {
				ConvexHullCircles[i] = Welzl.GetMinimumCircle(Points, ConvexHullIndexLists[i]);
				ConvexHullPoints[i] = new VectorListFromIndexList(Points, ConvexHullIndexLists[i]);
			}
		}
		public static int[][] DecomposePolygonIntoTriangles_HertelMehlhorn(Vec2[] vertices) {
			if (vertices.Length < 3) { return Array.Empty<int[]>(); }
			if (vertices.Length == 3) { return new int[][] { new[] { 0, 1, 2 } }; }
			List<int> indexes = Enumerable.Range(0, vertices.Length).ToList();
			List<int[]> convexHulls = new List<int[]>();
			int loopGuard = vertices.Length * 3;
			while (indexes.Count > 2 && loopGuard-- > 0) {
				bool earWasClipped = false;
				for (int i = 0; i < indexes.Count; i++) {
					int iPrev = (i == 0) ? indexes.Count - 1 : i - 1;
					int iNext = (i == indexes.Count - 1) ? 0 : i + 1;
					int p0 = indexes[iPrev], p1 = indexes[i], p2 = indexes[iNext];
					Vec2 v0 = vertices[p0], v1 = vertices[p1], v2 = vertices[p2];
					float crossProduct = (v1.X - v0.X) * (v2.Y - v1.Y) - (v1.Y - v0.Y) * (v2.X - v1.X); // TODO cross product function
					bool isCCWTriangle = crossProduct > 0;
					bool IsEmptyTriangle() {
						foreach (int pTest in indexes) {
							if (pTest == p0 || pTest == p1 || pTest == p2) { continue; }
							if (IsPointInsideTriangle(vertices[pTest], v0, v1, v2)) { return false; }
						}
						return true;
					}
					if (isCCWTriangle && IsEmptyTriangle()) {
						convexHulls.Add(new[] { p0, p1, p2 });
						indexes.RemoveAt(i); // Remove p1 from the polygon, clipping the 'ear'
						earWasClipped = true;
						break;
					}
				}
				if (!earWasClipped && indexes.Count > 3) {
					throw new Exception("ear clipping failed or polygon is non-simple.");
				}
			}
			return convexHulls.ToArray();
		}

		public static bool IsPointInsideTriangle(Vec2 P, Vec2 A, Vec2 B, Vec2 C) {
			float crossA = Vec2.Cross(B - A, P - A);
			float crossB = Vec2.Cross(C - B, P - B);
			float crossC = Vec2.Cross(A - C, P - C);
			bool signA = crossA >= 0;
			bool signB = crossB >= 0;
			bool signC = crossC >= 0;
			return signA && signB && signC;
		}

		public static int[][] ConvertTriangleListIntoConvexHulls(int[][] triangleIndexes, Vec2[] originalVertices) {
			if (triangleIndexes.Length == 0) { return null; }
			List<int[]> hulls = new List<int[]>(triangleIndexes);
			while (MergeHulls()) { }
			bool MergeHulls() {
				for (int a = 0; a < hulls.Count; a++) {
					for (int b = a + 1; b < hulls.Count; b++) {
						int[] hullA = hulls[a];
						int[] hullB = hulls[b];
						int[] newHull = TryMergeConvexHulls(originalVertices, hullA, hullB);
						if (newHull != null) {
							hulls.RemoveAt(b); // Remove the one with the higher index first
							hulls.RemoveAt(a);
							hulls.Add(newHull);
							return true;
						}
					}
				}
				return false;
			}
			return hulls.ToArray();
		}

		private static int[] TryMergeConvexHulls(Vec2[] vertList, int[] indexListHullA, int[] indexListHullB) {
			if (TryGetSharedEdgeIndices(indexListHullA, indexListHullB, out int hullAIndex, out int hullBIndex)) {
				int[] mergedHull = SpliceHullIndexList(indexListHullA, indexListHullB, hullAIndex, hullBIndex);
				if (IsHullConvex(vertList, mergedHull)) { // only give back convex shape
					return mergedHull;
				}
			}
			return null;
			bool TryGetSharedEdgeIndices(int[] indexListHullA, int[] indexListHullB, out int hullAIndex, out int hullBIndex) {
				for (hullAIndex = 0; hullAIndex < indexListHullA.Length; hullAIndex++) {
					int hullAvert0 = indexListHullA[hullAIndex];
					int hullAvert1 = indexListHullA[(hullAIndex + 1) % indexListHullA.Length];
					for (hullBIndex = 0; hullBIndex < indexListHullB.Length; hullBIndex++) {
						int hullBvert0 = indexListHullB[hullBIndex];
						if (hullAvert1 != hullBvert0) { continue; }
						int hullBvert1 = indexListHullB[(hullBIndex + 1) % indexListHullB.Length];
						if (hullAvert0 == hullBvert1) { return true; }
					}
				}
				hullBIndex = -1;
				return false;
			}
			int[] SpliceHullIndexList(int[] hullA, int[] hullB, int hullASharedEdgeIndex, int hullBSharedEdgeIndex) {
				List<int> newVertices = new List<int>();
				newVertices.AddRange(hullA);
				List<int> hullBsegment = new List<int>();
				int currIndex = (hullBSharedEdgeIndex + 2) % hullB.Length;
				while (currIndex != hullBSharedEdgeIndex) {
					hullBsegment.Add(hullB[currIndex]);
					currIndex = (currIndex + 1) % hullB.Length;
				}
				newVertices.InsertRange(hullASharedEdgeIndex + 1, hullBsegment);
				return newVertices.ToArray();
			}
			bool IsHullConvex(IList<Vec2> vertList, IList<int> indexList) {
				if (indexList.Count < 3) { return false; }
				if (indexList.Count == 3) { return true; } // assume triangles always convex (bad heuristic if CW order)
				for (int i = 0; i < indexList.Count; i++) {
					int iPrev = (i == 0) ? indexList.Count - 1 : i - 1;
					int iNext = (i == indexList.Count - 1) ? 0 : i + 1;
					Vec2 p0 = vertList[indexList[iPrev]], p1 = vertList[indexList[i]], p2 = vertList[indexList[iNext]];
					float directionalityOfPoints = Vec2.Cross(p0, p1, p2);
					bool isCCW = directionalityOfPoints > 0;
					if (!isCCW) { return false; }
				}
				return true;
			}
		}

		private bool IsPointInConvexPolygon(Vec2 point, int convexPolygon) {
			if (!GetCollisionBoundingCircle(convexPolygon).Contains(point)) { return false; }
			int[] poly = ConvexHullIndexLists[convexPolygon];
			for (int i = 0; i < poly.Length; i++) {
				Vec2 a = Points[poly[i]], b = Points[poly[(i + 1) % poly.Length]];
				Vec2 edgeDelta = b - a;
				Vec2 pointToA = point - a;
				float cross = Vec2.Cross(edgeDelta, pointToA);
				bool isCCW = cross > 0;
				if (!isCCW) { return false; }
			}
			return true;
		}
		public bool TryGetCircleCollision(Vec2 center, float radius,
		out Vec2 closestPoint, out Vec2 circleToPointDelta, out float distanceSq) {
			closestPoint = circleToPointDelta = Vec2.NaN;
			distanceSq = float.MaxValue;
			float closeDistSq;
			Vec2 closePoint, circle2PointDelta;
			for (int i = 0; i < ConvexHullIndexLists.Length; i++) {
				if (TryGetCircleCollisionConvex(i, center, radius, out closePoint, out circle2PointDelta, out closeDistSq)) {
					if (closeDistSq < distanceSq) {
						distanceSq = closeDistSq;
						closestPoint = closePoint;
						circleToPointDelta = circle2PointDelta;
					}
				}
			}
			float radiusSq = radius * radius;
			return distanceSq <= radiusSq;
		}
		public bool TryGetCircleCollisionConvex(int convexPolygonId, Vec2 circleCenter, float radius, out Vec2 closestPoint, out Vec2 circlePointDelta, out float circlePointDistanceSq) {
			closestPoint = circlePointDelta = Vec2.NaN;
			circlePointDistanceSq = float.MaxValue;
			if (!GetCollisionBoundingCircle(convexPolygonId).IsColliding(circleCenter, radius)) {
				return false;
			}
			int[] poly = ConvexHullIndexLists[convexPolygonId];
			float radiusSq = radius * radius;
			for (int i = 0; i < poly.Length; i++) {
				int indexA = poly[i], indexB = poly[(i + 1) % poly.Length];
				bool segmentABIsOnEdgeOfMainPolygon = IsIndexPairConsecutiveOnPolygonEdge(indexA, indexB);
				if (!segmentABIsOnEdgeOfMainPolygon) { continue; }
				Vec2 vecA = Points[indexA], vecB = Points[indexB];
				Vec2 closestOnSegment = GetClosestPointOnSegment(vecA, vecB, circleCenter);
				Vec2 delta = closestOnSegment - circleCenter;
				float distanceSq = delta.MagnitudeSqr;
				if (distanceSq < circlePointDistanceSq) {
					circlePointDistanceSq = distanceSq;
					closestPoint = closestOnSegment;
					circlePointDelta = delta;
				}
			}
			if (circlePointDistanceSq <= radiusSq) {
				return true;
			}
			if (IsPointInConvexPolygon(circleCenter, convexPolygonId)) { return true; }
			return false;
		}
		private bool IsIndexPairConsecutiveOnPolygonEdge(int indexA, int indexB) {
			return indexA + 1 == indexB || (indexB == 0 && indexA == (Points.Length - 1));
		}
		public static Vec2 GetClosestPointOnSegment(Vec2 a, Vec2 b, Vec2 p) {
			Vec2 lineDelta = b - a;
			Vec2 pointRelativeToLine = p - a;
			float pointProjectedOnLine = Vec2.Dot(pointRelativeToLine, lineDelta) / Vec2.Dot(lineDelta, lineDelta);
			pointProjectedOnLine = Math.Clamp(pointProjectedOnLine, 0f, 1f);
			return a + (lineDelta * pointProjectedOnLine);
		}
		public bool TryGetCrossingSegment(Vec2 lineStartLocalSpace, Vec2 lineEndLocalSpace,
		int convexPolygonId, out int segmentIndex, out Vec2 result) {
			int[] poly = ConvexHullIndexLists[convexPolygonId];
			for (int i = 0; i < poly.Length; i++) {
				int indexA = poly[i], indexB = poly[(i + 1) % poly.Length];
				bool segmentABIsOnEdgeOfMainPolygon = IsIndexPairConsecutiveOnPolygonEdge(indexA, indexB);
				if (!segmentABIsOnEdgeOfMainPolygon) { continue; }
				Vec2 vecA = Points[indexA], vecB = Points[indexB];
				if (Vec2.TryGetLineSegmentIntersection(lineStartLocalSpace, lineEndLocalSpace, vecA, vecB, out result)) {
					segmentIndex = i;
					return true;
				}
			}
			segmentIndex = -1;
			result = Vec2.NaN;
			return false;
		}
	}
}

using ConsoleMrV;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathMrV {
	public partial class Polygon {
		private int[][] convexHullIndexLists;
		public void DrawConvex(CommandLineCanvas canvas, int convexIndex) {
			UpdateCacheAsNeeded();
			List<Vec2> verts = GetConvexVerts(convexIndex);
			canvas.DrawSupersampledShape(IsInsidePolygon, cachedBoundBoxMin, cachedBoundBoxMax);
			bool IsInsidePolygon(Vec2 point) => PolygonShape.IsInPolygon(verts, point);
		}
		public void UpdateConvexHullIndexLists() {
			int[][] triangles = DecomposePolygonIntoTriangles_HertelMehlhorn(original.Points);
			convexHullIndexLists = ConvertTriangleListIntoConvexHulls(triangles, original.Points);
		}
		public List<Vec2> GetConvexVerts(int convexIndex) {
			int[] indexes = convexHullIndexLists[convexIndex];
			List<Vec2> verts = new List<Vec2>();
			for (int i = 0; i < indexes.Length; i++) {
				verts.Add(GetPoint(indexes[i]));
			}
			return verts;
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
					float crossProduct = (v1.X - v0.X) * (v2.Y - v1.Y) - (v1.Y - v0.Y) * (v2.X - v1.X);
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
					Console.WriteLine("Warning: Ear clipping failed or polygon is non-simple.");
					break;
				}
			}
			return convexHulls.ToArray();
		}

		private static bool IsPointInsideTriangle(Vec2 P, Vec2 A, Vec2 B, Vec2 C) {
			float CrossProduct2D(Vec2 a, Vec2 b) => (a.X * b.Y) - (a.Y * b.X);
			float crossA = CrossProduct2D(B - A, P - A);
			float crossB = CrossProduct2D(C - B, P - B);
			float crossC = CrossProduct2D(A - C, P - C);
			bool signA = crossA >= 0;
			bool signB = crossB >= 0;
			bool signC = crossC >= 0;
			return signA == signB && signB == signC;
		}

		public static int[][] ConvertTriangleListIntoConvexHulls(int[][] triangleIndices, Vec2[] originalVertices) {
			if (triangleIndices.Length == 0) { return null; }
			List<int[]> hulls = new List<int[]>(triangleIndices);
			while (MergeHulls()) {
			}
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
						int hullBvert1 = indexListHullB[(hullBIndex + 1) % indexListHullB.Length];
						if (hullAvert0 == hullBvert1 && hullAvert1 == hullBvert0) {
							return true;
						}
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
				if (indexList.Count == 3) { return true; } // Triangles are always convex
				for (int i = 0; i < indexList.Count; i++) {
					int iPrev = (i == 0) ? indexList.Count - 1 : i - 1;
					int iNext = (i == indexList.Count - 1) ? 0 : i + 1;
					Vec2 p0 = vertList[indexList[iPrev]], p1 = vertList[indexList[i]], p2 = vertList[indexList[iNext]];
					float crossProduct = (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
					bool isCCW = crossProduct > 0;
					if (!isCCW) { return false; }
				}
				return true;
			}
		}

		public bool IsPointInPolygon(Vec2 point) {
			for (int i = 0; i < convexHullIndexLists.Length; ++i) {
				if (IsPointInConvexPolygon(point, i)) { return true; }
			}
			return false;
		}
		private bool IsPointInConvexPolygon(Vec2 point, int convexPolygon) {
			int[] poly = convexHullIndexLists[convexPolygon];
			for (int i = 0; i < poly.Length; i++) {
				Vec2 a = cachedPoints[poly[i]], b = cachedPoints[poly[(i + 1) % poly.Length]];
				Vec2 edgeDelta = b - a;
				Vec2 pointToA = point - a;
				float cross = (edgeDelta.x * pointToA.y) - (edgeDelta.y * pointToA.x);
				bool isCCW = cross > 0;
				if (!isCCW) { return false; }
			}
			return true;
		}
		public bool TryGetCircleCollision(Vec2 center, float radius, out Vec2 closestPoint) {
			closestPoint = Vec2.NaN;
			float closestDistanceSq = float.MaxValue, closeDistSq;
			Vec2 closePoint;
			for (int i = 0; i < convexHullIndexLists.Length; i++) {
				if (TryGetCircleCollisionConvex(i, center, radius, out closePoint, out closeDistSq)) {
					if (closeDistSq < closestDistanceSq) {
						closestDistanceSq = closeDistSq;
						closestPoint = closePoint;
					}
				}
			}
			float radiusSq = radius * radius;
			return closestDistanceSq <= radiusSq;
		}
		private bool TryGetCircleCollisionConvex(int convexPolygon, Vec2 circleCenter, float radius, out Vec2 closestPoint, out float pointDistanceSq) {
			int[] poly = convexHullIndexLists[convexPolygon];
			float radiusSq = radius * radius;
			closestPoint = Vec2.NaN;
			pointDistanceSq = float.MaxValue;
			for (int i = 0; i < poly.Length; i++) {
				int indexA = poly[i], indexB = poly[(i + 1) % poly.Length];
				bool segmentABIsOnEdgeOfMainPolygon = IsIndexPairConsecutive(indexA, indexB);
				if (!segmentABIsOnEdgeOfMainPolygon) { continue; }
				Vec2 vecA = cachedPoints[indexA], vecB = cachedPoints[indexB];
				Vec2 closestOnSegment = GetClosestPointOnSegment(vecA, vecB, circleCenter);
				float distanceSq = Vec2.DistanceSquared(circleCenter, closestOnSegment);
				if (distanceSq < pointDistanceSq) {
					pointDistanceSq = distanceSq;
					closestPoint = closestOnSegment;
				}
			}
			if (pointDistanceSq <= radiusSq) { return true; }
			if (IsPointInConvexPolygon(circleCenter, convexPolygon)) { return true; }
			return false;
		}
		private bool IsIndexPairConsecutive(int indexA, int indexB) {
			return indexA + 1 == indexB || (indexB == 0 && indexA == (cachedPoints.Length-1));
		}
		public static Vec2 GetClosestPointOnSegment(Vec2 a, Vec2 b, Vec2 p) {
			Vec2 lineDelta = b - a;
			Vec2 pointRelativeToLine = p - a;
			float pointProjectedOnLine = Vec2.Dot(pointRelativeToLine, lineDelta) / Vec2.Dot(lineDelta, lineDelta);
			pointProjectedOnLine = Math.Clamp(pointProjectedOnLine, 0f, 1f);
			return a + (lineDelta * pointProjectedOnLine);
		}

		public struct CollisionData {
			public bool IsColliding;
			public Vec2 Normal; // The direction to push B out of A
			public float Depth;    // How far to push
			public Polygon polygonA, polygonB;
			public int vertIndex0, vertIndex1;
			public void Init() {
				IsColliding = false;
				Depth = float.MaxValue;
				Normal = Vec2.Zero;
			}
			public void Draw(CommandLineCanvas canvas) {
				if (polygonA.cachedPoints == null) {
					polygonA.ForceUpdateCache();
				}
				polygonA.UpdateCacheAsNeeded();
				Vec2 p0 = polygonA.GetPoint(vertIndex0);
				Vec2 p1 = polygonA.GetPoint(vertIndex1);
				canvas.SetColor(ConsoleColor.Green);
				canvas.DrawLine(p0, p1, 1);
				//if (hits != null) {
				//	for (int i = 0; i < hits.Count; ++i) {
				//		hits[i].Draw(canvas);
				//	}
				//}
				//canvas.SetColor(ConsoleColor.Magenta);
				//Vec2 center = (p0 + p1) / 2;
				//canvas.DrawLine(center, center + Normal);
				//canvas.DrawLine(polygonA.position, polygonA.position + Normal);
			}
			//public class PolygonConvexHit {
			//	public Polygon polygon;
			//	public int convexIndex;
			//	public float minA, maxA, minB, maxB;
			//	public Vec2 center, normal, p0, p1;
			//	public PolygonConvexHit(Polygon polygon, int convexIndex, Vec2 p0, Vec2 p1, Vec2 center, Vec2 normal, float minA, float maxA, float minB, float maxB) {
			//		this.polygon = polygon; this.convexIndex = convexIndex;
			//		this.p0 = p0; this.p1 = p1; this.center = center; this.normal = normal;
			//		this.minA = minA; this.maxA = maxA; this.minB = minB; this.maxB = maxB;
			//	}
			//	public void Draw(CommandLineCanvas canvas) {
			//		Vec2 perp = normal.Perpendicular();
			//		Vec2 centerA = center + perp;
			//		Vec2 centerB = center - perp;
			//		Vec2 startA = centerA + normal * minA;
			//		Vec2 endA = centerA + normal * maxA;
			//		Vec2 startB = centerB + normal * minB;
			//		Vec2 endB = centerB + normal * maxB;
			//		Vec2 delta = center - startA;
			//		startA += delta;
			//		endA += delta;
			//		startB += delta;
			//		endB += delta;
			//		canvas.SetColor(ConsoleColor.Red);
			//		canvas.DrawLine(startA, endA);
			//		canvas.SetColor(ConsoleColor.Blue);
			//		canvas.DrawLine(startB, endB);
			//	}
			//}
			//public List<PolygonConvexHit> hits;
			//public void AddPolygonHit(Polygon polygon, int convexIndex, Vec2 p0, Vec2 p1, Vec2 center, Vec2 normal, float minA, float maxA, float minB, float maxB) {
			//	if (hits == null) { hits = new List<PolygonConvexHit>(); }
			//	hits.Add(new PolygonConvexHit(polygon, convexIndex, p0, p1, center, normal, minA, maxA, minB, maxB));
			//}
		}
		public bool TryGetPolyCollision(Polygon other, ref List<CollisionData> collisionDatas) {
			UpdateCacheAsNeeded();
			other.UpdateCacheAsNeeded();
			//CollisionData result = new CollisionData();
			//result.Init();
			//if (!IsColliding(this, other, ref result)) { return false; }
			//if (!FindMinSeparation(other, this, ref result)) { return result; }
			//Vec2 direction = other.position - this.position;
			//if (Vec2.Dot(direction, result.Normal) < 0) {
			//	result.Normal = -result.Normal;
			//}
			//result.IsColliding = true;
			//return result;
			return TryGetPolygonCollision(this, other, ref collisionDatas);
		}

		private static bool TryGetPolygonCollision(Polygon mainPoly, Polygon otherPoly, ref List<CollisionData> collisionDatas) {
			bool foundCollision = false;
			for (int mainConvex = 0; mainConvex < mainPoly.convexHullIndexLists.Length; mainConvex++) {
				for (int otherConvex = 0; otherConvex < otherPoly.convexHullIndexLists.Length; otherConvex++) {
					CollisionData result = new CollisionData();
					result.Init();
					bool collisionJustHappened = CheckCollisionOfSubMeshes(mainPoly, otherPoly, mainConvex, otherConvex, ref result)
					&& CheckCollisionOfSubMeshes(otherPoly, mainPoly, otherConvex, mainConvex, ref result);
					if (collisionJustHappened) {
						result.IsColliding = true;
						if (collisionDatas == null) { collisionDatas = new List<CollisionData>(); }
						Vec2 direction = otherPoly.position - mainPoly.position;
						if (Vec2.Dot(direction, result.Normal) < 0) {
							result.Normal = -result.Normal;
						}
						collisionDatas.Add(result);
						foundCollision |= collisionJustHappened;
						return foundCollision;
					}
				}
			}
			return foundCollision;
		}

		private static bool CheckCollisionOfSubMeshes(Polygon mainPoly, Polygon otherPoly, int mainConvex, int otherConvex, ref CollisionData result) {
			int[] mainIndexList = mainPoly.convexHullIndexLists[mainConvex];
			for (int i = 0; i < mainIndexList.Length; i++) {
				int index0 = mainIndexList[i], index1 = mainIndexList[(i + 1) % mainIndexList.Length];
				Vec2 p0 = mainPoly.GetPoint(index0);
				Vec2 p1 = mainPoly.GetPoint(index1);
				Vec2 edge = p1 - p0;
				Vec2 edgeNormal = new Vec2(-edge.Y, edge.X);
				edgeNormal.Normalize(); // normalize for accurate depth measurement
				ProjectPolygon(edgeNormal, mainPoly, mainConvex, out float minA, out float maxA);
				ProjectPolygon(edgeNormal, otherPoly, otherConvex, out float minB, out float maxB);
				if (minA >= maxB || minB >= maxA) {
					return false; // Gap found, collision impossible
				}
				float axisDepth = Math.Min(maxB - minA, maxA - minB);
				if (mainPoly.IsIndexPairConsecutive(index0, index1) && axisDepth < result.Depth) {
					result.Depth = axisDepth;
					result.Normal = edgeNormal;
					result.vertIndex0 = index0;
					result.vertIndex1 = index1;
					result.polygonA = mainPoly;
					result.polygonB = otherPoly;
					//Vec2 center = (p0 + p1) / 2;
					//result.AddPolygonHit(mainPoly, mainConvex, p0, p1, center, axis, minA, maxA, minB, maxB);
					//result.AddPolygonHit(otherPoly, otherConvex, p0, p1, center, axis, minA, maxA, minB, maxB);
					//Vec2 direction = otherPoly.position - mainPoly.position;
					//if (Vec2.Dot(direction, result.Normal) < 0) {
					//	result.Normal = -result.Normal;
					//}
				}
			}
			return true;
		}
		private static void ProjectPolygon(Vec2 axis, Polygon poly, int convexIndex, out float min, out float max) {
			min = float.MaxValue; max = float.MinValue;
			int[] indexList = poly.convexHullIndexLists[convexIndex];
			for (int i = 0; i < indexList.Length; i++) {
				float projection = Vec2.Dot(poly.GetPoint(indexList[i]), axis);
				if (projection < min) { min = projection; }
				if (projection > max) { max = projection; }
			}
		}
	}
}

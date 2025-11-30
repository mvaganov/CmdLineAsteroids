using MathMrV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MrV.collision {

	public static class Collision {
		public static bool PolyCircle(Vec2[] poly, Vec2 circleCenter, float radius, out Vec2 closestPoint) {
			float radiusSq = radius * radius;
			closestPoint = Vec2.NaN;
			float minDistanceSq = float.MaxValue;
			for (int i = 0; i < poly.Length; i++) {
				Vec2 a = poly[i];
				Vec2 b = poly[(i + 1) % poly.Length];
				Vec2 closestOnSegment = GetClosestPointOnSegment(a, b, circleCenter);
				float dSq = Vec2.DistanceSquared(circleCenter, closestOnSegment);
				if (dSq < minDistanceSq) {
					minDistanceSq = dSq;
					closestPoint = closestOnSegment;
				}
			}
			if (minDistanceSq <= radiusSq) { return true; }
			if (IsPointInConvexPolygon(circleCenter, poly)) { return true; }
			return false;
		}

		private static Vec2 GetClosestPointOnSegment(Vec2 a, Vec2 b, Vec2 p) {
			Vec2 lineDelta = b - a;
			Vec2 pointRelativeToLine = p - a;
			float pointProjectedOnLine = Vec2.Dot(pointRelativeToLine, lineDelta) / Vec2.Dot(lineDelta, lineDelta);
			pointProjectedOnLine = Math.Clamp(pointProjectedOnLine, 0f, 1f);
			return a + (lineDelta * pointProjectedOnLine);
		}

		private static bool IsPointInConvexPolygon(Vec2 point, Vec2[] poly) {
			bool expectClockwiseWinding = false;
			for (int i = 0; i < poly.Length; i++) {
				Vec2 a = poly[i];
				Vec2 b = poly[(i + 1) % poly.Length];
				Vec2 edgeDelta = b - a;
				Vec2 pointToA = point - a;
				float cross = (edgeDelta.x * pointToA.y) - (edgeDelta.y * pointToA.x);
				bool isOnRight = (cross > 0);
				if (i == 0) {
					expectClockwiseWinding = isOnRight;
				} else if (isOnRight == expectClockwiseWinding) { return false; }
			}
			return true;
		}

		public static bool PolyPoly(IList<Vec2> polyA, IList<Vec2> polyB) {
			if (HasSeparatingAxis(polyA, polyB)) { return false; }
			if (HasSeparatingAxis(polyB, polyA)) { return false; }
			return true;
		}

		private static bool HasSeparatingAxis(IList<Vec2> mainPoly, IList<Vec2> otherPoly) {
			bool foundGap = false;
			for (int i = 0; i < mainPoly.Count && !foundGap; i++) {
				Vec2 a = mainPoly[i];
				Vec2 b = mainPoly[(i + 1) % mainPoly.Count];
				Vec2 edgeDelta = b - a;
				Vec2 normal = new Vec2(-edgeDelta.y, edgeDelta.x);
				ProjectPolygon(normal, mainPoly, out float minA, out float maxA);
				ProjectPolygon(normal, otherPoly, out float minB, out float maxB);
				foundGap = (maxA < minB || maxB < minA);
			}
			return foundGap;
		}

		private static void ProjectPolygon(Vec2 axis, IList<Vec2> poly, out float min, out float max) {
			min = float.MaxValue;
			max = float.MinValue;
			for (int i = 0; i < poly.Count; i++) {
				float projection = Vec2.Dot(poly[i], axis);
				if (projection < min) { min = projection; }
				if (projection > max) { max = projection; }
			}
		}

		public struct CollisionManifold {
			public bool IsColliding;
			public Vec2 Normal; // The direction to push B out of A
			public float Depth;    // How far to push
		}
		public static CollisionManifold PolyPoly(IList<Vec2> polyA, IList<Vec2> polyB, Vec2 centerA, Vec2 centerB) {
			CollisionManifold result = new CollisionManifold();
			result.IsColliding = false;
			result.Depth = float.MaxValue; // Start with a huge number
			result.Normal = Vec2.Zero;

			// 1. Check edges of Polygon A
			if (!FindMinSeparation(polyA, polyB, ref result)) return result; // Found a gap, exit

			// 2. Check edges of Polygon B
			if (!FindMinSeparation(polyB, polyA, ref result)) return result; // Found a gap, exit

			// 3. Fix Direction
			// The normal currently points perpendicular to the edge we collided on.
			// We need to make sure it points from A towards B so we know which way to push.
			Vec2 direction = centerB - centerA;

			if (Vec2.Dot(direction, result.Normal) < 0) {
				result.Normal = -result.Normal;
			}

			result.IsColliding = true;
			return result;
		}

		// Returns false if a gap is found (no collision)
		// Updates the 'result' struct if a smaller overlap is found
		private static bool FindMinSeparation(IList<Vec2> mainPoly, IList<Vec2> otherPoly, ref CollisionManifold result) {
			for (int i = 0; i < mainPoly.Count; i++) {
				Vec2 p1 = mainPoly[i];
				Vec2 p2 = mainPoly[(i + 1) % mainPoly.Count];

				Vec2 edge = p2 - p1;

				// CRITICAL: We must normalize the axis for accurate depth measurement
				// Normal is (-y, x)
				Vec2 axis = new Vec2(-edge.Y, edge.X);
				axis.Normalize();

				// Project both polygons
				ProjectPolygon(axis, mainPoly, out float minA, out float maxA);
				ProjectPolygon(axis, otherPoly, out float minB, out float maxB);

				// Check for Gap
				if (minA >= maxB || minB >= maxA) {
					return false; // Gap found, no collision possible
				}

				// Calculate Overlap
				float axisDepth = Math.Min(maxB - minA, maxA - minB);

				// If this is the smallest overlap we've seen so far, store it
				if (axisDepth < result.Depth) {
					result.Depth = axisDepth;
					result.Normal = axis;
				}
			}
			return true;
		}

		// Define a wrapper for a simple convex polygon
		// A simple struct to hold the vertices of a convex hull for easier manipulation during merging.
		public class ConvexHull {
			public List<Vec2> Vertices { get; set; } = new List<Vec2>();
		}

		public class Polygon {
			public Vec2 Position { get; set; }
			public ConvexHull[] Hulls { get; set; }

			public Polygon(Vec2[] verts) {
				int[][] triangles = DecomposePolygon(verts);
				for (int i = 0; i < triangles.Length; i++) {
					Console.WriteLine($"triange {i}: [" + string.Join(",", triangles[i]) + "]");
				}
				Hulls = MergeTrianglesToConvexHulls(triangles, verts).ToArray();
				for(int i = 0; i < Hulls.Length; ++i) {
					Console.WriteLine($"Hull {i}: [" + string.Join(",", Hulls[i].Vertices) + "]");
				}

				int[][] hulls = ConvertTriangleListIntoConvexHulls(triangles, verts);
				for (int i = 0; i < hulls.Length; ++i) {
					Console.WriteLine($"hull {i}: [" + string.Join(",", hulls[i]) + "]");
				}
			}

			public static CollisionManifold ConcavePolyPoly(Polygon objA, Polygon objB) {
				CollisionManifold finalManifold = new CollisionManifold {
					IsColliding = false,
					Depth = float.MaxValue
				};

				// We must check every convex hull of A against every convex hull of B
				foreach (var hullA in objA.Hulls) {
					foreach (var hullB in objB.Hulls) {
						// Call the existing SAT function (PolyPoly) you already have.
						// Note: You may need to translate hullA/hullB vertices 
						// from local space to world space before passing them here.

						CollisionManifold currentManifold = PolyPoly(
								hullA.Vertices,
								hullB.Vertices,
								objA.Position,
								objB.Position
						);

						// IMPORTANT: Track only the shallowest, most impactful collision
						if (currentManifold.IsColliding) {
							if (currentManifold.Depth < finalManifold.Depth) {
								finalManifold = currentManifold;
							}

							// Set IsColliding to true, even if we are still looping
							finalManifold.IsColliding = true;
						}
					}
				}

				return finalManifold;
			}
		}

		//public void HandleCollision(Vec2[] shipVertices, Vec2[] asteroidVertices, Vec2 shipPos, Vec2 astPos) {
		//	CollisionManifold manifold = Collision.PolyPoly(shipVertices, asteroidVertices, shipPos, astPos);

		//	if (manifold.IsColliding) {
		//		// 1. Positional Correction (Un-stick them)
		//		// Move asteroid away along the normal by the depth amount
		//		// (Usually you split this: move player 50% back and asteroid 50% forward)
		//		asteroidPosition += manifold.Normal * manifold.Depth;

		//		// 2. Resolve Velocity (Bounce)
		//		Vec2 relativeVelocity = asteroidVelocity - shipVelocity;
		//		float velAlongNormal = Vec2.Dot(relativeVelocity, manifold.Normal);

		//		// Only bounce if they are moving toward each other
		//		if (velAlongNormal < 0) {
		//			float restitution = 0.8f; // Bounciness (0 = rock, 1 = super ball)

		//			// Simple impulse scalar
		//			float j = -(1 + restitution) * velAlongNormal;

		//			// If objects have mass, divide j by (1/MassA + 1/MassB) here

		//			Vec2 impulse = manifold.Normal * j;

		//			asteroidVelocity += impulse; // Add to B
		//			shipVelocity -= impulse;     // Subtract from A
		//		}
		//	}

		//	// Assuming you have the following properties on your Asteroid class:
		//	// public float Mass { get; set; }        // M
		//	// public float Inertia { get; set; }     // I
		//	// public float AngularVelocity { get; set; } // omega

		//	// --- INSIDE YOUR COLLISION RESOLUTION LOGIC ---

		//	// 1. Get Collision Data (from previous step's manifold)
		//	// CollisionManifold manifold = ... 
		//	// float restitution = 0.8f;

		//	// 2. Approximate Point of Contact (for simplicity, using A's center and Normal/Depth)
		//	// More accurate: find the closest vertex of B to A, or midpoint of deepest edge.
		//	// For this example, let's use the object centers, which is less accurate but simpler:
		//	Vec2 contactPoint = shipPos + manifold.Normal * (manifold.Depth / 2f);

		//	// 3. Calculate Radius Vectors
		//	Vec2 rA = contactPoint - shipPos;
		//	Vec2 rB = contactPoint - asteroidPos;

		//	// 4. Calculate relative velocity, including rotation component
		//	// v_rel = (vB + (wB x rB)) - (vA + (wA x rA))
		//	// Cross product (w x r) in 2D is: (-w*ry, w*rx)
		//	Vec2 vA_rot = new Vec2(-ship.AngularVelocity * rA.Y, ship.AngularVelocity * rA.X);
		//	Vec2 vB_rot = new Vec2(-asteroid.AngularVelocity * rB.Y, asteroid.AngularVelocity * rB.X);

		//	Vec2 relativeVelocity = (asteroid.Velocity + vB_rot) - (ship.Velocity + vA_rot);
		//	float velAlongNormal = Vec2.Dot(relativeVelocity, manifold.Normal);

		//	// Only resolve if closing
		//	if (velAlongNormal >= 0) return;

		//	// 5. Calculate 2D Cross Products (r x n)
		//	// This is the scalar component of torque
		//	float rACrossN = (rA.X * manifold.Normal.Y) - (rA.Y * manifold.Normal.X);
		//	float rBCrossN = (rB.X * manifold.Normal.Y) - (rB.Y * manifold.Normal.X);

		//	// 6. Calculate the Full Impulse Denominator
		//	float denominator =
		//			(1f / ship.Mass) + (1f / asteroid.Mass) +
		//			(rACrossN * rACrossN / ship.Inertia) +
		//			(rBCrossN * rBCrossN / asteroid.Inertia);

		//	// 7. Calculate the final scalar impulse magnitude (j)
		//	float j = -(1f + restitution) * velAlongNormal / denominator;

		//	// 8. Apply Linear and Angular Impulse
		//	Vec2 impulse = manifold.Normal * j;

		//	// Linear Application
		//	ship.Velocity -= impulse * (1f / ship.Mass);
		//	asteroid.Velocity += impulse * (1f / asteroid.Mass);

		//	// Angular Application (This creates the spin!)
		//	ship.AngularVelocity -= (rACrossN * j) / ship.Inertia;
		//	asteroid.AngularVelocity += (rBCrossN * j) / asteroid.Inertia;
		//}
	//}
	//public static class PolygonDecomposer {
		// The main function to decompose a simple polygon (no self-intersections) into triangles.
		// NOTE: This algorithm guarantees convex hulls (triangles) but not the *minimum* number of them.
		// The output is an array of integer arrays, where each inner array is a set of 3 indices 
		// referencing the original 'vertices' array.
		public static int[][] DecomposePolygon(Vec2[] vertices) {
			if (vertices.Length < 3) return Array.Empty<int[]>();
			if (vertices.Length == 3) return new int[][] { new[] { 0, 1, 2 } };

			// We use a list of indices to easily remove vertices during clipping.
			List<int> indices = Enumerable.Range(0, vertices.Length).ToList();
			List<int[]> convexHulls = new List<int[]>();

			int safetyCounter = vertices.Length * 3; // Prevent infinite loops on complex/invalid input

			// Loop until only 3 vertices remain (the final triangle)
			while (indices.Count > 2 && safetyCounter-- > 0) {
				// Find an ear (a convex vertex whose triangle contains no other vertices)
				bool earFound = false;
				for (int i = 0; i < indices.Count; i++) {
					int iPrev = (i == 0) ? indices.Count - 1 : i - 1;
					int iNext = (i == indices.Count - 1) ? 0 : i + 1;

					int p0 = indices[iPrev];
					int p1 = indices[i];
					int p2 = indices[iNext];

					// Check 1: Is the vertex p1 convex?
					if (IsConvex(vertices[p0], vertices[p1], vertices[p2])) {
						// Check 2: Does the triangle (p0, p1, p2) contain any other reflex vertex?
						if (IsEar(vertices, indices, p0, p1, p2)) {
							// Found an ear! Clip it.
							convexHulls.Add(new[] { p0, p1, p2 });
							indices.RemoveAt(i); // Remove the vertex p1 from the polygon
							earFound = true;
							break;
						}
					}
				}

				if (!earFound && indices.Count > 3) {
					// This state suggests a non-simple (self-intersecting) polygon, 
					// or a highly complex concave case where simple ear clipping gets stuck.
					// For robust production code, this requires more complex decomposition logic.
					Console.WriteLine("Warning: Ear clipping failed or polygon is non-simple.");
					break;
				}
			}

			return convexHulls.ToArray();
		}

		// --- Helper Functions ---

		// Checks if the triangle p0, p1, p2 is convex (internal angle < 180 deg) 
		// assuming the polygon vertices are in CCW order.
		// If the 2D cross product (or Z component of the 3D cross product) is positive, it's convex.
		private static bool IsConvex(Vec2 p0, Vec2 p1, Vec2 p2) {
			// Vector v1 = p1 - p0
			// Vector v2 = p2 - p1
			// 2D Cross Product: (v1.X * v2.Y) - (v1.Y * v2.X)
			float crossProduct = (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);

			// CCW ordering means a positive cross product is convex.
			return crossProduct > 0;
		}

		// Checks if any other point (specifically, any reflex vertex) in the polygon 
		// is inside the candidate triangle (p0, p1, p2).
		private static bool IsEar(Vec2[] vertices, List<int> currentIndices, int p0, int p1, int p2) {
			Vec2 v0 = vertices[p0];
			Vec2 v1 = vertices[p1];
			Vec2 v2 = vertices[p2];

			// Check every other vertex (that is not p0, p1, or p2)
			foreach (int pTest in currentIndices) {
				if (pTest == p0 || pTest == p1 || pTest == p2) continue;

				// If the point is inside the triangle, it's not a valid ear.
				if (IsPointInTriangle(vertices[pTest], v0, v1, v2)) {
					return false;
				}
			}
			return true;
		}

		// Uses the Barycentric coordinate method (or signed area/cross product test) 
		// to check if point P is inside triangle (A, B, C).
		private static bool IsPointInTriangle(Vec2 P, Vec2 A, Vec2 B, Vec2 C) {
			// Area(ABC), Area(PBC), Area(PCA), Area(PAB) must all have the same sign 
			// (or sum of sub-areas equals total area).

			// Helper for cross product / signed area check
			float CrossProduct2D(Vec2 a, Vec2 b) => (a.X * b.Y) - (a.Y * b.X);

			// Check if P is on the same side of AB, BC, and CA.
			// We use the 2D cross product sign. CCW order assumed.

			// Is P right of edge AB?
			float cross1 = CrossProduct2D(B - A, P - A);
			// Is P right of edge BC?
			float cross2 = CrossProduct2D(C - B, P - B);
			// Is P right of edge CA?
			float cross3 = CrossProduct2D(A - C, P - C);

			// For a CCW triangle, all cross products should be non-negative if P is inside.
			// The check for P being "right" of the edge is `cross < 0` for CCW order.
			// A common implementation simply checks for consistency of sign.

			// Since the order is CCW, if P is inside, all signs should be positive (or zero for edge).
			// If we use the sign of the cross product for the direction, we check if all are positive (inside):
			bool s1 = cross1 >= 0;
			bool s2 = cross2 >= 0;
			bool s3 = cross3 >= 0;

			// True if all are positive (inside) or all are negative (outside, if winding was CW)
			// Since we know the triangle is defined CCW (from IsConvex check), we only check for positive.
			return s1 && s2 && s3;
		}

		/// <summary>
		/// Merges adjacent triangles (or small convex hulls) from a decomposition into 
		/// larger convex polygons, aiming for the fewest total hulls.
		/// </summary>
		/// <param name="triangleIndices">The array of triangle index arrays from DecomposePolygon.</param>
		/// <param name="originalVertices">The original array of Vec2 vertices.</param>
		/// <returns>A list of final, optimized convex hulls (as arrays of Vec2).</returns>
		public static List<ConvexHull> MergeTrianglesToConvexHulls(int[][] triangleIndices, Vec2[] originalVertices) {
			if (triangleIndices.Length == 0) { return new List<ConvexHull>(); }

			// 1. Convert index arrays into a working list of ConvexHull objects (vertex lists)
			List<ConvexHull> hulls = triangleIndices.Select(indices => new ConvexHull {
				Vertices = indices.Select(i => originalVertices[i]).ToList()
			}).ToList();

			bool merged = true;
			// Keep attempting merges until a full pass results in no changes
			while (merged) {
				merged = false;

				// Loop through all unique pairs of current hulls
				for (int i = 0; i < hulls.Count; i++) {
					for (int j = i + 1; j < hulls.Count; j++) {
						ConvexHull hullA = hulls[i];
						ConvexHull hullB = hulls[j];

						// Attempt to merge A and B
						ConvexHull newHull = TryMerge(hullA, hullB);

						if (newHull != null) {
							// Merge successful! Replace the two old hulls with the new one.
							hulls.RemoveAt(j); // Remove the one with the higher index first
							hulls.RemoveAt(i);
							hulls.Add(newHull);

							merged = true;
							// Restart the loop to ensure we check the new merged hull against others
							goto RestartLoop;
						}
					}
				}
			RestartLoop:;
			}

			// 3. Convert final internal hull list to the desired Vec2[] array
			return hulls;// hulls.Select(h => h.Vertices.ToArray()).ToList();
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
				int[] mergedHull = SpliceHull(indexListHullA, indexListHullB, hullAIndex, hullBIndex);
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
			int[] SpliceHull(int[] indexListHullA, int[] indexListHullB, int hullASharedEdgeIndex, int hullBSharedEdgeIndex) {
				List<int> newVertices = new List<int>();
				newVertices.AddRange(indexListHullA);
				List<int> hullBsegment = new List<int>();
				int currIndex = (hullBSharedEdgeIndex + 2) % indexListHullB.Length;
				while (currIndex != hullBSharedEdgeIndex) {
					hullBsegment.Add(indexListHullB[currIndex]);
					currIndex = (currIndex + 1) % indexListHullB.Length;
				}
				newVertices.InsertRange(hullASharedEdgeIndex + 1, hullBsegment);
				return newVertices.ToArray();
			}
		}

		/// <summary>
		/// Attempts to merge two adjacent convex polygons into a single new convex polygon.
		/// Returns the new hull if successful, otherwise null.
		/// </summary>
		private static ConvexHull TryMerge(ConvexHull hullA, ConvexHull hullB) {
			if (TryGetSharedEdgeIndices(hullA, hullB, out int hullAIndex, out int hullBIndex)) {
				List<Vec2> mergedHull = SpliceHull(hullA.Vertices, hullB.Vertices, hullAIndex, hullBIndex);
				if (IsHullConvex(mergedHull)) { // only give back convex shape
					return new ConvexHull { Vertices = mergedHull };
				}
			}
			return null;
			bool TryGetSharedEdgeIndices(ConvexHull hullA, ConvexHull hullB, out int hullAIndex, out int hullBIndex) {
				for (hullAIndex = 0; hullAIndex < hullA.Vertices.Count; hullAIndex++) {
					Vec2 hullAvert0 = hullA.Vertices[hullAIndex];
					Vec2 hullAvert1 = hullA.Vertices[(hullAIndex + 1) % hullA.Vertices.Count];
					for (hullBIndex = 0; hullBIndex < hullB.Vertices.Count; hullBIndex++) {
						Vec2 hullBvert0 = hullB.Vertices[hullBIndex];
						Vec2 hullBvert1 = hullB.Vertices[(hullBIndex + 1) % hullB.Vertices.Count];
						if(hullAvert0.Equals(hullBvert1) && hullAvert1.Equals(hullBvert0)) {
							return true;
						}
					}
				}
				hullBIndex = -1;
				return false;
			}
			List<Vec2> SpliceHull(IList<Vec2> hullAVerts, IList<Vec2> hullBVerts, int hullASharedEdgeIndex, int hullBSharedEdgeIndex) {
				List<Vec2> newVertices = new List<Vec2>();
				newVertices.AddRange(hullA.Vertices);
				List<Vec2> hullBsegment = new List<Vec2>();
				int currIndex = (hullBSharedEdgeIndex + 2) % hullB.Vertices.Count;
				while (currIndex != hullBSharedEdgeIndex) {
					hullBsegment.Add(hullB.Vertices[currIndex]);
					currIndex = (currIndex + 1) % hullB.Vertices.Count;
				}
				newVertices.InsertRange(hullASharedEdgeIndex + 1, hullBsegment);
				return newVertices;
			}
		}

		// Checks if a given list of vertices forms a convex polygon (assuming CCW order).
		// This is done by checking the convexity at EVERY vertex.
		private static bool IsHullConvex(IList<Vec2> vertices) {
			if (vertices.Count < 3) return false;
			if (vertices.Count == 3) return true; // Triangles are always convex

			for (int i = 0; i < vertices.Count; i++) {
				int iPrev = (i == 0) ? vertices.Count - 1 : i - 1;
				int iNext = (i == vertices.Count - 1) ? 0 : i + 1;

				// Reusing the existing IsConvex logic
				if (!IsConvex(vertices[iPrev], vertices[i], vertices[iNext])) {
					return false; // Found a reflex (concave) vertex
				}
			}
			return true;
		}

		private static bool IsHullConvex(IList<Vec2> vertList, IList<int> indexList) {
			if (indexList.Count < 3) { return false; }
			if (indexList.Count == 3) { return true; } // Triangles are always convex
			bool expectCCW = true;
			for (int i = 0; i < indexList.Count; i++) {
				int iPrev = (i == 0) ? indexList.Count - 1 : i - 1;
				int iNext = (i == indexList.Count - 1) ? 0 : i + 1;
				Vec2 p0 = vertList[indexList[iPrev]];
				Vec2 p1 = vertList[indexList[i]];
				Vec2 p2 = vertList[indexList[iNext]];
				float crossProduct = (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
				bool isCCW = crossProduct > 0;
				if (i == 0) { expectCCW = isCCW; }
				else if (isCCW != expectCCW) {
					return false;
				}
				//// Reusing the existing IsConvex logic
				//if (!IsConvex(vertList[indexList[iPrev]], vertList[indexList[i]], vertList[indexList[iNext]])) {
				//	return false; // Found a reflex (concave) vertex
				//}
			}
			return true;
		}


		// --- Helper Functions (Existing) ---

		//// Checks if the triangle p0, p1, p2 is convex (internal angle < 180 deg) 
		//// assuming the polygon vertices are in CCW order.
		//// If the 2D cross product (or Z component of the 3D cross product) is positive, it's convex.
		//private static bool IsConvex(Vec2 p0, Vec2 p1, Vec2 p2) {
		//	// Vector v1 = p1 - p0
		//	// Vector v2 = p2 - p1
		//	// 2D Cross Product: (v1.X * v2.Y) - (v1.Y * v2.X)
		//	float crossProduct = (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);

		//	// CCW ordering means a positive cross product is convex.
		//	return crossProduct > 0;
		//}

		//// Checks if any other point (specifically, any reflex vertex) in the polygon 
		//// is inside the candidate triangle (p0, p1, p2).
		//private static bool IsEar(Vec2[] vertices, List<int> currentIndices, int p0, int p1, int p2) {
		//	Vec2 v0 = vertices[p0];
		//	Vec2 v1 = vertices[p1];
		//	Vec2 v2 = vertices[p2];

		//	// Check every other vertex (that is not p0, p1, or p2)
		//	foreach (int pTest in currentIndices) {
		//		if (pTest == p0 || pTest == p1 || pTest == p2) continue;

		//		// If the point is inside the triangle, it's not a valid ear.
		//		if (IsPointInTriangle(vertices[pTest], v0, v1, v2)) {
		//			return false;
		//		}
		//	}
		//	return true;
		//}

		//// Uses the Barycentric coordinate method (or signed area/cross product test) 
		//// to check if point P is inside triangle (A, B, C).
		//private static bool IsPointInTriangle(Vec2 P, Vec2 A, Vec2 B, Vec2 C) {
		//	// Helper for cross product / signed area check
		//	float CrossProduct2D(Vec2 a, Vec2 b) => (a.X * b.Y) - (a.Y * b.X);

		//	// Check if P is on the same side of AB, BC, and CA.
		//	// We use the 2D cross product sign. CCW order assumed.

		//	// Is P right of edge AB?
		//	float cross1 = CrossProduct2D(B - A, P - A);
		//	// Is P right of edge BC?
		//	float cross2 = CrossProduct2D(C - B, P - B);
		//	// Is P right of edge CA?
		//	float cross3 = CrossProduct2D(A - C, P - C);

		//	// For a CCW triangle, all cross products should be non-negative if P is inside.
		//	bool s1 = cross1 >= 0;
		//	bool s2 = cross2 >= 0;
		//	bool s3 = cross3 >= 0;

		//	return s1 && s2 && s3;
		//}

		//public static Collision.ConcaveObject Compile(Vec2[] shipVertices) {
		//	int[][] hullIndices = DecomposePolygon(shipVertices);
		//	// Now, convert the indices back into ConvexHull structs (for the ConcaveObject class)
		//	List<ConvexHull> shipHulls = new List<ConvexHull>();
		//	foreach (var indices in hullIndices) {
		//		shipHulls.Add(new ConvexHull {
		//			Vertices = new Vec2[] { shipVertices[indices[0]], shipVertices[indices[1]], shipVertices[indices[2]] }
		//			// Note: In a production engine, you would also calculate the Mass, Inertia, and CenterOfMass for this small triangle hull here.
		//		});
		//	}
		//	return new ConcaveObject { Hulls = shipHulls.ToArray(), /* ... */ };
		//}
	}
}

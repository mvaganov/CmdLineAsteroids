using asteroids;
using MathMrV;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace collision {
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
			Vec2 pointRelativeToLine = p - a;
			Vec2 lineDelta = b - a;
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

		public static bool PolyPoly(Vec2[] polyA, Vec2[] polyB) {
			if (HasSeparatingAxis(polyA, polyB)) { return false; }
			if (HasSeparatingAxis(polyB, polyA)) { return false; }
			return true;
		}

		private static bool HasSeparatingAxis(Vec2[] mainPoly, Vec2[] otherPoly) {
			bool foundGap = false;
			for (int i = 0; i < mainPoly.Length && !foundGap; i++) {
				Vec2 a = mainPoly[i];
				Vec2 b = mainPoly[(i + 1) % mainPoly.Length];
				Vec2 edgeDelta = b - a;
				Vec2 normal = new Vec2(-edgeDelta.y, edgeDelta.x);
				ProjectPolygon(normal, mainPoly, out float minA, out float maxA);
				ProjectPolygon(normal, otherPoly, out float minB, out float maxB);
				foundGap = (maxA < minB || maxB < minA);
			}
			return foundGap;
		}

		private static void ProjectPolygon(Vec2 axis, Vec2[] poly, out float min, out float max) {
			min = float.MaxValue;
			max = float.MinValue;
			for (int i = 0; i < poly.Length; i++) {
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
		public static CollisionManifold PolyPoly(Vec2[] polyA, Vec2[] polyB, Vec2 centerA, Vec2 centerB) {
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
		private static bool FindMinSeparation(Vec2[] mainPoly, Vec2[] otherPoly, ref CollisionManifold result) {
			for (int i = 0; i < mainPoly.Length; i++) {
				Vec2 p1 = mainPoly[i];
				Vec2 p2 = mainPoly[(i + 1) % mainPoly.Length];

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
	}

	public class CollisionData {
		public ICollidable self;
		public ICollidable other;
		public Vec2 point;
		public Vec2 pointSelf;
		public Vec2 pointOther;
		public int colliderIndexSelf = -1;
		public int colliderIndexOther = -1;
		public List<CollisionLogic.Function> collisionFunctions;
		public string Name => (self is IGameObject a ? a.Name : "?") + "." + (other is IGameObject b ? b.Name : "?");
		public void Get<TypeA, TypeB>(out TypeA self, out TypeB other) {
			self = (TypeA)this.self;
			other = (TypeB)this.other;
		}
		public CollisionData(ICollidable self, ICollidable other, Vec2 point, Vec2 pointSelf, Vec2 pointOther) {
			this.self = self;
			this.other = other;
			this.point = point;
			this.pointSelf = pointSelf;
			this.pointOther = pointOther;
		}
		public void SetParticipants(ICollidable self, ICollidable other) { this.self = self; this.other = other; }
		public static CollisionData ForCircles(Circle a, Circle b) {
			if (Circle.TryGetCircleCollisionPoints(a, b, out Vec2 pa, out Vec2 pb)) {
				return new CollisionData(null, null, (pa + pb) / 2, pa, pb);
			}
			return null;
		}
		public override int GetHashCode() {
			int hash = 0;
			if (self != null) { hash ^= self.GetHashCode(); }
			if (other != null) { hash ^= other.GetHashCode(); }
			return hash;
		}
		public override bool Equals(object obj) => obj is CollisionData cd && Equals(cd);
		public bool Equals(CollisionData other) => this.self == other.self && this.other == other.other && this.point == other.point;
		public void CalculateCollisionResults(List<CollisionLogic.ToResolve> out_collisionResolutions) {
			// TODO organize CollisionData to avoid duplicates, and execute collisionFunctions after dups are culled.
			for (int i = 0; i < collisionFunctions.Count; i++) {
				CollisionLogic.Function f = collisionFunctions[i];
				Action collisionResult = f.Invoke(this);
				if (collisionResult != null) {
					out_collisionResolutions.Add((this, collisionResult));
				}
			}
		}
	}
	public struct CollisionPair {
		public Type a, b;
		public CollisionPair(ICollidable a, ICollidable b) : this(a.GetType(), b.GetType()) { }
		public CollisionPair(Type a, Type b) {
			Type collideType = typeof(ICollidable);
			bool validPair = collideType.IsAssignableFrom(a) && collideType.IsAssignableFrom(b);
			if (!validPair) {
				throw new Exception("invalid pair, both must be collidable");
			}
			this.a = a;
			this.b = b;
		}
		public override int GetHashCode() { return a.GetHashCode() ^ b.GetHashCode(); }
		public override bool Equals(object obj) => obj is CollisionPair other && Equals(other);
		public bool Equals(CollisionPair other) => a == other.a && b == other.b;
		public static implicit operator CollisionPair((Type a, Type b) tuple) => new CollisionPair(tuple.a, tuple.b);
	}

	public static class CollisionLogic {
		/// <returns>collision resolution function, null if no collision happened or collision was trivial</returns>
		public delegate Action Function(CollisionData collision);
		public struct ToResolve {
			public CollisionData collision;
			public Action resolution;
			public ToResolve(CollisionData collision, Action resolution) {
				this.collision = collision;
				this.resolution = resolution;
			}
			public static implicit operator ToResolve((CollisionData collision, Action resolution) tuple) =>
				new ToResolve(tuple.collision, tuple.resolution);
			public override int GetHashCode() => collision.GetHashCode();
			public override bool Equals(object obj) => obj is ToResolve other && Equals(other);
			public bool Equals(ToResolve other) => collision.Equals(other.collision);
		}
		//public static List<ToResolve> DoCollisionLogic<T>(IList<T> collidables,
		//	Dictionary<CollisionPair, List<Function>> rules) where T : ICollidable {
		//	List<CollisionData> collisionData = new List<CollisionData>();
		//	FindCollisions(collidables, rules, collisionData);
		//	List<ToResolve> collisionResolutions = new List<ToResolve>();
		//	CalculateCollisionResolution(collisionData, collisionResolutions);
		//	return collisionResolutions;
		//}
		public static void CalculateCollisions<T>(IList<T> collidables,
			Dictionary<CollisionPair, List<Function>> rules, IList<CollisionData> out_collisionData) where T : ICollidable {
			for (int i = 0; i < collidables.Count; i++) {
				ICollidable ci = collidables[i];
				for (int j = i + 1; j < collidables.Count; j++) {
					ICollidable cj = collidables[j];
					CollisionData a = DoCollisionLogicOnPair(ci, cj, rules);
					if (a != null) { out_collisionData.Add(a); }
					CollisionData b = DoCollisionLogicOnPair(cj, ci, rules);
					if (b != null) { out_collisionData.Add(b); }
				}
			}
		}
		public static CollisionData DoCollisionLogicOnPair(ICollidable a, ICollidable b,
			Dictionary<CollisionPair, List<Function>> rules) {
			if (!rules.TryGetValue(new CollisionPair(a, b), out List<Function> collisionFunctions)) {
				return null;
			}
			CollisionData collision = a.IsColliding(b);
			if (collision == null) {
				return null;
			}
			collision.SetParticipants(a, b);
			collision.collisionFunctions = collisionFunctions;
			return collision;
		}
		public static void CalculateCollisionResolution(IList<CollisionData> collisionData, List<ToResolve> out_collisionResolutions) {
			//for(int i = 0; i < collisionData.Count; ++i) {
			foreach(CollisionData collision in collisionData) {
				collision.CalculateCollisionResults(out_collisionResolutions);
			}
		}

		public static void DoCollisionLogicAndResolve<T>(IList<T> collidables,
			Dictionary<CollisionPair, List<Function>> rules) where T : ICollidable {
			//List<ToResolve> collisionResolutions = DoCollisionLogic<T>(collidables, rules);
			List<CollisionData> collisionData = new List<CollisionData>();
			CalculateCollisions(collidables, rules, collisionData);
			List<ToResolve> collisionResolutions = new List<ToResolve>();
			CalculateCollisionResolution(collisionData, collisionResolutions);
			collisionResolutions.ForEach(cr => cr.resolution.Invoke());
		}
	}
}

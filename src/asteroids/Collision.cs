using MathMrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class CollisionData {
		public ICollidable self;
		public ICollidable other;
		public Vec2 point;
		public Vec2 pointSelf;
		public Vec2 pointOther;
		public int colliderIndexSelf = -1;
		public int colliderIndexOther = -1;
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
		public static void DoCollisionLogic<T>(IList<T> collidables,
			Dictionary<CollisionPair, List<Function>> rules) where T : ICollidable {
			List<Action> collisionResolutions = new List<Action>();
			for (int i = 0; i < collidables.Count; i++) {
				ICollidable ci = collidables[i];
				for (int j = i + 1; j < collidables.Count; j++) {
					ICollidable cj = collidables[j];
					DoCollisionLogicOnPair(ci, cj, rules, collisionResolutions);
					DoCollisionLogicOnPair(cj, ci, rules, collisionResolutions);
				}
			}
			collisionResolutions.ForEach(r => r.Invoke());
		}
		private static void DoCollisionLogicOnPair(ICollidable a, ICollidable b,
			Dictionary<CollisionPair, List<Function>> rules, List<Action> collisionResolutions) {
			if (!rules.TryGetValue(new CollisionPair(a, b), out List<Function> collisionFunctions)) {
				return;
			}
			CollisionData collision = a.IsColliding(b);
			if (collision == null) {
				return;
			}
			collision.SetParticipants(a, b);
			for (int i = 0; i < collisionFunctions.Count; i++) {
				Function f = collisionFunctions[i];
				Action collisionResult = f.Invoke(collision);
				if (collisionResult != null) {
					collisionResolutions.Add(collisionResult);
				}
			}
		}
	}
}

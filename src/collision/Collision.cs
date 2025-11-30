using asteroids;
using MathMrV;
using System;
using System.Collections.Generic;

namespace collision {
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

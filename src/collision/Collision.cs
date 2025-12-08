using asteroids;
using MathMrV;
using System;
using System.Collections.Generic;
using ColliderID = System.Byte;

namespace collision {
	public class CollisionData {
		public ICollidable self;
		public ICollidable other;
		public Vec2 point;
		public Vec2 normal;
		public float depth;
		public int colliderIndexSelf = -1;
		public int colliderIndexOther = -1;
		public List<CollisionLogic.Function> collisionFunctions;
		public string Name => (self is IGameObject a ? a.Name : "?") + "." + (other is IGameObject b ? b.Name : "?");
		public void Get<TypeA, TypeB>(out TypeA self, out TypeB other) {
			self = (TypeA)this.self;
			other = (TypeB)this.other;
		}
		public CollisionData(ICollidable self, ICollidable other, Vec2 point, Vec2 normal, float depth) {
			this.self = self;
			this.other = other;
			this.point = point;
			this.normal = normal;
			if (normal.Magnitude > 1.1f || normal.Magnitude < 0.9f) {
				throw new Exception("bad normal");
			}
			this.depth = depth;
		}
		public void SetParticipants(ICollidable self, ICollidable other) { this.self = self; this.other = other; }
		public static CollisionData ForCircles(Circle a, Circle b) {
			if (Circle.TryGetCircleCollision(a, b, out Vec2 delta, out float depth)) {
				Vec2 normal = -delta.Normal;
				Vec2 centerOfCollision = a.center + normal * (a.radius - depth / 2);
				return new CollisionData(null, null, centerOfCollision, normal, depth);
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
			for (int i = 0; i < collisionFunctions.Count; i++) {
				CollisionLogic.Function f = collisionFunctions[i];
				Action collisionResult = f.Invoke(this);
				if (collisionResult != null) {
					out_collisionResolutions.Add((this, collisionResult));
				}
			}
		}
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
		public static void CalculateCollisions<T>(IList<T> collidables,
			Dictionary<(ColliderID,ColliderID), List<Function>> rules, IList<CollisionData> out_collisionData) where T : ICollidable {
			for (int objectAIndex = 0; objectAIndex < collidables.Count; objectAIndex++) {
				ICollidable objectA = collidables[objectAIndex];
				for (int objectBIndex = objectAIndex + 1; objectBIndex < collidables.Count; objectBIndex++) {
					ICollidable objectB = collidables[objectBIndex];
					CollisionData a = DetermineCollisionLogicForPair(objectA, objectB, rules);
					if (a != null) { out_collisionData.Add(a); }
					CollisionData b = DetermineCollisionLogicForPair(objectB, objectA, rules);
					if (b != null) { out_collisionData.Add(b); }
				}
			}
		}
		public static CollisionData DetermineCollisionLogicForPair(ICollidable a, ICollidable b,
			Dictionary<(ColliderID,ColliderID), List<Function>> rules) {
			if (!rules.TryGetValue((a.TypeId, b.TypeId), out List<Function> collisionFunctions)) {
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
			foreach(CollisionData collision in collisionData) {
				collision.CalculateCollisionResults(out_collisionResolutions);
			}
		}

		public static void DoCollisionLogicAndResolve<T>(IList<T> collidables,
			Dictionary<(ColliderID,ColliderID), List<Function>> rules) where T : ICollidable {
			List<CollisionData> collisionData = new List<CollisionData>();
			CalculateCollisions(collidables, rules, collisionData);
			List<ToResolve> collisionResolutions = new List<ToResolve>();
			CalculateCollisionResolution(collisionData, collisionResolutions);
			collisionResolutions.ForEach(cr => cr.resolution.Invoke());
		}
	}
}

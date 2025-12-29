using asteroids;
using ConsoleMrV;
using MathMrV;
using System;
using System.Collections.Generic;
using ColliderID = System.Byte;

namespace collision {
	public class CollisionRules {
		/// <returns>collision resolution function, null if no collision happened or collision was trivial</returns>
		public delegate Action Function(CollisionData collision);
		private Dictionary<(ColliderID, ColliderID), List<Function>> _rules;
		public bool TryGet((ColliderID,ColliderID) colliderTypePair, out IList<Function> rule) {
			if (_rules.TryGetValue(colliderTypePair, out List<Function> ruleList)) {
				rule = ruleList.AsReadOnly();
				return true;
			}
			rule = null;
			return false;
		}
		public CollisionRules(Dictionary<(ColliderID, ColliderID), List<Function>> rules) {
			_rules = rules;
		}
	}
	public class CollisionData {
		public ICollidable Self;
		public ICollidable Other;
		public Vec2 Point;
		public Vec2 Normal;
		public float Depth;
		public int ColliderIndexSelf = -1;
		public int ColliderIndexOther = -1;
		public IList<Vec2> Contacts;
		/// <summary>
		/// functions from the collision system that generate the function that resolves this collision
		/// </summary>
		internal IList<CollisionRules.Function> PrefetchedCollisionRules;
		public bool IsColliding;

		public static ObjectPool<CollisionData> collisionPool =
			new ObjectPool<CollisionData>();
		public string _source;

		public string Name => (Self is IGameObject a ? a.Name : "?") + "." + (Other is IGameObject b ? b.Name : "?");
		public CollisionData() { }
		public void Init(ICollidable self = null, ICollidable other = null, Vec2 point = default, Vec2 normal = default,
			float depth = float.MaxValue, int colliderIndexSelf = -1, int colliderIndexOther = -1, IList<Vec2> contacts = null) {
			Self = self;
			Other = other;
			Point = point;
			Normal = normal;
			Depth = depth;
			ColliderIndexSelf = colliderIndexSelf;
			ColliderIndexOther = colliderIndexOther;
			Contacts = contacts;
			PrefetchedCollisionRules = null;
			IsColliding = false;
			_source = Log.StackPosition(2);
		}
		public void Get<TypeA, TypeB>(out TypeA self, out TypeB other) {
			self = (TypeA)Self;
			other = (TypeB)Other;
		}
		public CollisionData(ICollidable self, ICollidable other, Vec2 point, Vec2 normal, float depth, IList<Vec2> contacts) {
			Self = self;
			Other = other;
			Point = point;
			Normal = normal;
			if (Vec2.IsNaN(normal) || normal.Length() > 1.1f || normal.Length() < 0.9f) {
				throw new Exception("bad normal");
			}
			Depth = depth;
			Contacts = contacts;
		}
		public void SetParticipants(ICollidable self, ICollidable other) { Self = self; Other = other; }
		public static CollisionData ForCircles(Circle a, Circle b) {
			if (Circle.TryGetCircleCollision(a, b, out Vec2 delta, out float depth)) {
				Vec2 normal = delta.Normal;
				Vec2 centerOfCollision = a.Center + normal * (a.Radius - depth / 2);
				IList<Vec2> contacts = Circle.FindCircleCircleIntersections(a, b);
				return CollisionData.Commission(null, null, centerOfCollision, -normal, depth, contacts:contacts);
			}
			return null;
		}
		public override int GetHashCode() {
			int hash = 0;
			if (Self != null) { hash ^= Self.GetHashCode(); }
			if (Other != null) { hash ^= Other.GetHashCode(); }
			return hash;
		}
		public override bool Equals(object obj) => obj is CollisionData cd && Equals(cd);
		public bool Equals(CollisionData other) => Self == other.Self && Other == other.Other && Point == other.Point;
		public void CalculateCollisionResults(List<CollisionLogic.ToResolve> out_collisionResolutions, CollisionRules rules) {
			if (PrefetchedCollisionRules == null && !rules.TryGet((Self.TypeId, Other.TypeId), out PrefetchedCollisionRules)) {
				throw new InvalidOperationException($"no rules for collision of types {Self.TypeId} and {Other.TypeId}");
			}
			for (int i = 0; i < PrefetchedCollisionRules.Count; i++) {
				CollisionRules.Function f = PrefetchedCollisionRules[i];
				Action collisionResult = f.Invoke(this);
				if (collisionResult != null) {
					out_collisionResolutions.Add((this, collisionResult));
				}
			}
		}

		static CollisionData() {
			collisionPool.Setup(CreateCollision, CommissionCollision);
		}
		private static CollisionData CreateCollision() => new CollisionData();
		private static void CommissionCollision(CollisionData data) => data.Init();
		public static int GetCollisionCount() {
			int count = collisionPool.Count;
			return count;
		}
		public static int ClearCollisions() {
			int count = collisionPool.Count;
			collisionPool.Clear();
			return count;
		}
		public static CollisionData Commission(ICollidable self = null, ICollidable other = null, Vec2 point = default, Vec2 normal = default,
			float depth = float.MaxValue, int colliderIndexSelf = -1, int colliderIndexOther = -1, IList<Vec2> contacts = null) {
			CollisionData data = collisionPool.Commission();
			data.Init(self, other, point, normal, depth, colliderIndexSelf, colliderIndexOther, contacts);
			data._source = Log.StackPosition(2);
			return data;
		}
		public static void Decommission(CollisionData data) {
			collisionPool.Decommission(data);
		}

		internal void Release() {
			Decommission(this);
		}
	}

	public static class CollisionLogic {
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
		public static void CalculateCollisions<T>(IList<T> collidables, CollisionRules rules,
		CollisionsPerAgent out_collisionData) where T : ICollidable {
			for (int objectAIndex = 0; objectAIndex < collidables.Count; objectAIndex++) {
				ICollidable objectA = collidables[objectAIndex];
				for (int objectBIndex = objectAIndex + 1; objectBIndex < collidables.Count; objectBIndex++) {
					ICollidable objectB = collidables[objectBIndex];
					CollisionData a = DetermineCollisionLogicForPair(objectA, objectB, rules);
					if (a != null) { out_collisionData.AddCollision(a); }
					CollisionData b = DetermineCollisionLogicForPair(objectB, objectA, rules);
					if (b != null) { out_collisionData.AddCollision(b); }
				}
			}
		}
		public static CollisionData DetermineCollisionLogicForPair(ICollidable a, ICollidable b, CollisionRules rules) {
			if (!rules.TryGet((a.TypeId, b.TypeId), out IList<CollisionRules.Function> collisionFunctions)) { return null; }
			CollisionData collision = a.IsColliding(b);
			if (collision == null) { return null; }
			collision.SetParticipants(a, b);
			collision.PrefetchedCollisionRules = collisionFunctions;
			return collision;
		}
		public static void CalculateCollisionResolution(IEnumerable<CollisionData> collisionData, List<ToResolve> out_collisionResolutions,
		CollisionRules rules) {
			foreach(CollisionData collision in collisionData) {
				collision.CalculateCollisionResults(out_collisionResolutions, rules);
			}
		}

		public static int Update(List<ICollidable> collideList, SpacePartition<ICollidable> spacePartition, CollisionRules collisionRules, CollisionsPerAgent collisionResults) {
			//spacePartition.DoCollisionLogicAndResolve(collideList, collisionRules);
			spacePartition.Populate(collideList);
			spacePartition.CalculateCollisionsAndResolve(collisionRules, collisionResults);
			//CollisionData.collisionPool.RemoveCommisioned();
			//CollisionLogic.DoCollisionLogicAndResolve(collideList, collisionRules);
			int unprocessedCollisions = CollisionData.ClearCollisions();
			return unprocessedCollisions;
		}
	}
}

using System;
using System.Collections.Generic;

namespace asteroids {
	public interface ICollidable {
		public bool IsColliding(ICollidable collidable);
		public byte TypeId { get; set; }
	}
	public struct CollisionPair {
		public Type a, b;
		public CollisionPair(ICollidable a, ICollidable b) : this(a.GetType(), b.GetType()) {}
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>collision resolution function, null if no collision happened or collision was trivial</returns>
		public delegate Action Function(ICollidable a, ICollidable b);
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
			if (!a.IsColliding(b)) {
				return;
			}
			for (int i = 0; i < collisionFunctions.Count; i++) {
				Function f = collisionFunctions[i];
				Action collisionResult = f.Invoke(a, b);
				if (collisionResult != null) {
					collisionResolutions.Add(collisionResult);
				}
			}
		}
	}
}

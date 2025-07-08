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
		public delegate void Function(ICollidable a, ICollidable b);
		public static void DoCollisionLogic(IList<ICollidable> collidables,
			Dictionary<CollisionPair, List<Function>> rules) {
			for (int i = 0; i < collidables.Count; i++) {
				ICollidable ci = collidables[i];
				for (int j = i + 1; j < collidables.Count; j++) {
					ICollidable cj = collidables[j];
					DoCollisionLogicOnPair(ci, cj, rules);
					DoCollisionLogicOnPair(cj, ci, rules);
				}
			}
		}
		private static void DoCollisionLogicOnPair(ICollidable a, ICollidable b,
			Dictionary<CollisionPair, List<Function>> rules) {
			if (!rules.TryGetValue(new CollisionPair(a, b), out List<Function> collisionFunctions)) {
				return;
			}
			if (a.IsColliding(b)) {
				collisionFunctions.ForEach(f => f.Invoke(a, b));
			}
		}
	}
}

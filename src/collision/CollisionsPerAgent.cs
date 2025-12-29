using System.Collections;
using System.Collections.Generic;

namespace collision {
	public class CollisionsPerAgent : IEnumerable<CollisionData> {
		// it's rare for an object to collide with more than one thing. so, using dictionary of Lists
		// because for less than 10, linear List checks are faster (and less memory) than Dictionary.
		public Dictionary<ICollidable, List<CollisionData>> collisionsPerAgent =
			new Dictionary<ICollidable, List<CollisionData>>();
		private List<List<CollisionData>> collisionLists =
			new List<List<CollisionData>>();
		private ObjectPool<List<CollisionData>> collisionListPool =
			new ObjectPool<List<CollisionData>>();

		public CollisionsPerAgent() {
			collisionListPool.Setup(CreateCollisionList, CommissionCollisionList);
		}
		private static List<CollisionData> CreateCollisionList() => new List<CollisionData>();
		private static void CommissionCollisionList(List<CollisionData> list) { list.Clear(); }
		public void Clear() { collisionsPerAgent.Clear(); collisionLists.Clear(); collisionListPool.Clear(); }
		public void AddCollision(CollisionData data) {
			if (!collisionsPerAgent.TryGetValue(data.Self, out List<CollisionData> collisions)) {
				collisionsPerAgent[data.Self] = collisions = collisionListPool.Commission();
				collisionLists.Add(collisions);
			}
			bool alreadyHaveThisCollision = collisions.Find(known => known.Other == data.Other) != null;
			if (!alreadyHaveThisCollision) {
				collisions.Add(data);
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<CollisionData> GetEnumerator() {
			for(int subList = 0; subList < collisionLists.Count; ++subList) {
				List<CollisionData> list = collisionLists[subList];
				for (int i = 0; i < list.Count; ++i) {
					yield return list[i];
				}
			}
		}
	}
}

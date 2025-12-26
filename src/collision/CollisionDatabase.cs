using asteroids;
using MathMrV;
using System;
using System.Collections;
using System.Collections.Generic;

namespace collision {
	public class CollisionDatabase : IList<CollisionData> {
		public Dictionary<ICollidable, List<CollisionData>> collisionsPerAgent =
			new Dictionary<ICollidable, List<CollisionData>>();
		private List<List<CollisionData>> collisionLists =
			new List<List<CollisionData>>();
		private ObjectPool<List<CollisionData>> collisionListPool =
			new ObjectPool<List<CollisionData>>();
		public int Count {
			get {
				int count = 0;
				for (int i = 0; i < collisionLists.Count; ++i) {
					count += collisionLists[i].Count;
				}
				return count;
			}
		}
		public bool IsReadOnly => false;
		public CollisionData this[int index] {
			get {
				GetTrueIndex(index, out int subList, out int subIndex);
				return collisionLists[subList][subIndex];
			}
			set => throw new NotImplementedException();
		}
		public CollisionDatabase() {
			collisionListPool.Setup(CreateCollisionList, CommissionCollisionList,
				DecommissionCollisionList, DestroyCollisionList);
		}
		private static List<CollisionData> CreateCollisionList() => new List<CollisionData>();
		private static void CommissionCollisionList(List<CollisionData> list) { ClearCollList(list); }
		private static void DecommissionCollisionList(List<CollisionData> list) { ClearCollList(list); }
		private static void DestroyCollisionList(List<CollisionData> list) { }
		private static void ClearCollList(List<CollisionData> list) {
			for(int i = list.Count -1; i > 0; --i) {
				CollisionData.Decommission(list[i]);
				list.RemoveAt(i);
			}
			list.Clear();
		}
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
		public List<CollisionData> GetCollisions(ICollidable obj) {
			if (!collisionsPerAgent.TryGetValue(obj, out List<CollisionData> collisions)) {
				return null;
			}
			return collisions;
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		private void GetTrueIndex(int iListIndex, out int subList, out int index) {
			if (!TryGetTrueIndex(iListIndex, out subList, out index)) {
				throw new ArgumentOutOfRangeException();
			}
		}
		private bool TryGetTrueIndex(int iListIndex, out int subList, out int index) {
			subList = 0;
			index = iListIndex;
			while (subList < collisionLists.Count && index - collisionLists[subList].Count > 0) {
				index -= collisionLists[subList].Count;
				++subList;
			}
			return index >= 0 && subList < collisionLists.Count;
		}
		private bool TryGetTrueIndexOf(CollisionData item, out int iListIndex, out int subList, out int index) {
			for (subList = 0; subList < collisionLists.Count; ++subList) {
				for (index = 0; index < collisionLists[subList].Count; ++index) {
					if (collisionLists[subList][index] == item) {
						iListIndex = index + index;
						return true;
					}
				}
				index += collisionLists[subList].Count;
			}
			iListIndex = subList = index = -1;
			return false;
		}
		public int IndexOf(CollisionData item) {
			TryGetTrueIndexOf(item, out int iListIndex, out int subList, out int index);
			return iListIndex;
		}

		public void Insert(int index, CollisionData item) {
			throw new NotImplementedException();
			// should not allow collision data to be inserted into lists directly like this.
			// collisions must be organized by element.self
			//GetTrueIndex(index, out int subList, out int subIndex);
			//collisionLists[subList].Add(item);
		}

		public void RemoveAt(int index) {
			GetTrueIndex(index, out int subList, out int subIndex);
			collisionLists[subList].RemoveAt(subIndex);
		}

		public void Add(CollisionData item) => AddCollision(item);

		public bool Contains(CollisionData item) => IndexOf(item) >= 0;

		public void CopyTo(CollisionData[] array, int arrayIndex) {
			IEnumerator<CollisionData> iter = GetEnumerator();
			for (int i = arrayIndex; iter.MoveNext() && i < array.Length; ++i) {
				array[i] = iter.Current;
			}
		}

		public bool Remove(CollisionData item) {
			if (!TryGetTrueIndexOf(item, out int iListIndex, out int subList, out int index)) {
				return false;
			}
			collisionLists[subList].Remove(item);
			return true;
		}
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

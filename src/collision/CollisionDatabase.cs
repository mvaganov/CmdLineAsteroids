using System;
using System.Collections;
using System.Collections.Generic;

namespace collision {
	public class CollisionDatabase : IList<CollisionData> {
		public Dictionary<ICollidable, List<CollisionData>> database = new Dictionary<ICollidable, List<CollisionData>>();
		List<List<CollisionData>> collisionLists = new List<List<CollisionData>>();
		ObjectPool<List<CollisionData>> collisionListPool = new ObjectPool<List<CollisionData>>();
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
			collisionListPool.Setup(CreateCollisionList, CommisionCollisionList, DecommisionCollisionList, DestroyCollisionList);
		}
		private static List<CollisionData> CreateCollisionList() => new List<CollisionData>();
		private static void CommisionCollisionList(List<CollisionData> list) { list.Clear(); }
		private static void DecommisionCollisionList(List<CollisionData> list) { list.Clear(); }
		private static void DestroyCollisionList(List<CollisionData> list) { }
		public void Clear() { database.Clear(); collisionLists.Clear(); collisionListPool.Clear(); }
		public void AddCollision(CollisionData data) {
			if (!database.TryGetValue(data.Self, out List<CollisionData> collisions)) {
				database[data.Self] = collisions = collisionListPool.Commission();
				collisionLists.Add(collisions);
			}
			for (int i = 0; i < collisions.Count; ++i) {
				if (collisions[i].Other == data.Other) {
#if !RELEASE
					//if (collisions[i].Equals(data)) {
					//	Log.d($"duplicate collision discovered: {data.Name}");
					//} else {
					//	Log.w($"multiple different collisions: {data.Name}");
					//}
#endif
					return;
				}
			}
			collisions.Add(data);
		}
		public List<CollisionData> GetCollisions(ICollidable obj) {
			if (!database.TryGetValue(obj, out List<CollisionData> collisions)) {
				return null;
			}
			return collisions;
		}
		public IEnumerator<CollisionData> GetEnumerator() => new Enumerator(this);
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

		public class Enumerator : IEnumerator<CollisionData> {
			CollisionDatabase database;
			private int subList, index;
			public Enumerator(CollisionDatabase database) { this.database = database; Reset(); }
			public void Reset() { subList = 0; index = -1; }
			public void Dispose() { }
			public CollisionData Current => database.collisionLists[subList][index]; object IEnumerator.Current => Current;
			public bool MoveNext() {
				if (subList >= database.collisionLists.Count) { return false; }
				++index;
				List<CollisionData> collisions = database.collisionLists[subList];
				if (index >= collisions.Count) { ++subList; index = -1; return MoveNext(); }
				return true;
			}
		}
	}

}

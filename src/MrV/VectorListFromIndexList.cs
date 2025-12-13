using MathMrV;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MrV {
	public class VectorListFromIndexList : IList<Vec2> {
		public IList<Vec2> VertexList;
		public IList<int> IndexList;
		public VectorListFromIndexList(IList<Vec2> vertList, IList<int> iList) {
			VertexList = vertList; IndexList = iList;
		}
		public Vec2 this[int index] { get => Get(index); set => Set(index, value); }
		public Vec2 Get(int index) => VertexList[IndexList[index]];
		public void Set(int index, Vec2 value) { VertexList[IndexList[index]] = value; }
		public int Count => IndexList.Count;
		public bool IsReadOnly => true;
		public void Add(Vec2 item) => throw new NotImplementedException();
		public void Clear() => throw new NotImplementedException();
		public bool Contains(Vec2 item) => IndexOf(item) >= 0;
		public void CopyTo(Vec2[] array, int arrayIndex) {
			int limit = Math.Min(Count, array.Length - arrayIndex);
			for (int i = 0; i < limit; i++) { array[arrayIndex++] = this[i]; }
		}
		public int IndexOf(Vec2 item) {
			for (int i = 0; i < Count; ++i) {
				if (this[i] == item) { return i; }
			}
			return -1;
		}
		public void Insert(int index, Vec2 item) => throw new NotImplementedException();
		public bool Remove(Vec2 item) => throw new NotImplementedException();
		public void RemoveAt(int index) => throw new NotImplementedException();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<Vec2> GetEnumerator() {
			// C# compiler magic generates the proper IEnumerator<T> implementation from this code
			for (int i = 0; i < Count; i++) { yield return this[i]; }
		}
	}
}

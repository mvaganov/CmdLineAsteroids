using MathMrV;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MrV {
	public class VectorListFromIndexList : IList<Vec2> {
		public IList<Vec2> VertexList;
		public IList<int> IndexList;
		//public Vec2 Offset, Rotation;
		public VectorListFromIndexList(IList<Vec2> vertList, IList<int> iList,
		Vec2 offset = default, Vec2 rotation = default) {
			VertexList = vertList; IndexList = iList;
			//Offset = offset; Rotation = rotation;
		}
		public Vec2 this[int index] { get => GetWorld(index); set => SetWorld(index, value); }
		public Vec2 GetLocal(int index) => VertexList[IndexList[index]];
		public void SetLocal(int index, Vec2 value) { VertexList[IndexList[index]] = value; }
		public Vec2 GetWorld(int index) {
			Vec2 value = GetLocal(index);
			//if (!Rotation.IsZero()) { value.Rotate(Rotation); }
			//if (!Offset.IsZero()) { value += Offset; }
			return value;
		}
		public void SetWorld(int index, Vec2 value) {
			//if (!Offset.IsZero()) { value -= Offset; }
			//if (!Rotation.IsZero()) {
			//	Vec2 unRotation = Rotation;
			//	unRotation.y *= -1;
			//	value.Rotate(unRotation);
			//}
			SetLocal(index, value);
		}
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
			// C# compiler magic generates the IEnumerator<T> implementation from this code
			for (int i = 0; i < Count; i++) { yield return this[i]; }
		}
	}
}

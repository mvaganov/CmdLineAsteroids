using ConsoleMrV;
using MathMrV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace asteroids {
	public class SpacePartition<T> {
		public delegate void DrawFunction(CommandLineCanvas canvas, SpacePartition<T> spacePartition, T obj);
		protected AABB aabb;
		protected Vec2 columnsRows;
		protected int depth;
		/// <summary>
		/// null for root
		/// </summary>
		protected SpacePartition<T> parent;
		/// <summary>
		/// null for branche nodes
		/// </summary>
		protected List<T> list;
		/// <summary>
		/// null for leaf nodes
		/// </summary>
		protected SpacePartition<T>[,] subpartition;
		public AABB AABB => aabb;
		public Vec2 Position => aabb.getCenter();
		public SpacePartition(Vec2 min, Vec2 max, int treeDepth, Vec2 columnsRows, SpacePartition<T> parent = null) {
			aabb = new AABB(min, max);
			this.columnsRows = columnsRows;
			depth = 0;
			this.parent = parent;
			while (parent != null) {
				++depth;
				parent = parent.parent;
			}
			//if (depth > 3) {
			//	return;
			//}
			if (treeDepth >= 0) {
				createSubPartition(treeDepth);
			}
		}

		private void createSubPartition(int treeDepth) {
			int columns = (int)columnsRows.x;
			int rows = (int)columnsRows.y;
			Vec2 size = aabb.GetSize();
			Vec2 cellSize = new Vec2(size.x / columns, size.y / rows);
			subpartition = new SpacePartition<T>[rows,columns];
			Vec2 cursor = aabb.min;
			for (int r = 0; r < rows; ++r) {
				cursor.x = aabb.min.x;
				for (int c = 0; c < columns; ++c) {
					SpacePartition<T> nextCell = new SpacePartition<T>(cursor, cursor+cellSize, treeDepth-1, columnsRows, this);
					subpartition[r,c] = nextCell;
					cursor.x += cellSize.x;
				}
				cursor.y += cellSize.y;
			}
		}

		private static ConsoleColor[] collisionSpaceColors = new ConsoleColor[] {
			ConsoleColor.DarkGray,
			ConsoleColor.DarkBlue,
			ConsoleColor.DarkCyan,
			ConsoleColor.DarkGreen,
			ConsoleColor.DarkYellow,
			ConsoleColor.DarkRed,
			ConsoleColor.DarkMagenta
		};
		private static int consoleCollisionSpaceColorIndex = -1;
		void NextConsoleCollisionColor(CommandLineCanvas ctx) {
			if (++consoleCollisionSpaceColorIndex >= collisionSpaceColors.Length) {
				consoleCollisionSpaceColorIndex = 0;
			}
			ctx.SetColor(collisionSpaceColors[consoleCollisionSpaceColorIndex]);
		}
		public void draw(CommandLineCanvas ctx, DrawFunction drawElementFunction) {
			if (parent == null) { consoleCollisionSpaceColorIndex = -1; }
			if (subpartition == null) { NextConsoleCollisionColor(ctx); }
			if (drawElementFunction == null) {
				aabb.draw(ctx);
			}
			if (subpartition != null) {
				for (int r = 0; r < subpartition.GetLength(0); ++r) {
					for (int c = 0; c < subpartition.GetLength(1); ++c) {
						subpartition[r,c].draw(ctx, drawElementFunction);
					}
				}
			}
			//Vec2 center = aabb.getCenter();
			//ctx.fillText(""+this.list.length, center.x, center.y);
			if (drawElementFunction != null && list != null) {
				for (int i = 0; i < list.Count; ++i) {
					T element = list[i];
					drawElementFunction.Invoke(ctx, this, element);
				}
			}
		}

		public bool CircleGoesHere(Circle circle) {
			return circle.radius > 0
			? aabb.IntersectsCircle(circle.center, circle.radius)
			: aabb.contains(circle.center);
		}

		public List<SpacePartition<T>> GetPartitions(Circle circle) {
			if (subpartition == null) {
				return new List<SpacePartition<T>>() { this };
			}
			// calculate range of sub partitions, quit early if out of bounds
			int lastRow = subpartition.GetLength(0) - 1;
			int lastCol = subpartition.GetLength(1) - 1;
			circle.TryGetAABB(out Vec2 min, out Vec2 max);
			min -= aabb.min;
			max -= aabb.min;
			Vec2 cellSize = subpartition[0, 0].AABB.GetSize();
			int startRow = (int)(min.y / cellSize.y);
			if (startRow > lastRow) { return null; }
			int startCol = (int)(min.x / cellSize.x);
			if (startCol > lastCol) { return null; }
			int endRow = (int)(max.y / cellSize.y);
			if (endRow < 0) { return null; };
			int endCol = (int)(max.x / cellSize.x);
			if (endCol < 0) { return null; }
			// limit search to calculated range
			startRow = Math.Max(0, startRow);
			startCol = Math.Max(0, startCol);
			endRow = Math.Min(lastRow, endRow);
			endCol = Math.Min(lastCol, endCol);
			List<SpacePartition<T>> totalFoundPartitions = null;
			for (int r = startRow; r <= endRow; ++r) {
				for (int c = startCol; c <= endCol; ++c) {
					SpacePartition<T> subPartition = subpartition[r,c];
					if (!subPartition.CircleGoesHere(circle)) {
						continue;
					}
					List<SpacePartition<T>> foundPartitions = subPartition.GetPartitions(circle);
					if (foundPartitions != null) {
						if (totalFoundPartitions == null) {
							totalFoundPartitions = foundPartitions;
						} else {
							totalFoundPartitions.AddRange(foundPartitions);
						}
					}
				}
			}
			return totalFoundPartitions;
		}

		public void Clear() {
			if (list != null) {
				list.Clear();
			}
			if (subpartition != null) {
				for (int r = 0; r < subpartition.GetLength(0); ++r) {
					for (int c = 0; c < subpartition.GetLength(1); ++c) {
						subpartition[r,c].Clear();
					}
				}
			}
		}

		public void Populate(IList<T> elementList, Func<T, Circle> convertElementToCircleMethod) {
			Clear();
			for (int i = 0; i < elementList.Count; ++i) {
				T element = elementList[i];
				Circle circle = convertElementToCircleMethod(element);
				List<SpacePartition<T>> partitions = GetPartitions(circle);
				//console.log(element.name+" in "+partitions.length+" partitions: "+partitions[0].aabb);
				if (partitions == null) {
					continue;
				}
				for (int p = 0; p < partitions.Count; ++p) {
					SpacePartition<T> partition = partitions[p];
					if (partition.list == null) {
						partition.list = new List<T>();
					}
					partition.list.Add(element);
				}
			}
		}

		public class CollisionDatabase : IList<CollisionData> {
			public Dictionary<ICollidable, List<CollisionData>> database = new Dictionary<ICollidable, List<CollisionData>>();
			List<List<CollisionData>> collisionLists = new List<List<CollisionData>>();
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
			public void Clear() { database.Clear(); collisionLists.Clear(); }
			public void AddCollision(CollisionData data) {
				if (!database.TryGetValue(data.self, out List<CollisionData> collisions)) {
					database[data.self] = collisions = new List<CollisionData>();
					collisionLists.Add(collisions);
				}
				for(int i = 0; i < collisions.Count; ++i) {
					if (collisions[i].other == data.other) {
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
				if(!TryGetTrueIndexOf(item, out int iListIndex, out int subList, out int index)) {
					return false;
				}
				collisionLists[subList].Remove(item);
				return true;
			}

			public class Enumerator : IEnumerator<CollisionData> {
				CollisionDatabase database;
				private int subList, index;
				public Enumerator(CollisionDatabase database) { this.database = database; Reset(); }
				public void Reset() { subList = 0; index = -1; } public void Dispose() { }
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

		public void FindCollisions(Dictionary<CollisionPair, List<CollisionLogic.Function>> rules, 
			IList<CollisionData> collisionData) {
			if (list != null) {
				CollisionLogic.CalculateCollisions(list as IList<ICollidable>, rules, collisionData);
			}
			if (subpartition != null) {
				int rows = subpartition.GetLength(0);
				int cols = subpartition.GetLength(1);
				for (int r = 0; r < rows; ++r) {
					for (int c = 0; c < cols; ++c) {
						subpartition[r, c].FindCollisions(rules, collisionData);
					}
				}
			}
		}

		public void FindCollisions(Dictionary<CollisionPair, List<CollisionLogic.Function>> rules,
	CollisionDatabase collisionDatabase) {
			if (list != null) {
				CollisionLogic.CalculateCollisions(list as IList<ICollidable>, rules, collisionDatabase);
			}
			if (subpartition != null) {
				int rows = subpartition.GetLength(0);
				int cols = subpartition.GetLength(1);
				for (int r = 0; r < rows; ++r) {
					for (int c = 0; c < cols; ++c) {
						subpartition[r, c].FindCollisions(rules, collisionDatabase);
					}
				}
			}
		}

		public List<CollisionLogic.ToResolve> DoCollisionLogic(Dictionary<CollisionPair, List<CollisionLogic.Function>> rules) {
			IList<CollisionData> collisionData;
			if (false) {
				List<CollisionData> collisionDataList = new List<CollisionData>();
				FindCollisions(rules, collisionDataList);
				collisionData = collisionDataList;
				// TODO empty duplicates
			} else {
				CollisionDatabase collisionDatabase = new CollisionDatabase();
				FindCollisions(rules, collisionDatabase);
				collisionData = collisionDatabase;
			}
			List<CollisionLogic.ToResolve> collisionResolutions = new List<CollisionLogic.ToResolve>();
			CollisionLogic.CalculateCollisionResolution(collisionData, collisionResolutions);
			return collisionResolutions;
		}
		bool FindDuplicateIndex(List<CollisionLogic.ToResolve> collisionsToResolve, out int a, out int b) {
			for (a = 0; a < collisionsToResolve.Count; ++a) {
				CollisionLogic.ToResolve resolveA = collisionsToResolve[a];
				for (b = a+1; b < collisionsToResolve.Count; ++b) {
					CollisionLogic.ToResolve resolveB = collisionsToResolve[b];
					if (resolveA.Equals(resolveB)) {
						return true;
					}
				}
			}
			a = b = -1;
			return false;
		}
		public void DoCollisionLogicAndResolve(Dictionary<CollisionPair, List<CollisionLogic.Function>> rules) {
			List<CollisionLogic.ToResolve> collisionsToResolve = DoCollisionLogic(rules);
			if (collisionsToResolve == null) {
				return;
			}
			collisionsToResolve.ForEach(collision => collision.resolution.Invoke());
		}

		public List<CollisionData> GetCollisions(T collidable, Func<T,Circle> convertElementToCircleMethod,
		Func<T, T, bool> verifyCollisionMoreThanCircleOverlap) {
			Circle circle = convertElementToCircleMethod(collidable);
			List<SpacePartition<T>> partitions = GetPartitions(circle);
			List<CollisionData> collisions = null;
			for (int i = 0; i < partitions.Count; ++i) {
				SpacePartition<T> subPart = partitions[i];
				if (subPart.list == null) {
					continue;
				}

				for (int e = 0; e < subPart.list.Count; ++e) {
					T element = subPart.list[e];
					//if (e == null) { continue; }
					Circle otherCircle = convertElementToCircleMethod(element);
					float targetDistance = otherCircle.radius;
					if (circle.radius != 0) {
						targetDistance += circle.radius;
					}
					if (!circle.IsColliding(otherCircle)) {
						continue;
					}
					if (verifyCollisionMoreThanCircleOverlap == null
					|| verifyCollisionMoreThanCircleOverlap.Invoke(collidable, element)) {
						if (collisions == null) {
							collisions = new List<CollisionData>();
						}
						CollisionData collision = CollisionData.ForCircles(circle, otherCircle);
						collision.self = collidable as ICollidable;
						collision.other = element as ICollidable;
						collisions.Add(collision);
					}
				}
			}
			return collisions;
		}
	}
}

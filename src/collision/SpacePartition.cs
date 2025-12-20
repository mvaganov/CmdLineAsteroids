using ConsoleMrV;
using MathMrV;
using System;
using System.Collections.Generic;
using ColliderID = System.Byte;
using ColliderPair = System.Tuple<System.Byte, System.Byte>;

namespace collision {
	/// <summary>
	/// Breaks up the world into axis-aligned chunks. Works recursively like a quad tree or octree.
	/// Can break up space into column*row branches for a more shallow tree (less recursion needed).
	/// </summary>
	/// <typeparam name="T">base type of object being located by the space partition tree</typeparam>
	public class SpacePartition<T> {
		protected SpacePartitionCell<T> root;
		protected ObjectPools mem = new ObjectPools();
		public class ObjectPools {
			public ObjectPool<SpacePartitionCell<T>> cellPool = new ObjectPool<SpacePartitionCell<T>>();
			public ObjectPool<List<T>> elementListPool = new ObjectPool<List<T>>();
			public ObjectPool<SpacePartitionCell<T>[,]> branchPool = new ObjectPool<SpacePartitionCell<T>[,]>();
		}
		public SpacePartition(Vec2 min, Vec2 max, byte treeDepth, byte columns, byte rows, SpacePartitionCell<T>.ConvertElementToCircleDelegate convertElementToCircle) {
			mem.cellPool.Setup(CreateCell, CommissionCell, DecommissionCell, DestroyCell);
			mem.elementListPool.Setup(() => new List<T>(), l => l.Clear());
			mem.branchPool.Setup(() => new SpacePartitionCell<T>[rows, columns]);
			root = mem.cellPool.Commission();
			root.Set(min, max, treeDepth, columns, rows, convertElementToCircle);
		}
		private SpacePartitionCell<T> CreateCell() => new SpacePartitionCell<T>(Vec2.Zero, Vec2.Zero, 0, 0, 0, null, null);
		private void CommissionCell(SpacePartitionCell<T> cell) { }
		private void DecommissionCell(SpacePartitionCell<T> cell) { }
		private void DestroyCell(SpacePartitionCell<T> cell) { }
		public Vec2 Position => root.Position;
		public void Populate(IEnumerable<T> collideList) {
			root.Populate(collideList, mem);
			EnsureTopLevelRootIfCellsExpandOut();
		}
		public void CalculateCollisionsAndResolve(Dictionary<(ColliderID,ColliderID), List<CollisionLogic.Function>> collisionRules, CollisionDatabase collisionDatabase) {
			root.CalculateCollisionsAndResolve(collisionRules, collisionDatabase);
		}
		private void EnsureTopLevelRootIfCellsExpandOut() {
			while (root.Parent != null) {
				root = root.Parent;
			}
		}

		internal void Draw(CommandLineCanvas graphics, SpacePartitionCell<T>.DrawCellObjectsOnCanvasDelegate drawFunction) {
			root.Draw(graphics, drawFunction);
			graphics.WriteAt($"cells: {mem.cellPool.Count}", 0, 0);
		}
	}
	public class SpacePartitionCell<T> {
		/// <summary>How to generalize all obects of type <see cref="{T}"/> into circles</summary>
		public delegate Circle ConvertElementToCircleDelegate(T obj);
		/// <summary>Draw method, used for visualization and debugging</summary>
		public delegate void DrawCellObjectsOnCanvasDelegate(CommandLineCanvas canvas, SpacePartitionCell<T> spacePartition, T obj);
		/// <summary>Area of this <see cref="SpacePartitionCell{T}"/></summary>
		protected AABB aabb;
		/// <summary>How to split this <see cref="SpacePartitionCell{T}"/></summary>
		protected byte rows, columns;
		/// <summary>How many times this <see cref="SpacePartitionCell{T}"/> can split</summary>
		protected byte availableDepth;
		/// <summary>How many elements are required to trigger a split</summary>
		protected byte maxElementsPerCell = 8;
		/// <summary>Function used to convert an element to a circle</summary>
		protected ConvertElementToCircleDelegate convertElementToCircle;
		/// <summary>null for root</summary>
		protected SpacePartitionCell<T> parent;
		/// <summary>All elements in this partition cell leaf. `null` for branch nodes</summary>
		protected List<T> elements;
		/// <summary>[row,column] separation for branch nodes. `null` for leaf nodes.</summary>
		protected SpacePartitionCell<T>[,] subPartition;
		public AABB AABB => aabb;
		public Vec2 Position => aabb.Center;
		public SpacePartitionCell<T> Parent => parent;
		public SpacePartitionCell(Vec2 min, Vec2 max, int treeDepth, int columns, int rows,
		ConvertElementToCircleDelegate convertElementToCircle, SpacePartitionCell<T> parent = null) {
			Set(min, max, treeDepth, columns, rows, convertElementToCircle);
		}
		public void Set(Vec2 min, Vec2 max, int treeDepth, int columns, int rows,
		ConvertElementToCircleDelegate convertElementToCircle, SpacePartitionCell<T> parent = null) {
			aabb = new AABB(min, max);
			this.availableDepth = (byte)treeDepth;
			this.columns = (byte)columns;
			this.rows = (byte)rows;
			this.convertElementToCircle = convertElementToCircle;
			this.parent = parent;
		}

		public bool Insert(IEnumerable<T> elements, SpacePartition<T>.ObjectPools mem) {
			bool inserted = false;
			foreach (T element in elements) {
				inserted |= Insert(element, mem);
			}
			return inserted;
		}
		public bool Insert(T element, SpacePartition<T>.ObjectPools mem) {
			Circle circle = convertElementToCircle(element);
			return Insert(circle, element, mem);
		}

		private bool Insert(Circle circle, T element, SpacePartition<T>.ObjectPools mem) {
			if (!CircleGoesHere(circle)) {
				if (parent == null) {
					InsertSelfIntoNewParent(circle, element, mem);
					return true;
				}
				return false;
			}
			if (subPartition != null) {
				return InsertIntoSubPartitions(circle, element, mem);
			}
			if (elements == null) {
				elements = mem.elementListPool.Commission();
			}
			elements.Add(element);
			if (availableDepth > 0 && elements.Count > maxElementsPerCell) {
				SplitSelf(mem);
			}
			return true;
		}
		private void SplitSelf(SpacePartition<T>.ObjectPools mem,
		Func<int, int, SpacePartitionCell<T>> optionallyGetExistingCellAtColumnRow = null) {
			if (availableDepth == 0) {
				throw new Exception("cannot split, already at zero available depth");
			}
			CreateSubSpacePartitions();
			TransferElementsToSubSpacePartitions();
			void CreateSubSpacePartitions() {
				Vec2 size = aabb.Size;
				Vec2 subCellSize = new Vec2(size.X / columns, size.Y / rows);
				if (subPartition == null) {
					subPartition = mem.branchPool.Commission();
				}
				Vec2 cursor = aabb.Min;
				for (int r = 0; r < rows; ++r) {
					cursor.X = aabb.Min.X;
					for (int c = 0; c < columns; ++c) {
						SpacePartitionCell<T> nextCell = null;
						if (optionallyGetExistingCellAtColumnRow != null) {
							nextCell = optionallyGetExistingCellAtColumnRow(c, r);
						}
						if (nextCell == null) {
							nextCell = mem.cellPool.Commission(); nextCell.Set(
								cursor, cursor + subCellSize,
								(byte)(availableDepth - 1), columns, rows, convertElementToCircle, this);
						}
						subPartition[r, c] = nextCell;
						cursor.X += subCellSize.X;
					}
					cursor.Y += subCellSize.Y;
				}
			}
			void TransferElementsToSubSpacePartitions() {
				if (elements == null) {
					return;
				}
				for (int i = 0; i < elements.Count; ++i) {
					T element = elements[i];
					Circle circle = convertElementToCircle(element);
					InsertIntoSubPartitions(circle, element, mem);
				}
				elements.Clear();
				mem.elementListPool.Decommission(elements);
				elements = null;
			}
		}
		private void InsertSelfIntoNewParent(Circle circle, T element, SpacePartition<T>.ObjectPools mem) {
			Vec2 parentCellSize = aabb.Size;
			Vec2 parentFullSize = parentCellSize.Scaled(new Vec2(columns, rows));
			Vec2 center = (circle.Center + Position) / 2;
			AABB parentEstimate = AABB.CreateAt(center, parentFullSize);
			Vec2 positionInParent = Position - parentEstimate.Min;
			int col = (int)(positionInParent.X / parentCellSize.X);
			int row = (int)(positionInParent.Y / parentCellSize.Y);
			if (col < 0) { col = 0; }
			if (col >= columns) { col = columns - 1; }
			if (row < 0) { row = 0; }
			if (row >= rows) { row = rows - 1; }
			Vec2 parentMin = aabb.Min;
			parentMin.X -= parentCellSize.X * col;
			parentMin.Y -= parentCellSize.Y * row;
			Vec2 parentMax = parentMin + parentFullSize;
			SpacePartitionCell<T> newParent = mem.cellPool.Commission();
			newParent.Set(parentMin, parentMax, (byte)(availableDepth + 1), columns, rows, convertElementToCircle);
			parent = newParent;
			parent.SplitSelf(mem, (c, r) => (c == col && r == row) ? this : null);
			parent.Insert(circle, element, mem);
		}
		private bool InsertIntoSubPartitions(Circle circle, T element, SpacePartition<T>.ObjectPools mem) {
			bool inserted = false;
			for (int r = 0; r < rows; ++r) {
				for (int c = 0; c < columns; ++c) {
					inserted |= subPartition[r, c].Insert(circle, element, mem);
				}
			}
			return inserted;
		}

		private static ConsoleColor[] collisionSpaceColors = new ConsoleColor[] {
			ConsoleColor.DarkGray,
			ConsoleColor.DarkBlue,
			ConsoleColor.DarkCyan,
			ConsoleColor.DarkGreen,
		};
		private static int consoleCollisionSpaceColorIndex = -1;
		private void NextConsoleCollisionColor(CommandLineCanvas ctx) {
			if (++consoleCollisionSpaceColorIndex >= collisionSpaceColors.Length) {
				consoleCollisionSpaceColorIndex = 0;
			}
			ctx.SetColor(collisionSpaceColors[consoleCollisionSpaceColorIndex]);
		}
		public void Draw(CommandLineCanvas ctx, DrawCellObjectsOnCanvasDelegate drawElementFunction) {
			if (parent == null) { consoleCollisionSpaceColorIndex = -1; }
			if (subPartition == null) { NextConsoleCollisionColor(ctx); }
			if (drawElementFunction == null) {
				ctx.FillRect(aabb);
			}
			if (subPartition != null) {
				for (int r = 0; r < subPartition.GetLength(0); ++r) {
					for (int c = 0; c < subPartition.GetLength(1); ++c) {
						subPartition[r,c].Draw(ctx, drawElementFunction);
					}
				}
			}
			if (drawElementFunction != null && elements != null) {
				for (int i = 0; i < elements.Count; ++i) {
					T element = elements[i];
					drawElementFunction.Invoke(ctx, this, element);
				}
			}
		}

		public bool CircleGoesHere(Circle circle) {
			return circle.Radius > 0 ? circle.IntersectsAABB(aabb) : aabb.Contains(circle.Center);
		}
		public List<SpacePartitionCell<T>> GetLeafPartitions(Circle circle) {
			circle.TryGetAABB(out Vec2 min, out Vec2 max);
			return GetLeafCells(min, max, space => space.CircleGoesHere(circle));
		}

		public List<SpacePartitionCell<T>> GetLeafCells(Vec2 min, Vec2 max, Func<SpacePartitionCell<T>,bool> isValidCell) {
			if (subPartition == null) {
				List<SpacePartitionCell<T>> list = new List<SpacePartitionCell<T>>() { this };
				return list;
			}
			// calculate range of sub partitions, quit early if clearly out of bounds
			int lastRow = subPartition.GetLength(0) - 1;
			int lastCol = subPartition.GetLength(1) - 1;
			min -= aabb.Min;
			max -= aabb.Min;
			Vec2 cellSize = subPartition[0, 0].AABB.Size;
			int startRow = (int)(min.Y / cellSize.Y);
			if (startRow > lastRow) { return null; }
			int startCol = (int)(min.X / cellSize.X);
			if (startCol > lastCol) { return null; }
			int endRow = (int)(max.Y / cellSize.Y);
			if (endRow < 0) { return null; };
			int endCol = (int)(max.X / cellSize.X);
			if (endCol < 0) { return null; }
			// limit search to calculated range
			startRow = Math.Max(0, startRow);
			startCol = Math.Max(0, startCol);
			endRow = Math.Min(lastRow, endRow);
			endCol = Math.Min(lastCol, endCol);
			List<SpacePartitionCell<T>> totalFoundPartitions = null;
			for (int r = startRow; r <= endRow; ++r) {
				for (int c = startCol; c <= endCol; ++c) {
					SpacePartitionCell<T> subPartition = this.subPartition[r,c];
					if (isValidCell != null && !isValidCell(subPartition)) {
						continue;
					}
					List<SpacePartitionCell<T>> foundPartitions = subPartition.GetLeafCells(min, max, isValidCell);
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

		public void Clear(SpacePartition<T>.ObjectPools mem) {
			if (elements != null) {
				elements.Clear();
				mem.elementListPool.Decommission(elements);
				elements = null;
			}
			if (subPartition != null) {
				for (int r = 0; r < subPartition.GetLength(0); ++r) {
					for (int c = 0; c < subPartition.GetLength(1); ++c) {
						subPartition[r,c].Clear(mem);
						mem.cellPool.Decommission(subPartition[r, c]);
					}
				}
				mem.branchPool.Decommission(subPartition);
				subPartition = null;
			}
		}

		public void Populate(IEnumerable<T> elementList, SpacePartition<T>.ObjectPools mem) {
			Clear(mem);
			Insert(elementList, mem);
		}

		public void FindCollisions(Dictionary<(ColliderID,ColliderID), List<CollisionLogic.Function>> rules, 
			IList<CollisionData> collisionData) {
			if (elements != null) {
				CollisionLogic.CalculateCollisions(elements as IList<ICollidable>, rules, collisionData);
			}
			if (subPartition != null) {
				int rows = subPartition.GetLength(0);
				int cols = subPartition.GetLength(1);
				for (int r = 0; r < rows; ++r) {
					for (int c = 0; c < cols; ++c) {
						subPartition[r, c].FindCollisions(rules, collisionData);
					}
				}
			}
		}

		public void FindCollisions(Dictionary<(ColliderID,ColliderID), List<CollisionLogic.Function>> rules,
	CollisionDatabase collisionDatabase) {
			if (elements != null) {
				CollisionLogic.CalculateCollisions(elements as IList<ICollidable>, rules, collisionDatabase);
			}
			if (subPartition != null) {
				int rows = subPartition.GetLength(0);
				int cols = subPartition.GetLength(1);
				for (int r = 0; r < rows; ++r) {
					for (int c = 0; c < cols; ++c) {
						subPartition[r, c].FindCollisions(rules, collisionDatabase);
					}
				}
			}
		}

		public List<CollisionLogic.ToResolve> CalculateCollisionResolutions(Dictionary<(ColliderID,ColliderID),
		List<CollisionLogic.Function>> rules, CollisionDatabase collisionDatabase) {
			IList<CollisionData> collisionData;
			if (false) {
				List<CollisionData> collisionDataList = new List<CollisionData>();
				FindCollisions(rules, collisionDataList);
				collisionData = collisionDataList;
				// TODO empty duplicates
			} else {
				if (collisionDatabase == null) {
					collisionDatabase = new CollisionDatabase();
				} else {
					collisionDatabase.Clear();
				}
				FindCollisions(rules, collisionDatabase);
				collisionData = collisionDatabase;
			}
			List<CollisionLogic.ToResolve> collisionResolutions = new List<CollisionLogic.ToResolve>();
			CollisionLogic.CalculateCollisionResolution(collisionData, collisionResolutions);
			return collisionResolutions;
		}
		public void CalculateCollisionsAndResolve(Dictionary<(ColliderID,ColliderID), List<CollisionLogic.Function>> rules, CollisionDatabase collisionDatabase) {
			List<CollisionLogic.ToResolve> collisionsToResolve = CalculateCollisionResolutions(rules, collisionDatabase);
			if (collisionsToResolve == null || collisionsToResolve.Count == 0) {
				return;
			}
			collisionsToResolve.ForEach(collision => collision.resolution.Invoke());
		}

		public List<CollisionData> GetCollisions(T collidable, Func<T,Circle> convertElementToCircleMethod,
		Func<T, T, bool> verifyCollisionMoreThanCircleOverlap) {
			Circle circle = convertElementToCircleMethod(collidable);
			List<SpacePartitionCell<T>> partitions = GetLeafPartitions(circle);
			List<CollisionData> collisions = null;
			for (int i = 0; i < partitions.Count; ++i) {
				SpacePartitionCell<T> subPart = partitions[i];
				if (subPart.elements == null) {
					continue;
				}

				for (int e = 0; e < subPart.elements.Count; ++e) {
					T element = subPart.elements[e];
					Circle otherCircle = convertElementToCircleMethod(element);
					float targetDistance = otherCircle.Radius;
					if (circle.Radius != 0) {
						targetDistance += circle.Radius;
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
						collision.Self = collidable as ICollidable;
						collision.Other = element as ICollidable;
						collisions.Add(collision);
					}
				}
			}
			return collisions;
		}
	}
}

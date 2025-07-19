using ConsoleMrV;
using MathMrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class SpacePartition<T> {
		public delegate void DrawFunction(CommandLineCanvas canvas, SpacePartition<T> spacePartition, T obj);
		protected AABB aabb;
		protected List<T> list = new List<T>();
		protected Vec2 columnsRows;
		protected int depth;
		protected SpacePartition<T> parent;
		protected List<SpacePartition<T>> subpartition;
		SpacePartition(Vec2 min, Vec2 max, float minArea, Vec2 columnsRows, SpacePartition<T> parent = null) {
			aabb = new AABB(min, max);
			this.columnsRows = columnsRows;
			depth = 0;
			list = new List<T>();
			this.parent = parent;
			while (parent != null) {
				++depth;
				parent = parent.parent;
			}
			if (depth > 3) {
				return;
			}
			createSubPartition(minArea);
		}

		private void createSubPartition(float minArea) {
			int columns = (int)columnsRows.x;
			int rows = (int)columnsRows.y;
			Vec2 size = aabb.getSize();
			Vec2 cellSize = new Vec2(size.x / columns, size.y / rows);
			float cellArea = cellSize.x * cellSize.y;
			if (cellArea < minArea) {
				return;
			}
			subpartition = new List<SpacePartition<T>>();
			Vec2 cursor = aabb.min;
			for (int r = 0; r < rows; ++r) {
				cursor.x = aabb.min.x;
				for (int c = 0; c < columns; ++c) {
					SpacePartition<T> nextCell = new SpacePartition<T>(cursor, cursor+cellSize, minArea, columnsRows, this);
					subpartition.Add(nextCell);
					cursor.x += cellSize.x;
				}
				cursor.y += cellSize.y;
			}
		}

		public void draw(CommandLineCanvas ctx, DrawFunction drawElementFunction) {
			aabb.draw(ctx);
			if (subpartition != null) {
				for (int i = 0; i < subpartition.Count; i++) {
					subpartition[i].draw(ctx, drawElementFunction);
				}
			}
			Vec2 center = aabb.getCenter();
			//ctx.fillText(""+this.list.length, center.x, center.y);
			if (drawElementFunction != null) {
				for (int i = 0; i < list.Count; ++i) {
					T element = list[i];
					drawElementFunction.Invoke(ctx, this, element);
				}
			}
		}

		public List<SpacePartition<T>> getPartitions(Circle circle) {
			if (subpartition == null) {
				return new List<SpacePartition<T>>() { this };
			}
			List<SpacePartition<T>> totalFoundPartitions = null;
			for (int i = 0; i < subpartition.Count; i++) {
				SpacePartition<T> subPartition = subpartition[i];
				if (circle.radius != 0) {
					if (!subPartition.aabb.IntersectsCircle(circle.center, circle.radius)) { continue; }
				} else {
					if (!subPartition.aabb.contains(circle.center)) { continue; }
				}
				List<SpacePartition<T>> foundPartitions = subPartition.getPartitions(circle);
				if (foundPartitions != null) {
					if (totalFoundPartitions == null) { totalFoundPartitions = foundPartitions; }
					else {
						totalFoundPartitions.AddRange(foundPartitions);
					}
				}
			}
			//if (totalFoundPartitions == null) {
			//	totalFoundPartitions = new List<SpacePartition<T>>() { this };
			//}
			return totalFoundPartitions;
		}

		public void clearList() {
			list.Clear();
			if (subpartition != null) {
				for (int i = 0; i < subpartition.Count; i++) {
					subpartition[i].clearList();
				}
			}
		}

		public void populate(IList<T> list, Func<T, Circle> convertElementToCircleMethod) {
			clearList();
			for (int i = 0; i < list.Count; ++i) {
				T element = list[i];
				Circle circle = convertElementToCircleMethod(element);
				List<SpacePartition<T>> partitions = getPartitions(circle);
				//console.log(element.name+" in "+partitions.length+" partitions: "+partitions[0].aabb);
				for (int p = 0; p < partitions.Count; ++p) {
					SpacePartition<T> partition = partitions[p];
					partition.list.Add(element);
				}
			}
		}

		public List<CollisionData> getCollisions(Circle circle, Func<T,Circle> convertElementToCircleMethod) {
			List<SpacePartition<T>> partitions = getPartitions(circle);
			//console.log(partitions.length);
			List<CollisionData> collisions = null;
			for (int i = 0; i < partitions.Count; ++i) {
				//console.log(partitions[i].list.length);
				for (int e = 0; e < partitions[i].list.Count; ++e) {
					T element = partitions[i].list[e];
					if (e == null) { continue; }
					Circle otherCircle = convertElementToCircleMethod(element);
					float targetDistance = otherCircle.radius;
					if (circle.radius != 0) {
						targetDistance += circle.radius;
					}
					float distance = Vec2.Distance(circle.center, otherCircle.center);
					if (distance <= targetDistance) {
						if (collisions == null) { collisions = new List<CollisionData>(); }
						CollisionData collision = CollisionData.ForCircles(circle, otherCircle);
						collision.other = element as ICollidable;
						collisions.Add(collision);
					}
				}
			}
			return collisions;
		}
	}
}

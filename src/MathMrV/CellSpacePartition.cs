using MathMrV;
using System;
using System.Collections.Generic;
using System.Text;

interface Cell_I<T> where T : HasRectangle {
	/**
	 * @param location
	 * @return a list containing elements at the given location. can be null.
	 */
	public List<T> GetAt(AABB location);
	public void add(T entity);
	public void clear();
	public bool has(AABB area);
}

namespace MathMrV {
//	class Cell<T extends HasRectangle> implements Cell_I<T> {
//	protected ArrayList<T> list;
//	protected AABB area = new AABB();
//	public Cell(AABB area) {
//		this.area.set(area);
//	}
//	public ArrayList<T> getAt(AABB location) {
//		ArrayList<T> result = null;
//		if (list != null) {
//			AABB r;
//			T obj;
//			for (int i = 0; i < list.size(); ++i) {
//				obj = list.get(i);
//				r = obj.getRectangle();
//				if (location.intersects(r)) {
//					if (result == null) {
//						result = new ArrayList<T>();
//					}
//					result.add(obj);
//				}
//			}
//		}
//		return result;
//	}
//	public void add(T entity) {
//		if (list == null)
//			list = new ArrayList<T>();
//		list.add(entity);
//	}
//	public void clear() {
//		if (list != null)
//			list.clear();
//	}
//	public boolean has(AABB area) {
//		return this.area.intersects(area);
//	}
//	public void draw(Graphics2D g) {
//		area.draw(g);
//		if (list != null) {
//			AABB r;
//			for (int i = 0; i < list.size(); ++i) {
//				r = list.get(i).getRectangle();
//				r.draw(g);
//				area.getCenter().drawTo(g, r.min);
//			}
//		}
//	}
//	public AABB getArea() {
//		return area;
//	}
//}

//public class CellSpacePartition<T extends HasRectangle> implements Cell_I<T> {
//	// can't make an array of Cell<T> because arrays need runtime information
//	// about types :-/
//	private ArrayList<Cell<T>> cells;
//private Vec2I gridSize = new Vec2I();
//private Vec2D cellDimensions = new Vec2D();
//private AABB area = new AABB();

//public CellSpacePartition(AABB totalArea, Vec2I gridSize) {
//	area.set(totalArea);
//	this.gridSize.set(gridSize);
//	cells = new ArrayList<Cell<T>>();
//	cellDimensions.set(
//			totalArea.getWidth() / gridSize.x,
//			totalArea.getHeight() / gridSize.y);
//	Vec2D cursor = new Vec2D(area.min);
//	AABB cursorR = new AABB();
//	for (int r = 0; r < gridSize.y; ++r) {
//		for (int c = 0; c < gridSize.x; ++c) {
//			cursorR.set(cursor, cursor.sum(cellDimensions));
//			Cell<T> cell = new Cell<T>(cursorR);
//			cells.add(cell);
//			cursor.x += cellDimensions.x;
//		}
//		cursor.x = area.min.x;
//		cursor.y += cellDimensions.y;
//	}
//	//System.out.println(gridSize+"  "+area+"   "+cellDimensions);
//}
//private int getIndex(int row, int col) {
//	return row * gridSize.x + col;
//}
///**
// * @param location
// * @return
// */
//public int[] getCellListFor(AABB location) {
//	int[] list;
//	AABB rect = getGridIndexeRange(location, cellDimensions, gridSize);
//	list = new int[(int)((rect.getWidth() + 1) * (rect.getHeight() + 1))];
//	int index = 0;
//	for (int r = (int)rect.min.y; r <= rect.max.y; ++r) {
//		for (int c = (int)rect.min.x; c <= rect.max.x; ++c) {
//			list[index++] = getIndex(r, c);
//		}
//	}
//	return list;
//}
//public static AABB getGridIndexeRange(AABB location, Point2D cellSize, Vec2I gridSize) {
//	AABB rect = new AABB(location);
//	rect.min.divide(cellSize);
//	rect.max.divide(cellSize);
//	if (rect.min.x < 0) rect.min.x = 0;
//	if (rect.min.y < 0) rect.min.y = 0;
//	if (rect.max.x >= gridSize.x) rect.max.x = gridSize.x - 1;
//	if (rect.max.y >= gridSize.y) rect.max.y = gridSize.y - 1;
//	rect.clapToInt();
//	return rect;
//}
///**
// * @param location
// * @return can be null! always check for null!
// */
//public ArrayList<T> getAt(AABB location) {
//	ArrayList<T> totalResults = null, results = null;
//	int[] hitlist = getCellListFor(location);
//	for (int i = 0; i < hitlist.length; ++i) {
//		results = cells.get(hitlist[i]).getAt(location);
//		if (results != null) {
//			if (totalResults == null) {
//				totalResults = new ArrayList<T>();
//			}
//			totalResults.addAll(results);
//		}
//	}
//	return results;
//}
//public void add(T entity) {
//	int[] hitlist = getCellListFor(entity.getRectangle());
//	for (int i = 0; i < hitlist.length; ++i) {
//		cells.get(hitlist[i]).add(entity);
//	}
//}
//public void clear() {
//	for (int i = 0; i < cells.size(); ++i)
//		cells.get(i).clear();
//}
//public boolean has(AABB area) {
//	return this.area.intersects(area);
//}

//public void draw(Graphics2D g) {
//	area.draw(g);
//	for (int i = 0; i < cells.size(); ++i)
//		cells.get(i).draw(g);
//}
//}
}

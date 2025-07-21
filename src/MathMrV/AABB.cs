using ConsoleMrV;
using System;
using System.Collections.Generic;

namespace MathMrV {
	public interface HasRectangle {
		public AABB GetRectangle();
	}

	public class AABB : HasRectangle {
		public static AABB zero = new AABB(0, 0, 0, 0);
		public Vec2 min = new Vec2(), max = new Vec2();
		public void set(float minx, float miny, float maxx, float maxy) {
			min.x = minx; min.y = miny;
			max.x = maxx; max.y = maxy;
		}
		public void set(Vec2 min, Vec2 max) {
			this.min = min;
			this.max = max;
		}
		public AABB(float minx, float miny, float maxx, float maxy) {
			set(minx, miny, maxx, maxy);
		}
		public AABB(AABB r) {
			min = r.min;
			max = r.max;
		}
		public AABB() { }
		public AABB(Vec2 min, Vec2 max) {
			set(min, max);
		}
		public static AABB createFrom(Vec2 center, Vec2 dimensions) {
			Vec2 half = new Vec2(dimensions.x / 2, dimensions.y / 2);
			return new AABB(center.x - half.x, center.y - half.y,
							center.x + half.x, center.y + half.y);
		}
		public bool intersects(AABB r) {
			if (!isValid() || !r.isValid()) return false;
			return
				!(min.x >= r.max.x || max.x <= r.min.x
				|| min.y >= r.max.y || max.y <= r.min.y);
		}
		public bool IntersectsCircle(Vec2 center, float radius) {
			if (this.contains(center)) { return true; }
			Vec2 radSize = new Vec2(radius, radius);
			Vec2 expandedMin = this.min - radSize;
			Vec2 expandedMax = this.max + radSize;
			if (AABB.Contains(center, expandedMin, expandedMax)) {
				Vec2 cornerCase = Vec2.NaN;
				if (center.x < this.min.x) {
					if (center.y < this.min.y) {
						cornerCase = this.min;
					} else if (center.y > this.max.y) {
						cornerCase = new Vec2(this.min.x, this.max.y);
					}
				} else if (center.x > this.max.x) {
					if (center.y < this.min.y) {
						cornerCase = new Vec2(this.max.x, this.min.y);
					} else if (center.y > this.max.y) {
						cornerCase = this.max;
					}
				}
				if (!cornerCase.IsNaN()) {
					float distanceSqr = (cornerCase-center).MagnitudeSqr;
					return distanceSqr <= radius * radius;
				}
				return true;
			}
			return false;
		}
		public bool contains(Vec2 p) => Contains(p, min, max);
		public static bool Contains(Vec2 p, Vec2 min, Vec2 max) {
			return p.x >= min.x && p.x < max.x
				&& p.y >= min.y && p.y < max.y;
		}
		public void inset(float inset) {
			Vec2 ins = new Vec2(inset, inset);
			min += ins;
			max -= ins;
		}
		public float GetWidth() { return max.x - min.x; }
		public float GetHeight() { return max.y - min.y; }
		/** width/height vector */
		public Vec2 GetSize() { return new Vec2(GetWidth(), GetHeight()); }
		public float getArea() {
			Vec2 size = GetSize();
			return size.x * size.y;
		}
		// TODO draw with canvas
		public void draw(CommandLineCanvas g) {
			g.FillRect(min, max);
		}
		// TODO draw with canvas
		//	public void fill(Graphics2D g) {
		//	g.fillRect((int)min.x, (int)min.y,
		//			(int)getWidth(), (int)getHeight());
		//}
		public void translate(Vec2 delta) {
			min += delta;
			max += delta;
		}
		public void translate(float dx, float dy) {
			Vec2 delta = new Vec2(dx, dy);
			translate(delta);
		}
		public void clapToInt() {
			min.ClampToInt();
			max.ClampToInt();
		}
		public void roundToInt() {
			min.RoundToInt();
			max.RoundToInt();
		}
		public void set(AABB r) {
			min = r.min;
			max = r.max;
		}
		public override string ToString() {
			return "[min(" + min.x + "," + min.y + "), max(" + max.x + "," + max.y + "), w/h(" + (int)GetWidth() + "," + (int)GetHeight() + ")]";
		}
		public Vec2 getCenter() => (min + max) / 2;
		public bool contains(AABB r) {
			return min.x <= r.min.x && max.x >= r.max.x
				&& min.y <= r.min.y && max.y >= r.max.y;
		}
		public AABB GetRectangle() => this;
		public bool isValid() => GetWidth() > 0 && GetHeight() > 0;
		/**
		 * @param r
		 * @return where this and r overlap
		 */
		public AABB getUnion(AABB r) {
			if (!intersects(r)) return null;
			AABB union = new AABB(
					MathF.Max(min.x, r.min.x), MathF.Max(min.y, r.min.y),
					MathF.Min(max.x, r.max.x), MathF.Min(max.y, r.max.y));
			return union.isValid() ? union : null;
		}

		public void Add(AABB r) {
			if (r.min.x < min.x) min.x = r.min.x;
			if (r.max.x > max.x) max.x = r.max.x;
			if (r.min.y < min.y) min.y = r.min.y;
			if (r.max.y > max.y) max.y = r.max.y;
		}
		public void Add(Vec2 p) {
			if (p.x < min.x) min.x = p.x;
			if (p.x > max.x) max.x = p.x;
			if (p.y < min.y) min.y = p.y;
			if (p.y > max.y) max.y = p.y;
		}
		public bool CanMerge(AABB r) {
			return (min.y == r.min.y && max.y == r.max.y && (min.x == r.max.x || max.x == r.min.x))
				|| (min.x == r.min.x && max.x == r.max.x && (min.y == r.max.y || max.y == r.min.y));
		}

		public static void Merge(List<AABB> list) {
			AABB ra, rb;
			// TODO iterate list backwards, so --a is not required
			for (int a = 0; a < list.Count; ++a) {
				ra = list[a];
				if (!ra.isValid()) {
					list.RemoveAt(a);
					--a;
					continue;
				}
				// TODO iterate list backwards, so --a is not required
				for (int b = a + 1; b < list.Count; ++b) {
					rb = list[b];
					if (ra.CanMerge(rb)) {
						ra.Add(rb);
						list.RemoveAt(b);
						--b;
					}
				}
			}
		}
		/** move this rectangle assuming, this rectangle is the unit grid size */
		public void gridTranslate(int colTranslate, int rowTranslate) {
			Vec2 delta = new Vec2(GetWidth() * colTranslate, GetHeight() * rowTranslate);
			translate(delta);
		}
		/** a new rectangle that would be translated, assuming this rectangle is the unit grid size */
		public AABB getGridTranslated(int colTranslate, int rowTranslate) {
			AABB moved = new AABB(this);
			moved.gridTranslate(colTranslate, rowTranslate);
			return moved;
		}
		// TODO use canvas
		//public static void draw(Graphics2D g, List<AABB> list) {
		//	if (list == null || list.size() == 0)
		//		return;
		//	for (int i = 0; i < list.size(); ++i) {
		//		list.get(i).draw(g);
		//	}
		//}
		//public static void fill(Graphics2D g, List<AABB> list) {
		//	if (list == null || list.size() == 0)
		//		return;
		//	for (int i = 0; i < list.size(); ++i) {
		//		list.get(i).fill(g);
		//	}
		//}
		/** collision with the North side of the rectangle */
		public const int N = 0;
		/** collision with the West side of the rectangle */
		public const int W = 1;
		/** collision with the South side of the rectangle */
		public const int S = 2;
		/** collision with the East side of the rectangle */
		public const int E = 3;
		/** @return {@link #N},{@link #W},{@link #S},{@link #E}*/
		public int squeezOutOf(AABB r, Vec2 a_out) {
			// up, left, down, right
			float[] squeeze = { max.y - r.min.y, max.x - r.min.x, r.max.y - min.y, r.max.x - min.x };
			int collidingSide = 0;
			for (int i = 0; i < squeeze.Length; ++i) {
				if (squeeze[i] < squeeze[collidingSide])
					collidingSide = i;
			}
			float dx = 0, dy = 0;
			switch (collidingSide) {
				case N: dy = -squeeze[0]; break;
				case W: dx = -squeeze[1]; break;
				case S: dy = squeeze[2]; break;
				case E: dx = squeeze[3]; break;
			}
			if (dx != 0 || dy != 0) {
				if (a_out != null) {
					a_out.x += dx;
					a_out.y += dy;
				}
				translate(dx, dy);
				return collidingSide;
			}
			return -1;
		}
		public void multiply(float d) {
			min *= d;
			max *= d;
		}
		public void multiply(Vec2 v) {
			min.Scale(v);
			max.Scale(v);
		}
		private static Vec2 hflip = new Vec2(-1, 1);
		public void horizontalFlip() {
			multiply(hflip);
			correctNegative();
		}
		public void correctNegative() {
			float t;
			if (min.x > max.x) { t = min.x; min.x = max.x; max.x = t; }
			if (min.y > max.y) { t = min.y; min.y = max.y; max.y = t; }
		}
	}
}

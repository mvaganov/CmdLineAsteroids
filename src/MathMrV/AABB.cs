using ConsoleMrV;
using System;
using System.Collections.Generic;
using static MathMrV.AABB;

namespace MathMrV {
	public interface HasRectangle {
		public AABB GetRectangle();
	}

	public struct AABB : HasRectangle {
		public static AABB zero = new AABB(0, 0, 0, 0);
		public Vec2 Min, Max;
		public float CenterX => (Min.x + Min.y) / 2;
		public float CenterY => (Min.y + Min.y) / 2;
		public void Set(float minx, float miny, float maxx, float maxy) {
			Min.x = minx; Min.y = miny;
			Max.x = maxx; Max.y = maxy;
		}
		public void Set(Vec2 min, Vec2 max) {
			this.Min = min;
			this.Max = max;
		}
		public AABB(float minx, float miny, float maxx, float maxy) {
			Min = new Vec2(minx, miny);
			Max = new Vec2(maxx, maxy);
		}
		public AABB(AABB r) {
			Min = r.Min;
			Max = r.Max;
		}
		//public AABB() { Min = Max = Vec2.Zero; }
		public AABB(Vec2 min, Vec2 max) {
			Min = min; Max = max;
		}
		public static AABB CreateAt(Vec2 center, Vec2 size) {
			Vec2 extents = size / 2;
			return new AABB(center - extents, center + extents);
		}
		public bool Intersects(AABB r) {
			if (!IsValid() || !r.IsValid()) return false;
			return !(Min.x >= r.Max.x || Max.x <= r.Min.x || Min.y >= r.Max.y || Max.y <= r.Min.y);
		}
		public bool IntersectsCircle(Vec2 center, float radius) {
			if (this.Contains(center)) { return true; }
			Vec2 radSize = new Vec2(radius, radius);
			Vec2 expandedMin = this.Min - radSize;
			Vec2 expandedMax = this.Max + radSize;
			if (AABB.Contains(center, expandedMin, expandedMax)) {
				Vec2 cornerCase = Vec2.NaN;
				if (center.x < this.Min.x) {
					if (center.y < this.Min.y) {
						cornerCase = this.Min;
					} else if (center.y > this.Max.y) {
						cornerCase = new Vec2(this.Min.x, this.Max.y);
					}
				} else if (center.x > this.Max.x) {
					if (center.y < this.Min.y) {
						cornerCase = new Vec2(this.Max.x, this.Min.y);
					} else if (center.y > this.Max.y) {
						cornerCase = this.Max;
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
		public bool Contains(Vec2 p) => Contains(p, Min, Max);
		public static bool Contains(Vec2 p, Vec2 min, Vec2 max) {
			return p.x >= min.x && p.x < max.x && p.y >= min.y && p.y < max.y;
		}
		public void Inset(float inset) {
			Inset(new Vec2(inset, inset));
		}
		public void Inset(Vec2 inset) {
			Min += inset;
			Max -= inset;
		}
		public float GetWidth() { return Max.x - Min.x; }
		public float GetHeight() { return Max.y - Min.y; }
		/** width/height vector */
		public Vec2 GetSize() { return new Vec2(GetWidth(), GetHeight()); }
		public float GetArea() {
			Vec2 size = GetSize();
			return size.x * size.y;
		}
		// TODO draw with canvas
		public void draw(CommandLineCanvas g) {
			g.FillRect(Min, Max);
		}
		public void Translate(Vec2 delta) {
			Min += delta;
			Max += delta;
		}
		public void Translate(float dx, float dy) {
			Vec2 delta = new Vec2(dx, dy);
			Translate(delta);
		}
		public void ClampToInt() {
			Min.ClampToInt();
			Max.ClampToInt();
		}
		public void RoundToInt() {
			Min.RoundToInt();
			Max.RoundToInt();
		}
		public void Set(AABB r) {
			Min = r.Min;
			Max = r.Max;
		}
		public override string ToString() {
			return "[min(" + Min.x + "," + Min.y + "), max(" + Max.x + "," + Max.y + "), w/h(" + (int)GetWidth() + "," + (int)GetHeight() + ")]";
		}
		public Vec2 GetCenter() => (Min + Max) / 2;
		public bool Contains(AABB r) {
			return Min.x <= r.Min.x && Max.x >= r.Max.x && Min.y <= r.Min.y && Max.y >= r.Max.y;
		}
		public AABB GetRectangle() => this;
		public bool IsValid() => GetWidth() > 0 && GetHeight() > 0;
		public bool TryGetUnion(AABB r, out AABB union) {
			if (!Intersects(r)) { 
				union = new AABB(Vec2.NaN, Vec2.NaN);
				return false;
			}
			union = new AABB(
					MathF.Max(Min.x, r.Min.x), MathF.Max(Min.y, r.Min.y),
					MathF.Min(Max.x, r.Max.x), MathF.Min(Max.y, r.Max.y));
			return union.IsValid();
		}

		public void Add(AABB r) {
			if (r.Min.x < Min.x) Min.x = r.Min.x;
			if (r.Max.x > Max.x) Max.x = r.Max.x;
			if (r.Min.y < Min.y) Min.y = r.Min.y;
			if (r.Max.y > Max.y) Max.y = r.Max.y;
		}
		public void Add(Vec2 p) {
			if (p.x < Min.x) Min.x = p.x;
			if (p.x > Max.x) Max.x = p.x;
			if (p.y < Min.y) Min.y = p.y;
			if (p.y > Max.y) Max.y = p.y;
		}
		/// <summary>
		/// Determines if rectangles are adjacent, with identical adjacent lengths. These can be merged.
		/// </summary>
		/// <param name="r"></param>
		/// <returns></returns>
		public bool CanMerge(AABB r) {
			return (Min.y == r.Min.y && Max.y == r.Max.y && (Min.x == r.Max.x || Max.x == r.Min.x))
					|| (Min.x == r.Min.x && Max.x == r.Max.x && (Min.y == r.Max.y || Max.y == r.Min.y));
		}

		public static void Merge(List<AABB> list) {
			AABB ra, rb;
			// TODO iterate list backwards, so --a is not required
			for (int a = 0; a < list.Count; ++a) {
				ra = list[a];
				if (!ra.IsValid()) {
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
		/// <summary>
		/// move this rectangle assuming, this rectangle is the unit grid size
		/// </summary>
		public void GridTranslate(int colTranslate, int rowTranslate) {
			Vec2 delta = new Vec2(GetWidth() * colTranslate, GetHeight() * rowTranslate);
			Translate(delta);
		}
		/// <summary>
		/// a new rectangle that would be translated, assuming this rectangle is the unit grid size
		/// </summary>
		/// <param name="colTranslate"></param>
		/// <param name="rowTranslate"></param>
		/// <returns></returns>
		public AABB GetGridTranslated(int colTranslate, int rowTranslate) {
			AABB moved = new AABB(this);
			moved.GridTranslate(colTranslate, rowTranslate);
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
		public enum SideIndex { Invalid = -1, MinY = 0, MinX = 1, MaxY = 2, MaxX = 3 }
		public SideIndex SqueezOutOf(AABB r, out Vec2 a_out) {
			// up, left, down, right
			float[] squeeze = { Max.y - r.Min.y, Max.x - r.Min.x, r.Max.y - Min.y, r.Max.x - Min.x };
			SideIndex collidingSide = SideIndex.MinY;
			for (int i = 1; i < squeeze.Length; ++i) {
				if (squeeze[i] < squeeze[(int)collidingSide])
					collidingSide = (SideIndex)i;
			}
			a_out = Vec2.Zero;
			switch (collidingSide) {
				case SideIndex.MinY: a_out.y = -squeeze[(int)SideIndex.MinY]; break;
				case SideIndex.MinX: a_out.x = -squeeze[(int)SideIndex.MinX]; break;
				case SideIndex.MaxY: a_out.y = squeeze[(int)SideIndex.MaxY]; break;
				case SideIndex.MaxX: a_out.x = squeeze[(int)SideIndex.MaxX]; break;
			}
			if (a_out.x != 0 || a_out.y != 0) {
				Translate(a_out);
				return collidingSide;
			}
			return SideIndex.Invalid;
		}
		public void multiply(float d) {
			Min *= d;
			Max *= d;
		}
		public void Scale(Vec2 v) {
			Min.Scale(v);
			Max.Scale(v);
		}
		private static Vec2 hflip = new Vec2(-1, 1);
		public void HorizontalFlip() {
			Scale(hflip);
			CorrectNegative();
		}
		public void CorrectNegative() {
			float swap;
			if (Min.x > Max.x) { swap = Min.x; Min.x = Max.x; Max.x = swap; }
			if (Min.y > Max.y) { swap = Min.y; Min.y = Max.y; Max.y = swap; }
		}
		public override int GetHashCode() => Min.GetHashCode() ^ Max.GetHashCode();
		public override bool Equals(object obj) => obj is AABB aabb && Equals(aabb);
		public bool Equals(AABB aabb) => Min == aabb.Min && Max == aabb.Max;
		public static bool operator==(AABB aabb1, AABB aabb2) => aabb1.Equals(aabb2);
		public static bool operator!=(AABB aabb1, AABB aabb2) => !aabb1.Equals(aabb2);
	}
}

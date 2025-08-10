using System;

namespace MathMrV {
	public struct AABB {
		public static AABB zero = new AABB(0, 0, 0, 0);
		public Vec2 Min, Max;
		public float CenterX => (Min.x + Min.y) / 2;
		public float CenterY => (Min.y + Min.y) / 2;
		public Vec2 Center => new Vec2(CenterX, CenterY);
		public float Width => (Max.x - Min.x);
		public float Height => (Max.y - Min.y);
		public Vec2 Size => new Vec2(Width, Height);
		public AABB(float minx, float miny, float maxx, float maxy) :
			this(new Vec2(minx, miny), new Vec2(maxx, maxy)) { }
		public AABB(AABB r) : this(r.Min, r.Max) { }
		public AABB(Vec2 min, Vec2 max) { Min = min; Max = max; }
		public static AABB CreateAt(Vec2 center, Vec2 size) {
			Vec2 extents = size / 2;
			return new AABB(center - extents, center + extents);
		}
		public bool Intersects(AABB r) {
			if (!IsValid() || !r.IsValid()) {
				return false;
			}
			return !(Min.x >= r.Max.x || Max.x <= r.Min.x || Min.y >= r.Max.y || Max.y <= r.Min.y);
		}
		public bool Contains(Vec2 p) => Contains(p, Min, Max);
		public static bool Contains(Vec2 p, Vec2 min, Vec2 max) {
			return p.x >= min.x && p.x < max.x && p.y >= min.y && p.y < max.y;
		}
		public void Translate(float dx, float dy) {
			Vec2 delta = new Vec2(dx, dy);
			Translate(delta);
		}
		public void Translate(Vec2 delta) { Min += delta; Max += delta; }
		public override string ToString() => $"[min{Min}, max{Max}, w/h({Width}, {Height})]";
		public bool Contains(AABB r) {
			return Min.x <= r.Min.x && Max.x >= r.Max.x && Min.y <= r.Min.y && Max.y >= r.Max.y;
		}
		public bool IsValid() => Width > 0 && Height > 0;
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
		public enum SideIndex { Invalid = -1, MinY = 0, MinX = 1, MaxY = 2, MaxX = 3 }
		public SideIndex SqueezOutOf(AABB r, out Vec2 a_out) {
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
		public void Scale(float d) { Min *= d; Max *= d; }
		public void Scale(Vec2 v) { Min.Scale(v); Max.Scale(v); }
		public void CorrectNegativeDimensions() {
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

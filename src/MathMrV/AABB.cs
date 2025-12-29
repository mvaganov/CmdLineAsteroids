using System;

namespace MathMrV {
	public struct AABB {
		public static AABB zero = new AABB(0, 0, 0, 0);
		public Vec2 Min, Max;
		public float CenterX => (Min.X + Max.X) / 2;
		public float CenterY => (Min.Y + Max.Y) / 2;
		public Vec2 Center => new Vec2(CenterX, CenterY);
		public float Width => (Max.X - Min.X);
		public float Height => (Max.Y - Min.Y);
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
			return !(Min.X >= r.Max.X || Max.X <= r.Min.X || Min.Y >= r.Max.Y || Max.Y <= r.Min.Y);
		}
		public bool Contains(Vec2 p) => Contains(p, Min, Max);
		public static bool Contains(Vec2 p, Vec2 min, Vec2 max) {
			return p.X >= min.X && p.X < max.X && p.Y >= min.Y && p.Y < max.Y;
		}
		public void Translate(float dx, float dy) {
			Vec2 delta = new Vec2(dx, dy);
			Translate(delta);
		}
		public void Translate(Vec2 delta) { Min += delta; Max += delta; }
		public override string ToString() => $"[min{Min}, max{Max}, w/h({Width}, {Height})]";
		public bool Contains(AABB r) {
			return Min.X <= r.Min.X && Max.X >= r.Max.X && Min.Y <= r.Min.Y && Max.Y >= r.Max.Y;
		}
		public bool IsValid() => Width > 0 && Height > 0;
		public bool TryGetUnion(AABB r, out AABB union) {
			if (!Intersects(r)) { 
				union = new AABB(Vec2.NaN, Vec2.NaN);
				return false;
			}
			union = new AABB(
					MathF.Max(Min.X, r.Min.X), MathF.Max(Min.Y, r.Min.Y),
					MathF.Min(Max.X, r.Max.X), MathF.Min(Max.Y, r.Max.Y));
			return union.IsValid();
		}

		public void Add(AABB r) {
			if (r.Min.X < Min.X) Min.X = r.Min.X;
			if (r.Max.X > Max.X) Max.X = r.Max.X;
			if (r.Min.Y < Min.Y) Min.Y = r.Min.Y;
			if (r.Max.Y > Max.Y) Max.Y = r.Max.Y;
		}
		public void Add(Vec2 p) {
			if (p.X < Min.X) Min.X = p.X;
			if (p.X > Max.X) Max.X = p.X;
			if (p.Y < Min.Y) Min.Y = p.Y;
			if (p.Y > Max.Y) Max.Y = p.Y;
		}
		public enum SideIndex { Invalid = -1, MinY = 0, MinX = 1, MaxY = 2, MaxX = 3 }
		public SideIndex SqueezOutOf(AABB r, out Vec2 a_out) {
			float[] squeeze = { Max.Y - r.Min.Y, Max.X - r.Min.X, r.Max.Y - Min.Y, r.Max.X - Min.X };
			SideIndex collidingSide = SideIndex.MinY;
			for (int i = 1; i < squeeze.Length; ++i) {
				if (squeeze[i] < squeeze[(int)collidingSide])
					collidingSide = (SideIndex)i;
			}
			a_out = Vec2.Zero;
			switch (collidingSide) {
				case SideIndex.MinY: a_out.Y = -squeeze[(int)SideIndex.MinY]; break;
				case SideIndex.MinX: a_out.X = -squeeze[(int)SideIndex.MinX]; break;
				case SideIndex.MaxY: a_out.Y = squeeze[(int)SideIndex.MaxY]; break;
				case SideIndex.MaxX: a_out.X = squeeze[(int)SideIndex.MaxX]; break;
			}
			if (a_out.X != 0 || a_out.Y != 0) {
				Translate(a_out);
				return collidingSide;
			}
			return SideIndex.Invalid;
		}
		public void Scale(float d) { Min *= d; Max *= d; }
		public void Scale(Vec2 v) { Min *= v; Max *= v; }
		public void CorrectNegativeDimensions() {
			float swap;
			if (Min.X > Max.X) { swap = Min.X; Min.X = Max.X; Max.X = swap; }
			if (Min.Y > Max.Y) { swap = Min.Y; Min.Y = Max.Y; Max.Y = swap; }
		}
		public override int GetHashCode() => Min.GetHashCode() ^ Max.GetHashCode();
		public override bool Equals(object obj) => obj is AABB aabb && Equals(aabb);
		public bool Equals(AABB aabb) => Min == aabb.Min && Max == aabb.Max;
		public static bool operator==(AABB aabb1, AABB aabb2) => aabb1.Equals(aabb2);
		public static bool operator!=(AABB aabb1, AABB aabb2) => !aabb1.Equals(aabb2);
	}
}

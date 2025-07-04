using MathMrV;

namespace asteroids {
	public interface ICollidable {
		public bool TryGetAABB(out Vec2 min, out Vec2 max);
		public bool TryGetCircle(out Circle circle);
	}
}

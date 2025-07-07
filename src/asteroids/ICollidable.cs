using MathMrV;
using System.Collections.Generic;

namespace asteroids {
	public interface ICollidable {
		public bool TryGetAABB(out Vec2 min, out Vec2 max);
		public bool TryGetCircle(out Circle circle);
		public bool TryGetIntersection(ICollidable collidable, List<Vec2> intersections);
		public bool IsColliding(ICollidable collidable);
	}
}

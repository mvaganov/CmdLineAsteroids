using System.Drawing;

namespace asteroids {
	public interface IGameObject {
		public void Draw();
		public void Update();
	}
	public interface IMobileEntity {
		public Vec2 Position { get; set; }
		public Vec2 Velocity { get; set; }
	}
	public interface ICollidable {
		public bool TryGetAABB(out Vec2 min, out Vec2 max);
		public bool TryGetCircle(out Circle circle);
	}
	public class MobileEntity {
	}
}

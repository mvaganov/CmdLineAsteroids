using MathMrV;
using ColliderID = System.Byte; // Alias Directive instead of Generic (aka template)

namespace collision {
	public interface ICollidable {
		public CollisionData IsColliding(ICollidable collidable);
		public Circle GetCollisionBoundingCircle();
		public ColliderID TypeId { get; set; }
	}
}

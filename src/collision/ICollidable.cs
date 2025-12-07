using MathMrV;
using ColliderID = System.Byte; // Alias Directive instead of Generic (Template)

namespace collision {
	public interface ICollidable {
		public CollisionData IsColliding(ICollidable collidable);
		public Circle GetCollisionBoundingCircle();
		public ColliderID TypeId { get; set; }
	}
}

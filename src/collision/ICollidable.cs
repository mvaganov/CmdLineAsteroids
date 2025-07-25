using MathMrV;
namespace collision {
	public interface ICollidable {
		public CollisionData IsColliding(ICollidable collidable);
		public Circle GetCollisionBoundingCircle();
		public byte TypeId { get; set; }
	}
}

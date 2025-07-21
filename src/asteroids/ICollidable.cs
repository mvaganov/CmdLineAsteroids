using MathMrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public interface ICollidable {
		public CollisionData IsColliding(ICollidable collidable);
		public Circle GetCollisionBoundingCircle();
		public byte TypeId { get; set; }
	}
}

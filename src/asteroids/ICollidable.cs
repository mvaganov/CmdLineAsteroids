using System;
using System.Collections.Generic;

namespace asteroids {
	public interface ICollidable {
		public CollisionData IsColliding(ICollidable collidable);
		public byte TypeId { get; set; }
	}
}

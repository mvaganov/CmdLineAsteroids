using System;
using System.Collections.Generic;
using System.Text;

namespace asteroids {
	public class MobileEntity {
		public Vec2 velocity;
		public Vec2 position;
		private Polygon polygon;
		private Circle collisionCircle;
	}
}

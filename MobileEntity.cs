using System;
using System.Collections.Generic;
using System.Text;

namespace asteroids {
	public class MobileEntity {
		public Vec2 velocity;
		public Vec2 direction;
		private Polygon polygon;
		public Vec2 position { get => polygon.OriginOffset; set => polygon.OriginOffset = value; }
	}
}

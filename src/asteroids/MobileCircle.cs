using ConsoleMrV;
using MathMrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class MobileCircle : MobileObject, ICollidable {
		private Circle circle;
		public override Vec2 Position { get => circle.Position; set => circle.Position = value; }
		public override Vec2 Direction { get => Velocity.ToUnitVector(); set => throw new NotImplementedException(); }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }

		public MobileCircle(Circle circle) {
			this.circle = circle;
		}
		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) return;
			circle.Draw(graphicsContext);
		}

		public bool TryGetAABB(out Vec2 min, out Vec2 max) => circle.TryGetAABB(out min, out max);

		public bool TryGetCircle(out Circle circle) {
			circle = this.circle;
			return true;
		}

		public bool IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle c: return circle.IsColliding(c.circle);
			}
			return false;
		}

		public bool TryGetIntersection(ICollidable collidable, List<Vec2> intersections) {
			throw new NotImplementedException();
		}
	}
}

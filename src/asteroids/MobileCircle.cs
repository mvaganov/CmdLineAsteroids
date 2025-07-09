using ConsoleMrV;
using MathMrV;
using System;

namespace asteroids {
	public class MobileCircle : MobileObject, ICollidable {
		private Circle circle;
		public override Vec2 Position { get => circle.Position; set => circle.Position = value; }
		public override Vec2 Direction { get => Velocity.ToUnitVector(); set => throw new NotImplementedException(); }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }
		public Circle Circle {  get => circle; set => circle = value; }
		public MobileCircle(Circle circle) {
			this.circle = circle;
		}
		public virtual void Copy(MobileCircle other) {
			base.Copy(other);
			circle = other.circle;
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) return;
			circle.Draw(graphicsContext);
			float speed = Velocity.Magnitude;
			if (speed > 0) {
				Vec2 dir = Velocity / speed;
				Vec2 start = Position + dir * Radius;
				Vec2 end = start + Velocity;
				graphicsContext.DrawLine(start, end);
			}
		}

		public bool IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle c: return circle.IsColliding(c.circle);
				case MobilePolygon mp: return mp.IsColliding(circle);
			}
			return false;
		}
	}
}

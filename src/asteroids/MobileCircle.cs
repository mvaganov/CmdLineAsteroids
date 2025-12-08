using collision;
using ConsoleMrV;
using MathMrV;
using System;

namespace asteroids {
	public class MobileCircle : MobileObject, ICollidable {
		private Circle circle;
		public override Vec2 Position { get => circle.Position; set => circle.Position = value; }
		public override Vec2 Direction { get => Velocity.Normal; set { } }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }
		public Circle Circle {  get => circle; set => circle = value; }
		public static bool DebugShowVelocity = false;
		public MobileCircle(Circle circle) {
			this.circle = circle;
		}
		public virtual void Copy(MobileCircle other) {
			base.Copy(other);
			circle = other.circle;
		}
		public Circle GetCollisionBoundingCircle() => circle;
		public override void Draw(CommandLineCanvas canvas) {
			if (!_active) return;
			circle.Draw(canvas);
			if (DebugShowVelocity) {
				ShowDebugVelocity(canvas);
			}
		}

		private void ShowDebugVelocity(CommandLineCanvas graphicsContext) {
			float speed = Velocity.Magnitude;
			if (speed == 0) {
				return;
			}
			Vec2 dir = Velocity / speed;
			Vec2 start = Position + dir * Radius;
			Vec2 end = start + Velocity;
			graphicsContext.SetColor(ConsoleColor.White);
			graphicsContext.DrawLine(start, end);
		}

		public CollisionData IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle c:
					if (circle.IsColliding(c.circle)) {
						CollisionData data = CollisionData.ForCircles(circle, c.circle);
						data.SetParticipants(this, collidable);
						return data;
					}
					return null;
				case MobilePolygon mp: return mp.GetCollision(circle);
			}
			return null;
		}
	}
}

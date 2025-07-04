using ConsoleMrV;
using MathMrV;
using System;

namespace asteroids {
	public class MobileCircle : MobileObject {
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
	}
}

using ConsoleMrV;
using MathMrV;

namespace asteroids {
	public class MobilePolygon : MobileObject {
		protected Polygon polygon;
		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }
		public Polygon Polygon { get => polygon; set => polygon = value; }
		public float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) {
				return;
			}
			polygon.Draw(graphicsContext);
		}
	}
}

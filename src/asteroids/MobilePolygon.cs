using ConsoleMrV;
using MathMrV;
using MrV;
using System.Collections.Generic;

namespace asteroids {
	public class MobilePolygon : MobileObject, ICollidable {
		protected Polygon polygon;
		protected Circle[] _collisionCircles;

		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }
		public Polygon Polygon { get => polygon; set => polygon = value; }
		public float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public Circle[] CollisionCircles { get => _collisionCircles; set => _collisionCircles = value; }
		public Circle CollisionBoundingCircle;
		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) {
				return;
			}
			if (_collisionCircles == null) {
				polygon.Draw(graphicsContext);
			} else {
				BlinkBetweenPolygonAndCollisionCircles(graphicsContext);
			}
		}

		private void BlinkBetweenPolygonAndCollisionCircles(CommandLineGraphicsContext graphicsContext) {
			if (((int)(Time.TimeSeconds*5)) % 2 == 0) {
				polygon.Draw(graphicsContext);
			} else {
				for (int i = 0; i < _collisionCircles.Length; i++) {
					GetCollisionCircle(i).Draw(graphicsContext);
				}
			}
		}

		public Circle GetCollisionCircle(int index) {
			Circle circle = _collisionCircles[index];
			circle.Position = Position + circle.position.RotatedRadians(RotationRadians);
			return circle;
		}

		public bool TryGetAABB(out Vec2 min, out Vec2 max) {
			throw new System.NotImplementedException();
		}

		public bool TryGetCircle(out Circle circle) {
			throw new System.NotImplementedException();
		}

		public bool TryGetIntersection(ICollidable collidable, List<Vec2> intersections) {
			throw new System.NotImplementedException();
		}

		public bool IsColliding(ICollidable collidable) {
			throw new System.NotImplementedException();
		}
	}
}

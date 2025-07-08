using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class MobilePolygon : MobileObject, ICollidable {
		protected Polygon polygon;
		public Circle BoundingCircle;
		protected Circle[] _detailedCollisionCircles;

		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }
		public Polygon Polygon { get => polygon; set => polygon = value; }
		public float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public Circle[] CollisionCircles { get => _detailedCollisionCircles; set => _detailedCollisionCircles = value; }
		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
			BoundingCircle = Welzl.MakeCircle(playerPoly);
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) {
				return;
			}
			if (_detailedCollisionCircles == null) {
				polygon.Draw(graphicsContext);
			} else {
				BlinkBetweenPolygonAndCollisionCircles(graphicsContext);
			}
		}

		private void BlinkBetweenPolygonAndCollisionCircles(CommandLineGraphicsContext graphicsContext) {
			if (((int)(Time.TimeSeconds*5)) % 2 == 0) {
				polygon.Draw(graphicsContext);
			} else {
				for (int i = 0; i < _detailedCollisionCircles.Length; i++) {
					GetCollisionCircleInSpace(i).Draw(graphicsContext);
				}
			}
		}

		public Circle GetCollisionCircleInSpace(int index) {
			Circle circle = _detailedCollisionCircles[index];
			circle.Position = Position + circle.position.RotatedRadians(RotationRadians);
			return circle;
		}
		public Circle GetBoundingCircleInSpace() {
			Circle circle = BoundingCircle;
			circle.Position = Position + circle.position.RotatedRadians(RotationRadians);
			return circle;
		}

		public virtual bool IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle mc: return IsColliding(mc.Circle);
				case MobilePolygon mp: return IsColliding(mp);
			}
			return false;
		}

		public bool IsColliding(Circle circleInSpace) {
			if (!GetBoundingCircleInSpace().IsColliding(circleInSpace)) {
				return false;
			}
			if (_detailedCollisionCircles == null) {
				return true;
			}
			return IsCollidingInternal(circleInSpace);
		}
		private bool IsCollidingInternal(Circle c) {
			for (int i = 0; i < _detailedCollisionCircles.Length; ++i) {
				if (c.IsColliding(_detailedCollisionCircles[i])) {
					return true;
				}
			}
			return false;
		}
		public bool IsColliding(MobilePolygon other) {
			if (!GetBoundingCircleInSpace().IsColliding(other.GetBoundingCircleInSpace())) {
				return false;
			}
			if (CollisionCircles == null && other.CollisionCircles == null) {
				return true;
			}
			if (CollisionCircles == null && other.CollisionCircles != null) {
				return other.IsCollidingInternal(GetBoundingCircleInSpace());
			}
			if (CollisionCircles != null && other.CollisionCircles == null) {
				return IsCollidingInternal(other.GetBoundingCircleInSpace());
			}
			for (int c = 0; c < other.CollisionCircles.Length; ++c) {
				for (int i = 0; i < CollisionCircles.Length; ++i) {
					if (other.CollisionCircles[c].IsColliding(CollisionCircles[i])) {
						return true;
					}
				}
			}
			return false;
		}
	}
}

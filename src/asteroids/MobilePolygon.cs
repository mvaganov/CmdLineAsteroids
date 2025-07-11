using ConsoleMrV;
using MathMrV;
using MrV;

namespace asteroids {
	public class MobilePolygon : MobileObject, ICollidable {
		public static bool ShowCollisionCircles = false;
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
			BoundingCircle = Welzl.GetMinimumCircle(playerPoly);
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			if (!_active) {
				return;
			}
			if (!ShowCollisionCircles || _detailedCollisionCircles == null) {
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

		public virtual CollisionData IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle mc:
					return MyCollisionWith(collidable, GetCollision(mc.Circle));
				case MobilePolygon mp: return IsColliding(mp);
			}
			return null;
		}

		public CollisionData GetCollision(Circle circleInSpace) {
			if (!GetBoundingCircleInSpace().IsColliding(circleInSpace)) {
				return null;
			}
			if (_detailedCollisionCircles == null) {
				return CollisionData.ForCircles(GetBoundingCircleInSpace(), circleInSpace);
			}
			return GetCollisionInternal(circleInSpace);
		}
		private CollisionData GetCollisionInternal(Circle otherCircle) {
			for (int i = 0; i < _detailedCollisionCircles.Length; ++i) {
				Circle selfCircle = GetCollisionCircleInSpace(i);
				if (otherCircle.IsColliding(selfCircle)) {
					CollisionData data = CollisionData.ForCircles(selfCircle, otherCircle);
					data.colliderIndexSelf = i;
					data.self = this;
					return data;
				}
			}
			return null;
		}
		public CollisionData IsColliding(MobilePolygon other) {
			if (!GetBoundingCircleInSpace().IsColliding(other.GetBoundingCircleInSpace())) {
				return null;
			}
			if (_detailedCollisionCircles == null && other._detailedCollisionCircles == null) {
				return MyCollisionWith(other, GetBoundingCircleInSpace(), other.GetBoundingCircleInSpace());
			}
			if (_detailedCollisionCircles == null && other._detailedCollisionCircles != null) {
				return MyCollisionWith(other, other.GetCollisionInternal(GetBoundingCircleInSpace()));
			}
			if (_detailedCollisionCircles != null && other._detailedCollisionCircles == null) {
				return MyCollisionWith(other, GetCollisionInternal(other.GetBoundingCircleInSpace()));
			}
			for (int c = 0; c < other._detailedCollisionCircles.Length; ++c) {
				for (int i = 0; i < _detailedCollisionCircles.Length; ++i) {
					Circle selfCircle = GetCollisionCircleInSpace(i);
					Circle otherCircle = other.GetCollisionCircleInSpace(i);
					if (selfCircle.IsColliding(otherCircle)) {
						CollisionData data = MyCollisionWith(other, selfCircle, otherCircle);
						data.self = this;
						data.colliderIndexSelf = i;
						data.other = other;
						data.colliderIndexOther = c;
						return data;
					}
				}
			}
			return null;
		}
		private CollisionData MyCollisionWith(ICollidable other, CollisionData collision) {
			if (collision != null) {
				collision.SetParticipants(this, other);
			}
			return collision;
		}
		private CollisionData MyCollisionWith(ICollidable other, Circle myCircle, Circle otherCircle) {
			return MyCollisionWith(other, CollisionData.ForCircles(myCircle, otherCircle));
		}
	}
}

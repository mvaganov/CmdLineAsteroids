using collision;
using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class MobilePolygon : MobileObject, ICollidable {
		public static bool ShowCollisionCircles = false;
		protected Polygon polygon;
		protected Circle[] _detailedCollisionCirclesInLocalSpace;// TODO remove these
		private float _angularVelocity;
		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }
		public Polygon Polygon { get => polygon; set => polygon = value; }
		public override float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public override float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public override float Area => polygon.Area;
		public override float Inertia => polygon.InertiaWithoutDensity * Density;
		public Circle[] CollisionCircles { get => _detailedCollisionCirclesInLocalSpace; set => _detailedCollisionCirclesInLocalSpace = value; }
		public override float AngularVelocity { get => _angularVelocity; set => _angularVelocity = value; }

		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
		}
		public override void Draw(CommandLineCanvas canvas) {
			if (!_active) {
				return;
			}
			if (!ShowCollisionCircles || _detailedCollisionCirclesInLocalSpace == null) {
				polygon.Draw(canvas);
			} else {
				BlinkBetweenPolygonAndCollisionCircles(canvas);
			}
		}

		private void BlinkBetweenPolygonAndCollisionCircles(CommandLineCanvas canvas) {
			if (((int)(Time.TimeSeconds*5)) % 2 == 0) {
				polygon.Draw(canvas);
			} else {
				for (int i = 0; i < _detailedCollisionCirclesInLocalSpace.Length; i++) {
					GetCollisionCircle(i).Draw(canvas);
				}
			}
		}

		public Circle GetCollisionCircle(int index) {
			Circle circle = _detailedCollisionCirclesInLocalSpace[index];
			circle.Position = Position + circle.center.RotatedRadians(RotationRadians);
			return circle;
		}
		public Circle GetCollisionBoundingCircle() => Polygon.GetCollisionBoundingCircle();

		public virtual CollisionData IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle mc:
					return MyCollisionWith(collidable, GetCollision(mc.Circle));
				case MobilePolygon mp: return IsColliding(mp);
			}
			return null;
		}

		public CollisionData GetCollision(Circle circle) {
			if (!GetCollisionBoundingCircle().IsColliding(circle)) {
				return null;
			}
			if (polygon.TryGetCircleCollision(circle.Position, circle.radius,
			out Vec2 closestPoint, out Vec2 circleToPointDelta, out float closestDistanceSq)) {
				float depthOfCircleOverlap = circle.radius - MathF.Sqrt(closestDistanceSq);
				Vec2 normal = circleToPointDelta.Normal;
				return new CollisionData(this, null, closestPoint, normal, depthOfCircleOverlap);
			}
			return null;
		}
		private CollisionData GetCollisionInternal(Circle otherCircle) {
			for (int i = 0; i < _detailedCollisionCirclesInLocalSpace.Length; ++i) {
				Circle selfCircle = GetCollisionCircle(i);
				if (otherCircle.IsColliding(selfCircle)) {
					CollisionData data = CollisionData.ForCircles(selfCircle, otherCircle);
					data.ColliderIndexSelf = i;
					data.Self = this;
					return data;
				}
			}
			return null;
		}
		public CollisionData IsColliding(MobilePolygon other) {
			if (!GetCollisionBoundingCircle().IsColliding(other.GetCollisionBoundingCircle())) {
				return null;
			}
			List<Polygon.CollisionData> collisions = null;
			bool isColliding = other.Polygon.TryGetPolyCollision(Polygon, ref collisions);
			if (isColliding) {
				Vec2 point = Vec2.Zero;
				Vec2 normal = Vec2.Zero;
				float depth = 0;
				for (int i = 0; i < collisions.Count; ++i) {
					Polygon.CollisionData data = collisions[i];
					Polygon a = data.objectA;
					Polygon b = data.objectB;
					Circle circleA = a.ConvexHullCircles[data.ObjectAConvexIndex];
					Circle circleB = b.ConvexHullCircles[data.ObjectBConvexIndex];
					Circle.TryGetCircleCollision(circleA, circleB, out Vec2 estimatedCollisionPoint);
					//b.TryGetCircleCollisionConvex(data.ObjectBConvexIndex, circleA.center, 0,
					//	out Vec2 closestPointOnB, out _, out _);
					//a.TryGetCircleCollisionConvex(data.ObjectAConvexIndex, circleB.center, 0,
					//	out Vec2 closestPointOnA, out _, out _);
					//Vec2 estimatedCollisionPoint = (closestPointOnA + closestPointOnB) / 2;
					point += estimatedCollisionPoint;
					depth += data.Depth;
					normal += data.Normal;
				}
				if (collisions.Count > 1) {
					point /= collisions.Count;
					normal /= collisions.Count;
					normal.Normalize();
					depth /= collisions.Count;
				}
				return new CollisionData(this, other, point, normal, depth);
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

using collision;
using ConsoleMrV;
using MathMrV;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace asteroids {
	public class MobilePolygon : MobileObject, ICollidable {
		protected Polygon polygon;
		private float _angularVelocity;
		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }
		public Polygon Polygon { get => polygon; set => polygon = value; }
		public override float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public override float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public override float Area => polygon.Area;
		public override float Inertia => polygon.InertiaWithoutDensity * Density;
		public override float AngularVelocity { get => _angularVelocity; set => _angularVelocity = value; }

		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
		}
		public override void Draw(CommandLineCanvas canvas) {
			if (!_active) {
				return;
			}
			polygon.Draw(canvas);
		}

		public Circle GetCollisionBoundingCircle() => Polygon.GetCollisionBoundingCircle();

		public virtual CollisionData IsColliding(ICollidable collidable) {
			switch (collidable) {
				case MobileCircle mc:
					CollisionData collision = GetCollision(mc.Circle);
					collision?.SetParticipants(this, collidable);
					return collision;
				case MobilePolygon mp: return IsColliding(mp);
			}
			return null;
		}

		public CollisionData GetCollision(Circle circle) {
			if (!GetCollisionBoundingCircle().IsColliding(circle)) {
				return null;
			}
			if (polygon.TryGetCircleCollision(circle.Center, circle.Radius,
			out Vec2 closestPoint, out Vec2 circleToPointDelta, out float closestDistanceSq)) {
				float depthOfCircleOverlap = circle.Radius - MathF.Sqrt(closestDistanceSq);
				Vec2 normal = circleToPointDelta.Normal;
				return new CollisionData(this, null, closestPoint, normal, depthOfCircleOverlap, null);
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
				List<Vec2> contacts = null;
				for (int i = 0; i < collisions.Count; ++i) {
					Polygon.CollisionData data = collisions[i];
					Polygon a = data.objectA;
					Polygon b = data.objectB;
					Circle circleA = a.model.ConvexHullCircles[data.ObjectAConvexIndex];
					Circle circleB = b.model.ConvexHullCircles[data.ObjectBConvexIndex];
					Circle.TryGetCircleCollision(circleA, circleB, out Vec2 estimatedCollisionPoint);
					IList<Vec2> convexContacts = Polygon.GetIntersections(a, data.ObjectAConvexIndex, b, data.ObjectBConvexIndex);
					if (convexContacts != null) {
						if  (contacts == null) {  contacts = new List<Vec2>(); }
						contacts.AddRange(convexContacts);
					}
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
				return new CollisionData(this, other, point, normal, depth, contacts);
			}
			return null;
		}
	}
}

using asteroids;
using MathMrV;
using System;

namespace MrV.Physics {

	public static class InertiaCalculator {
		public static void CalculatePolygonAreaAndInertia(Vec2[] vertices, out float area, out float inertiaWithoutDensity) {
			float totalArea = 0f;
			float inertiaMomentNumeratorSum = 0f;
			for (int i = 0; i < vertices.Length; i++) {
				Vec2 a = vertices[i], b = vertices[(i + 1) % vertices.Length];
				float areaOfParallelogramDefinedByAZeroB = Vec2.Cross(a, b);
				float triangleAreaSigned = 0.5f * areaOfParallelogramDefinedByAZeroB;
				totalArea += triangleAreaSigned; //negative values from concave crossproduct subtracts correctly
				float longnessTerm = Vec2.Dot(a, a) + Vec2.Dot(a, b) + Vec2.Dot(b, b);
				inertiaMomentNumeratorSum += triangleAreaSigned * longnessTerm;
			}
			area = totalArea;
			float secondMomentOfArea = inertiaMomentNumeratorSum / 6.0f;
			inertiaWithoutDensity = Math.Abs(secondMomentOfArea);
		}

		public static void CalculateCircleAreaAndInertia(float radius, out float area, out float inertiaWithoutDensity) {
			area = MathF.PI * radius * radius;
			inertiaWithoutDensity = 0.5f * area * radius * radius;
		}
	}
	public static class Collision {
		public static void SeparateObjects(MobileObject mobA, MobileObject mobB, Vec2 normal, float depth) {
			float massAPercentage = mobA.Mass / (mobA.Mass + mobB.Mass);
			Vec2 bump = normal * depth;
			mobA.Position += bump * massAPercentage;
			mobB.Position -= bump * (1 - massAPercentage);
		}
		public static void BounceVelocities(MobileObject mobA, MobileObject mobB, Vec2 collisionNormal, float bounciness = 0.5f) {
			Vec2 relativeVelocity = mobA.Velocity - mobB.Velocity;
			float velAlongNormal = Vec2.Dot(relativeVelocity, collisionNormal);
			bool velocitiesAreAligned = velAlongNormal > 0;
			if (velocitiesAreAligned) { return; }
			float inverseMassSum = (1f / mobA.Mass) + (1f / mobB.Mass);
			float impulseMagnitude = -(1 + bounciness) * velAlongNormal;
			impulseMagnitude /= inverseMassSum;
			Vec2 impulseVector = collisionNormal * impulseMagnitude;
			mobA.Velocity += impulseVector / mobA.Mass;
			mobB.Velocity += -impulseVector / mobB.Mass;
		}
		public static void BounceVelocitiesAndTorque(MobileObject mobA, MobileObject mobB,
		Vec2 contactPoint, Vec2 collisionNormal, float bounciness = 0.5f) {
			Vec2 contactOffsetA = contactPoint - mobA.Position;
			Vec2 contactOffsetB = contactPoint - mobB.Position;
			Vec2 pointVelocityA = mobA.Velocity + GetTangentialVelocity(contactOffsetA, mobA.AngularVelocity);
			Vec2 pointVelocityB = mobB.Velocity + GetTangentialVelocity(contactOffsetB, mobB.AngularVelocity);
			Vec2 GetTangentialVelocity(Vec2 point, float angularVelocity) {
				return new Vec2(-angularVelocity * point.Y, angularVelocity * point.X);
			}
			Vec2 relativeVelocity = pointVelocityA - pointVelocityB;
			float velAlongNormal = Vec2.Dot(relativeVelocity, collisionNormal);
			bool velocitiesAreAligned = velAlongNormal > 0;
			if (velocitiesAreAligned) { return; }
			float leverageA = Vec2.Cross(contactOffsetA, collisionNormal);
			float leverageB = Vec2.Cross(contactOffsetB, collisionNormal);
			float inverseMassSum = (1f / mobA.Mass) + (1f / mobB.Mass);
			float rotationResistance = (leverageA * leverageA / mobA.Inertia) + (leverageB * leverageB / mobB.Inertia);
			float impulseMagnitude = -(1 + bounciness) * velAlongNormal;
			impulseMagnitude /= (inverseMassSum + rotationResistance);
			Vec2 impulseVector = impulseMagnitude * collisionNormal;
			mobA.Velocity += impulseVector / mobA.Mass;
			mobB.Velocity += -impulseVector / mobB.Mass;
			mobA.AngularVelocity += Vec2.Cross(contactOffsetA, impulseVector) / mobA.Inertia;
			mobB.AngularVelocity += Vec2.Cross(contactOffsetB, -impulseVector) / mobB.Inertia;
		}
	}
}

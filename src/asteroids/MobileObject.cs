using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Numerics;
using ColliderID = System.Byte;

namespace asteroids {
	public abstract class MobileObject : IGameObject {
		protected string _name;
		protected bool _active = true;
		protected Vec2 _velocity;
		protected ConsoleColor _color;
		protected float _density = 1;
		public virtual string Name { get => _name; set => _name = value; }
		public virtual Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public virtual bool IsActive { get => _active; set => _active = value; }
		public virtual bool IsVisible { get => IsActive; set => IsActive = value; }
		public abstract Vec2 Position { get; set; }
		public abstract Vec2 Direction { get; set; }
		public abstract float AngularVelocity { get; set; }
		public abstract float Area { get; }
		public virtual float Density { get => _density; set => _density = value; }
		public virtual float Mass {
			get => Density * Area;
			set => Density = value / Area;
		}
		public virtual float Inertia { get; }
		public virtual float RotationRadians {
			get => Direction.NormalToRadians();
			set { Direction = Vec2.NormalFromRadians(value); }
		}
		public virtual float RotationDegrees {
			get => Direction.NormalToDegrees();
			set { Direction = Vec2.NormalFromDegrees(value); }
		}
		public ConsoleColor Color { get => _color; set => _color = value; }
		public ColliderID TypeId { get; set; }
		public abstract void Draw(CommandLineCanvas canvas);
		public virtual void Update() {
			if (!_active) { return; }
			float t = Time.DeltaTimeSeconds;
			Vec2 moveThisFrame = _velocity * t;
			Position += moveThisFrame;
			if (AngularVelocity != 0) {
				float angleRadians = Direction.NormalToRadians();
				angleRadians += AngularVelocity * t;
				Direction = Vec2.NormalFromRadians(angleRadians);
			}
		}
		public virtual void Copy(MobileObject other) {
			TypeId = other.TypeId;
			IsActive = other.IsActive;
			Velocity = other.Velocity;
			Color = other.Color;
		}

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

		public static void CollisionBounceWithTorque(MobileObject mobA, MobileObject mobB,
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

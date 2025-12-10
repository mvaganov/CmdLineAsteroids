using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using ColliderID = System.Byte;

namespace asteroids {
	public abstract class MobileObject : IGameObject {
		protected string _name;
		protected bool _active = true;
		protected Vec2 _velocity;
		protected ConsoleColor _color;
		public virtual string Name { get => _name; set => _name = value; }
		public virtual Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public virtual bool IsActive { get => _active; set => _active = value; }
		public virtual bool IsVisible { get => IsActive; set => IsActive = value; }
		public abstract Vec2 Position { get; set; }
		public abstract Vec2 Direction { get; set; }
		public abstract float AngularVelocity { get; set; }
		public abstract float Area { get; }
		public virtual float Density { get => 1; set => throw new NotImplementedException(); }
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
			IsActive = other._active;
			Velocity = other._velocity;
			Color = other._color;
		}

		public static void SeparateObjects(MobileObject mobA, MobileObject mobB, Vec2 normal, float depth, float massAPercentage) {
			Vec2 bump = normal * depth;
			mobA.Position += bump * massAPercentage;
			mobB.Position -= bump * (1 - massAPercentage);
		}
		public static void BounceVelocities(MobileObject mobA, MobileObject mobB, float massA, float massB, Vec2 collisionNormal) {
			Vec2 relativeVelocity = mobA.Velocity - mobB.Velocity;
			float velAlongNormal = Vec2.Dot(relativeVelocity, collisionNormal);
			bool velocitiesAreAligned = velAlongNormal > 0;
			if (velocitiesAreAligned) { return; }
			float restitution = 0.8f; // Bounciness (0 = rock, 1 = super ball)
			float invMassA = 1.0f / massA;
			float invMassB = 1.0f / massB;
			float numerator = -(1 + restitution) * velAlongNormal;
			float denominator = invMassA + invMassB;
			float impulseScalar = numerator / denominator;
			Vec2 impulse = collisionNormal * impulseScalar;
			mobA.Velocity += impulse * invMassA;
			mobB.Velocity -= impulse * invMassB;
		}
		public static void CollisionTorque(MobileObject ship, MobileObject asteroid, Vec2 contactPoint,
			float shipMass, float asteroidMass, ref float shipAngularVelocity, ref float asteroidAngularVelocity, Vec2 collisionNormal) {
			// 2. Approximate Point of Contact (for simplicity, using A's center and Normal/Depth)
			// More accurate: find the closest vertex of B to A, or midpoint of deepest edge.
			// For this example, let's use the object centers, which is less accurate but simpler:
			//Vec2 contactPoint = playerCharacter.Polygon + bounceNormal * (collisionAdjustment.Depth / 2f);

			// 3. Calculate Radius Vectors
			Vec2 rA = contactPoint - ship.Position;
			Vec2 rB = contactPoint - asteroid.Position;

			// 4. Calculate relative velocity, including rotation component
			// v_rel = (vB + (wB x rB)) - (vA + (wA x rA))
			// Cross product (w x r) in 2D is: (-w*ry, w*rx)
			Vec2 vA_rot = new Vec2(-shipAngularVelocity * rA.Y, shipAngularVelocity * rA.X);
			Vec2 vB_rot = new Vec2(-asteroidAngularVelocity * rB.Y, asteroidAngularVelocity * rB.X);

			Vec2 relativeVelocity = (asteroid.Velocity + vB_rot) - (ship.Velocity + vA_rot);
			//float velAlongNormal = Vec2.Dot(relativeVelocity, manifold.Normal);

			float velAlongNormal = Vec2.Dot(relativeVelocity, collisionNormal);
			// Only resolve if closing
			if (velAlongNormal >= 0) return;

			// 5. Calculate 2D Cross Products (r x n)
			// This is the scalar component of torque
			float rACrossN = (rA.X * collisionNormal.y) - (rA.Y * collisionNormal.x);
			float rBCrossN = (rB.X * collisionNormal.y) - (rB.Y * collisionNormal.x);

			float restitution = 0.8f; // Bounciness (0 = rock, 1 = super ball)
			float numerator = -(1f + restitution) * velAlongNormal;
			// 6. Calculate the Full Impulse Denominator
			// TODO
			//float shipInertia = 1;
			//float asteroidInertia = 1;

			float denominator =
					(1f / shipMass) + (1f / asteroidMass) +
					(rACrossN * rACrossN / ship.Inertia) +
					(rBCrossN * rBCrossN / asteroid.Inertia);

			// 7. Calculate the final scalar impulse magnitude (j)
			float j = numerator / denominator;

			// 8. Apply Linear and Angular Impulse
			//Vec2 impulse = collisionNormal * j;

			//// Linear Application
			//ship.Velocity -= impulse * (1f / shipMass);
			//asteroid.Velocity += impulse * (1f / asteroidMass);

			// Angular Application (This creates the spin!)
			shipAngularVelocity += (rACrossN * j) / ship.Inertia;
			asteroidAngularVelocity -= (rBCrossN * j) / asteroid.Inertia;
		}
	}
}

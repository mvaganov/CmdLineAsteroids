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

	}
}

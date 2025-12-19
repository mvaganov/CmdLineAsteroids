using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class Particle : IGameObject {
		public string _name;
		public Circle circle;
		public bool Disabled;
		private Vec2 _velocity;
		private ConsoleColor _color;
		public float CurrentLife;
		public float Lifetime;
		public float StartSize;
		public ParticleSystem ParticleSystem;
		public virtual string Name { get => _name; set => _name = value; }
		public Vec2 Position { get => circle.Center; set => circle.Center = value; }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }
		public Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public bool IsActive { get => !Disabled; set => Disabled = !value; }
		public bool IsVisible { get => IsActive; set => IsActive = value; }
		public ConsoleColor Color { get => _color; set => _color = value; }
		public byte TypeId { get; set; }
		public Particle(ParticleSystem parent, Vec2 position, float size, Vec2 velocity, ConsoleColor color, float lifetime) {
			ParticleSystem = parent;
			StartSize = size;
			circle = new Circle(position, size);
			Velocity = velocity;
			Color = color;
			Lifetime = lifetime;
			CurrentLife = 0;
			Disabled = false;
			TypeId = 0;
		}
		public void Draw(CommandLineCanvas canvas) {
			canvas.SetColor(Color);
			circle.Draw(canvas);
		}
		public void Update() {
			if (Disabled) {
				return;
			}
			if (CurrentLife >= Lifetime) {
				ParticleSystem.Absorb(this);
				return;
			}
			CurrentLife += Time.DeltaTimeSeconds;
			float normalizedLife = CurrentLife / Lifetime;
			Radius = StartSize * ParticleSystem.GetSizeAtTime(normalizedLife);
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			Position += moveThisFrame;
		}
	}
}

using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class Particle : IGameObject {
		public Circle circle;
		public bool Disabled;
		private Vec2 _velocity;
		private ConsoleColor _color;
		public float CurrentLife;
		public float Lifetime;
		public float StartSize;
		public ParticleSystem _particleSystem;
		public Vec2 Position { get => circle.position; set => circle.position = value; }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }
		public Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public bool IsActive { get => !Disabled; set => Disabled = !value; }
		public bool IsVisible { get => IsActive; set => IsActive = value; }
		public ConsoleColor Color { get => _color; set => _color = value; }
		public Action<CommandLineGraphicsContext> DrawSetup { get => null; set { } }
		public byte TypeId { get; set; }
		public Particle(ParticleSystem parent, Vec2 position, float size, Vec2 velocity, ConsoleColor color, float lifetime) {
			_particleSystem = parent;
			StartSize = size;
			circle = new Circle(position, size);
			_velocity = velocity;
			_color = color;
			Lifetime = lifetime;
			CurrentLife = 0;
			Disabled = false;
			TypeId = 0;
		}
		public void Draw(CommandLineGraphicsContext graphicsContext) {
			graphicsContext.SetColor(Color);
			circle.Draw(graphicsContext);
		}
		public void Update() {
			if (Disabled) {
				return;
			}
			if (CurrentLife >= Lifetime) {
				_particleSystem.Absorb(this);
				return;
			}
			CurrentLife += Time.DeltaTimeSeconds;
			float normalizedLife = CurrentLife / Lifetime;
			Radius = StartSize * _particleSystem.GetSizeAtTime(normalizedLife);
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			Position += moveThisFrame;
		}
	}
}

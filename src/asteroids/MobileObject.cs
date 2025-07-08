using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public abstract class MobileObject : IGameObject {
		public virtual Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public virtual bool IsActive { get => _active; set => _active = value; }
		public abstract Vec2 Position { get; set; }
		public abstract Vec2 Direction { get; set; }
		public Action<CommandLineGraphicsContext> DrawSetup { get => _preDraw; set => _preDraw = value; }
		public byte TypeId { get; set; }
		protected bool _active = true;
		protected Vec2 _velocity;
		protected Action<CommandLineGraphicsContext> _preDraw;
		public abstract void Draw(CommandLineGraphicsContext graphicsContext);
		public virtual void Update() {
			if (!_active) {
				return;
			}
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			float dx = _velocity.x * Time.DeltaTimeSeconds;
			float dy = _velocity.y * Time.DeltaTimeSeconds;

			moveThisFrame = new Vec2(dx, dy);
			Position += moveThisFrame;
		}
	}
}

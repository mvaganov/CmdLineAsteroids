using ConsoleMrV;
using MathMrV;
using System;

namespace asteroids {
	public interface IGameObject {
		public Vec2 Direction { get; set; }
		public Vec2 Position { get; set; }
		public bool IsActive { get; set; }
		public Action<CommandLineGraphicsContext> DrawSetup { get; set; }
		public void Draw(CommandLineGraphicsContext graphicsContext);
		public void Update();
	}
}

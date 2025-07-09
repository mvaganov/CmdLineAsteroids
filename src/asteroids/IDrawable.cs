using ConsoleMrV;
using System;

namespace asteroids.src.asteroids {
	public interface IDrawable {
		public bool IsVisible { get; set; }
		public void Draw(CommandLineGraphicsContext graphicsContext);
		public Action<CommandLineGraphicsContext> DrawSetup { get; set; }
	}
}

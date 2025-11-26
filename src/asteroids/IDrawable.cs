using ConsoleMrV;
using System;

namespace asteroids.src.asteroids {
	public interface IDrawable {
		public bool IsVisible { get; set; }
		public void Draw(CommandLineCanvas canvas);
		public ConsoleColor Color { get; set; }
	}
}

using MathMrV;
using System;

namespace MrV {
	public static partial class Algorithms {
		public static void DrawRectangle(char letterToPrint, Vec2 position, Vec2 size) {
			for (int row = 0; row < size.Y; ++row) {
				Console.SetCursorPosition((int)position.X, (int)position.Y + row);
				for (int col = 0; col < size.X; ++col) {
					Console.Write(letterToPrint);
				}
			}
		}

		public static void ClearConsole() => DrawRectangle(' ', (0, 0), (Console.BufferWidth, 30));
	}
}

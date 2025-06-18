using System;

namespace asteroids {
	public struct ConsoleGlyph {
		public char letter;
		public ConsoleColor fore, back;
		public ConsoleGlyph(char letter, ConsoleColor fore, ConsoleColor back) {
			this.letter = letter; this.fore = fore; this.back = back;
		}
		public static ConsoleGlyph Default = new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black);
		public static ConsoleGlyph Empty = new ConsoleGlyph('\0', ConsoleColor.Black, ConsoleColor.Black);
		public static bool operator ==(ConsoleGlyph a, ConsoleGlyph b) {
			return a.letter == b.letter && a.fore == b.fore && a.back == b.back;
		}
		public static bool operator !=(ConsoleGlyph a, ConsoleGlyph b) {
			return a.letter != b.letter || a.fore != b.fore || a.back != b.back;
		}
		public override bool Equals(object obj) => obj is ConsoleGlyph g && this == g;
		public override int GetHashCode() => (int)letter | ((int)fore << 8) | ((int)back << 16);
		public override string ToString() {
			return $"{letter}";
		}
		public void ApplyColor() {
			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
	}
}

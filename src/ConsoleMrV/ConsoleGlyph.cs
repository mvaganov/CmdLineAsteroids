using System;

namespace ConsoleMrV {
	public struct ConsoleColorPair {
		public ConsoleColor fore, back;
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			this.fore = fore;
			this.back = back;
		}
		public void Apply() {
			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
		public ConsoleColorPair Invert() => new ConsoleColorPair(back, fore);
		public static ConsoleColorPair Default = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.Black);
		public static ConsoleColorPair Current => new ConsoleColorPair(Console.ForegroundColor, Console.BackgroundColor);
		public static ConsoleColorPair operator+(ConsoleColorPair pair, ConsoleColor color) => new ConsoleColorPair(color, pair.back);
		public static ConsoleColorPair operator-(ConsoleColorPair pair, ConsoleColor color) => new ConsoleColorPair(pair.fore, color);
		static ConsoleColorPair() {
			Default = Current;
		}
	}
	public struct ConsoleGlyph {
		private char letter;
		private ConsoleColorPair colorPair;
		public char Letter {
			get { return letter; }
			set {
				letter = value;
				if (letter != '\n' && (letter < 32 || letter > 127)) {
					throw new Exception("out of ascii range");
				}
			}
		}
		public ConsoleColor fore { get { return colorPair.fore; } set { colorPair.fore = value; } }
		public ConsoleColor back { get { return colorPair.back; } set { colorPair.back = value; } }
		public ConsoleGlyph(char letter, ConsoleColorPair colorPair) {
			this.letter = letter; this.colorPair = colorPair;
		}
		public ConsoleGlyph(char letter, ConsoleColor fore, ConsoleColor back) {
			this.letter = letter; this.colorPair = new ConsoleColorPair(fore, back);
		}
		public static ConsoleGlyph Default => new ConsoleGlyph(' ', ConsoleColorPair.Default);
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
			colorPair.Apply();
		}
		public static ConsoleGlyph[] Convert(string text,
			ConsoleColor fore = ConsoleColor.Gray, ConsoleColor back = ConsoleColor.Black) {
			ConsoleGlyph[] result = new ConsoleGlyph[text.Length];
			for (int i = 0; i < result.Length; i++) {
				ConsoleGlyph g = new ConsoleGlyph(text[i], fore, back);
				result[i] = g;
			}
			return result;
		}
	}
}

//#define ConsoleColorPair_nybbles
#define ConsoleColorPair_bytes
using System;

namespace ConsoleMrV {
	public struct ConsoleColorPair {
#if ConsoleColorPair_nybbles
		private byte data;
		private static byte CompileByte(byte hiNybble, byte loNybble)
			=> (byte)((loNybble & 0x0f) | ((hiNybble & 0x0f) << 4));
		private static byte GetHiNybble(byte data) => (byte)((data & 0xf0) >> 4);
		private static byte GetLoNybble(byte data) => (byte)((data & 0x0f));
		public ConsoleColor fore {
			get => (ConsoleColor)GetLoNybble(data);
			set => CompileByte((byte)back, (byte)value);
		}
		public ConsoleColor back {
			get => (ConsoleColor)GetHiNybble(data);
			set => CompileByte((byte)value, (byte)fore);
		}
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			data = CompileByte((byte)back, (byte)fore);
		}
#elif ConsoleColorPair_bytes
		private byte _fore, _back;
		public ConsoleColor fore {
			get => (ConsoleColor)_fore;
			set => _fore = (byte)value;
		}
		public ConsoleColor back {
			get => (ConsoleColor)_back;
			set => _back = (byte)value;
		}
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			_back = (byte)back;
			_fore = (byte)fore;
		}
#else
		ConsoleColor fore;
		ConsoleColor back;
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			this.back = back;
			this.fore = fore;
		}
#endif
		public void Apply() {
			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
		public ConsoleColorPair Invert() => new ConsoleColorPair(back, fore);
		public static ConsoleColorPair Default = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.Black);
		public static ConsoleColorPair Current => new ConsoleColorPair(Console.ForegroundColor, Console.BackgroundColor);
		public static ConsoleColorPair operator +(ConsoleColorPair pair, ConsoleColor color) => new ConsoleColorPair(color, pair.back);
		public static ConsoleColorPair operator -(ConsoleColorPair pair, ConsoleColor color) => new ConsoleColorPair(pair.fore, color);
		static ConsoleColorPair() {
			Default = Current;
		}
	}
}

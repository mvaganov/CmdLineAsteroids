//#define ConsoleColorPair_nybbles
#define ConsoleColorPair_bytes
using System;

namespace ConsoleMrV {
#if ConsoleColorPair_nybbles
	/// <summary>
	/// use 8 bits for entire structure.
	/// </summary>
	public struct ConsoleColorPair {
		private byte data;
		private static byte MakeByte(byte hiNybble, byte loNybble)
			=> (byte)((loNybble & 0x0f) | ((hiNybble & 0x0f) << 4));
		private static byte HiNybble(byte data) => (byte)((data & 0xf0) >> 4);
		private static byte LoNybble(byte data) => (byte)((data & 0x0f));
		public ConsoleColor fore { get => (ConsoleColor)LoNybble(data); set => MakeByte((byte)back, (byte)value); }
		public ConsoleColor back { get => (ConsoleColor)HiNybble(data); set => MakeByte((byte)value, (byte)fore); }
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			data = MakeByte((byte)back, (byte)fore);
		}
#elif ConsoleColorPair_bytes
	/// <summary>
	/// use 16 bits for entire structure.
	/// potentially useful if TTY console supports 256 colors.
	/// faster than using bitwise operations
	/// </summary>
	public struct ConsoleColorPair {
		private byte _fore, _back;
		public ConsoleColor fore { get => (ConsoleColor)_fore; set => _fore = (byte)value; }
		public ConsoleColor back { get => (ConsoleColor)_back; set => _back = (byte)value; }
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			_back = (byte)back;
			_fore = (byte)fore;
		}
#else
	/// <summary>
	/// use default enum size for each color component, likely 64 bits total
	/// </summary>
	public struct ConsoleColorPair {
		ConsoleColor fore, back;
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			this.back = back;
			this.fore = fore;
		}
#endif
		public void Apply() {
			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
		public static ConsoleColorPair Default = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.Black);
		public static ConsoleColorPair Current => new ConsoleColorPair(Console.ForegroundColor, Console.BackgroundColor);
		public ConsoleColorPair WithFore(ConsoleColor color) => new ConsoleColorPair(color, back);
		public ConsoleColorPair Invert() => new ConsoleColorPair(back, fore);
		static ConsoleColorPair() {
			Default = Current;
		}
	}
}

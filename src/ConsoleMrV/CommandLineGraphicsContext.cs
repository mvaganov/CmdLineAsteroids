#define IN_COLOR
using MathMrV;
using System;

namespace ConsoleMrV {
#if IN_COLOR
	public class CommandLineGraphicsContext {
		protected ConsoleGlyph[,] _currentBuffer;
		protected ConsoleGlyph[,] _previousBuffer;
		public int Width => _currentBuffer.GetLength(0);
		public int Height => _currentBuffer.GetLength(1);
		public Vec2 Size => new Vec2(Width, Height);
		public ConsoleGlyph this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CommandLineGraphicsContext(int width, int height) {
			SetSize(width, height);
		}
		public void SetSize(int width, int height) {
			ResizeBuffer(ref _previousBuffer, width, height);
			ResizeBuffer(ref _currentBuffer, width, height);
		}
		private static void ResizeBuffer(ref ConsoleGlyph[,] buffer, int width, int height) {
			ConsoleGlyph[,] oldBuffer = buffer;
			buffer = new ConsoleGlyph[width, height];
			if (oldBuffer == null) {
				return;
			}
			int oldW = oldBuffer.GetLength(0);
			int oldH = oldBuffer.GetLength(1);
			int rowsToCopy = Math.Min(height, oldH);
			int colsToCopy = Math.Min(width, oldW);
			for (int x = 0; x < colsToCopy; ++x) {
				for (int y = 0; y < rowsToCopy; ++y) {
					buffer[x, y] = oldBuffer[x, y];
				}
			}
		}

		public Vec2 WriteAt(string text, int col, int row) => WriteAt(ConsoleGlyph.Convert(text), col, row);
		public Vec2 WriteAt(ConsoleGlyph[] text, int col, int row) => WriteAt(text, col, row, false);
		public Vec2 WriteAt(ConsoleGlyph[] text, int col, int row, bool useNewBgColor) {
			for (int i = 0; i < text.Length; i++) {
				ConsoleGlyph g = text[i];
				switch (g.Letter) {
					case '\n': col = 0; ++row; break;
					default: WriteAt(g, col, row, useNewBgColor); ++col; break;
				}
			}
			return new Vec2(col, row);
		}
		public void WriteAt(ConsoleGlyph glyph, int col, int row) => WriteAt(glyph, col, row, true);
		public void WriteAt(ConsoleGlyph glyph, int col, int row, bool useNewBgColor) {
			if (!IsValidCoordinate(col, row)) {
				return;
			}
			if (!useNewBgColor) {
				glyph.back = _currentBuffer[col, row].back;
			}
			_currentBuffer[col, row] = glyph;
		}
		bool IsValidCoordinate(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
		public static void PrintBuffer(ConsoleGlyph[,] buffer) {
			int height = buffer.GetLength(0), width = buffer.GetLength(1);
			for (int row = 0; row < height; ++row) {
				Console.SetCursorPosition(0, row);
				for (int col = 0; col < width; ++col) {
					ConsoleGlyph g = buffer[row, col];
					g.ApplyColor();
					Console.Write(g);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
		public virtual void PrintUnoptimized() => PrintBuffer(_currentBuffer);
		public virtual void PrintModifiedCharactersOnly() {
			bool cursorInCorrectPlace;
			ConsoleColorPair oldColors = ConsoleColorPair.Current;
			for (int row = 0; row < Height; ++row) {
				cursorInCorrectPlace = false;
				for (int col = 0; col < Width; ++col) {
					if (_currentBuffer[col, row] != _previousBuffer[col, row]) {
						if (col < 0 || row < 0 || col >= Console.BufferWidth || row >= Console.BufferHeight) {
							cursorInCorrectPlace = false;
						} else {
							if (!cursorInCorrectPlace) {
								Console.SetCursorPosition(col, row);
							}
							ConsoleGlyph g = _currentBuffer[col, row];
							g.ApplyColor();
							Console.Write(g);
							cursorInCorrectPlace = true;
						}
					} else {
						cursorInCorrectPlace = false;
					}
				}
			}
			oldColors.Apply();
		}
		public void FinishedRender() {
			SwapBuffers();
			Clear();
		}
		public void SwapBuffers() {
			ConsoleGlyph[,] swap = _currentBuffer;
			_currentBuffer = _previousBuffer;
			_previousBuffer = swap;
		}
		/// <summary>
		/// make canvas 'clear', all <see cref="ConsoleGlyph.Default"/>
		/// </summary>
		public void Clear() => Clear(_currentBuffer, ConsoleGlyph.Default);
		public void Clear(ConsoleGlyph[,] buffer, ConsoleGlyph background) {
			for (int row = 0; row < Height; ++row) {
				for (int col = 0; col < Width; ++col) {
					buffer[col, row] = background;
				}
			}
		}
		/// <summary>
		/// set all canvas glyphs to invalid, to force total redraw
		/// </summary>
		public void SetDirty() => Clear(_previousBuffer, ConsoleGlyph.Empty);
	}
#else
	public class CommandLineGraphicsContext {
		protected char[,] _currentBuffer;
		protected char[,] _previousBuffer;
		public int Width => _currentBuffer.GetLength(0);
		public int Height => _currentBuffer.GetLength(1);
		public Vec2 Size => new Vec2(Width, Height);
		public char this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CommandLineGraphicsContext(int width, int height) {
			SetSize(width, height);
		}
		public void SetSize(int width, int height) {
			_previousBuffer = new char[width, height];
			_currentBuffer = new char[width, height];
		}
		public Vec2 WriteAt(string text, int col, int row) {
			for (int i = 0; i < text.Length; i++) {
				char letter = text[i];
				switch (letter) {
					case '\n': col = 0; ++row; break;
					default: WriteAt(letter, col, row); ++col; break;
				}
			}
			return new Vec2(col, row);
		}
		public void WriteAt(char glyph, int col, int row) {
			if (!IsValidCoordinate(col, row)) {
				return;
			}
			_currentBuffer[col, row] = glyph;
		}
		bool IsValidCoordinate(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
		public static void PrintBuffer(char[,] buffer) {
			int height = buffer.GetLength(0), width = buffer.GetLength(1);
			for (int row = 0; row < height; ++row) {
				Console.SetCursorPosition(0, row);
				for (int col = 0; col < width; ++col) {
					Console.Write(buffer[row, col]);
				}
			}
		}
		public virtual void PrintUnoptimized() => PrintBuffer(_currentBuffer);
		public virtual void PrintModifiedCharactersOnly() {
			bool cursorInCorrectPlace;
			for (int row = 0; row < Height; ++row) {
				cursorInCorrectPlace = false;
				for (int col = 0; col < Width; ++col) {
					if (_currentBuffer[col, row] != _previousBuffer[col, row]) {
						if (col < 0 || row < 0 || col >= Console.BufferWidth || row >= Console.BufferHeight) {
							cursorInCorrectPlace = false;
						} else {
							if (!cursorInCorrectPlace) {
								Console.SetCursorPosition(col, row);
							}
							Console.Write(_currentBuffer[col, row]);
							cursorInCorrectPlace = true;
						}
					} else {
						cursorInCorrectPlace = false;
					}
				}
			}
		}
		public void FinishedRender() {
			SwapBuffers();
			Clear();
		}
		public void SwapBuffers() {
			char[,] swap = _currentBuffer;
			_currentBuffer = _previousBuffer;
			_previousBuffer = swap;
		}
		public void Clear() => Clear(_currentBuffer, ' ');
		public void Clear(char[,] buffer, char background) {
			for (int row = 0; row < Height; ++row) {
				for (int col = 0; col < Width; ++col) {
					buffer[col, row] = background;
				}
			}
		}
	}
#endif
}

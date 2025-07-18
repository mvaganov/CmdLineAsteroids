using MathMrV;
using System;
using System.Drawing;

namespace ConsoleMrV {
	/// <summary>
	/// double-buffered 2D array for drawing to the command line
	/// </summary>
	public class CommandLineGraphicsContext {
		protected ConsoleGlyph[,] _currentBuffer;
		protected ConsoleGlyph[,] _previousBuffer;
		protected int _printOffsetRow;
		protected int _printOffsetCol;
		public int Width;
		public int Height;

		public Vec2 PrintOffset {
			get => (_printOffsetCol, _printOffsetRow);
			set { _printOffsetRow = (int)value.y; _printOffsetCol = (int)value.x; }
		}
		public Vec2 Size => new Point(Width, Height);

		public ConsoleGlyph this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}

		public CommandLineGraphicsContext(int width, int height, int offsetCol, int offsetRow) {
			SetSize(width, height);
			_printOffsetRow = offsetRow;
			_printOffsetCol = offsetCol;
		}
		public void SetSize(int width, int height) {
			ResizeBuffer(ref _previousBuffer, width, height);
			ResizeBuffer(ref _currentBuffer, width, height);
			Width = width;
			Height = height;
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

		public Point WriteAt(string text, int col, int row) => WriteAt(ConsoleGlyph.Convert(text), col, row);
		public Point WriteAt(ConsoleGlyph[] text, int col, int row) => WriteAt(text, col, row, false);
		public Point WriteAt(ConsoleGlyph[] text, int col, int row, bool alsoUseBackground) {
			for (int i = 0; i < text.Length; i++) {
				ConsoleGlyph g = text[i];
				switch (g.Letter) {
					case '\n': col = 0; ++row; break;
					default: WriteAt(g, col, row, alsoUseBackground); ++col; break;
				}
			}
			return new Point(col, row);
		}
		public void WriteAt(ConsoleGlyph glyph, int col, int row) => WriteAt(glyph, col, row, true);
		public void WriteAt(ConsoleGlyph glyph, int col, int row, bool alsoUseGlyphBackground) {
			if (!IsValidCoordinate(col, row)) {
				return;
			}
			if (!alsoUseGlyphBackground) {
				glyph.back = _currentBuffer[col, row].back;
			}
			_currentBuffer[col, row] = glyph;
		}
		bool IsValidCoordinate(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
		public static void PrintBuffer(ConsoleGlyph[,] buffer, int minX, int minY) {
			int height = buffer.GetLength(0), width = buffer.GetLength(1);
			for (int row = 0; row < height; ++row) {
				Console.SetCursorPosition(minX, minY + row);
				for (int col = 0; col < width; ++col) {
					ConsoleGlyph g = buffer[row, col];
					g.ApplyColor();
					Console.Write(g);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
		public virtual void PrintUnoptimized() => PrintBuffer(_currentBuffer, _printOffsetCol, _printOffsetRow);
		public virtual void PrintModifiedCharactersOnly() {
			bool cursorInCorrectPlace;
			int x, y;
			ConsoleColorPair oldColors = ConsoleColorPair.Current;
			for (int row = 0; row < Height; ++row) {
				cursorInCorrectPlace = false;
				for (int col = 0; col < Width; ++col) {
					if (_currentBuffer[col, row] != _previousBuffer[col, row]) {
						x = col + _printOffsetCol;
						y = row + _printOffsetRow;
						if (x < 0 || y < 0 || x >= Console.BufferWidth || y >= Console.BufferHeight) {
							cursorInCorrectPlace = false;
						} else {
							if (!cursorInCorrectPlace) {
								Console.SetCursorPosition(x, y);
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
}

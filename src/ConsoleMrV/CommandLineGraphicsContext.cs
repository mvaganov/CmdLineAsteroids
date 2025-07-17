using MathMrV;
using System;
using System.Drawing;

namespace ConsoleMrV {
	public class CommandLineGraphicsContext {
		protected ConsoleGlyph[,] _currentBuffer;
		protected ConsoleGlyph[,] _previousBuffer;
		private Vec2 _printOffset;
		public int Width;
		public int Height;

		public Vec2 PrintOffset { get => _printOffset; set => _printOffset = value; }
		public Vec2 Size => new Point(Width, Height);



		public void SetBufferCharacter(Vec2 staticBufferPosition, ConsoleGlyph value) {
			int x = (int)staticBufferPosition.x, y = (int)staticBufferPosition.y;
			if (x < 0 || y < 0 || x >= Size.X || y >= Size.Y) {
				return;
			}
			_currentBuffer[x, y] = value;
		}

		public Point WriteAt(ConsoleGlyph[] text, int row, int col) => WriteAt(text, row, col, false);
		public Point WriteAt(ConsoleGlyph[] text, int row, int col, bool alsoUseBackground) {
			for (int i = 0; i < text.Length; i++) {
				ConsoleGlyph g = text[i];
				switch (g.Letter) {
					case '\n':
						col = 0;
						++row;
						break;
					default:
						if (IsValidCoordinate(col, row)) {
							if (!alsoUseBackground) {
								g.back = _currentBuffer[col, row].back;
							}
							_currentBuffer[col, row] = g;
						}
						++col;
						break;
				}
			}
			return new Point(col, row);
		}

		public Point WriteAt(string text, int row, int col) => WriteAt(ConsoleGlyph.Convert(text), row, col);

		bool IsValidCoordinate(int x, int y) {
			return x >= 0 && x < Width && y >= 0 && y < Height;
		}


		public ConsoleGlyph this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CommandLineGraphicsContext(int width, int height, Vec2 offset) {
			SetSize(width, height);
			_printOffset = offset;
		}
		public void SetSize(int width, int height) {
			ResizeBuffer(ref _previousBuffer, width, height);
			ResizeBuffer(ref _currentBuffer, width, height);
			Width = width;
			Height = height;
		}
		private static void ResizeBuffer(ref ConsoleGlyph[,] buffer, int width, int height) {
			int oldW = buffer.GetLength(0);
			int oldH = buffer.GetLength(1);
			ConsoleGlyph[,] oldBuffer = buffer;
			buffer = new ConsoleGlyph[width, height];
			if (oldBuffer != null) {
				int rowsToCopy = Math.Min(height, oldH);
				int colsToCopy = Math.Min(width, oldW);
				for (int x = 0; x < colsToCopy; ++x) {
					for (int y = 0; y < rowsToCopy; ++y) {
						buffer[x, y] = oldBuffer[x, y];
					}
				}
			}
		}
		public void Clear(ConsoleGlyph[,] buffer, ConsoleGlyph background) {
			for (int row = 0; row < Height; ++row) {
				for (int col = 0; col < Width; ++col) {
					buffer[col, row] = background;
				}
			}
		}
		public void Clear() {
			Clear(_currentBuffer, ConsoleGlyph.Default);
		}
		public void Print(char[,] buffer, int minX, int minY) {
			for (int row = 0; row < Height; ++row) {
				Console.SetCursorPosition(minX, minY + row);
				for (int col = 0; col < Width; ++col) {
					ConsoleGlyph g = _currentBuffer[col, row];
					g.ApplyColor();
					Console.Write(g);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
		public void SetDirty() {
			for (int x = 0; x < Width; ++x) {
				for (int y = 0; y < Height; ++y) {
					_previousBuffer[x, y] = ConsoleGlyph.Empty;
				}
			}
		}

		public virtual void PrintUnoptimized() {
			for (int row = 0; row < Height; ++row) {
				Console.SetCursorPosition((int)_printOffset.X, (int)_printOffset.Y + row);
				for (int col = 0; col < Width; ++col) {
					ConsoleGlyph g = _currentBuffer[col, row];
					g.ApplyColor();
					Console.Write(g);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
		public virtual void PrintModifiedCharactersOnly() {
			bool cursorInCorrectPlace;
			int x, y;
			ConsoleColorPair oldColors = ConsoleColorPair.Current;
			for (int row = 0; row < Height; ++row) {
				cursorInCorrectPlace = false;
				for (int col = 0; col < Width; ++col) {
					if (_currentBuffer[col, row] != _previousBuffer[col, row]) {
						x = col + (int)_printOffset.X;
						y = row + (int)_printOffset.Y;
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

		public void SwapBuffers() {
			ConsoleGlyph[,] swap = _currentBuffer;
			_currentBuffer = _previousBuffer;
			_previousBuffer = swap;
		}

		public void FinishedRender() {
			SwapBuffers();
			Clear(_currentBuffer, ConsoleGlyph.Default);
		}
	}
}

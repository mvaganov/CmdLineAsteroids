using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace asteroids {
	public class CmdLineBufferGraphicsContext {
		/// <summary>
		/// X, Y
		/// </summary>
		private char[,] _currentBuffer;
		private char[,] _previousBuffer;
		private Vec2 _scale;
		private Vec2 _offset;
		private Vec2 _printOffset;
		private Vec2 _pivot; // for zoom (and rotation?) TODO
		public char[] valueForSamplesFound;

		public int Width;
		public int Height;

		public Vec2 Scale { get => _scale; set => _scale = value; }
		public Point Size => new Point(Width, Height);
		public Vec2 PrintOffset { get => _printOffset; set => _printOffset = value; }
		public Vec2 Offset { get => _offset; set => _offset = value; }
		public char this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CmdLineBufferGraphicsContext(int width, int height, Vec2 scale, Vec2 offset, char[] valueForSamplesFound) {
			SetSize(width, height);
			_scale = scale;
			_printOffset = offset;
			this.valueForSamplesFound = valueForSamplesFound;
		}
		public void SetSize(int width, int height) {
			SetSize(ref _previousBuffer, width, height);
			SetSize(ref _currentBuffer, width, height);
		}
		private void SetSize(ref char[,] buffer, int width, int height) {
			int oldW = Width;
			int oldH = Height;
			char[,] oldBuffer = buffer;
			Width = width;
			Height = height;
			buffer = new char[Width, Height];
			if (oldBuffer != null) {
				for (int x = 0; x < Width && x < oldW; ++x) {
					for (int y = 0; y < Height && y < oldH; ++y) {
						buffer[x, y] = oldBuffer[x, y];
					}
				}
			}
		}
		public void Clear(char[,] buffer, char background) {
			for (int row = 0; row < Height; ++row) {
				for (int col = 0; col < Width; ++col) {
					buffer[col, row] = background;
				}
			}
		}
		public void Print(char[,] buffer, int minX, int minY) {
			for (int row = 0; row < Height; ++row) {
				Console.SetCursorPosition(minX, minY + row);
				for (int col = 0; col < Width; ++col) {
					Console.Write(buffer[col, row]);
				}
			}
		}
		public void PrintUnoptimized() {
			for (int row = 0; row < Height; ++row) {
				Console.SetCursorPosition((int)_printOffset.X, (int)_printOffset.Y + row);
				for (int col = 0; col < Width; ++col) {
					Console.Write(_currentBuffer[col, row]);
				}
			}
		}
		public void SetDirty() {
			for (int x = 0; x < Width; ++x) {
				for (int y = 0; y < Height; ++y) {
					_previousBuffer[x, y] = (char)0;
				}
			}
		}

		public void PrintModifiedCharactersOnly() {
			bool cursorInCorrectPlace;
			for (int row = 0; row < Height; ++row) {
				cursorInCorrectPlace = false;
				for (int col = 0; col < Width; ++col) {
					if (_currentBuffer[col, row] != _previousBuffer[col, row]) {
						int x = col + (int)_printOffset.X;
						int y = row + (int)_printOffset.Y;
						if (x < 0 || y < 0 || x >= Console.BufferWidth || y >= Console.BufferHeight) {
							cursorInCorrectPlace = false;
						} else {
							if (!cursorInCorrectPlace) {
								Console.SetCursorPosition(x, y);
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
		public void SwapBuffers() {
			char[,] swap = _currentBuffer;
			_currentBuffer = _previousBuffer;
			_previousBuffer = swap;
		}
		public void FinishedRender() {
			SwapBuffers();
			Clear(_currentBuffer, '.');
		}

		public void DrawSupersampledShape(Func<Vec2, bool> isInsideShape, Vec2 min, Vec2 max) {
			min -= _offset;
			max -= _offset;
			min.InverseScale(_scale);
			max.InverseScale(_scale);
			min.Floor();
			max.Ceil();
			Vec2 coord = min;
			for (; coord.Y < max.Y; coord.Y += 1) {
				coord.X = min.X;
				for (; coord.X < max.X; coord.X += 1) {
					if (coord.X < 0 || coord.Y < 0) { continue; }
					Vec2 supersample = Vec2.Zero;
					const int SUPERSAMPLE = 2;
					int samplesFound = 0;
					for (int row = 0; row < SUPERSAMPLE; ++row) {
						supersample.X = 0;
						for (int col = 0; col < SUPERSAMPLE; ++col) {
							Vec2 point = ((coord.X + supersample.X) * _scale.X, (coord.Y + supersample.Y) * _scale.Y);
							point += _offset;
							if (isInsideShape.Invoke(point)) {
								samplesFound++;
							}
							supersample.X += 1f / SUPERSAMPLE;
						}
						supersample.Y += 1f / SUPERSAMPLE;
					}
					if (samplesFound > 0) {
						int x = (int)(coord.X);
						int y = (int)(coord.Y);
						if (x >= 0 && y >= 0 && x < Width && y < Height) {
							this[x, y] = valueForSamplesFound[samplesFound];
						}
					}
				}
			}
		}
	}

}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace asteroids {
	public class CmdLineBufferGraphicsContext {
		/// <summary>
		/// X, Y
		/// </summary>
		private char[,] _currentBuffer;
		private char[,] _previousBuffer;
		private Vec2 _scale;
		private Vec2 _originOffsetULCorner;
		private Vec2 _printOffset;
		private Vec2 _pivotAsPercentage; // percentage, for zoom (and rotation?) TODO
		public char[] valueForSamplesFound;

		public int Width;
		public int Height;

		void DebugPivot() {
			Console.SetCursorPosition(10, 25);
			Vec2 currentPivotOffset = new Vec2(_pivotAsPercentage.X * Size.X * _scale.X, _pivotAsPercentage.Y * Size.Y * _scale.Y);
			Vec2 pivotAbsolute = _originOffsetULCorner + currentPivotOffset;
			Vec2 nextScale = _scale / 1.5f;
			Vec2 nextPivotOffset = new Vec2(_pivotAsPercentage.X * Size.X * nextScale.X, _pivotAsPercentage.Y * Size.Y * nextScale.Y);
			Vec2 nextOriginOffset = pivotAbsolute - nextPivotOffset;
			Console.WriteLine($"p{_pivotAsPercentage} s{Size}*{_scale} o{_originOffsetULCorner} A{pivotAbsolute} N{nextPivotOffset}     NO{nextOriginOffset}                          ");
			//Vec2 pivot = new Vec2(_pivot.X * Size.X, _pivot.Y * Size.Y);
			//int x = (int)pivot.x, y = (int)pivot.y;
			//SetBuffer(pivot, 'X');
			//SetBuffer(nextOrigin, '!');
			SetCharacter(pivotAbsolute, 'X');
			SetCharacter(nextOriginOffset, '!');
		}

		public void SetBufferCharacter(Vec2 staticBufferPosition, char value) {
			int x = (int)staticBufferPosition.x, y = (int)staticBufferPosition.y;
			if (x < 0 || y < 0 || x >= Size.X || y >= Size.Y) {
				return;
			}
			_currentBuffer[x, y] = value;
		}

		public void SetCharacter(Vec2 position, char value) {
			Vec2 relativeToBufferPosition = (position - _originOffsetULCorner).InverseScaled(_scale);
			SetBufferCharacter(relativeToBufferPosition, value);
		}

		public Vec2 Scale {
			get => _scale;
			set {
				_originOffsetULCorner = GetNextOriginOffsetAfterScale(Size, _originOffsetULCorner, _scale, value, _pivotAsPercentage);
				_scale = value;
			}
		}
		private static Vec2 GetNextOriginOffsetAfterScale(Point size, Vec2 ulCornerOrigin, Vec2 currentScale, Vec2 nextScale, Vec2 pivotAsPercentage) {
			Vec2 currentPivotOffset = new Vec2(pivotAsPercentage.X * size.X * currentScale.X, pivotAsPercentage.Y * size.Y * currentScale.Y);
			Vec2 pivotAbsolute = ulCornerOrigin + currentPivotOffset;
			Vec2 nextPivotOffset = new Vec2(pivotAsPercentage.X * size.X * nextScale.X, pivotAsPercentage.Y * size.Y * nextScale.Y);
			Vec2 nextOriginOffset = pivotAbsolute - nextPivotOffset;
			return nextOriginOffset;
		}
		public Point Size => new Point(Width, Height);
		public Vec2 PrintOffset { get => _printOffset; set => _printOffset = value; }
		public Vec2 Offset { get => _originOffsetULCorner; set => _originOffsetULCorner = value; }
		public char this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CmdLineBufferGraphicsContext(int width, int height, Vec2 scale, Vec2 offset, char[] valueForSamplesFound) {
			SetSize(width, height);
			_pivotAsPercentage = new Vec2(0.5f, 0.5f);
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
			int x, y;
			DebugPivot();

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
			Clear(_currentBuffer, ' ');
		}

		public void DrawSupersampledShape(Func<Vec2, bool> isInsideShape, Vec2 aabbMin, Vec2 aabbMax) {
			aabbMin -= _originOffsetULCorner;
			aabbMax -= _originOffsetULCorner;
			aabbMin.InverseScale(_scale);
			aabbMax.InverseScale(_scale);
			aabbMin.Floor();
			aabbMax.Ceil();
			Vec2 coord = aabbMin;
			for (; coord.Y < aabbMax.Y; coord.Y += 1) {
				coord.X = aabbMin.X;
				for (; coord.X < aabbMax.X; coord.X += 1) {
					if (coord.X < 0 || coord.Y < 0) { continue; }
					Vec2 supersample = Vec2.Zero;
					const int SUPERSAMPLE = 2;
					int samplesFound = 0;
					for (int row = 0; row < SUPERSAMPLE; ++row) {
						supersample.X = 0;
						for (int col = 0; col < SUPERSAMPLE; ++col) {
							Vec2 point = ((coord.X + supersample.X) * _scale.X, (coord.Y + supersample.Y) * _scale.Y);
							point += _originOffsetULCorner;
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

using MathMrV;
using System;
using System.Drawing;

namespace ConsoleMrV {
	// TODO refactor... make a simple double buffer, and child class that does scale/offset/supersample
	public class CommandLineGraphicsContext {
		private ConsoleGlyph[,] _currentBuffer;
		private ConsoleGlyph[,] _previousBuffer;
		public int Width;
		public int Height;
		private Vec2 _scale;
		private Vec2 _originOffsetULCorner;
		private Vec2 _printOffset;
		private Vec2 _pivotAsPercentage; // percentage, for zoom
		public ConsoleGlyph[] AntiAliasedGradient;
		/// <summary>
		/// map of color gradients per console color, for antialiased drawing
		/// </summary>
		public static ConsoleGlyph[][] AntiAliasedGradientPerColor;

		static CommandLineGraphicsContext() {
			AntiAliasedGradientPerColor = GenerateAntiAliasedGradientPerColorMapForConsoleColors();
		}

		private static ConsoleGlyph[][] GenerateAntiAliasedGradientPerColorMapForConsoleColors() {
			ConsoleGlyph[][] colorMap = new ConsoleGlyph[16][];
			ConsoleGlyph[] colors = new ConsoleGlyph[16];
			for (int i = 0; i < 16; ++i) {
				colors[i] = new ConsoleGlyph((ConsoleColor)i);
			}
			colorMap[(int)ConsoleColor.Black] = new ConsoleGlyph[]{
				colors[(int)ConsoleColor.Black]
			};
			for (int i = 1; i <= 6; ++i) {
				colorMap[i] = new ConsoleGlyph[] {
					colors[(int)ConsoleColor.Black],
					colors[i]
				};
			}
			colorMap[(int)ConsoleColor.Gray] = new ConsoleGlyph[] {
				colors[(int)ConsoleColor.Black],
				colors[(int)ConsoleColor.DarkGray],
				colors[(int)ConsoleColor.DarkGray],
				colors[(int)ConsoleColor.Gray],
				colors[(int)ConsoleColor.Gray],
			};
			colorMap[(int)ConsoleColor.DarkGray] = new ConsoleGlyph[] {
				colors[(int)ConsoleColor.Black],
				colors[(int)ConsoleColor.DarkGray]
			};
			for (int i = 1 + 8; i <= 6 + 8; ++i) {
				colorMap[i] = new ConsoleGlyph[] {
					colors[(int)ConsoleColor.Black],
					colors[i-8],
					colors[i-8],
					colors[i-8],
					colors[i],
				};
			}
			colorMap[(int)ConsoleColor.White] = new ConsoleGlyph[] {
				colors[(int)ConsoleColor.Black],
				colors[(int)ConsoleColor.DarkGray],
				colors[(int)ConsoleColor.Gray],
				colors[(int)ConsoleColor.Gray],
				colors[(int)ConsoleColor.White],
			};
			return colorMap;
		}

		public void SetColor(ConsoleColor color) {
			AntiAliasedGradient = AntiAliasedGradientPerColor[(int)color];
		}

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
		public void WriteAt(string text, Vec2 position, bool alsoUseBackground = false) =>
			WriteAt(ConsoleGlyph.Convert(text), position, alsoUseBackground);
		public void WriteAt(ConsoleGlyph[] text, Vec2 position, bool alsoUseBackground = false) {
			Vec2 realPosition = position - _originOffsetULCorner;
			realPosition.InverseScale(_scale);
			WriteAt(text, (int)realPosition.y, (int)realPosition.x, alsoUseBackground);
		}

		bool IsValidCoordinate(int x, int y) {
			return x >= 0 && x < Width && y >= 0 && y < Height;
		}

		public void SetCharacter(Vec2 position, ConsoleGlyph value) {
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
		public Vec2 Size => new Point(Width, Height);
		public Vec2 PrintOffset { get => _printOffset; set => _printOffset = value; }
		public Vec2 Offset { get => _originOffsetULCorner; set => _originOffsetULCorner = value; }
		public ConsoleGlyph this[int x, int y] {
			get => _currentBuffer[x, y];
			set => _currentBuffer[x, y] = value;
		}
		public CommandLineGraphicsContext(int width, int height, Vec2 scale = default, Vec2 offset = default) {
			if (scale == default) { scale = Vec2.One; }
			SetSize(width, height);
			_pivotAsPercentage = new Vec2(0.5f, 0.5f);
			_scale = scale;
			_printOffset = offset;
			SetColor(ConsoleColor.White);
		}
		public void SetSize(int width, int height) {
			SetSize(ref _previousBuffer, width, height);
			SetSize(ref _currentBuffer, width, height);
		}
		private void SetSize(ref ConsoleGlyph[,] buffer, int width, int height) {
			int oldW = Width;
			int oldH = Height;
			ConsoleGlyph[,] oldBuffer = buffer;
			Width = width;
			Height = height;
			buffer = new ConsoleGlyph[Width, Height];
			if (oldBuffer != null) {
				for (int x = 0; x < Width && x < oldW; ++x) {
					for (int y = 0; y < Height && y < oldH; ++y) {
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
		public void PrintUnoptimized() {
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
		public void SetDirty() {
			for (int x = 0; x < Width; ++x) {
				for (int y = 0; y < Height; ++y) {
					_previousBuffer[x, y] = ConsoleGlyph.Empty;
				}
			}
		}

		public void PrintModifiedCharactersOnly() {
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

		public void DrawSupersampledShape(Func<Vec2, bool> isInsideShape, Vec2 aabbMin, Vec2 aabbMax) {
			Vec2 renderMin = aabbMin - _originOffsetULCorner;
			Vec2 renderMax = aabbMax - _originOffsetULCorner;
			renderMin.InverseScale(_scale);
			renderMax.InverseScale(_scale);
			renderMin.Floor();
			renderMax.Ceil();
			if (renderMin.X < 0) { renderMin.X = 0; }
			if (renderMin.Y < 0) { renderMin.X = 0; }
			if (renderMax.X > Width) { renderMax.X = Width; }
			if (renderMax.Y > Height) { renderMax.Y = Height; }
			for (int y = (int)renderMin.y; y < renderMax.y; ++y) {
				for (int x = (int)renderMin.x; x < renderMax.x; ++x) {
					Vec2 supersample = Vec2.Zero;
					const int SUPERSAMPLE = 2;
					int samplesFound = 0;
					for (int row = 0; row < SUPERSAMPLE; ++row) {
						supersample.X = 0;
						for (int col = 0; col < SUPERSAMPLE; ++col) {
							Vec2 point = ((x + supersample.x) * _scale.x, (y + supersample.y) * _scale.y);
							point += _originOffsetULCorner;
							if (isInsideShape.Invoke(point)) {
								samplesFound++;
							}
							supersample.X += 1f / SUPERSAMPLE;
						}
						supersample.Y += 1f / SUPERSAMPLE;
					}
					if (samplesFound > 0) {
						if (x >= 0 && y >= 0 && x < Width && y < Height) {
							int sampleIndex = samplesFound;
							if (sampleIndex >= AntiAliasedGradient.Length) {
								sampleIndex = AntiAliasedGradient.Length - 1;
							}
							this[x, y] = AntiAliasedGradient[sampleIndex];
						}
					}
				}
			}
		}

		public void DrawLine(Vec2 start, Vec2 end, float lineWidth = 0.5f) {
			lineWidth *= Scale.y;
			DrawLineUnscaled(start, end, lineWidth);
		}
		public void DrawLineUnscaled(Vec2 start, Vec2 end, float lineWidth = 0.5f) {
			Vec2 delta = end - start;
			float lineLength = delta.Magnitude;
			Vec2 direction = delta / lineLength;
			Vec2 perp = direction.Perpendicular() * (lineWidth / 2);
			Vec2[] line = new Vec2[4] { start + perp, start - perp, end - perp, end + perp, };
			Polygon.TryGetAABB(line, out Vec2 min, out Vec2 max);
			DrawSupersampledShape(IsInsideLine, min, max);
			bool IsInsideLine(Vec2 point) => Polygon.IsInPolygon(line, point);
		}
	}
}

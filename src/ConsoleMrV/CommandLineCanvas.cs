#define IN_COLOR
using MathMrV;
using System;

namespace ConsoleMrV {
#if IN_COLOR
	/// <summary>
	/// double-buffered 2D canvas of console glyphs
	/// </summary>
	public class CommandLineCanvas : CommandLineGraphicsContext {
		/// <summary>
		/// map of color gradients per console color, for antialiased drawing
		/// </summary>
		public static ConsoleGlyph[][] AntiAliasedGradientPerColor;
		public ConsoleGlyph[] AntiAliasedGradient;
		private Vec2 _scale;
		private Vec2 _originOffsetULCorner;
		private Vec2 _pivotAsPercentage; // percentage, for zoom around a target

		public Vec2 Scale {
			get => _scale;
			set {
				_originOffsetULCorner = GetNextOriginOffsetAfterScale(Size, _originOffsetULCorner, _scale, value, _pivotAsPercentage);
				_scale = value;
			}
		}
		public Vec2 PivotAsPercentage { get => _pivotAsPercentage; set { _pivotAsPercentage = value; } }
		public Vec2 Offset { get => _originOffsetULCorner; set => _originOffsetULCorner = value; }
		private static Vec2 GetNextOriginOffsetAfterScale(Vec2 size, Vec2 ulCornerOrigin, Vec2 currentScale,
		Vec2 nextScale, Vec2 pivotAsPercentage) {
			Vec2 currentPivotOffset = new Vec2(pivotAsPercentage.X * size.X * currentScale.X, 
			                                   pivotAsPercentage.Y * size.Y * currentScale.Y);
			Vec2 pivotPosition = ulCornerOrigin + currentPivotOffset;
			Vec2 nextPivotOffset = new Vec2(pivotAsPercentage.X * size.X * nextScale.X,
			                                pivotAsPercentage.Y * size.Y * nextScale.Y);
			return pivotPosition - nextPivotOffset;
		}

		static CommandLineCanvas() {
			AntiAliasedGradientPerColor = GenerateAntiAliasedGradientPerColorMapForConsoleColors();
		}

		private static ConsoleGlyph[][] GenerateAntiAliasedGradientPerColorMapForConsoleColors() {
			ConsoleGlyph[][] colorMap = new ConsoleGlyph[16][];
			ConsoleGlyph[] color = new ConsoleGlyph[16];
			for (int i = 0; i < 16; ++i) {
				color[i] = new ConsoleGlyph((ConsoleColor)i);
			}
			int blackIndex = (int)ConsoleColor.Black;
			colorMap[blackIndex] = new ConsoleGlyph[]{ color[blackIndex] };
			// all dark colors have no gradient
			for (int i = 1; i <= 6; ++i) {
				colorMap[i] = new ConsoleGlyph[] { color[blackIndex], color[i] };
			}
			colorMap[(int)ConsoleColor.Gray] = new ConsoleGlyph[] {
				color[blackIndex], color[(int)ConsoleColor.DarkGray], color[(int)ConsoleColor.DarkGray],
				color[(int)ConsoleColor.Gray], color[(int)ConsoleColor.Gray],
			};
			colorMap[(int)ConsoleColor.DarkGray] = new ConsoleGlyph[] {
				color[blackIndex], color[(int)ConsoleColor.DarkGray]
			};
			// all light colors use dark colors as partial-value part of gradient
			for (int i = 1 + 8; i <= 6 + 8; ++i) {
				colorMap[i] = new ConsoleGlyph[] { color[blackIndex], color[i-8], color[i-8], color[i-8], color[i] };
			}
			colorMap[(int)ConsoleColor.White] = new ConsoleGlyph[] {
				color[blackIndex], color[(int)ConsoleColor.DarkGray],
				color[(int)ConsoleColor.Gray], color[(int)ConsoleColor.Gray], color[(int)ConsoleColor.White],
			};
			return colorMap;
		}

		public CommandLineCanvas(int width, int height, Vec2 scale = default) : base(width, height) {
			if (scale == default) { scale = Vec2.One; }
			_pivotAsPercentage = new Vec2(0.5f, 0.5f);
			_scale = scale;
			SetColor(ConsoleColor.White);
		}

		public void SetColor(ConsoleColor color) {
			AntiAliasedGradient = AntiAliasedGradientPerColor[(int)color];
		}

		public Vec2 GetConsolePosition(Vec2 canvasPosition) =>
			(canvasPosition - _originOffsetULCorner).InverseScaled(_scale);

		public void WriteAt(string text, Vec2 position, bool useNewBgColor = false) =>
			WriteAt(ConsoleGlyph.Convert(text), position, useNewBgColor);

		public void WriteAt(ConsoleGlyph[] text, Vec2 position, bool useConsoleGlyphBgColor = true) {
			Vec2 consolePosition = GetConsolePosition(position);
			WriteAt(text, (int)consolePosition.X, (int)consolePosition.Y, useConsoleGlyphBgColor);
		}

		public void DrawSupersampledShape(Func<Vec2, bool> isInsideShape, Vec2 aabbMin, Vec2 aabbMax) {
			aabbMin.Floor();
			aabbMax.Ceil();
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
			for (int y = (int)renderMin.Y; y < renderMax.Y; ++y) {
				for (int x = (int)renderMin.X; x < renderMax.X; ++x) {
					Vec2 supersample = Vec2.Zero;
					const int SUPERSAMPLE = 2;
					int samplesFound = 0;
					for (int row = 0; row < SUPERSAMPLE; ++row) {
						supersample.X = 0;
						for (int col = 0; col < SUPERSAMPLE; ++col) {
							Vec2 point = ((x + supersample.X) * _scale.X, (y + supersample.Y) * _scale.Y);
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
		public void FillRect(AABB aabb) => FillRect(aabb.Min, aabb.Max);
		public void FillRect(Vec2 aabbMin, Vec2 aabbMax) {
			bool IsInsideRectangle(Vec2 p) => p.X >= aabbMin.X && p.Y >= aabbMin.Y && p.X <= aabbMax.X && p.Y <= aabbMax.Y;
			DrawSupersampledShape(IsInsideRectangle, aabbMin, aabbMax);
		}
		public void DrawLine(Vec2 start, Vec2 end, float lineWidth = 0.5f) {
			lineWidth *= Scale.Y;
			DrawLineUnscaled(start, end, lineWidth);
		}
		public void DrawLineUnscaled(Vec2 start, Vec2 end, float lineWidth = 0.5f) {
			Vec2 delta = end - start;
			float lineLength = delta.Magnitude;
			Vec2 direction = delta / lineLength;
			Vec2 perp = direction.Perpendicular() * (lineWidth / 2);
			Vec2[] line = new Vec2[4] { start + perp, start - perp, end - perp, end + perp, };
			PolygonShape.TryGetAABB(line, out Vec2 min, out Vec2 max);
			DrawSupersampledShape(IsInsideLine, min, max);
			bool IsInsideLine(Vec2 point) => PolygonShape.IsInPolygon(line, point);
		}
	}
#else
	public class CommandLineCanvas : CommandLineGraphicsContext {
		public char[] AntiAliasedGradient = { ' ', '.', ':', 'x', '#' };
		private Vec2 _scale;
		private Vec2 _originOffsetULCorner;
		private Vec2 _pivotAsPercentage; // percentage, for zoom around a target

		public Vec2 Scale {
			get => _scale;
			set {
				_originOffsetULCorner = GetNextOriginOffsetAfterScale(Size, _originOffsetULCorner, _scale, value, _pivotAsPercentage);
				_scale = value;
			}
		}
		public Vec2 PivotAsPercentage { get => _pivotAsPercentage; set { _pivotAsPercentage = value; } }
		public Vec2 Offset { get => _originOffsetULCorner; set => _originOffsetULCorner = value; }
		private static Vec2 GetNextOriginOffsetAfterScale(Vec2 size, Vec2 ulCornerOrigin, Vec2 currentScale,
		Vec2 nextScale, Vec2 pivotAsPercentage) {
			Vec2 currentPivotOffset = new Vec2(pivotAsPercentage.X * size.X * currentScale.X,
																				 pivotAsPercentage.Y * size.Y * currentScale.Y);
			Vec2 pivotPosition = ulCornerOrigin + currentPivotOffset;
			Vec2 nextPivotOffset = new Vec2(pivotAsPercentage.X * size.X * nextScale.X,
																			pivotAsPercentage.Y * size.Y * nextScale.Y);
			return pivotPosition - nextPivotOffset;
		}

		public CommandLineCanvas(int width, int height, Vec2 scale = default) : base(width, height) {
			if (scale == default) { scale = Vec2.One; }
			_pivotAsPercentage = new Vec2(0.5f, 0.5f);
			_scale = scale;
			SetColor(ConsoleColor.White);
		}

		public void SetColor(ConsoleColor color) {
			//AntiAliasedGradient = AntiAliasedGradientPerColor[(int)color];
		}

		public Vec2 GetConsolePosition(Vec2 canvasPosition) =>
			(canvasPosition - _originOffsetULCorner).InverseScaled(_scale);

		public void WriteAt(string text, Vec2 position, bool alsoUseBackground = false) =>
			WriteAt(ConsoleGlyph.Convert(text), position, alsoUseBackground);

		public void WriteAt(ConsoleGlyph[] text, Vec2 position, bool alsoUseBackground = false) {
			Vec2 consolePosition = GetConsolePosition(position);
			//WriteAt(text, (int)consolePosition.x, (int)consolePosition.y, alsoUseBackground);
			WriteAt(ConsoleGlyph.Convert(text), (int)consolePosition.x, (int)consolePosition.y);
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
		public void FillRect(Vec2 aabbMin, Vec2 aabbMax) {
			bool IsInsideRectangle(Vec2 p) => p.x >= aabbMin.x && p.y >= aabbMin.y && p.x <= aabbMax.x && p.y <= aabbMax.y;
			DrawSupersampledShape(IsInsideRectangle, aabbMin, aabbMax);
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
#endif
}

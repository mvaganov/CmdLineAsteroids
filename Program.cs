using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace asteroids {
	internal class Program {
		static void Main(string[] args) {
			char[] sampleValue = { ' ', '·', '-', 'o', '0' };
			Vec2 scale = (0.5f, 1);
			Vec2 offset = (0, 0);
			CmdLineBufferGraphicsContext graphics = new CmdLineBufferGraphicsContext(80, 20, (0.5f, 1), (0,0), sampleValue);
			bool running = true;
			Vec2 position = (18, 12);
			float radius = 10;
			float moveAdjust = 0.25f;
			Polygon player = new Polygon(new Vec2[] { (5,0), (-3,3), (-3,-3) });
			float playerRotationAngle = MathF.PI / 64;
			Stopwatch timer = new Stopwatch();
				while (running) {
					timer.Restart();
				DrawCircle(graphics, sampleValue, position, radius, scale, offset);
				DrawPolygon(graphics, sampleValue, player, scale, offset);
				//graphics.PrintUnoptimized(0, 0);
				graphics.PrintOptimized(0, 0);
				graphics.FinishedRender();
				Console.SetCursorPosition(0, 20);
				Console.WriteLine($"{timer.ElapsedMilliseconds}   ");
				while (Console.KeyAvailable) {
					char c = Console.ReadKey().KeyChar;
					switch (c) {
						case 'w': player.OriginOffset += Vec2.DirectionMinY / 5; break;
						case 'a': player.OriginOffset += Vec2.DirectionMinX / 5; break;
						case 's': player.OriginOffset += Vec2.DirectionMaxY / 5; break;
						case 'd': player.OriginOffset += Vec2.DirectionMaxX / 5; break;
						//case 'i': graphics.off += Vec2.DirectionMinY / 5; break;
						//case 'j': player.OriginOffset += Vec2.DirectionMinX / 5; break;
						//case 'k': player.OriginOffset += Vec2.DirectionMaxY / 5; break;
						//case 'l': player.OriginOffset += Vec2.DirectionMaxX / 5; break;
						case 'r': radius -= moveAdjust / 2; break;
						case 't': radius += moveAdjust / 2; break;
						case '=': scale /= 1.5f; break;
						case '-': scale *= 1.5f; break;
						case 'x': position += Vec2.Random - Vec2.Half; break;
						case (char)27: running = false; break;
						case 'q': player.RotationRadians -= playerRotationAngle; break;
						case 'e': player.RotationRadians += playerRotationAngle; break;
					}
				}
			}
		}
		public struct Vec2 {
			public float x, y;
			public float X { get => x; set => x = value; }
			public float Y { get => y; set => y = value; }
			public Vec2(float x, float y) { this.x = x; this.y = y; }
			public static implicit operator Vec2((float X, float Y) tuple) => new Vec2(tuple.X, tuple.Y);
			public static implicit operator Point(Vec2 v) => new Point((int)v.X, (int)v.Y);
			public static implicit operator Vec2(Point v) => new Vec2(v.X, v.Y);
			public static Vec2 operator +(Vec2 a, Vec2 b) => (a.X + b.X, a.Y + b.Y);
			public static Vec2 operator -(Vec2 a, Vec2 b) => (a.X - b.X, a.Y - b.Y);
			public static Vec2 operator *(Vec2 a, float scalar) => (a.X * scalar, a.Y * scalar);
			public static Vec2 operator /(Vec2 a, float scalar) => (a.X / scalar, a.Y / scalar);
			public override string ToString() => $"({x},{y})";
			public Vec2 Scaled(Vec2 scale) => (x * scale.X, y * scale.Y);
			public Vec2 InverseScaled(Vec2 scale) => (x / scale.X, y / scale.Y);
			public void Scale(Vec2 scale) { x *= scale.X; y *= scale.Y; }
			public void InverseScale(Vec2 scale) { x /= scale.X; y /= scale.Y; }
			public void Floor() { x = MathF.Floor(x); y = MathF.Floor(y); }
			public void Ceil() { x = MathF.Ceiling(x); y = MathF.Ceiling(y); }
			public static Vec2 Zero = (0, 0);
			public static Vec2 One = (1, 1);
			public static Vec2 Half = (1f / 2, 1f / 2);
			public static Vec2 Max = (float.MaxValue, float.MaxValue);
			public static Vec2 Min = (float.MinValue, float.MinValue);
			public static Vec2 DirectionMinX = (-1, 0);
			public static Vec2 DirectionMaxX = ( 1, 0);
			public static Vec2 DirectionMinY = ( 0,-1);
			public static Vec2 DirectionMaxY = ( 0, 1);

			public static Vec2 Random => ((float)randomGenerator.NextDouble(), (float)randomGenerator.NextDouble());
			public static System.Random randomGenerator = new System.Random();
		}
		public static void DrawRectangle(char letterToPrint, Vec2 position, Vec2 size) {
			for (int row = 0; row < size.Y; ++row) {
				Console.SetCursorPosition((int)position.X, (int)position.Y + row);
				for (int col = 0; col < size.X; ++col) {
					Console.Write(letterToPrint);
				}
			}
		}
		public static void DrawCircle(char letterToPrint, Vec2 pos, float radius) {
			Vec2 extent = (radius, radius);
			Vec2 start = pos - extent;
			Vec2 end = pos + extent;
			Vec2 coord = start;
			float r2 = radius * radius;
			for (; coord.Y < end.Y; coord.Y += 1) {
				coord.X = start.X;
				for (; coord.X < end.X; coord.X += 1) {
					if (coord.X < 0 || coord.Y < 0) { continue; }
					float dx = (coord.X - pos.X);
					float dy = (coord.Y - pos.Y);
					if (dx * dx + dy * dy < r2) {
						Console.SetCursorPosition((int)coord.X, (int)coord.Y);
						Console.Write(letterToPrint);
					}
				}
			}
		}
		public static void DrawCircle(char letterToPrint, Vec2 pos, float radius, Vec2 pixelSize, Vec2 bufferOrigin) {
			Vec2 extent = (radius, radius);
			extent.Scale(pixelSize);
			Vec2 start = pos.Scaled(pixelSize) - extent;
			Vec2 end = pos.Scaled(pixelSize) + extent;
			Vec2 coord = start;
			float r2 = radius * radius;
			for (; coord.Y < end.Y; coord.Y += 1) {
				coord.X = start.X;
				for (; coord.X < end.X; coord.X += 1) {
					if (coord.X < 0 || coord.Y < 0) { continue; }
					float dx = (coord.X / pixelSize.X - pos.X);
					float dy = (coord.Y / pixelSize.Y - pos.Y);
					if (dx * dx + dy * dy < r2) {
						Console.SetCursorPosition((int)(coord.X + bufferOrigin.X), (int)(coord.Y + bufferOrigin.Y));
						Console.Write(letterToPrint);
					}
				}
			}
		}
		public static void DrawSupersampledShape(CmdLineBufferGraphicsContext g, char[] valueForSamplesFound, Func<Vec2,bool> isInsideShape, 
			Vec2 pixelSize, Point bufferOrigin, Vec2 min, Vec2 max) {
			min.InverseScale(pixelSize);
			max.InverseScale(pixelSize);
			min.Floor();
			max.Ceil();
			Vec2 coord = min;
			Console.WriteLine($"#####  ({min}) {max} vs {g.Size}");
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
							Vec2 point = ((coord.X + supersample.X) * pixelSize.X, (coord.Y + supersample.Y) * pixelSize.Y);
							if (isInsideShape.Invoke(point)) {
								samplesFound++;
							}
							supersample.X += 1f / SUPERSAMPLE;
						}
						supersample.Y += 1f / SUPERSAMPLE;
					}
					if (g != null) {
						if (samplesFound > 0) {
							int x = (int)(coord.X + bufferOrigin.X);
							int y = (int)(coord.Y + bufferOrigin.Y);
							if (x > 0 && y > 0 && x < g.Width && y < g.Height) {
								g[x, y] = valueForSamplesFound[samplesFound];
							}
						}
					} else {
						Console.SetCursorPosition((int)(coord.X + bufferOrigin.X), (int)(coord.Y + bufferOrigin.Y));
						Console.Write(valueForSamplesFound[samplesFound]);
					}
				}
			}
		}
		public static void DrawCircle(CmdLineBufferGraphicsContext g, char[] valueForSamplesFound, Vec2 pos, float radius, Vec2 pixelSize, Point bufferOrigin) {
			Vec2 extent = (radius, radius);
			Vec2 min = pos - extent;
			Vec2 max = pos + extent;
			float r2 = radius * radius, dx, dy;
			DrawSupersampledShape(g, valueForSamplesFound, IsInsideCircle, pixelSize, bufferOrigin, min, max);
			bool IsInsideCircle(Vec2 point) {
				dx = point.X - pos.X;
				dy = point.Y - pos.Y;
				return (dx * dx + dy * dy <= r2);
			}
		}
		public static void DrawPolygon(CmdLineBufferGraphicsContext g, char[] valueForSamplesFound, IList<Vec2> points, Vec2 pixelSize,
			Point bufferOrigin) {
			Polygon.TryGetAABB(points, out Vec2 min, out Vec2 max);
			min.Floor();
			max.Ceil();
			Console.WriteLine($"\n------------ {min} -> {max}");
			DrawPolygon(g, valueForSamplesFound, points, pixelSize, bufferOrigin, min, max);
		}

		public static void DrawPolygon(CmdLineBufferGraphicsContext g, char[] valueForSamplesFound, IList<Vec2> points, Vec2 pixelSize, 
			Point bufferOrigin, Point min, Point max) {
			DrawSupersampledShape(g, valueForSamplesFound, IsInsidePolygon, pixelSize, bufferOrigin, min, max);
			bool IsInsidePolygon(Vec2 point) => Polygon.IsInPolygon(points, point);
		}
		public class Circle {
			public Vec2 position;
			public float radius;
		}
		public class Polygon : IList<Vec2> {
			private Vec2[] originalPoints;
			private float rotationRadians = 0;
			private Vec2 originOffset;

			private Vec2[] cachedPoints;
			private bool cacheValid = false;
			private Vec2 cachedBoundBoxMin, cachedBoundBoxMax;
			public Vec2 OriginOffset { get => originOffset; set { originOffset = value; cacheValid = false; } }
			public float RotationRadians { get => rotationRadians; set { rotationRadians = value; cacheValid = false; } }

			public int Count => originalPoints.Length;

			public bool IsReadOnly => false;

			Vec2 IList<Vec2>.this[int index] { get => GetPoint(index); set => SetPoint(index, value); }
			public Polygon(IList<Vec2> points) {
				originalPoints = new Vec2[points.Count];
				for(int i = 0; i < points.Count; ++i) {
					originalPoints[i] = points[i];
				}
			}
			public float GetRotation() { return rotationRadians * 180 / MathF.PI; }
			public void SetRotation(float value) { RotationRadians = value * MathF.PI / 180; }
			private void UpdateCacheAsNeeded() {
				if (cacheValid) { return; }
				if (cachedPoints == null || cachedPoints.Length != originalPoints.Length) {
					cachedPoints = new Vec2[originalPoints.Length];
				}
				float ca = MathF.Cos(rotationRadians);
				float sa = MathF.Sin(rotationRadians);
				for (int i = 0; i < originalPoints.Length; i++) {
					Vec2 v = originalPoints[i];
					cachedPoints[i] = new Vec2(ca * v.X - sa * v.Y + originOffset.X, sa * v.X + ca * v.Y + originOffset.Y);
				}
				TryGetAABB(out cachedBoundBoxMin, out cachedBoundBoxMax);
				cacheValid = true;
			}
			public Vec2 GetOriginalPoint(int index) => originalPoints[index];
			public void SetOriginalPoint(int index, Vec2 value) {
				originalPoints[index] = value;
				cacheValid = false;
			}
			public Vec2 GetPoint(int index) {
				UpdateCacheAsNeeded();
				return cachedPoints[index];
			}
			public void SetPoint(int index, Vec2 point) {
				cachedPoints[index] = point;
				float ca = MathF.Cos(-rotationRadians);
				float sa = MathF.Sin(-rotationRadians);
				originalPoints[index] = new Vec2(ca * point.X - sa * point.Y, sa * point.X + ca * point.Y);
			}
			public static Vec2 RotateRadians(Vec2 v, float radians) {
				float ca = MathF.Cos(radians);
				float sa = MathF.Sin(radians);
				return new Vec2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
			}
			public static bool IsInPolygon(IList<Vec2> poly, Vec2 p) {
				Vec2 p1, p2;
				bool inside = false;
				if (poly.Count < 3) {
					return inside;
				}
				var oldPoint = new Vec2(poly[poly.Count - 1].X, poly[poly.Count - 1].Y);
				for (int i = 0; i < poly.Count; i++) {
					var newPoint = new Vec2(poly[i].X, poly[i].Y);
					if (newPoint.X > oldPoint.X) {
						p1 = oldPoint;
						p2 = newPoint;
					} else {
						p1 = newPoint;
						p2 = oldPoint;
					}
					if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
							&& (p.Y - (long)p1.Y) * (p2.X - p1.X)
							< (p2.Y - (long)p1.Y) * (p.X - p1.X)) {
						inside = !inside;
					}
					oldPoint = newPoint;
				}
				return inside;
			}
			public static bool TryGetAABB(IList<Vec2> points, out Vec2 min, out Vec2 max) {
				min = Vec2.Max;
				max = Vec2.Min;
				if (points.Count == 0) {
					return false;
				}
				for (int i = 0; i < points.Count; ++i) {
					Vec2 p = points[i];
					if (p.x < min.x) { min.x = p.x; }
					if (p.y < min.y) { min.y = p.y; }
					if (p.x > max.x) { max.x = p.x; }
					if (p.y > max.y) { max.y = p.y; }
				}
				return true;
			}
			public bool Contains(Vec2 point) => IsInPolygon(cachedPoints, point);
			public bool TryGetAABB(out Vec2 min, out Vec2 max) => TryGetAABB(cachedPoints, out min, out max);

			int IList<Vec2>.IndexOf(Vec2 item) => Array.IndexOf(cachedPoints, item);

			void IList<Vec2>.Insert(int index, Vec2 item) => throw new NotImplementedException();

			public void RemoveAt(int index) => throw new NotImplementedException();

			void ICollection<Vec2>.Add(Vec2 item) => throw new NotImplementedException();

			public void Clear() => throw new NotImplementedException();

			public void CopyTo(Vec2[] array, int arrayIndex) => Array.Copy(originalPoints, 0, array, arrayIndex, Count);

			bool ICollection<Vec2>.Remove(Vec2 item) => throw new NotImplementedException();

			IEnumerator<Vec2> IEnumerable<Vec2>.GetEnumerator() {
				throw new NotImplementedException();
			}

			public IEnumerator GetEnumerator() {
				throw new NotImplementedException();
			}
		}
		public class CmdLineBufferGraphicsContext {
			/// <summary>
			/// X, Y
			/// </summary>
			private char[,] _presentationBuffer;
			private char[,] _previousStateBuffer;
			private Vec2 _scale;
			private Vec2 _offset;
			public char[] valueForSamplesFound;

			public int Width;
			public int Height;

			public Point Size => new Point(Width, Height);
			public Vec2 Offset { get => _offset; set => _offset = value; }
			public char this[int x, int y] {
				get => _presentationBuffer[x, y];
				set => _presentationBuffer[x, y] = value;
			}
			public CmdLineBufferGraphicsContext(int width, int height, Vec2 scale, Vec2 offset, char[] valueForSamplesFound) {
				SetSize(width, height);
				_scale = scale;
				_offset = offset;
				this.valueForSamplesFound = valueForSamplesFound;
			}
			public void SetSize(int width, int height) {
				SetSize(ref _previousStateBuffer, width, height);
				SetSize(ref _presentationBuffer, width, height);
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
						for (int y = 0; y <Height && y < oldH; ++y) {
							buffer[x, y] = oldBuffer[x,y];
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
			public void PrintUnoptimized(int minX, int minY) {
				for (int row = 0; row < Height; ++row) {
					Console.SetCursorPosition(minX, minY + row);
					for (int col = 0; col < Width; ++col) {
						Console.Write(_presentationBuffer[col, row]);
					}
				}
			}
			public void PrintOptimized(int minX, int minY) {
				bool cursorInCorrectPlace;
				for (int row = 0; row < Height; ++row) {
					cursorInCorrectPlace = false;
					for (int col = 0; col < Width; ++col) {
						if (_presentationBuffer[col, row] != _previousStateBuffer[col, row]) {
							if (!cursorInCorrectPlace) {
								Console.SetCursorPosition(minX + col, minY + row);
							}
							Console.Write(_presentationBuffer[col, row]);
							cursorInCorrectPlace = true;
						} else {
							cursorInCorrectPlace = false;
						}
					}
				}
			}
			public void SwapBuffers() {
				char[,] swap = _presentationBuffer;
				_presentationBuffer = _previousStateBuffer;
				_previousStateBuffer = swap;
			}
			public void FinishedRender() {
				SwapBuffers();
				Clear(_presentationBuffer, ' ');
			}
		}
	}
}

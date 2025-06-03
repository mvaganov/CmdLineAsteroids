using System;
using System.Diagnostics;
using System.Drawing;

namespace asteroids {
	internal class Program {
		static void Main(string[] args) {
			ConsoleColor defaultColor = Console.BackgroundColor;
			int[] colors = { 0, 1, 9, 3, 11,0, 5, 13, 4, 12, 0, 2, 10, 6, 14 };
			//int[] colors = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 ,14 ,15 };
			for (int i = 0; i < colors.Length; ++i) {
				Console.BackgroundColor = (ConsoleColor)colors[i];
				Console.WriteLine($"testing {colors[i]}");
			}
			Console.BackgroundColor = defaultColor;
			Console.ReadKey();
			char[] sampleValue = { ' ', ':', '+', 'o', '0' };
			Vec2 scale = (0.5f, 1);
			Vec2 offset = (0, 0);
			CmdLineBufferGraphicsContext graphics = new CmdLineBufferGraphicsContext(80, 20, (0.5f, 1), (0,0), sampleValue);
			bool running = true;
			float moveAdjust = 0.25f;
			Polygon player = new Polygon(new Vec2[] { (5,0), (-3,3), (0,0), (-3,-3) });
			Circle circle = new Circle((18, 12), 10);
			float playerRotationAngle = 1;// MathF.PI / 64;
			Stopwatch timer = new Stopwatch();
			while (running) {
				timer.Restart();
				circle.Draw(graphics);
				player.Draw(graphics);
				//graphics.PrintUnoptimized();
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, 20);
				Console.WriteLine($"{graphics.PrintOffset}  {timer.ElapsedMilliseconds}   ");
				while (Console.KeyAvailable) {
					char c = Console.ReadKey().KeyChar;
					switch (c) {
						case 'w': player.OriginOffset += Vec2.DirectionMinY / 5; break;
						case 'a': player.OriginOffset += Vec2.DirectionMinX / 5; break;
						case 's': player.OriginOffset += Vec2.DirectionMaxY / 5; break;
						case 'd': player.OriginOffset += Vec2.DirectionMaxX / 5; break;
						case 'i': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMinY / 2); break;
						case 'j': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMinX / 2); break;
						case 'k': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMaxY / 2); break;
						case 'l': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMaxX / 2); break;
						case 'r': circle.radius -= moveAdjust / 2; break;
						case 't': circle.radius += moveAdjust / 2; break;
						case '=': if (graphics.Scale.x > 1f / 128) { graphics.Scale /= 1.5f; } break;
						case '-': if (graphics.Scale.x < 128) { graphics.Scale *= 1.5f; } break;
						case 'x': circle.position += Vec2.Random - Vec2.Half; break;
						case (char)27: running = false; break;
						case 'q': player.RotationDegrees -= playerRotationAngle; break;
						case 'e': player.RotationDegrees += playerRotationAngle; break;
					}
					System.Threading.Thread.Sleep(1);
				}
			}
		}

		public static void DrawRectangle(char letterToPrint, Vec2 position, Vec2 size) {
			for (int row = 0; row < size.Y; ++row) {
				Console.SetCursorPosition((int)position.X, (int)position.Y + row);
				for (int col = 0; col < size.X; ++col) {
					Console.Write(letterToPrint);
				}
			}
		}

		public static void ClearConsole() => DrawRectangle(' ', (0, 0), (Console.BufferWidth, 30));	
	}
}

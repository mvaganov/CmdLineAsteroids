using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Diagnostics;

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
			ConsoleGlyph[] sampleValue = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(':', ConsoleColor.Gray, ConsoleColor.DarkBlue),
				new ConsoleGlyph('+', ConsoleColor.Gray, ConsoleColor.Blue),
				new ConsoleGlyph('o', ConsoleColor.Gray, ConsoleColor.DarkCyan),
				new ConsoleGlyph('0', ConsoleColor.Gray, ConsoleColor.Cyan)
			};
			Vec2 scale = (0.5f, 1);
			Vec2 offset = (0, 0);
			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 20, (0.5f, 1), (0,0), sampleValue);
			bool running = true;
			float moveAdjust = 0.25f;
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			//Polygon player = new Polygon(playerPoly);
			Circle circle = new Circle((18, 12), 10);
			float playerRotationAngle = 5;// MathF.PI / 64;
			Stopwatch timer = new Stopwatch();

			MobilePolygon player = new MobilePolygon(playerPoly);
			player.DirectionMatchesVelocity = true;
			player.Velocity = (1, 1);

			while (running) {
				timer.Restart();
				circle.Draw(graphics);
				Console.SetCursorPosition(0, 20);
				Console.WriteLine($"c {timer.ElapsedMilliseconds}   ");
				timer.Restart();
				player.Update();
				player.Draw(graphics);
				Console.SetCursorPosition(0, 21);
				Console.WriteLine($"p {timer.ElapsedMilliseconds}   {player.Position}");
				//graphics.PrintUnoptimized();
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Time.Update();
				Console.SetCursorPosition(0, 22);
				Console.WriteLine($"t {Time.DeltaTimeMs}   {player.Direction.UnitVectorToRadians()} ");
				float playerSpeed = 5;
				float playerRotationRadianSpeed = MathF.PI * 4;
				while (Console.KeyAvailable) {
					char c = Console.ReadKey().KeyChar;
					switch (c) {
						//case 'w': player.Position += Vec2.DirectionMinY / 5; break;
						//case 'a': player.Position += Vec2.DirectionMinX / 5; break;
						//case 's': player.Position += Vec2.DirectionMaxY / 5; break;
						//case 'd': player.Position += Vec2.DirectionMaxX / 5; break;
						case 'w': player.Velocity = player.Direction * playerSpeed; break;
						case 's': player.Velocity = player.Direction * -playerSpeed; break;
						case ' ': player.Velocity = Vec2.Zero; break;
						case 'a':
							player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : -playerRotationRadianSpeed;
							break;
						case 'd':
							player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : playerRotationRadianSpeed;
							break;
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
						case '9': player.SmoothRotateTarget(MathF.PI * -1 / 4, playerRotationRadianSpeed); break;
						case '1': player.SmoothRotateTarget(MathF.PI * 3 / 4, playerRotationRadianSpeed); break;
						case '2': player.SmoothRotateTarget(MathF.PI * 2 / 4, playerRotationRadianSpeed); break;
						case '3': player.SmoothRotateTarget(MathF.PI * 1 / 4, playerRotationRadianSpeed); break;
						case '4': player.SmoothRotateTarget(MathF.PI * 4 / 4, playerRotationRadianSpeed); break;
						case '6': player.SmoothRotateTarget(MathF.PI * 0 / 4, playerRotationRadianSpeed); break;
						case '7': player.SmoothRotateTarget(MathF.PI * -3 / 4, playerRotationRadianSpeed); break;
						case '8': player.SmoothRotateTarget(MathF.PI * -2 / 4, playerRotationRadianSpeed); break;
					}
				}
				Time.SleepWithoutConsoleKeyPress(10);
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

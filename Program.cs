using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	internal class Program {
		static void Main(string[] args) {
			ConsoleGlyph[] blueSamples = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkBlue),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Blue),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkCyan),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Cyan)
			};
			ConsoleGlyph[] redSamples = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkMagenta),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Magenta),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkRed),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Red)
			};
			ConsoleGlyph[] yellowSamples = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkGreen),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Green),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkYellow),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Yellow)
			};
			ConsoleGlyph[] darkGraySamples = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkGray),
			};
			KeyInput keyInput = new KeyInput();

			Vec2 scale = (0.5f, 1);
			Vec2 offset = (0, 0);
			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 20, (0.5f, 1), (0,0), blueSamples);
			bool running = true;
			float moveAdjust = 0.25f;
			float sqrt3 = MathF.Sqrt(3);
			float projScale = 3;
			float projRotation = 10;
			Vec2[] projectilePoly = new Vec2[3] { (projScale / sqrt3, 0), (-projScale / (2 * sqrt3), -projScale / 2), (-projScale / (2 * sqrt3), projScale / 2) };
			int currentProjectile = 0;
			MobilePolygon[] projectiles = new MobilePolygon[100];
			for (int i = 0; i < projectiles.Length; ++i) {
				MobilePolygon projectile = projectiles[i] = new MobilePolygon(projectilePoly);
				projectile.Direction = Vec2.DirectionMaxX;
				projectile.IsActive = false;
				projectile.DrawSetup = g => g.ColorPerSample = redSamples;
			}
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			MobileCircle circle = new MobileCircle(new Circle((18, 12), 10));
			circle.DrawSetup = g => g.ColorPerSample = yellowSamples;
			float playerRotationAngle = 5;
			ControlledPolygon player = new ControlledPolygon(playerPoly);
			player.DrawSetup = g => g.ColorPerSample = blueSamples;
			player.DirectionMatchesVelocity = true;
			player.Velocity = (1, 1);

			player.MaxSpeed = 5;
			float playerAutoRotationSpeedRadianPerSecond = MathF.PI * 4;
			float playerFreeRotationSpeedRadianPerSecond = MathF.PI * 2;
			float minThrustDuration = 1f / 4;
			float lineWidth = 1f/2;
			float lineLength = 20;
			float projectileSpeed = 15;
			Vec2[] line = new Vec2[4] { (lineLength/2, lineWidth / -2), (lineLength/2, lineWidth / 2), (lineLength, lineWidth / 2), (lineLength, lineWidth / -2), };
			Polygon linePoly = new Polygon(line);
			void RefreshLine() {
				float len = lineLength * graphics.Scale.y;
				float wid = lineWidth * graphics.Scale.y;
				line[0] = (len / 4, wid / -2);
				line[1] = (len / 4, wid / 2);
				line[2] = (len, wid / 2);
				line[3] = (len, wid / -2);
				linePoly.SetDirty();
			}

			List<IGameObject> objects = new List<IGameObject>();
			objects.Add(player);
			objects.Add(circle);
			objects.AddRange(projectiles);
			List<Action> extraUpdates = new List<Action>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			extraUpdates.Add(() => {
				for (int i = 0; i < projectiles.Length; i++) {
					projectiles[i].RotationRadians += Time.DeltaTimeSeconds * projRotation;
				}
			});
			preDraw.Add(() => {
				linePoly.Direction = player.Direction;
				linePoly.Position = player.Position;
				graphics.ColorPerSample = darkGraySamples;
				linePoly.Draw(graphics);
			});
			postDraw.Add(() => {
				for (int i = 0; i < projectiles.Length; i++) {
					if (!projectiles[i].IsActive) {
						continue;
					}
					graphics.WriteAt(ConsoleGlyph.Convert("p" + i, ConsoleColor.Magenta), projectiles[i].Position);
				}
				graphics.WriteAt(ConsoleGlyph.Convert("player", ConsoleColor.Green), player.Position);
				graphics.WriteAt(ConsoleGlyph.Convert("t0", ConsoleColor.Red), circle.Position);
			});

			keyInput.BindKey((char)27, ki => running = false);
			keyInput.BindKey('w', playerForward);
			keyInput.BindKey('s', playerBrakes);
			keyInput.BindKey(' ', playerShoot);
			void playerForward(KeyInput ki) {
				Thrust();
				player.RotationRadiansPerSecond = 0;
			}
			void playerBrakes(KeyInput ki) {
				player.Brakes();
				player.RotationRadiansPerSecond = 0;
			}
			void playerShoot(KeyInput ki) {
				MobileObject projectile = projectiles[currentProjectile];
				projectile.Position = player.Position + player.Direction * playerPoly[0].x;
				projectile.Direction = player.Direction;
				projectile.Velocity = player.Velocity + player.Direction * projectileSpeed;
				projectile.IsActive = true;
				++currentProjectile;
				if (currentProjectile >= projectiles.Length) {
					currentProjectile = 0;
				}
			}
			keyInput.BindKey('-', zoomOut);
			keyInput.BindKey('=', zoomIn);
			void zoomIn(KeyInput ki) {
				if (graphics.Scale.x < 1f / 128) { return; }
				graphics.Scale /= 1.5f;
				RefreshLine();
			}
			void zoomOut(KeyInput ki) {
				if (graphics.Scale.x > 128) { return; }
				graphics.Scale *= 1.5f;
				RefreshLine();
			}

			RefreshLine();
			while (running) {
				Time.Update();
				//keyInput.Update();
				objects.ForEach(o => o.Update());
				extraUpdates.ForEach(a => a.Invoke());

				// draw
				Vec2 halfScreen = graphics.Size / 2;
				graphics.Offset = player.Position - halfScreen.Scaled(graphics.Scale);
				preDraw.ForEach(a => a.Invoke());
				objects.ForEach(o => {
					o.DrawSetup?.Invoke(graphics);
					o.Draw(graphics);
				});
				postDraw.ForEach(a => a.Invoke());

				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, 22);
				Console.WriteLine($"t {Time.DeltaTimeMs}   {player.Direction.UnitVectorToRadians()} {player._brake}   {player.Speed}");
				while (Console.KeyAvailable) {
					char c = Console.ReadKey().KeyChar;
					switch (c) {
						case 'w':
							Thrust();
							player.RotationRadiansPerSecond = 0;
							break;
						case 's':
							player.Brakes();
							player.RotationRadiansPerSecond = 0;
							break;
						case 'a': player.RotationDegrees -= playerRotationAngle; break;
						case 'd': player.RotationDegrees += playerRotationAngle; break;
						case 'q':
							player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : -playerFreeRotationSpeedRadianPerSecond;
							player.TargetRotation = float.NaN;
							break;
						case 'e':
							player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : playerFreeRotationSpeedRadianPerSecond;
							player.TargetRotation = float.NaN;
							break;
						case '9': MovePlayer(-1 / 4f); break;
						case '1': MovePlayer(3 / 4f); break;
						case '2': MovePlayer(2 / 4f); break;
						case '3': MovePlayer(1 / 4f); break;
						case '4': MovePlayer(4 / 4f); break;
						case '6': MovePlayer(0 / 4f); break;
						case '7': MovePlayer(-3 / 4f); break;
						case '8': MovePlayer(-2 / 4f); break;
						case ' ':
							MobileObject projectile = projectiles[currentProjectile];
							projectile.Position = player.Position + player.Direction * playerPoly[0].x;
							projectile.Direction = player.Direction;
							projectile.Velocity = player.Velocity + player.Direction * projectileSpeed;
							projectile.IsActive = true;
							++currentProjectile;
							if (currentProjectile >= projectiles.Length) {
								currentProjectile = 0;
							}
							break;
						case 'i': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMinY / 2); break;
						case 'j': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMinX / 2); break;
						case 'k': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMaxY / 2); break;
						case 'l': graphics.Offset += graphics.Scale.Scaled(Vec2.DirectionMaxX / 2); break;
						case 'r': circle.Radius -= moveAdjust / 2; break;
						case 't': circle.Radius += moveAdjust / 2; break;
						case '=': if (graphics.Scale.x > 1f / 128) { graphics.Scale /= 1.5f; RefreshLine(); } break;
						case '-': if (graphics.Scale.x < 128) { graphics.Scale *= 1.5f; RefreshLine(); } break;
						case 'x': circle.Position += Vec2.Random - Vec2.Half; break;
						case (char)27: running = false; break;
					}
				}
				Time.SleepWithoutConsoleKeyPress(5);
			}

			void MovePlayer(float normalizedRadian) {
				player.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationSpeedRadianPerSecond);
				Thrust();
				player.AutoStopWithoutThrust = true;
			}
			void Thrust() {
				player.AutoStopWithoutThrust = false;
				player.ThrustDuration = minThrustDuration;
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

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
			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 25, (0.5f, 1), (0,0), blueSamples);
			bool running = true;
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
			float minThrustDuration = 1f / 2;
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
			preDraw.Add(() => {
				Vec2 halfScreen = graphics.Size / 2;
				graphics.Offset = player.Position - halfScreen.Scaled(graphics.Scale);
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

			keyInput.BindKey('w', playerForward);
			keyInput.BindKey('s', playerBrakes);
			keyInput.BindKey('a', playerTurnLeft);
			keyInput.BindKey('d', playerTurnRight);
			keyInput.BindKey('q', playerSpinLeft);
			keyInput.BindKey('e', playerSpinRight);
			keyInput.BindKey(' ', playerShoot);
			keyInput.BindKey('1', ki => MovePlayer(3 / 4f));
			keyInput.BindKey('2', ki => MovePlayer(2 / 4f));
			keyInput.BindKey('3', ki => MovePlayer(1 / 4f));
			keyInput.BindKey('4', ki => MovePlayer(4 / 4f));
			keyInput.BindKey('5', playerBrakes);
			keyInput.BindKey('6', ki => MovePlayer(0 / 4f));
			keyInput.BindKey('7', ki => MovePlayer(-3 / 4f));
			keyInput.BindKey('8', ki => MovePlayer(-2 / 4f));
			keyInput.BindKey('9', ki => MovePlayer(-1 / 4f));
			void MovePlayer(float normalizedRadian) {
				player.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationSpeedRadianPerSecond);
				Thrust();
				player.AutoStopWithoutThrust = true;
			}
			void Thrust() {
				player.AutoStopWithoutThrust = false;
				player.ThrustDuration = minThrustDuration;
			}
			void playerForward(KeyInput ki) {
				Thrust();
				player.RotationRadiansPerSecond = 0;
			}
			void playerBrakes(KeyInput ki) {
				player.Brakes();
				player.RotationRadiansPerSecond = 0;
			}
			void playerTurnLeft(KeyInput keyInput) { player.RotationDegrees -= playerRotationAngle; }
			void playerTurnRight(KeyInput keyInput) { player.RotationDegrees += playerRotationAngle; }
			void playerSpinLeft(KeyInput keyInput) {
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : -playerFreeRotationSpeedRadianPerSecond;
				player.TargetRotation = float.NaN;
			}
			void playerSpinRight(KeyInput keyInput) {
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : playerFreeRotationSpeedRadianPerSecond;
				player.TargetRotation = float.NaN;
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
			keyInput.BindKey((char)27, ki => running = false);
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

			List<IGameObject> objects = new List<IGameObject>() { player, circle };
			objects.AddRange(projectiles);

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				objects.ForEach(o => o.Update());
				extraUpdates.ForEach(a => a.Invoke());

				// draw
				preDraw.ForEach(a => a.Invoke());
				objects.ForEach(o => {
					o.DrawSetup?.Invoke(graphics);
					o.Draw(graphics);
				});
				postDraw.ForEach(a => a.Invoke());
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, (int)graphics.Size.y);
				Time.SleepWithoutConsoleKeyPress(5);
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

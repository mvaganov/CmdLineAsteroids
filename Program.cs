using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	internal class Program {
		static void Main(string[] args) {
			// initialize system
			// TODO make gradient array in the graphics context
			ConsoleGlyph[] blueGradient = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkBlue),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Blue),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkCyan),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Cyan)
			};
			ConsoleGlyph[] redGradient = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkMagenta),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Magenta),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkRed),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Red)
			};
			ConsoleGlyph[] yellowGradient = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkGreen),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Green),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkYellow),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Yellow)
			};
			ConsoleGlyph[] darkGrayNoGradient = {
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.Black),
				new ConsoleGlyph(' ', ConsoleColor.Gray, ConsoleColor.DarkGray),
			};
			KeyInput keyInput = new KeyInput();
			List<Action> extraUpdates = new List<Action>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();

			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 25, (0.5f, 1), (0,0), blueGradient);
			bool running = true;

			// initialize player
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			float playerRotationAngle = 5;
			ControlledPolygon player = new ControlledPolygon(playerPoly);
			player.DrawSetup = g => g.AntiAliasedGradient = blueGradient;
			player.DirectionMatchesVelocity = true;
			player.Velocity = (1, 1);
			player.MaxSpeed = 5;
			// TODO collision
			player.CollisionCircles = new Circle[] {
				new Circle((1.25f,0), 1.25f),
				new Circle((3.625f,0), 0.5f),
				new Circle((-0.6875f,-1.4375f), .5f),
				new Circle((-0.6875f, 1.4375f), .5f)
			};
			player.CollisionBoundingCircle = new Circle((1.1875f, 0), 3f);
			float playerAutoRotationSpeedRadianPerSecond = MathF.PI * 4;
			float playerFreeRotationSpeedRadianPerSecond = MathF.PI * 2;
			float playerMinThrustDuration = 1f / 2;
			preDraw.Add(() => {
				Vec2 halfScreen = graphics.Size / 2;
				graphics.Offset = player.Position - halfScreen.Scaled(graphics.Scale);
			});

			// initialize projectiles
			float projectileScale = 3;
			float projectileRotation = 10;
			float projectileSpeed = 15;
			float sqrt3 = MathF.Sqrt(3);
			Vec2[] projectilePoly = new Vec2[3] {
				(projectileScale / sqrt3, 0),
				(-projectileScale / (2 * sqrt3), -projectileScale / 2),
				(-projectileScale / (2 * sqrt3), projectileScale / 2)
			};
			int currentProjectile = 0;
			MobilePolygon[] projectiles = new MobilePolygon[100];
			for (int i = 0; i < projectiles.Length; ++i) {
				MobilePolygon projectile = projectiles[i] = new MobilePolygon(projectilePoly);
				projectile.Direction = Vec2.DirectionMaxX;
				projectile.IsActive = false;
				projectile.DrawSetup = g => g.AntiAliasedGradient = redGradient;
			}
			extraUpdates.Add(() => {
				for (int i = 0; i < projectiles.Length; i++) {
					projectiles[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
				}
			});

			// initialize asteroids
			MobileCircle circle = new MobileCircle(new Circle((18, 12), 10));
			circle.DrawSetup = g => g.AntiAliasedGradient = yellowGradient;

			// initialize direction marker
			float lineWidth = 1f/2;
			float lineLength = 20;
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
			preDraw.Add(() => {
				linePoly.Direction = player.Direction;
				linePoly.Position = player.Position;
				graphics.AntiAliasedGradient = darkGrayNoGradient;
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

			// initialize key binding
			keyInput.BindKey((char)27, quit);
			keyInput.BindKey('-', zoomOut);
			keyInput.BindKey('=', zoomIn);
			void quit(KeyInput ki) => running = false;
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
			//int colCirc = 4;
			//float inc = 1f / 32;
			//keyInput.BindKey('j', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Position += (inc, 0);
			//	player.CollisionCircles[colCirc] = c;
			//});
			//keyInput.BindKey('l', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Position -= (inc, 0);
			//	player.CollisionCircles[colCirc] = c;
			//});
			//keyInput.BindKey('i', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Position += (0, inc);
			//	player.CollisionCircles[colCirc] = c;
			//});
			//keyInput.BindKey('k', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Position -= (0, inc);
			//	player.CollisionCircles[colCirc] = c;
			//});
			//keyInput.BindKey('[', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Radius -= inc;
			//	player.CollisionCircles[colCirc] = c;
			//});
			//keyInput.BindKey(']', k => {
			//	Circle c = player.CollisionCircles[colCirc];
			//	c.Radius += inc;
			//	player.CollisionCircles[colCirc] = c;
			//});

			// player keybinding
			keyInput.BindKey('w', playerForward);
			keyInput.BindKey('s', playerBrakes);
			keyInput.BindKey('a', playerTurnLeft);
			keyInput.BindKey('d', playerTurnRight);
			keyInput.BindKey('q', playerSpinLeft);
			keyInput.BindKey('e', playerSpinRight);
			keyInput.BindKey(' ', playerShoot);
			keyInput.BindKey('1', k => playerMove(3 / 4f));
			keyInput.BindKey('2', k => playerMove(2 / 4f));
			keyInput.BindKey('3', k => playerMove(1 / 4f));
			keyInput.BindKey('4', k => playerMove(4 / 4f));
			keyInput.BindKey('5', playerBrakes);
			keyInput.BindKey('6', k => playerMove(0 / 4f));
			keyInput.BindKey('7', k => playerMove(-3 / 4f));
			keyInput.BindKey('8', k => playerMove(-2 / 4f));
			keyInput.BindKey('9', k => playerMove(-1 / 4f));
			void playerMove(float normalizedRadian) {
				player.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationSpeedRadianPerSecond);
				Thrust();
				player.AutoStopWithoutThrust = true;
			}
			void Thrust() {
				player.AutoStopWithoutThrust = false;
				player.ThrustDuration = playerMinThrustDuration;
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
				// TODO attack cooldown
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

			List<IGameObject> objects = new List<IGameObject>() { player, circle };
			List<ICollidable> collidables = new List<ICollidable>() { player, circle };
			objects.AddRange(projectiles);
			collidables.AddRange(projectiles);

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				// TODO collision detection here
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
	}
}

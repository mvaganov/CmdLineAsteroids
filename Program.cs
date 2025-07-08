using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	internal class Program {
		enum AsteroidType { None, Player, Asteroid, Projectile }
		static void Main(string[] args) {
			// initialize system
			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 25, (0.5f, 1), (0, 0));
			KeyInput keyInput = new KeyInput();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			List<Action> extraUpdates = new List<Action>();
			List<IGameObject> objects = new List<IGameObject>();
			List<IGameObject> updateList = new List<IGameObject>();
			List<IGameObject> drawList = new List<IGameObject>();
			List<ICollidable> collideList = new List<ICollidable>();
			bool running = true;
			void PopulateUpdateLists() {
				updateList.Clear();
				drawList.Clear();
				collideList.Clear();
				for (int i = 0; i < objects.Count; ++i) {
					IGameObject obj = objects[i];
					if (!obj.IsActive) {
						continue;
					}
					drawList.Add(obj);
					updateList.Add(obj);
					if (obj is ICollidable collidable) {
						collideList.Add(collidable);
					}
				}
			}

			// initialize player
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			float playerRotationAngleDegrees = 5;
			long playerShootCooldownMs = 50;
			long playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			long playerScore = 0;
			ControlledPolygon player = new ControlledPolygon(playerPoly);
			player.TypeId = (int)AsteroidType.Player;
			player.DrawSetup = g => g.UseColorGradient(ConsoleColor.Blue);
			player.DirectionMatchesVelocity = true;
			player.MaxSpeed = 5;
			player.CollisionCircles = new Circle[] {
				new Circle((1.25f,0), 1.25f),
				new Circle((3.625f,0), 0.5f),
				new Circle((-0.6875f,-1.4375f), .5f),
				new Circle((-0.6875f, 1.4375f), .5f)
			};
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
				projectile.TypeId = (int)AsteroidType.Projectile;
				projectile.Direction = Vec2.DirectionMaxX;
				projectile.IsActive = false;
				projectile.DrawSetup = g => g.UseColorGradient(ConsoleColor.Red);
			}
			extraUpdates.Add(() => {
				for (int i = 0; i < projectiles.Length; i++) {
					projectiles[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
				}
			});

			// initialize asteroids
			MobileCircle[] asteroids = new MobileCircle[10];
			Vec2 asteroidStartRadius = new Vec2(40, 0);
			for (int i = 0; i < asteroids.Length; ++i) {
				MobileCircle asteroid = new MobileCircle(new Circle(asteroidStartRadius, 10));
				asteroid.DrawSetup = AsteroidDrawSetup;
				asteroid.TypeId = (int)AsteroidType.Asteroid;
				asteroids[i] = asteroid;
				asteroidStartRadius.RotateRadians(MathF.PI * 2/ asteroids.Length);
			}
			void AsteroidDrawSetup(CommandLineGraphicsContext context) {
				context.UseColorGradient(ConsoleColor.Yellow);
			}

			// initialize direction marker
			float lineWidth = 1f/2;
			float lineLength = 20;
			Vec2[] line = new Vec2[4] {
				(lineLength / 4, lineWidth / -2),
				(lineLength / 4, lineWidth / 2),
				(lineLength, lineWidth / 2),
				(lineLength, lineWidth / -2),
			};
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
				graphics.UseColorGradient(ConsoleColor.DarkGray);
				linePoly.Draw(graphics);
			});

			postDraw.Add(DrawScore);
			postDraw.Add(DebugDraw);
			void DrawScore() {
				graphics.WriteAt($"score: {playerScore}", (int)graphics.Size.y - 1, 0);
			}
			void DebugDraw(){
				LabelList(projectiles, "p", ConsoleColor.Magenta);
				graphics.WriteAt(ConsoleGlyph.Convert("player", ConsoleColor.Green), player.Position);
				LabelList(asteroids, "a", ConsoleColor.DarkYellow);
			}
			void LabelList(IList<IGameObject> objects, string prefix, ConsoleColor textColor) {
				for (int i = 0; i < objects.Count; i++) {
					if (!objects[i].IsActive) {
						continue;
					}
					graphics.WriteAt(ConsoleGlyph.Convert(prefix + i, textColor), objects[i].Position);
				}
			}

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
			void playerTurnLeft(KeyInput keyInput) { player.RotationDegrees -= playerRotationAngleDegrees; }
			void playerTurnRight(KeyInput keyInput) { player.RotationDegrees += playerRotationAngleDegrees; }
			void playerSpinLeft(KeyInput keyInput) {
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : -playerFreeRotationSpeedRadianPerSecond;
				player.TargetRotation = float.NaN;
			}
			void playerSpinRight(KeyInput keyInput) {
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : playerFreeRotationSpeedRadianPerSecond;
				player.TargetRotation = float.NaN;
			}
			void playerShoot(KeyInput ki) {
				// TODO ammo limit
				if (Time.TimeMs < playerShootNextPossibleMs) {
					return;
				}
				MobileObject projectile = projectiles[currentProjectile];
				projectile.Position = player.Position + player.Direction * playerPoly[0].x;
				projectile.Direction = player.Direction;
				projectile.Velocity = player.Velocity + player.Direction * projectileSpeed;
				projectile.IsActive = true;
				++currentProjectile;
				if (currentProjectile >= projectiles.Length) {
					currentProjectile = 0;
				}
				playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			}

			var collisionRules = new Dictionary<CollisionPair, List<CollisionLogic.Function>>() {
				[(typeof(MobileCircle), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(a, b) => {
						// TODO asteroids that collide with each other should bounce off of each other
						if (a.TypeId == (byte)AsteroidType.Asteroid && b.TypeId == (byte)AsteroidType.Asteroid) {
						}
					}
				},
				[(typeof(MobilePolygon), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(poly, circle) => {
						if (poly.TypeId == (byte)AsteroidType.Projectile && circle.TypeId == (byte)AsteroidType.Asteroid) {
							MobilePolygon projectile = (MobilePolygon)poly;
							MobileCircle asteroid = (MobileCircle)circle;
							projectile.IsActive= false;
							asteroid.IsActive = false;
							// TODO create smaller asteroids, until a small enough size. after the minimum size, asteroids become ammo powerups
							++playerScore;
						}
					}
				}
			};

			objects.Add(player);
			objects.AddRange(projectiles);
			objects.AddRange(asteroids);

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				CollisionLogic.DoCollisionLogic(collideList, collisionRules);
				PopulateUpdateLists();
				updateList.ForEach(o => o.Update());
				extraUpdates.ForEach(a => a.Invoke());

				// draw
				preDraw.ForEach(a => a.Invoke());
				drawList.ForEach(o => {
					o.DrawSetup?.Invoke(graphics);
					o.Draw(graphics);
				});
				postDraw.ForEach(a => a.Invoke());
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, (int)graphics.Size.y);
				Time.SleepWithoutConsoleKeyPress(15);
			}
		}
	}
}

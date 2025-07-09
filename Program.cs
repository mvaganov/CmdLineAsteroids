using asteroids.src.asteroids;
using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	internal class Program {
		enum AsteroidType { None, Player, Asteroid, Projectile, Powerup }
		static void Main(string[] args) {
			// initialize system
			Random random = new Random();
			CommandLineGraphicsContext graphics = new CommandLineGraphicsContext(80, 25, (0.5f, 1), (0, 0));
			KeyInput keyInput = new KeyInput();
			List<IGameObject> objects = new List<IGameObject>();
			List<IGameObject> updateList = new List<IGameObject>();
			List<IDrawable> drawList = new List<IDrawable>();
			List<ICollidable> collideList = new List<ICollidable>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			List<Action> postUpdate = new List<Action>();
			bool running = true;
			bool updating = true;
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
			postUpdate.Add(() => {
				for (int i = 0; i < projectiles.Length; i++) {
					projectiles[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
				}
			});

			// initialize asteroids / powerups
			MobileCircle[] asteroids = new MobileCircle[100];
			int activeAsteroidCount = 10;
			float asteroidStartRadius = 10;
			float asteroidMinimumRadius = 2;
			float asteroidBreakupCount = 3;
			Vec2 asteroidStartPosition = new Vec2(40, 0);
			for (int i = 0; i < asteroids.Length; ++i) {
				MobileCircle asteroid = new MobileCircle(new Circle(asteroidStartPosition, asteroidStartRadius));
				asteroid.DrawSetup = AsteroidDrawSetup;
				asteroid.TypeId = (int)AsteroidType.Asteroid;
				asteroids[i] = asteroid;
				asteroid.IsActive = i < activeAsteroidCount;
				if (asteroid.IsActive) {
					asteroidStartPosition.RotateRadians(MathF.PI * 2 / activeAsteroidCount);
				}
			}
			void AsteroidDrawSetup(CommandLineGraphicsContext context) {
				context.UseColorGradient(ConsoleColor.Yellow);
			}
			void BreakApartAsteroid(MobileCircle asteroid, MobileObject projectile) {
				if (asteroid.Radius <= asteroidMinimumRadius*2) {
					if (asteroids[activeAsteroidCount-1] != asteroid) {
						MobileCircle lastAsteroid = asteroids[activeAsteroidCount - 1];
						asteroid.Copy(lastAsteroid);
						lastAsteroid.IsActive = false;
					} else {
						asteroid.IsActive = false;
					}
					--activeAsteroidCount;
					CreatePowerup(projectile);
				} else {
					Vec2 deltaFromCenter = projectile.Position - asteroid.Position;
					Vec2 direction = deltaFromCenter.Normal;
					float degreesSeperatingFragments = 360f / asteroidBreakupCount;
					float newRadius = asteroid.Radius / 2;
					Vec2 positionRadius = direction * newRadius;
					positionRadius.RotateDegrees(degreesSeperatingFragments/2);
					for(int i = 0; i < asteroidBreakupCount -1; ++i) {
						if (activeAsteroidCount < asteroids.Length) {
							MobileCircle newAsteroid = asteroids[activeAsteroidCount];
							++activeAsteroidCount;
							newAsteroid.IsActive = true;
							newAsteroid.Radius = newRadius;
							newAsteroid.Position = asteroid.Position + positionRadius;
							newAsteroid.Velocity = asteroid.Velocity + positionRadius;
						}
						positionRadius.RotateDegrees(degreesSeperatingFragments);
					}
					asteroid.IsActive = true;
					asteroid.Radius = newRadius;
					asteroid.Position = asteroid.Position + positionRadius;
					asteroid.Velocity = asteroid.Velocity + positionRadius;
				}
			}

			// initialize powerups
			MobileCircle[] powerups = new MobileCircle[100];
			int activePowerupCount = 0;
			float powerupRadius = asteroidMinimumRadius / 2;
			for (int i = 0; i < asteroids.Length; ++i) {
				MobileCircle powerup = new MobileCircle(new Circle(Vec2.Zero, powerupRadius));
				powerup.DrawSetup = PowerupDrawSetup;
				powerup.TypeId = (int)AsteroidType.Powerup;
				powerups[i] = powerup;
				powerup.IsActive = false;
			}
			void PowerupDrawSetup(CommandLineGraphicsContext context) {
				context.UseColorGradient(ConsoleColor.Cyan);
			}
			void CreatePowerup(MobileObject projectile) {
				if (activePowerupCount >= powerups.Length) {
					return;
				}
				MobileCircle powerup = powerups[activePowerupCount];
				powerup.IsActive = true;
				powerup.Position = projectile.Position;
				powerup.Velocity = -projectile.Velocity.Normal;
				activePowerupCount++;
			}

			// add acceleration force to bring objects back to center if they stray too far
			float WorldExtentSize = 50;
			Vec2 WorldMin = new Vec2(-WorldExtentSize, -WorldExtentSize);
			Vec2 WorldMax = new Vec2(WorldExtentSize, WorldExtentSize);
			postUpdate.Add(BringBackStrayObjects);
			void BringBackStrayObjects() {
				for (int i = 0; i < objects.Count; ++i) {
					MobileObject mob = objects[i] as MobileObject;
					if (!mob.IsActive) {
						continue;
					}
					if (IsObjectStraying(mob)) {
						NudgeObjectBackToWorld(mob);
					}
				}
			}
			bool IsObjectStraying(MobileObject obj) => !obj.Position.IsWithin(WorldMin, WorldMax);
			void NudgeObjectBackToWorld(MobileObject obj) {
				Vec2 p = obj.Position;
				Vec2 v = obj.Velocity;
				if (p.x < WorldMin.x && v.x < 1) { v.x += Time.DeltaTimeSeconds; }
				if (p.x > WorldMax.x && v.x > -1) { v.x -= Time.DeltaTimeSeconds; }
				if (p.y < WorldMin.y && v.y < 1) { v.y += Time.DeltaTimeSeconds; }
				if (p.y > WorldMin.y && v.y > -1) { v.y -= Time.DeltaTimeSeconds; }
			}

			// direction marker, underlay GUI
			float lineWidth = 1f/2;
			float lineLength = 20;
			preDraw.Add(() => {
				graphics.UseColorGradient(ConsoleColor.DarkGray);
				Vec2 lineEnd = player.Position + player.Direction * (lineLength * graphics.Scale.y);
				graphics.DrawLine(player.Position, lineEnd, lineWidth);
			});

			// additional labels, overlay GUI
			postDraw.Add(DrawScore);
			postDraw.Add(DebugDraw);
			void DrawScore() {
				graphics.WriteAt($"score: {playerScore}", (int)graphics.Size.y - 1, 0);
			}
			void DebugDraw(){
				LabelList(projectiles, "p", ConsoleColor.Magenta);
				graphics.WriteAt(ConsoleGlyph.Convert("player", ConsoleColor.Green), player.Position);
				LabelList(asteroids, "a", ConsoleColor.DarkYellow);
				LabelList(powerups, "+", ConsoleColor.Green);
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
			keyInput.BindKey('p', toggleUpdating);
			keyInput.BindKey('-', zoomOut);
			keyInput.BindKey('=', zoomIn);
			void quit(KeyInput ki) => running = false;
			void toggleUpdating(KeyInput ki) => updating = !updating;
			void zoomIn(KeyInput ki) {
				if (graphics.Scale.x < 1f / 128) { return; }
				graphics.Scale /= 1.5f;
			}
			void zoomOut(KeyInput ki) {
				if (graphics.Scale.x > 128) { return; }
				graphics.Scale *= 1.5f;
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
			keyInput.BindKey('i', k => player.Position += Vec2.DirectionMinY);
			keyInput.BindKey('j', k => player.Position += Vec2.DirectionMinX);
			keyInput.BindKey('k', k => player.Position += Vec2.DirectionMaxY);
			keyInput.BindKey('l', k => player.Position += Vec2.DirectionMaxX);
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
						if (a.TypeId == (byte)AsteroidType.Asteroid && b.TypeId == (byte)AsteroidType.Asteroid) {
							MobileCircle astA = (MobileCircle)a;
							MobileCircle astB = (MobileCircle)b;
							Vec2 delta = astB.Position - astA.Position;
							Vec2 dir = delta.Normal;
							Vec2 center = delta / 2 + astA.Position;
							Action postCollisionReflection = () => {
								astA.Velocity = Vec2.Reflect(astA.Velocity, dir);
								astB.Velocity = Vec2.Reflect(astB.Velocity, dir);
							};
							return postCollisionReflection;
						}
						return null;
					}
				},
				[(typeof(MobilePolygon), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(poly, circle) => {
						if (poly.TypeId == (byte)AsteroidType.Projectile && circle.TypeId == (byte)AsteroidType.Asteroid) {
							MobilePolygon projectile = (MobilePolygon)poly;
							MobileCircle asteroid = (MobileCircle)circle;
							BreakApartAsteroid(asteroid, projectile);
							projectile.IsActive= false;
							++playerScore;
							return null;
						}
						if (poly.TypeId == (byte)AsteroidType.Player) {
							switch(circle.TypeId) {
								case (byte)AsteroidType.Asteroid:
									Log.i("TODO small asteroids bounce, and turn into powerups moving away. big asteroids cause damage");
									break;
								case (byte)AsteroidType.Powerup:
									Log.i("TODO powerup");
									break;
							}
						}
						return null;
					}
				}
			};

			objects.Add(player);
			objects.AddRange(projectiles);
			objects.AddRange(asteroids);
			objects.AddRange(powerups);

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				if (updating) {
					CollisionLogic.DoCollisionLogic(collideList, collisionRules);
					PopulateUpdateLists();
					updateList.ForEach(o => o.Update());
					postUpdate.ForEach(a => a.Invoke());
				}

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

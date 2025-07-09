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
			List<ICollidable> collideList = new List<ICollidable>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			List<Action> postUpdate = new List<Action>();
			bool running = true;
			bool updating = true;

			// initialize player
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			float playerRotationAngleDegrees = 5;
			long playerShootCooldownMs = 50;
			long playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			long playerScore = 0;
			ControlledPolygon player = new ControlledPolygon(playerPoly);
			player.TypeId = (int)AsteroidType.Player;
			player.DrawSetup = g => g.SetColor(ConsoleColor.Blue);
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
			objects.Add(player);
			collideList.Add(player);

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
			ObjectPool<MobilePolygon> projectilePool = new ObjectPool<MobilePolygon>();
			projectilePool.Setup(() => {
				MobilePolygon projectile = new MobilePolygon(projectilePoly);
				projectile.TypeId = (int)AsteroidType.Projectile;
				projectile.DrawSetup = DrawSetupProjectile;
				return projectile;
			}, projectile => {
				projectile.IsActive = true;
				objects.Add(projectile);
				collideList.Add(projectile);
			}, projectile => {
				projectile.IsActive = false;
				objects.Remove(projectile);
				collideList.Remove(projectile);
			}, projectile => {
				projectile.DrawSetup = null;
				projectile.TypeId = (int)AsteroidType.None;
			});
			void DrawSetupProjectile(CommandLineGraphicsContext g) => g.SetColor(ConsoleColor.Red);
			postUpdate.Add(() => {
				for (int i = 0; i < projectilePool.Count; i++) {
					projectilePool[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
				}
			});

			// initialize asteroids / powerups
			float asteroidStartRadius = 10;
			ObjectPool<MobileCircle> asteroidPool = new ObjectPool<MobileCircle>();
			asteroidPool.Setup(() => new MobileCircle(new Circle()), asteroid => {
				asteroid.DrawSetup = AsteroidDrawSetup;
				asteroid.TypeId = (int)AsteroidType.Asteroid;
				asteroid.Radius = asteroidStartRadius;
				asteroid.IsActive = true;
				objects.Add(asteroid);
				collideList.Add(asteroid);
			}, asteroid => {
				asteroid.IsActive = false;
				objects.Remove(asteroid);
				collideList.Remove(asteroid);
			}, asteroid => {
				asteroid.DrawSetup = null;
				asteroid.TypeId = (int)AsteroidType.None;
			});
			int activeAsteroidCount = 10;
			float asteroidMinimumRadius = 2;
			float asteroidBreakupCount = 3;
			Vec2 asteroidStartPosition = new Vec2(40, 0);
			for(int i = 0; i < activeAsteroidCount; i++) {
				MobileCircle asteroid = asteroidPool.Alloc();
				asteroid.Position = asteroidStartPosition;
				asteroidStartPosition.RotateRadians(MathF.PI * 2 / activeAsteroidCount);
			}
			void AsteroidDrawSetup(CommandLineGraphicsContext context) {
				context.SetColor(ConsoleColor.Yellow);
			}
			void BreakApartAsteroid(MobileCircle asteroid, MobileObject projectile) {
				if (asteroid.Radius <= asteroidMinimumRadius*2) {
					asteroidPool.Free(asteroid);
					CreatePowerup(projectile);
				} else {
					Vec2 deltaFromCenter = projectile.Position - asteroid.Position;
					Vec2 direction = deltaFromCenter.Normal;
					float degreesSeperatingFragments = 360f / asteroidBreakupCount;
					float newRadius = asteroid.Radius / 2;
					Vec2 positionRadius = direction * newRadius;
					positionRadius.RotateDegrees(degreesSeperatingFragments/2);
					for(int i = 0; i < asteroidBreakupCount; ++i) {
						MobileCircle newAsteroid = asteroidPool.Alloc();
						newAsteroid.IsActive = true;
						newAsteroid.Radius = newRadius;
						newAsteroid.Position = asteroid.Position + positionRadius;
						newAsteroid.Velocity = asteroid.Velocity + positionRadius;
						positionRadius.RotateDegrees(degreesSeperatingFragments);
					}
					asteroidPool.Free(asteroid);
				}
			}

			// initialize powerups
			float powerupRadius = asteroidMinimumRadius / 2;
			ObjectPool<MobileCircle> powerupPool = new ObjectPool<MobileCircle>();
			powerupPool.Setup(() => {
				MobileCircle powerup = new MobileCircle(new Circle(Vec2.Zero, powerupRadius));
				powerup.DrawSetup = PowerupDrawSetup;
				powerup.TypeId = (int)AsteroidType.Powerup;
				return powerup;
			}, powerup => {
				powerup.IsActive = true;
				objects.Add(powerup);
				collideList.Add(powerup);
			}, powerup => {
				powerup.IsActive = false;
				objects.Remove(powerup);
				collideList.Remove(powerup);
			}, powerup => {
				powerup.TypeId = (int)AsteroidType.None;
				powerup.DrawSetup = null;
			});
			void PowerupDrawSetup(CommandLineGraphicsContext context) {
				context.SetColor(ConsoleColor.Cyan);
			}
			void CreatePowerup(MobileObject projectile) {
				MobileCircle powerup = powerupPool.Alloc();
				powerup.IsActive = true;
				powerup.Position = projectile.Position;
				powerup.Velocity = -projectile.Velocity.Normal;
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
				graphics.SetColor(ConsoleColor.DarkGray);
				Vec2 lineEnd = player.Position + player.Direction * (lineLength * graphics.Scale.y);
				graphics.DrawLine(player.Position, lineEnd, lineWidth);
			});

			// additional labels, overlay GUI
			postDraw.Add(DrawScore);
			postDraw.Add(DebugDraw);
			void DrawScore() {
				graphics.WriteAt($"score: {playerScore}", (int)graphics.Size.y - 1, 0);
			}
			void DebugDraw() {
				LabelList(projectilePool, "p", ConsoleColor.Magenta);
				graphics.WriteAt(ConsoleGlyph.Convert("player", ConsoleColor.Green), player.Position);
				LabelList(asteroidPool, "a", ConsoleColor.DarkYellow);
				LabelList(powerupPool, "*", ConsoleColor.Green);
			}
			void LabelList<T>(IList<T> objects, string prefix, ConsoleColor textColor) where T : IGameObject {
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
				//MobileObject projectile = projectiles[currentProjectile];
				MobileObject projectile = projectilePool.Alloc();
				projectile.Position = player.Position + player.Direction * playerPoly[0].x;
				projectile.Direction = player.Direction;
				projectile.Velocity = player.Velocity + player.Direction * projectileSpeed;
				projectile.IsActive = true;
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

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				if (updating) {
					CollisionLogic.DoCollisionLogic(collideList, collisionRules);
					//PopulateUpdateLists();
					//updateList.ForEach(o => o.Update());
					objects.ForEach(o => o.Update());
					postUpdate.ForEach(a => a.Invoke());
				}

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
				Time.SleepWithoutConsoleKeyPress(15);
			}
		}
	}
}

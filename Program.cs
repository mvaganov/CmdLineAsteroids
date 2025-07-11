using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	internal class Program {
		enum AsteroidType { None, Player, Asteroid, Projectile, Powerup }
		public static ParticleSystem marker = new ParticleSystem(.25f, .5f, 0,
			ParticleSystem.Kind.None, ConsoleColor.Magenta, 0, null);
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
			ParticleSystem.OnParticleCommission = particle => objects.Add(particle);
			ParticleSystem.OnParticleDecommission = particle => objects.Remove(particle);
			float targetFps = 30;
			int targetMsDelay = (int)(1000 / targetFps);

			// particle systems
			ParticleSystem explosion = new ParticleSystem((.25f, .5f), (1, 2), (2, 3),
				ParticleSystem.Kind.Explosion, ConsoleColor.Red, 0, ValueOverTime.GrowAndShrink);
			List<ParticleSystem> particleSystems = new List<ParticleSystem>() { marker, explosion };

			// initialize player
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			float playerRotationAngleDegrees = 5;
			long playerShootCooldownMs = 50;
			long playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			long playerScore = 0;
			int playerAmmo = 10;
			float playerMaxHp = 10;
			float playerHp = playerMaxHp;
			ControlledPolygon player = new ControlledPolygon(playerPoly);
			player.TypeId = (int)AsteroidType.Player;
			player.DrawSetup = g => g.SetColor(ConsoleColor.Blue);
			player.DirectionMatchesVelocity = true;
			player.MaxSpeed = 15;
			player.CollisionCircles = new Circle[] {
				new Circle((1.25f, 0), 1.25f),
				new Circle((-0.6875f,-1.4375f), 0.5f),
				new Circle((-0.6875f, 1.4375f), 0.5f),
				new Circle((3.625f,0), 0.5f),
			};
			float playerAutoRotationSpeedRadianPerSecond = MathF.PI * 4;
			float playerFreeRotationSpeedRadianPerSecond = MathF.PI * 2;
			float playerMinThrustDuration = 1f / 2;
			bool cameraLookAhead = false;
			preDraw.Add(() => {
				Vec2 halfScreen = graphics.Size / 2;
				Vec2 screenAnchor = player.Position - halfScreen.Scaled(graphics.Scale);
				if (cameraLookAhead) {
					Vec2 cameraTargetScreenOffset = screenAnchor + player.Velocity * (graphics.Scale.y / 2);
					Vec2 delta = cameraTargetScreenOffset - graphics.Offset;
					float dist = delta.Magnitude;
					float cameraSpeed = 1;
					if (dist < cameraSpeed) {
						graphics.Offset = cameraTargetScreenOffset;
					} else {
						graphics.Offset += delta / (dist * cameraSpeed);
					}
				} else {
					graphics.Offset = screenAnchor;
				}
			});
			objects.Add(player);
			collideList.Add(player);

			// initialize projectiles
			float projectileScale = 3;
			float projectileRotation = 10;
			float projectileSpeed = 20;
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

			// initialize asteroids
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
			float asteroidMinimumRadiusThatDivides = 3;
			int asteroidBreakupCount = 3;
			Vec2 asteroidStartPosition = new Vec2(40, 0);
			for(int i = 0; i < activeAsteroidCount; i++) {
				MobileCircle asteroid = asteroidPool.Commission();
				asteroid.Position = asteroidStartPosition;
				asteroidStartPosition.RotateRadians(MathF.PI * 2 / activeAsteroidCount);
			}
			void AsteroidDrawSetup(CommandLineGraphicsContext context) {
				context.SetColor(ConsoleColor.Gray);
			}
			void BreakApartAsteroid(MobileCircle asteroid, MobileObject projectile, Vec2 collisionPosition) {
				explosion.Emit((int)(asteroid.Radius * asteroid.Radius) + 1, asteroid.Position, ConsoleColor.Gray, (0, asteroid.Radius));
				if (asteroid.Radius <= asteroidMinimumRadiusThatDivides) {
					asteroidPool.Decommission(asteroid);
					CreatePowerup(projectile, collisionPosition);
					explosion.Emit(10, collisionPosition, ConsoleColor.Cyan, 0);
				} else {
					if (projectile != null) {
						explosion.Emit(10, collisionPosition, ConsoleColor.Red, 0);
					}
					Vec2 deltaFromCenter = collisionPosition - asteroid.Position;
					Vec2 direction = deltaFromCenter.Normal;
					float degreesSeperatingFragments = 360f / asteroidBreakupCount;
					Vec2 positionRadius = direction * (asteroid.Radius / 2);
					positionRadius.RotateDegrees(degreesSeperatingFragments/2);
					Vec2[] points = Polygon.CreateRegular(asteroidBreakupCount, positionRadius);
					float subAsteroidRadius = points[0].Distance(points[1]) / 2;
					for(int i = 0; i < points.Length; ++i) {
						MobileCircle newAsteroid = asteroidPool.Commission();
						newAsteroid.Radius = subAsteroidRadius;
						newAsteroid.Position = asteroid.Position + points[i];
						newAsteroid.Velocity = asteroid.Velocity + points[i];
					}
					asteroidPool.Decommission(asteroid);
				}
				if (projectile != null && projectile.IsActive) {
					projectilePool.Decommission(projectile as MobilePolygon);
				}
			}

			// initialize powerups
			float powerupRadius = asteroidMinimumRadiusThatDivides / 2;
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
			void CreatePowerup(MobileObject projectile, Vec2 position) {
				MobileCircle powerup = powerupPool.Commission();
				powerup.Position = position;
				if (projectile != null) {
					powerup.Velocity = -projectile.Velocity.Normal;
				}
			}

			// add acceleration force to bring objects back to center if they stray too far
			float WorldExtentSize = 50;
			Vec2 WorldMin = new Vec2(-WorldExtentSize, -WorldExtentSize);
			Vec2 WorldMax = new Vec2(WorldExtentSize, WorldExtentSize);
			postUpdate.Add(BringBackStrayObjects);
			void BringBackStrayObjects() {
				for (int i = 0; i < objects.Count; ++i) {
					MobileObject mob = objects[i] as MobileObject;
					if (mob == null || !mob.IsActive) {
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
				graphics.WriteAt($"score: {playerScore}\nammo: {playerAmmo}\nhp: {playerHp}/{playerMaxHp}", (int)graphics.Size.y - 3, 0);
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
			keyInput.BindKey('.', ki => cameraLookAhead = !cameraLookAhead);
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
				player.ClearRotation();
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : -playerFreeRotationSpeedRadianPerSecond;
			}
			void playerSpinRight(KeyInput keyInput) {
				player.ClearRotation();
				player.RotationRadiansPerSecond = player.RotationRadiansPerSecond != 0 ? 0 : playerFreeRotationSpeedRadianPerSecond;
			}
			void playerShoot(KeyInput ki) {
				if (playerAmmo <= 0 || Time.TimeMs < playerShootNextPossibleMs) {
					return;
				}
				MobileObject projectile = projectilePool.Commission();
				projectile.Position = player.Position + player.Direction * playerPoly[0].x;
				projectile.Direction = player.Direction;
				projectile.Velocity = player.Velocity + player.Direction * projectileSpeed;
				playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
				--playerAmmo;
			}

			var collisionRules = new Dictionary<CollisionPair, List<CollisionLogic.Function>>() {
				[(typeof(MobileCircle), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(CollisionData collision) => {
						collision.Get(out MobileCircle astA, out MobileCircle astB);
						if (astA.TypeId == (byte)AsteroidType.Asteroid && astB.TypeId == (byte)AsteroidType.Asteroid) {
							return MoveAsteroidOutOf(astB, collision.point);
						}
						return null;
					}
				},
				[(typeof(MobilePolygon), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(CollisionData collision) => {
						collision.Get(out MobilePolygon projectile, out MobileCircle asteroid);
						if (projectile.TypeId == (byte)AsteroidType.Projectile && asteroid.TypeId == (byte)AsteroidType.Asteroid) {
							BreakApartAsteroid(asteroid, projectile, projectile.Position);
							++playerScore;
						}
						return null;
					}
				},
				[(typeof(ControlledPolygon), typeof(MobileCircle))] = new List<CollisionLogic.Function>() {
					(CollisionData collision) => {
						collision.Get(out ControlledPolygon player, out MobileCircle circle);
						if (player.TypeId == (byte)AsteroidType.Player) {
							switch(circle.TypeId) {
								case (byte)AsteroidType.Asteroid:
									float hpLost = circle.Radius;
									explosion.Emit((int)hpLost*2+1, collision.point, ConsoleColor.Magenta, 2, 1);
									player.ClearRotation();
									BreakApartAsteroid(circle, null, collision.point);
									Action moveAsteroid = MoveAsteroidOutOf(circle, collision.point);
									Action movePlayer = MoveMobilePolygoneOutOf(player, collision);
									return () => {
										moveAsteroid?.Invoke();
										movePlayer?.Invoke();
										playerHp -= hpLost;
									};
								case (byte)AsteroidType.Powerup:
									powerupPool.Decommission(circle);
									playerAmmo += 5;
									if (++playerHp > playerMaxHp) { playerHp = playerMaxHp; }
									return null;
							}
						}
						return null;
					}
				}
			};
			Action MoveMobilePolygoneOutOf(MobilePolygon poly, CollisionData collision) {
				int index = collision.self == poly ? collision.colliderIndexSelf :
				            collision.other == poly ? collision.colliderIndexOther : -1;
				Circle collidingSubCircle = poly.GetCollisionCircleInSpace(index);
				return MoveObjectOutOf(poly, collidingSubCircle, collision.point);
			}
			Action MoveAsteroidOutOf(MobileCircle asteroid, Vec2 point) {
				return MoveObjectOutOf(asteroid, asteroid.Circle, point);
			}
			Action MoveObjectOutOf(MobileObject obj, Circle collisionCircle, Vec2 point) {
				Vec2 delta = point - collisionCircle.Position;
				float distance = delta.Magnitude;
				if (distance > collisionCircle.Radius) {
					return null;
				}
				Vec2 dir = delta / distance;
				float overlap = collisionCircle.Radius - distance;
				Vec2 bumpMove = dir * overlap / 2;
				Action postCollisionReflection = () => {
					obj.Velocity = Vec2.Reflect(obj.Velocity, dir);
					obj.Position -= bumpMove;
				};
				return postCollisionReflection;
			}

			while (running) {
				// input
				keyInput.UpdateKeyInput();

				// update
				Time.Update();
				keyInput.TriggerKeyBinding();
				if (updating) {
					CollisionLogic.DoCollisionLogic(collideList, collisionRules);
					objects.ForEach(o => o.Update());
					postUpdate.ForEach(a => a.Invoke());
					particleSystems.ForEach(ps => ps.Update());
					ActionQueue.Update();
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
				Time.SleepWithoutConsoleKeyPress(targetMsDelay);
			}
		}
	}
}

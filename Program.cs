using collision;
using ConsoleMrV;
using MathMrV;
using MrV;
using MrV.Physics;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class Program {
		enum AsteroidType : byte { None, Player, Asteroid, Projectile, Powerup }
		static void Main(string[] args) {
			// initialize system
			Random random = new Random();
			CommandLineCanvas graphics = new CommandLineCanvas(80, 25, (0.5f, 1));
			KeyInput keyInput = new KeyInput();
			List<IGameObject> gameObjects = new List<IGameObject>();
			List<ICollidable> collideList = new List<ICollidable>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			List<Action> postUpdate = new List<Action>();
			IList<Vec2> SpecialDebugPoints = null;
			bool running = true;
			bool updating = true;
			bool visible = true;
			bool throttle = true;
			bool showSpacePartition = false;
			bool recycleCollisionDatabse = true;
			float targetFps = 20;
			int targetMsDelay = (int)(1000 / targetFps);

			// particle systems
			ParticleSystem explosion = new ParticleSystem((.25f, .5f), (1, 2), (2, 3),
				ParticleSystem.Kind.Explosion, ConsoleColor.Red, 0, ValueOverTime.GrowAndShrink);
			ParticleSystem marker = new ParticleSystem(.25f, .5f, 0,
				ParticleSystem.Kind.None, ConsoleColor.Magenta, 0, null);
			ParticleSystem.OnParticleCommission = particle => gameObjects.Add(particle);
			ParticleSystem.OnParticleDecommission = particle => gameObjects.Remove(particle);
			List<ParticleSystem> particleSystems = new List<ParticleSystem>() { marker, explosion };

			Vec2[] derelictVesselVerts = new Vec2[] { (8,0),(1,1),(0,5),(-1,1),(-5,0),(-1,-1),(0,-5),(1,-1) };
			Vec2[] playerPolyVerts = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };
			Geometry2D derelictVesselGeom = new Geometry2D(derelictVesselVerts);
			Geometry2D playerPolyGeom = new Geometry2D(playerPolyVerts);

			float playerRotationAnglularVelocity = 5;
			long playerShootCooldownMs = 50;
			long playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			long playerScore = 0;
			int playerAmmo = 10;
			float playerMaxHp = 10;
			float playerHp = playerMaxHp;
			MobilePolygon npcCharacter = new MobilePolygon(derelictVesselGeom);
			npcCharacter.Name = "test";
			npcCharacter.TypeId = (int)AsteroidType.Player;
			npcCharacter.Color = ConsoleColor.DarkGray;
			npcCharacter.Position = (10, 5);
			npcCharacter.RotationDegrees = -90;
			MobilePolygon playerCharacter = new MobilePolygon(playerPolyGeom);
			playerCharacter.Name = "player";
			playerCharacter.TypeId = (int)AsteroidType.Player;
			playerCharacter.Color = ConsoleColor.Blue;
			MobileObjectController playerControl = new MobileObjectController(playerCharacter);
			float playerAutoRotationAngularVelocity = MathF.PI * 4;
			float playerFreeSpinAngularVelocity = MathF.PI * 2;
			float playerMinThrustDuration = 1f / 2;
			playerControl.MaxSpeed = 10;
			playerControl.DirectionMatchesVelocity = true;
			List<IGameObject> playerObjects = new List<IGameObject>() { playerCharacter, npcCharacter };
			gameObjects.AddRange(playerObjects);
			gameObjects.Add(playerControl);
			collideList.AddRange(playerObjects.ConvertAll(p => (ICollidable)p));
			Action playerTriggerAfterDeath = null;
			postUpdate.Add(PlayerDeathWatch);
			void PlayerDeathWatch() {
				if (playerHp <= 0 && playerControl.Target.IsActive) {
					playerHp = 0;
					playerControl.Target.IsActive = false;
					explosion.Emit(100, playerControl.Position, ConsoleColor.Blue, (1, 4), (1, 2));
					ActionQueue.Instance.EnqueueDelayed(1, () => playerTriggerAfterDeath?.Invoke());
				}
			}
			void RestartPlayer() {
				playerScore = 0;
				playerAmmo = 10;
				playerMaxHp = 10;
				playerHp = playerMaxHp;
				playerShootCooldownMs = 50;
				playerControl.Position = Vec2.Zero;
				playerControl.Velocity = Vec2.Zero;
				playerControl.AngularVelocity = 0;
				playerControl.RotationRadians = 0;
				playerControl.IsActive = true;
				playerControl.Target.IsActive = true;

				npcCharacter.Position = (10, 5);
				npcCharacter.Velocity = Vec2.Zero;
				npcCharacter.AngularVelocity = 0;
				npcCharacter.RotationDegrees = -90;
			}

			// camera
			bool cameraLookAhead = false;
			float targetScaleY = 1;
			Vec2 userCameraOffset = Vec2.Zero;
			preDraw.Add(CameraFollowsPlayer);
			void CameraFollowsPlayer() {
				Vec2 halfScreen = graphics.Size / 2;
				Vec2 screenAnchor = playerControl.Position - (halfScreen * graphics.Scale) + userCameraOffset;
				if (cameraLookAhead) {
					Vec2 cameraTargetScreenOffset = screenAnchor + playerControl.Velocity * (graphics.Scale.Y / 2);
					Vec2 delta = cameraTargetScreenOffset - graphics.Offset;
					float dist = delta.Length();
					float cameraSpeed = 1;
					if (dist < cameraSpeed) {
						graphics.Offset = cameraTargetScreenOffset;
					} else {
						graphics.Offset += delta / (dist * cameraSpeed);
					}
				} else {
					graphics.Offset = screenAnchor;
				}
				//graphics.PivotAsPercentage = graphics.GetConsolePosition(player.Position).Scaled((1/graphics.Width, 1/graphics.Height));
				Vec2 scaleChangePerFrame = graphics.Scale * 0.1f;
				if (graphics.Scale.Y + scaleChangePerFrame.Y < targetScaleY) {
					graphics.Scale += scaleChangePerFrame;
				}
				if (graphics.Scale.Y - scaleChangePerFrame.Y > targetScaleY) {
					graphics.Scale -= scaleChangePerFrame;
				}
			}

			// initialize projectiles
			float projectileScale = 3;
			float projectileRotation = 10;
			float projectileSpeed = 10;// 20;
			float sqrt3 = MathF.Sqrt(3);
			Vec2[] projectilePolyVerts = new Vec2[3] {
				(projectileScale / sqrt3, 0),
				(-projectileScale / (2 * sqrt3), projectileScale / 2),
				(-projectileScale / (2 * sqrt3), -projectileScale / 2),
			};
			Geometry2D projectilePolyGeom = new Geometry2D(projectilePolyVerts);
			ObjectPool<MobilePolygon> projectilePool = new ObjectPool<MobilePolygon>();
			projectilePool.Setup(() => {
				MobilePolygon projectile = new MobilePolygon(projectilePolyGeom);
				projectile.TypeId = (byte)AsteroidType.Projectile;
				projectile.Color = ConsoleColor.Red;
				projectile.AngularVelocity = projectileRotation;
				return projectile;
			}, projectile => {
				projectile.IsActive = true;
				gameObjects.Add(projectile);
				collideList.Add(projectile);
				NameObjectsByIndex(projectilePool, "");
			}, projectile => {
				projectile.IsActive = false;
				gameObjects.Remove(projectile);
				collideList.Remove(projectile);
				NameObjectsByIndex(projectilePool, "");
			}, projectile => {
				projectile.Color = ConsoleColor.Magenta;
				projectile.TypeId = (byte)AsteroidType.None;
			});
			void NameObjectsByIndex<T>(IList<T> objects, string prefix) where T : IGameObject {
				for(int i = 0; i < objects.Count; ++i) {
					T obj = objects[i];
					obj.Name = prefix + i;
				}
			}
			postUpdate.Add(AlwaysRotateAllProjectiles);
			void AlwaysRotateAllProjectiles() {
				//for (int i = 0; i < projectilePool.Count; i++) {
				//	//projectilePool[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
				//}
			}

			// initialize asteroids
			float asteroidStartRadius = 10;
			ObjectPool<MobileCircle> asteroidPool = new ObjectPool<MobileCircle>();
			asteroidPool.Setup(() => new MobileCircle(new Circle()), asteroid => {
				asteroid.Color = ConsoleColor.Gray;
				asteroid.TypeId = (byte)AsteroidType.Asteroid;
				asteroid.Radius = asteroidStartRadius;
				asteroid.IsActive = true;
				gameObjects.Add(asteroid);
				collideList.Add(asteroid);
				NameObjectsByIndex(asteroidPool, "a");
			}, asteroid => {
				asteroid.IsActive = false;
				gameObjects.Remove(asteroid);
				collideList.Remove(asteroid);
				NameObjectsByIndex(asteroidPool, "a");
			}, asteroid => {
				asteroid.Color = ConsoleColor.Magenta;
				asteroid.TypeId = (byte)AsteroidType.None;
			});

			float asteroidMinimumRadiusThatDivides = 3;
			int asteroidBreakupCount = 3;
			void StartAsteroids() {
				int activeAsteroidCount = 8;//0;// 
				Vec2 asteroidStartPosition = new Vec2(40, 0);
				void MakeAsteroidRing() {
					for (int i = 0; i < activeAsteroidCount; i++) {
						MobileCircle asteroid = asteroidPool.Commission();
						asteroid.Position = asteroidStartPosition;
						asteroidStartPosition.RotateRadians(MathF.PI * 2 / activeAsteroidCount);
						asteroid.Velocity = Vec2.NormalFromDegrees(Rand.Number * 360);
					}
				}
				for (int layers = 0; layers < 2; ++layers) {
					MakeAsteroidRing();
					asteroidStartPosition *= 2;
					activeAsteroidCount *= 2;
				}
			}
			bool BreakApartAsteroid(MobileCircle asteroid, Vec2 collisionPosition) {
				explosion.Emit((int)(asteroid.Radius * asteroid.Radius) + 1, asteroid.Position, ConsoleColor.Gray, (0, asteroid.Radius));
				asteroidPool.DecommissionDelayed(asteroid);
				if (asteroid.Radius < asteroidMinimumRadiusThatDivides) {
					return false;
				}
				Vec2 deltaFromCenter = collisionPosition - asteroid.Position;
				Vec2 direction = deltaFromCenter.Normal;
				float degreesSeperatingFragments = 360f / asteroidBreakupCount;
				Vec2 positionRadius = direction * (asteroid.Radius / 2);
				positionRadius.RotateDegrees(degreesSeperatingFragments/2);
				Vec2[] points = PolygonShape.CreateRegular(asteroidBreakupCount, positionRadius);
				float subAsteroidRadius = points[0].Distance(points[1]) / 2;
				for(int i = 0; i < points.Length; ++i) {
					MobileCircle newAsteroid = asteroidPool.Commission();
					newAsteroid.Radius = subAsteroidRadius;
					newAsteroid.Position = asteroid.Position + points[i];
					newAsteroid.Velocity = asteroid.Velocity + points[i];
				}
				return true;
			}

			// initialize powerups
			float powerupRadius = asteroidMinimumRadiusThatDivides / 2;
			ObjectPool<MobileCircle> powerupPool = new ObjectPool<MobileCircle>();
			powerupPool.Setup(() => {
				MobileCircle powerup = new MobileCircle(new Circle(Vec2.Zero, powerupRadius));
				powerup.Color = ConsoleColor.Cyan;
				powerup.TypeId = (int)AsteroidType.Powerup;
				return powerup;
			}, powerup => {
				powerup.IsActive = true;
				gameObjects.Add(powerup);
				collideList.Add(powerup);
				NameObjectsByIndex(powerupPool, ".");
			}, powerup => {
				powerup.IsActive = false;
				gameObjects.Remove(powerup);
				collideList.Remove(powerup);
				NameObjectsByIndex(powerupPool, ".");
			}, powerup => {
				powerup.TypeId = (int)AsteroidType.None;
				powerup.Color = ConsoleColor.Magenta;
			});
			void CreatePowerup(Vec2 position, Vec2 velocityOfPowerup) {
				MobileCircle powerup = powerupPool.Commission();
				powerup.Position = position;
				powerup.Velocity = velocityOfPowerup;
			}

			// add acceleration force to bring objects back to center if they stray too far
			float WorldExtentSize = 100;
			Vec2 WorldMin = new Vec2(-WorldExtentSize, -WorldExtentSize);
			Vec2 WorldMax = new Vec2(WorldExtentSize, WorldExtentSize);
			postUpdate.Add(BringBackStrayObjects);
			void BringBackStrayObjects() {
				for (int i = 0; i < gameObjects.Count; ++i) {
					MobileObject mob = gameObjects[i] as MobileObject;
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
				const float NudgeStrength = 1;
				const float MaxNudgeSpeed = 10;
				if (p.X < WorldMin.X && v.X < MaxNudgeSpeed) { v.X += Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.X > WorldMax.X && v.X > -MaxNudgeSpeed) { v.X -= Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.Y < WorldMin.Y && v.Y < MaxNudgeSpeed) { v.Y += Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.Y > WorldMax.Y && v.Y > -MaxNudgeSpeed) { v.Y -= Time.DeltaTimeSeconds * NudgeStrength; }
				obj.Velocity = v;
			}

			// direction marker, underlay GUI
			float lineWidth = 1f/2;
			float lineLength = 20;
			preDraw.Add(ShowPlayerDirectionUnderlay);
			void ShowPlayerDirectionUnderlay() {
				if (!visible) { return; }
				graphics.SetColor(ConsoleColor.DarkGray);
				Vec2 lineEnd = playerControl.Position + playerControl.Direction * (lineLength * graphics.Scale.Y);
				graphics.DrawLine(playerControl.Position, lineEnd, lineWidth);
			}

			int collisionCount = 0;
			// additional labels, overlay GUI
			postDraw.Add(DebugDraw);
			postDraw.Add(DrawScore);
			void DrawScore() {
				graphics.WriteAt(ConsoleGlyph.Convert($"score: {playerScore}    DT:{Time.DeltaTimeMsAverage}   \n" +
					$"ammo: {playerAmmo}\nhp: {playerHp}/{playerMaxHp}  coll:{collisionCount}  {playerControl.Speed:0.00}"), 0, (int)graphics.Size.Y - 3, true);
			}
			void DebugDraw() {
				LabelList(projectilePool, ConsoleColor.Magenta);
				LabelList(playerObjects, ConsoleColor.Green);
				LabelList(asteroidPool, ConsoleColor.DarkYellow);
				LabelList(powerupPool, ConsoleColor.Green);
				//dampenRotation(npcCharacter, 1);
				if (SpecialDebugPoints != null) {
					for (int i = 0; i < SpecialDebugPoints.Count; ++i) {
						Vec2 point = SpecialDebugPoints[i];
						char c = (i < 10) ? (char)('0' + i) : (char)('A' + (i - 10));
						graphics.WriteAt(ConsoleGlyph.Convert($"{c}", ConsoleColor.Gray, ConsoleColor.Red), point);
					}
				}
			}
			void dampenRotation(MobileObject npcCharacter, float dampenSpeed) {
				float spin = MathF.Abs(npcCharacter.AngularVelocity);
				const float spinEpsilon = 1f / (1 << 16);
				if (spin > spinEpsilon) {
					float dampen = Time.DeltaTimeSeconds * dampenSpeed;
					if (dampen > spin) {
						npcCharacter.AngularVelocity = 0;
					} else {
						if (npcCharacter.AngularVelocity > 0) {
							dampen *= -1;
						}
						npcCharacter.AngularVelocity += dampen;
					}
				}
			}
			void LabelList<T>(IList<T> objects, ConsoleColor textColor) where T : IGameObject {
				string name;
				for (int i = 0; i < objects.Count; i++) {
					if (!objects[i].IsActive || (name = objects[i].Name) == null) {
						continue;
					}
					graphics.WriteAt(ConsoleGlyph.Convert(name, textColor), objects[i].Position, false);
				}
			}

			// initialize key binding for system and tests
			keyInput.BindKey((char)27, quit);
			keyInput.BindKey('u', toggleUpdating);
			keyInput.BindKey('v', toggleVisible);
			keyInput.BindKey('t', toggleThrottle);
			keyInput.BindKey('r', toggleRecycleCollisionDatabase);
			keyInput.BindKey('p', toggleShowSpacePartition);
			keyInput.BindKey('y', toggleBigDeltatTimeSampleSize);
			keyInput.BindKey('.', ki => cameraLookAhead = !cameraLookAhead);
			keyInput.BindKey('-', zoomOut);
			keyInput.BindKey('=', zoomIn);
			void quit(KeyInput ki) => running = false;
			void toggleUpdating(KeyInput ki) => updating = !updating;
			void toggleVisible(KeyInput ki) => visible = !visible;
			void toggleThrottle(KeyInput ki) => throttle = !throttle;
			void toggleShowSpacePartition(KeyInput ki) => showSpacePartition = !showSpacePartition;
			void toggleRecycleCollisionDatabase(KeyInput ki) => recycleCollisionDatabse = !recycleCollisionDatabse;
			void toggleBigDeltatTimeSampleSize(KeyInput ki) {
				if (Time.DeltaTimeSampleCount < 100) {
					Time.DeltaTimeSampleCount = 200;
				} else {
					Time.DeltaTimeSampleCount = 20;
				}
			}
			void zoomIn(KeyInput ki) {
				if (targetScaleY < 1f/128) { return; }
				targetScaleY /= 1.5f;
			}
			void zoomOut(KeyInput ki) {
				if (targetScaleY > 128) { return; }
				targetScaleY *= 1.5f;
			}

			// player keybinding
			keyInput.BindKey('w', playerForward);
			keyInput.BindKey('s', playerBrakes);
			keyInput.BindKey('a', playerTurnLeft);
			keyInput.BindKey('d', playerTurnRight);
			keyInput.BindKey('q', playerSpinLeft);
			keyInput.BindKey('e', playerSpinRight);
			keyInput.BindKey('R', playerRestartGame);
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
			keyInput.BindKey('b', testMove);
			void testMove(KeyInput k) {
				playerCharacter.Velocity = -Vec2.UnitX;
				playerControl.Velocity = Vec2.UnitX;
			}
			keyInput.BindKey('I', k => playerControl.Position += -Vec2.UnitY);
			keyInput.BindKey('J', k => playerControl.Position += -Vec2.UnitX);
			keyInput.BindKey('K', k => playerControl.Position += Vec2.UnitY);
			keyInput.BindKey('L', k => playerControl.Position += Vec2.UnitX);
			keyInput.BindKey('i', k => userCameraOffset += -Vec2.UnitY * graphics.Scale);
			keyInput.BindKey('j', k => userCameraOffset += -Vec2.UnitX * graphics.Scale);
			keyInput.BindKey('k', k => userCameraOffset += Vec2.UnitY * graphics.Scale);
			keyInput.BindKey('l', k => userCameraOffset += Vec2.UnitX * graphics.Scale);
			keyInput.BindKey('O', k => playerHp = playerMaxHp = 100000);
			void playerMove(float normalizedRadian) {
				playerControl.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationAngularVelocity);
				Thrust();
				playerControl.AutoStopWithoutThrust = true;
			}
			void Thrust() {
				playerControl.AutoStopWithoutThrust = false;
				playerControl.ThrustDuration = playerMinThrustDuration;
			}
			void playerForward(KeyInput ki) {
				Thrust();
				playerControl.AngularVelocity = 0;
			}
			void playerBrakes(KeyInput ki) {
				playerControl.Brakes();
				playerControl.AngularVelocity = 0;
			}
			void playerTurnLeft(KeyInput keyInput) { playerControl.RotationDegrees -= playerRotationAnglularVelocity; }
			void playerTurnRight(KeyInput keyInput) { playerControl.RotationDegrees += playerRotationAnglularVelocity; }
			void playerSpinLeft(KeyInput keyInput) { playerSpinToggle(-playerFreeSpinAngularVelocity); }
			void playerSpinRight(KeyInput keyInput) { playerSpinToggle(playerFreeSpinAngularVelocity); }
			void playerSpinToggle(float newSpinDirection) {
				bool wasSpinningBeforeNewDirectionGiven = playerControl.AngularVelocity != 0;
				playerControl.ClearRotationTarget();
				playerControl.AngularVelocity = wasSpinningBeforeNewDirectionGiven ? 0 : newSpinDirection;
			}
			void playerShoot(KeyInput ki) {
				if (playerAmmo <= 0 || Time.TimeMs < playerShootNextPossibleMs) { return; }
				MobileObject projectile = projectilePool.Commission();
				projectile.Position = playerControl.Position + playerControl.Direction * (playerPolyVerts[0].X + 1);
				projectile.Direction = playerControl.Direction;
				projectile.Velocity = playerControl.Velocity + playerControl.Direction * projectileSpeed;
				playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
				--playerAmmo;
			}

			(byte, byte) CollRule(AsteroidType a, AsteroidType b) => ((byte)a, (byte)b);
			CollisionRules collisionRules = new CollisionRules() {
				[CollRule(AsteroidType.Asteroid, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollideAsteroids
				},
				[CollRule(AsteroidType.Projectile, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollideProjectileAndAsteroid
				},
				[CollRule(AsteroidType.Player, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollidePlayerAndAsteroid
				},
				[CollRule(AsteroidType.Player, AsteroidType.Powerup)] = new List<CollisionLogic.Function>() {
					CollidePlayerAndPowerup
				},
				[CollRule(AsteroidType.Player, AsteroidType.Player)] = new List<CollisionLogic.Function>() {
					CollidePlayers
				},
				[CollRule(AsteroidType.Player, AsteroidType.Projectile)] = new List<CollisionLogic.Function>() {
					CollidePlayers
				},
			};
			Action CollidePlayers(CollisionData collision) {
				return () => {
					collision.Get(out MobilePolygon player, out MobilePolygon asteroid);
					Collision.SeparateObjects(player, asteroid, collision.Normal, collision.Depth);
					//Collision.BounceVelocities(player, asteroid, collision.Normal);
					//SpecialDebugPoints = new Vec2[] { collision.Point, collision.Point + collision.Normal };
					Collision.BounceVelocitiesAndTorque(player, asteroid, collision.Point, collision.Normal);
					//player.Velocity = Vec2.Zero;
					//asteroid.Velocity = Vec2.Zero;
					//player.AngularVelocity = 0;
					//asteroid.AngularVelocity = 0;
					//SpecialDebugPoints = collision.Contacts;
				};
			}

			Action CollideAsteroids(CollisionData collision) {
				return () => {
					collision.Get(out MobileCircle objA, out MobileCircle objB);
					Collision.SeparateObjects(objA, objB, collision.Normal, collision.Depth);
					Collision.BounceVelocities(objA, objB, collision.Normal);
					//Collision.BounceVelocitiesAndTorque(objA, objB, collision.Point, collision.Normal);
				};
			}
			Action CollideProjectileAndAsteroid(CollisionData collision) {
				collision.Get(out MobilePolygon projectile, out MobileCircle asteroid);
				return () => {
					explosion.Emit(10, projectile.Position, projectile.Color, 0);
					PlayerBrokeAsteroid(asteroid, collision, -projectile.Velocity.Normal);
					if (projectile.IsActive) {
						projectilePool.DecommissionDelayed(projectile);
					}
				};
			}
			Action CollidePlayerAndAsteroid(CollisionData collision) {
				collision.Get(out MobilePolygon player, out MobileCircle asteroid);
				return () => {
					bool asteroidDestroyed = false;
					if (player == playerControl.Target) {
						float hpLost = asteroid.Radius;
						explosion.Emit((int)hpLost * 2 + 1, collision.Point, ConsoleColor.Magenta, 2, 1);
						asteroidDestroyed = asteroid.Radius < playerHp;
						playerHp -= hpLost;
						playerControl.ClearRotationTarget();
					}
					Collision.SeparateObjects(player, asteroid, collision.Normal, collision.Depth);
					//Collision.BounceVelocities(player, asteroid, collision.Normal);
					Collision.BounceVelocitiesAndTorque(player, asteroid, collision.Point, collision.Normal);
					if (asteroidDestroyed) {
						PlayerBrokeAsteroid(asteroid, collision, -asteroid.Velocity.Normal);
					}
				};
			}
			void PlayerBrokeAsteroid(MobileCircle asteroid, CollisionData collision, Vec2 velocityOfPowerup) {
				bool brokenIntoMoreAsteroids = BreakApartAsteroid(asteroid, collision.Point);
				if (!brokenIntoMoreAsteroids) {
					CreatePowerup(asteroid.Position, velocityOfPowerup);
				}
				++playerScore;
			}
			Action CollidePlayerAndPowerup(CollisionData collision) {
				collision.Get(out MobilePolygon player, out MobileCircle powerup);
				if (player != playerControl.Target) {
					return null;
				}
				return () => {
					powerupPool.DecommissionDelayed(powerup);
					playerAmmo += 5;
					if (++playerHp > playerMaxHp) { playerHp = playerMaxHp; }
				};
			}

			void playerRestartGame(KeyInput keyInput) => RestartGame();
			void RestartGame() {
				asteroidPool.Clear();
				projectilePool.Clear();
				powerupPool.Clear();
				RestartPlayer();
				StartAsteroids();
				playerTriggerAfterDeath = RestartGame;
			}
			RestartGame();

			SpacePartition<ICollidable> spacePartition = new SpacePartition<ICollidable>(WorldMin, WorldMax, 3, 3, 3, GetCircle);
			Circle GetCircle(ICollidable collidable) => collidable.GetCollisionBoundingCircle();
			CollisionDatabase collisionDatabase = new CollisionDatabase();

			while (running) {
				keyInput.UpdateKeyInput();
				Update();
				Draw();
				if (throttle) {
					Time.ThrottleWithoutConsoleKeyPress(targetMsDelay);
				}
			}

			void Update() {
				Time.Update();
				Time.UpdateAverageDeltaTime();
				keyInput.TriggerKeyBinding();
				if (updating) {
					//spacePartition.DoCollisionLogicAndResolve(collideList, collisionRules);
					spacePartition.Populate(collideList);
					spacePartition.CalculateCollisionsAndResolve(collisionRules, recycleCollisionDatabse?collisionDatabase:null);
					//CollisionData.collisionPool.RemoveCommisioned();
					//CollisionLogic.DoCollisionLogicAndResolve(collideList, collisionRules);
					collisionCount = CollisionData.ClearCollisions();
					gameObjects.ForEach(o => o.Update());
					postUpdate.ForEach(a => a.Invoke());
					particleSystems.ForEach(ps => ps.Update());
					ActionQueue.Update();
					asteroidPool.Update();
					projectilePool.Update();
					powerupPool.Update();
				}
			}

			void Draw() {
				if (showSpacePartition) {
					spacePartition.Draw(graphics, null);
				}
				preDraw.ForEach(a => a.Invoke());
				if (visible) {
					gameObjects.ForEach(o => {
						graphics.SetColor(o.Color);
						o.Draw(graphics);
					});
				}
				//spacePartition.draw(graphics, DrawFunctionCollidable);
				postDraw.ForEach(a => a.Invoke());
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, (int)graphics.Size.Y);
			}

			void DrawFunctionCollidable(CommandLineCanvas canvas, SpacePartitionCell<ICollidable> spacePartition, ICollidable obj) {
				Circle c = obj.GetCollisionBoundingCircle();
				canvas.DrawLine(spacePartition.Position, c.Center);
			}
		}
	}
}

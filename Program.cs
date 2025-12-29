using collision;
using ConsoleMrV;
using MathMrV;
using MrV;
using MrV.CommandLine;
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
			bool recycleCollisionMemory = true;
			bool showControls = false;
			float targetFps = 20;
			int targetMsDelay = (int)(1000 / targetFps);
			KeyResponseRecord<char>[] playerInput = null;

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
					KeyInput.Instance.UnbindKeyResponse(playerInput);
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
			postDraw.Add(ControlsOverlay);
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
						graphics.WriteAt(ConsoleGlyph.Convert($"{c}", ConsoleColor.Gray, ConsoleColor.Red), point, false);
					}
				}
			}
			void ControlsOverlay() {
				if (!showControls) { return; }
				int row = 0, col = 0;
				foreach(var keyBind in KeyInput.Instance) {
					string keyName = keyBind.Key switch {
						(char)27 => "esc", ' ' => "space",
						_ => keyBind.Key.ToString()
					};
					graphics.WriteAt($"{keyName} : {keyBind.Note}", col, row++, true);
					if (row >= graphics.Height) {
						col += graphics.Width / 2;
						row = 0;
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
			KeyInput.Bind((char)27, () => running = false, "quit");
			KeyInput.Bind('u', () => updating = !updating, "toggle updating");
			KeyInput.Bind('v', () => visible = !visible, "toggle visible");
			KeyInput.Bind('t', () => throttle = !throttle, "toggle throttle");
			KeyInput.Bind('r', () => recycleCollisionMemory = !recycleCollisionMemory, "toggle recycle collision memory");
			KeyInput.Bind('p', () => showSpacePartition = !showSpacePartition, "toggle space partition visibility");
			KeyInput.Bind('c', () => showControls = !showControls, "toggle show controls");
			KeyInput.Bind('y', toggleBigDeltatTimeSampleSize, "toggle big deltaTime sample size");
			KeyInput.Bind('R', RestartGame, "restart game");
			KeyInput.Bind('.', () => cameraLookAhead = !cameraLookAhead, "toggle camera lookahead");
			KeyInput.Bind('i', () => userCameraOffset += -Vec2.UnitY * graphics.Scale, "shift camera left");
			KeyInput.Bind('j', () => userCameraOffset += -Vec2.UnitX * graphics.Scale, "shift camera left");
			KeyInput.Bind('k', () => userCameraOffset += Vec2.UnitY * graphics.Scale, "shift camera left");
			KeyInput.Bind('l', () => userCameraOffset += Vec2.UnitX * graphics.Scale, "shift camera left");
			KeyInput.Bind('-', zoomOut, "zoom out");
			KeyInput.Bind('=', zoomIn, "zoom in");
			void toggleBigDeltatTimeSampleSize() {
				if (Time.DeltaTimeSampleCount < 100) {
					Time.DeltaTimeSampleCount = 200;
				} else {
					Time.DeltaTimeSampleCount = 20;
				}
			}
			void zoomOut() {
				if (targetScaleY > 128) { return; }
				targetScaleY *= 1.5f;
			}
			void zoomIn() {
				if (targetScaleY < 1f/128) { return; }
				targetScaleY /= 1.5f;
			}

			// player keybinding
			playerInput = new KeyResponseRecord<char>[] {
				('w', playerForward, "player forward"),
				('s', playerBrakes, "player brakes"),
				('a', playerTurnLeft, "player turn left"),
				('d', playerTurnRight, "player turn right"),
				('q', playerSpinLeft, "player spin right (CCW)"),
				('e', playerSpinRight, "player spin right (CW)"),
				(' ', playerShoot, "player shoot projectile"),
				('1', () => playerMove(3 / 4f), "player move down left"),
				('2', () => playerMove(2 / 4f), "player move down"),
				('3', () => playerMove(1 / 4f), "player move down right"),
				('4', () => playerMove(4 / 4f), "player move left"),
				('5', playerBrakes, "player brakes"),
				('6', () => playerMove(0 / 4f), "player move right"),
				('7', () => playerMove(-3 / 4f), "player move up left"),
				('8', () => playerMove(-2 / 4f), "player move up"),
				('9', () => playerMove(-1 / 4f), "player move up right"),
				('I', () => playerControl.Position += -Vec2.UnitY, "shift player up"),
				('J', () => playerControl.Position += -Vec2.UnitX, "shift player left"),
				('K', () => playerControl.Position += Vec2.UnitY, "shift player down"),
				('L', () => playerControl.Position += Vec2.UnitX, "shift player right"),
				('O', () => playerHp = playerMaxHp = 100000, "player super HP"),
			};
			void playerMove(float normalizedRadian) {
				playerControl.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationAngularVelocity);
				Thrust();
				playerControl.AutoStopWithoutThrust = true;
			}
			void Thrust() {
				playerControl.AutoStopWithoutThrust = false;
				playerControl.ThrustDuration = playerMinThrustDuration;
			}
			void playerForward() {
				Thrust();
				playerControl.AngularVelocity = 0;
			}
			void playerBrakes() {
				playerControl.Brakes();
				playerControl.AngularVelocity = 0;
			}
			void playerTurnLeft() { playerControl.RotationDegrees -= playerRotationAnglularVelocity; }
			void playerTurnRight() { playerControl.RotationDegrees += playerRotationAnglularVelocity; }
			void playerSpinLeft() { playerSpinToggle(-playerFreeSpinAngularVelocity); }
			void playerSpinRight() { playerSpinToggle(playerFreeSpinAngularVelocity); }
			void playerSpinToggle(float newSpinDirection) {
				bool wasSpinningBeforeNewDirectionGiven = playerControl.AngularVelocity != 0;
				playerControl.ClearRotationTarget();
				playerControl.AngularVelocity = wasSpinningBeforeNewDirectionGiven ? 0 : newSpinDirection;
			}
			void playerShoot() {
				if (playerAmmo <= 0 || Time.TimeMs < playerShootNextPossibleMs) { return; }
				MobileObject projectile = projectilePool.Commission();
				projectile.Position = playerControl.Position + playerControl.Direction * (playerPolyVerts[0].X + 1);
				projectile.Direction = playerControl.Direction;
				projectile.Velocity = playerControl.Velocity + playerControl.Direction * projectileSpeed;
				playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
				--playerAmmo;
			}

			(byte, byte) CollRule(AsteroidType a, AsteroidType b) => ((byte)a, (byte)b);
			CollisionRules collisionRules = new CollisionRules(new Dictionary<(byte, byte), List<CollisionRules.Function>> {
				[CollRule(AsteroidType.Asteroid, AsteroidType.Asteroid)] = new List<CollisionRules.Function>() {
					CollideAsteroids
				},
				[CollRule(AsteroidType.Projectile, AsteroidType.Asteroid)] = new List<CollisionRules.Function>() {
					CollideProjectileAndAsteroid
				},
				[CollRule(AsteroidType.Player, AsteroidType.Asteroid)] = new List<CollisionRules.Function>() {
					CollidePlayerAndAsteroid
				},
				[CollRule(AsteroidType.Player, AsteroidType.Powerup)] = new List<CollisionRules.Function>() {
					CollidePlayerAndPowerup
				},
				[CollRule(AsteroidType.Player, AsteroidType.Player)] = new List<CollisionRules.Function>() {
					CollidePlayers
				},
				[CollRule(AsteroidType.Player, AsteroidType.Projectile)] = new List<CollisionRules.Function>() {
					CollidePlayers
				},
			});
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

			void RestartGame() {
				asteroidPool.Clear();
				projectilePool.Clear();
				powerupPool.Clear();
				RestartPlayer();
				StartAsteroids();
				playerTriggerAfterDeath = RestartGame;
				KeyInput.Instance.BindKeyResponse(playerInput);
			}
			RestartGame();

			SpacePartition<ICollidable> spacePartition = new SpacePartition<ICollidable>(WorldMin, WorldMax, 3, 3, 3, GetCircle);
			Circle GetCircle(ICollidable collidable) => collidable.GetCollisionBoundingCircle();
			CollisionsPerAgent collisionDatabase = new CollisionsPerAgent();

			while (running) {
				KeyInput.Read();
				Update();
				Draw();
				if (throttle) {
					Time.ThrottleWithoutConsoleKeyPress(targetMsDelay);
				}
			}

			void Update() {
				Time.Update();
				Time.UpdateAverageDeltaTime();
				KeyInput.TriggerEvents();
				if (updating) {
					collisionCount = CollisionLogic.Update(collideList, spacePartition, collisionRules, recycleCollisionMemory ? collisionDatabase : null);
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
				if (showSpacePartition) {
					spacePartition.Draw(graphics, DrawFunctionCollidable);
				}
				postDraw.ForEach(a => a.Invoke());
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, (int)graphics.Size.Y);
			}

			void DrawFunctionCollidable(CommandLineCanvas canvas, SpacePartitionCell<ICollidable> spacePartition, ICollidable obj) {
				Circle c = obj.GetCollisionBoundingCircle();
				canvas.SetColor(ConsoleColor.Black);
				canvas.DrawLine(spacePartition.Position, c.Center);
			}
		}
	}
}

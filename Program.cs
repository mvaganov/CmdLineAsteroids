using collision;
using ConsoleMrV;
using MathMrV;
using MrV;
using System;
using System.Collections.Generic;

namespace asteroids {
	public class Program {
		enum AsteroidType : byte { None, Player, Asteroid, Projectile, Powerup, Test }
		static void Main(string[] args) {
			// initialize system
			Random random = new Random();
			CommandLineCanvas graphics = new CommandLineCanvas(80, 25, (0.5f, 1));
			KeyInput keyInput = new KeyInput();
			List<IGameObject> objects = new List<IGameObject>();
			ParticleSystem.OnParticleCommission = particle => objects.Add(particle);
			ParticleSystem.OnParticleDecommission = particle => objects.Remove(particle);
			List<ICollidable> collideList = new List<ICollidable>();
			List<Action> preDraw = new List<Action>();
			List<Action> postDraw = new List<Action>();
			List<Action> postUpdate = new List<Action>();
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
			List<ParticleSystem> particleSystems = new List<ParticleSystem>() { marker, explosion };

			// test polygon
			Vec2[] testPoly = new Vec2[] {  (5, 0), (6,2), (7,5), (2,5), (3,1), (0, 3), (-1, 0), (0, -3) };//(5, 0), (-3, 3), (0, 0), (-3, -3) };//(5,0),(0,3),(0,-3) };//
			// initialize player
			Vec2[] playerPoly = new Vec2[] { (5, 0), (-3, 3), (0, 0), (-3, -3) };//(5, 0), (0, 3), (0, -3) };//

			float playerRotationAngleDegrees = 5;
			long playerShootCooldownMs = 50;
			long playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
			long playerScore = 0;
			int playerAmmo = 10;
			float playerMaxHp = 10;
			float playerHp = playerMaxHp;
			MobilePolygon testCharacter = new MobilePolygon(testPoly);
			testCharacter.Name = "test";
			testCharacter.TypeId = (int)AsteroidType.Test;
			testCharacter.Color = ConsoleColor.DarkGray;
			testCharacter.Position = (10, 3);
			MobilePolygon playerCharacter = new MobilePolygon(playerPoly);
			playerCharacter.Name = "player";
			playerCharacter.TypeId = (int)AsteroidType.Player;
			playerCharacter.Color = ConsoleColor.Blue;
			playerCharacter.CollisionCircles = new Circle[] {
				new Circle((1.25f, 0), 1.25f),
				new Circle((-0.6875f,-1.4375f), 0.5f),
				new Circle((-0.6875f, 1.4375f), 0.5f),
				new Circle((3.625f,0), 0.5f),
			};
			MobileObjectController playerControl = new MobileObjectController(playerCharacter);
			float playerAutoRotationSpeedRadianPerSecond = MathF.PI * 4;
			float playerFreeRotationSpeedRadianPerSecond = MathF.PI * 2;
			float playerMinThrustDuration = 1f / 2;
			playerControl.MaxSpeed = 10;
			playerControl.DirectionMatchesVelocity = true;
			List<IGameObject> playerObjects = new List<IGameObject>() { playerCharacter, testCharacter };
			objects.AddRange(playerObjects);
			objects.Add(playerControl);
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
				playerControl.RotationRadians = 0;
				playerControl.IsActive = true;
				playerControl.Target.IsActive = true;
			}

			// camera
			bool cameraLookAhead = false;
			float targetScaleY = 1;
			preDraw.Add(CameraFollowsPlayer);
			void CameraFollowsPlayer() {
				Vec2 halfScreen = graphics.Size / 2;
				Vec2 screenAnchor = playerControl.Position - halfScreen.Scaled(graphics.Scale);
				if (cameraLookAhead) {
					Vec2 cameraTargetScreenOffset = screenAnchor + playerControl.Velocity * (graphics.Scale.y / 2);
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
				//graphics.PivotAsPercentage = graphics.GetConsolePosition(player.Position).Scaled((1/graphics.Width, 1/graphics.Height));
				Vec2 scaleChangePerFrame = graphics.Scale * 0.1f;
				if (graphics.Scale.y + scaleChangePerFrame.y < targetScaleY) {
					graphics.Scale += scaleChangePerFrame;
				}
				if (graphics.Scale.y - scaleChangePerFrame.y > targetScaleY) {
					graphics.Scale -= scaleChangePerFrame;
				}
			}

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
				projectile.TypeId = (byte)AsteroidType.Projectile;
				projectile.Color = ConsoleColor.Red;
				return projectile;
			}, projectile => {
				projectile.IsActive = true;
				objects.Add(projectile);
				collideList.Add(projectile);
				NameObjectsByIndex(projectilePool, "");
			}, projectile => {
				projectile.IsActive = false;
				objects.Remove(projectile);
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
				for (int i = 0; i < projectilePool.Count; i++) {
					projectilePool[i].RotationRadians += Time.DeltaTimeSeconds * projectileRotation;
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
				objects.Add(asteroid);
				collideList.Add(asteroid);
				NameObjectsByIndex(asteroidPool, "a");
			}, asteroid => {
				asteroid.IsActive = false;
				objects.Remove(asteroid);
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
				objects.Add(powerup);
				collideList.Add(powerup);
				NameObjectsByIndex(powerupPool, ".");
			}, powerup => {
				powerup.IsActive = false;
				objects.Remove(powerup);
				collideList.Remove(powerup);
				NameObjectsByIndex(powerupPool, ".");
			}, powerup => {
				powerup.TypeId = (int)AsteroidType.None;
				powerup.Color = ConsoleColor.Magenta;
			});
			void CreatePowerup(MobileObject projectile, Vec2 position) {
				MobileCircle powerup = powerupPool.Commission();
				powerup.Position = position;
				if (projectile != null) {
					powerup.Velocity = -projectile.Velocity.Normal;
				}
			}

			// add acceleration force to bring objects back to center if they stray too far
			float WorldExtentSize = 100;
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
				const float NudgeStrength = 1;
				const float MaxNudgeSpeed = 10;
				if (p.x < WorldMin.x && v.x < MaxNudgeSpeed) { v.x += Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.x > WorldMax.x && v.x > -MaxNudgeSpeed) { v.x -= Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.y < WorldMin.y && v.y < MaxNudgeSpeed) { v.y += Time.DeltaTimeSeconds * NudgeStrength; }
				if (p.y > WorldMax.y && v.y > -MaxNudgeSpeed) { v.y -= Time.DeltaTimeSeconds * NudgeStrength; }
				obj.Velocity = v;
			}

			// direction marker, underlay GUI
			float lineWidth = 1f/2;
			float lineLength = 20;
			preDraw.Add(ShowPlayerDirectionUnderlay);
			void ShowPlayerDirectionUnderlay() {
				if (!visible) { return; }
				graphics.SetColor(ConsoleColor.DarkGray);
				Vec2 lineEnd = playerControl.Position + playerControl.Direction * (lineLength * graphics.Scale.y);
				graphics.DrawLine(playerControl.Position, lineEnd, lineWidth);
			}

			// additional labels, overlay GUI
			postDraw.Add(DebugDraw);
			postDraw.Add(DrawScore);
			void DrawScore() {
				graphics.WriteAt(ConsoleGlyph.Convert($"score: {playerScore}    DT:{Time.DeltaTimeMsAverage}   \n" +
					$"ammo: {playerAmmo}\nhp: {playerHp}/{playerMaxHp}  {playerControl.Speed}:{playerCharacter.Velocity.Magnitude}"), 0, (int)graphics.Size.y - 3, true);
			}
			void DebugDraw() {
				LabelList(projectilePool, ConsoleColor.Magenta);
				LabelList(playerObjects, ConsoleColor.Green);
				//graphics.WriteAt(ConsoleGlyph.Convert("player", ConsoleColor.Green), player.Position);
				LabelList(asteroidPool, ConsoleColor.DarkYellow);
				LabelList(powerupPool, ConsoleColor.Green);
				List<Polygon.CollisionData> collisions = null;
				bool isColliding = testCharacter.Polygon.TryGetPolyCollision(playerCharacter.Polygon, ref collisions);
				if (isColliding) {
					//graphics.SetColor(ConsoleColor.White);
					//testCharacter.Draw(graphics);
					Vec2 collisionAdjustment = Vec2.Zero;
					for(int i = 0; i < collisions.Count; ++i) {
						collisionAdjustment += collisions[i].Normal * collisions[i].Depth;
						collisions[i].Draw(graphics);
					}
					playerCharacter.Position += collisionAdjustment;

					// TODO Resolve Velocity (Bounce)
					Vec2 bounceNormal = collisionAdjustment.Normal;
					Vec2 testObjectVelocity = Vec2.Zero;//-bounceNormal;// 
					Vec2 relativeVelocity = playerControl.Velocity - testObjectVelocity;
					float velAlongNormal = Vec2.Dot(relativeVelocity, bounceNormal);
					bool velocitiesAreOpposed = velAlongNormal < 0;
					if (velocitiesAreOpposed) {
						float restitution = 0.8f; // Bounciness (0 = rock, 1 = super ball)
						// Simple impulse scalar
						float j = -(1 + restitution) * velAlongNormal;
						// If objects have mass, divide j by (1/MassA + 1/MassB) here
						Vec2 impulse = bounceNormal * j;
						testObjectVelocity -= impulse; // Add to B
						playerControl.Velocity += impulse;     // Subtract from A

						//// 2. Approximate Point of Contact (for simplicity, using A's center and Normal/Depth)
						//// More accurate: find the closest vertex of B to A, or midpoint of deepest edge.
						//// For this example, let's use the object centers, which is less accurate but simpler:
						//Vector2 contactPoint = playerCharacter.Polygon + bounceNormal * (collisionAdjustment.Depth / 2f);

						//// 3. Calculate Radius Vectors
						//Vec2 rA = contactPoint - playerCharacter.Polygon;
						//Vec2 rB = contactPoint - asteroidPos;

						//// 4. Calculate relative velocity, including rotation component
						//// v_rel = (vB + (wB x rB)) - (vA + (wA x rA))
						//// Cross product (w x r) in 2D is: (-w*ry, w*rx)
						//Vector2 vA_rot = new Vector2(-ship.AngularVelocity * rA.Y, ship.AngularVelocity * rA.X);
						//Vector2 vB_rot = new Vector2(-asteroid.AngularVelocity * rB.Y, asteroid.AngularVelocity * rB.X);

						//Vector2 relativeVelocity = (asteroid.Velocity + vB_rot) - (ship.Velocity + vA_rot);
						////float velAlongNormal = Vector2.Dot(relativeVelocity, manifold.Normal);

						//// Only resolve if closing
						//if (velAlongNormal >= 0) return;

						//// 5. Calculate 2D Cross Products (r x n)
						//// This is the scalar component of torque
						//float rACrossN = (rA.X * manifold.Normal.Y) - (rA.Y * manifold.Normal.X);
						//float rBCrossN = (rB.X * manifold.Normal.Y) - (rB.Y * manifold.Normal.X);

						//// 6. Calculate the Full Impulse Denominator
						//float denominator =
						//		(1f / ship.Mass) + (1f / asteroid.Mass) +
						//		(rACrossN * rACrossN / ship.Inertia) +
						//		(rBCrossN * rBCrossN / asteroid.Inertia);

						//// 7. Calculate the final scalar impulse magnitude (j)
						//float j = -(1f + restitution) * velAlongNormal / denominator;

						//// 8. Apply Linear and Angular Impulse
						//Vector2 impulse = manifold.Normal * j;

						//// Linear Application
						//ship.Velocity -= impulse * (1f / ship.Mass);
						//asteroid.Velocity += impulse * (1f / asteroid.Mass);

						//// Angular Application (This creates the spin!)
						//ship.AngularVelocity -= (rACrossN * j) / ship.Inertia;
						//asteroid.AngularVelocity += (rBCrossN * j) / asteroid.Inertia;
					}

				} else {
					//graphics.SetColor(ConsoleColor.DarkGray);
					//testConcaveObj.Draw(graphics);
				}
			}
			void LabelList<T>(IList<T> objects, ConsoleColor textColor) where T : IGameObject {
				string name;
				for (int i = 0; i < objects.Count; i++) {
					if (!objects[i].IsActive || (name = objects[i].Name) == null) {
						continue;
					}
					graphics.WriteAt(ConsoleGlyph.Convert(name, textColor), objects[i].Position);
				}
			}

			// initialize key binding
			keyInput.BindKey((char)27, quit);
			keyInput.BindKey('p', toggleUpdating);
			keyInput.BindKey('v', toggleVisible);
			keyInput.BindKey('t', toggleThrottle);
			keyInput.BindKey('r', toggleRecycleCollisionDatabase);
			keyInput.BindKey('u', toggleShowSpacePartition);
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
				playerCharacter.Velocity = Vec2.DirectionMinX;
				playerControl.Velocity = Vec2.DirectionMaxX;
			}
			keyInput.BindKey('I', k => playerControl.Position += Vec2.DirectionMinY);
			keyInput.BindKey('J', k => playerControl.Position += Vec2.DirectionMinX);
			keyInput.BindKey('K', k => playerControl.Position += Vec2.DirectionMaxY);
			keyInput.BindKey('L', k => playerControl.Position += Vec2.DirectionMaxX);
			keyInput.BindKey('i', k => playerControl.Velocity = Vec2.DirectionMinY * playerControl.MaxSpeed);
			keyInput.BindKey('j', k => playerControl.Velocity = Vec2.DirectionMinX * playerControl.MaxSpeed);
			keyInput.BindKey('k', k => playerControl.Velocity = Vec2.DirectionMaxY * playerControl.MaxSpeed);
			keyInput.BindKey('l', k => playerControl.Velocity = Vec2.DirectionMaxX * playerControl.MaxSpeed);
			keyInput.BindKey('O', k => playerHp = playerMaxHp = 100000);
			void playerMove(float normalizedRadian) {
				playerControl.SmoothRotateTarget(MathF.PI * normalizedRadian, playerAutoRotationSpeedRadianPerSecond);
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
			void playerTurnLeft(KeyInput keyInput) { playerControl.RotationDegrees -= playerRotationAngleDegrees; }
			void playerTurnRight(KeyInput keyInput) { playerControl.RotationDegrees += playerRotationAngleDegrees; }
			void playerSpinLeft(KeyInput keyInput) {
				playerControl.ClearRotationTarget();
				playerControl.AngularVelocity = playerControl.AngularVelocity != 0 ? 0 : -playerFreeRotationSpeedRadianPerSecond;
			}
			void playerSpinRight(KeyInput keyInput) {
				playerControl.ClearRotationTarget();
				playerControl.AngularVelocity = playerControl.AngularVelocity != 0 ? 0 : playerFreeRotationSpeedRadianPerSecond;
			}
			void playerShoot(KeyInput ki) {
				if (playerAmmo <= 0 || Time.TimeMs < playerShootNextPossibleMs) {
					return;
				}
				MobileObject projectile = projectilePool.Commission();
				projectile.Position = playerControl.Position + playerControl.Direction * playerPoly[0].x;
				projectile.Direction = playerControl.Direction;
				projectile.Velocity = playerControl.Velocity + playerControl.Direction * projectileSpeed;
				playerShootNextPossibleMs = Time.TimeMs + playerShootCooldownMs;
				--playerAmmo;
			}

			(byte, byte) Rule(AsteroidType a, AsteroidType b) => ((byte)a, (byte)b);
			var collisionRules = new Dictionary<(byte, byte), List<CollisionLogic.Function>>() {
				[Rule(AsteroidType.Asteroid, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollideAsteroids
				},
				[Rule(AsteroidType.Projectile, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollideProjectileAndAsteroid
				},
				[Rule(AsteroidType.Player, AsteroidType.Asteroid)] = new List<CollisionLogic.Function>() {
					CollidePlayerAndAsteroid
				},
				[Rule(AsteroidType.Player, AsteroidType.Powerup)] = new List<CollisionLogic.Function>() {
					CollidePlayerAndPowerup
				},
			};

			Action CollideAsteroids(CollisionData collision) {
				return () => {
					collision.Get(out MobileCircle objA, out MobileCircle objB);
					float objAMass = objA.Radius * objA.Radius;
					float objBMass = objB.Radius * objB.Radius;
					float massAPercentage = objAMass / (objAMass + objBMass);
					MobileObject.SeparateObjects(objA, objB, collision.normal, collision.depth, massAPercentage);
					MobileObject.BounceVelocities(objA, objB, objAMass, objBMass, collision.normal);
				};
			}
			Action CollideProjectileAndAsteroid(CollisionData collision) {
				collision.Get(out MobilePolygon projectile, out MobileCircle asteroid);
				return () => {
					explosion.Emit(10, projectile.Position, projectile.Color, 0);
					if (!BreakApartAsteroid(asteroid, projectile.Position)) {
						CreatePowerup(projectile, projectile.Position);
					}
					if (projectile.IsActive) {
						projectilePool.DecommissionDelayed(projectile);
					}
					++playerScore;
				};
			}
			Action CollidePlayerAndAsteroid(CollisionData collision) {
				collision.Get(out MobilePolygon player, out MobileCircle asteroid);
				return () => {
					float hpLost = asteroid.Radius;
					explosion.Emit((int)hpLost * 2 + 1, collision.point, ConsoleColor.Magenta, 2, 1);
					bool asteroidDestroyed = asteroid.Radius < playerHp;
					playerHp -= hpLost;
					playerControl.ClearRotationTarget();
					float playerMass = player.Area;// Polygon.Area;
					float asteroidMass = asteroid.Area;// Circle.Area;
					float playerMassPercentage = playerMass / (playerMass + asteroidMass);
					MobileObject.SeparateObjects(player, asteroid, collision.normal, collision.depth, playerMassPercentage);
					MobileObject.BounceVelocities(player, asteroid, asteroidMass, playerMass, collision.normal);
					float playerAngularVelocity = playerControl.AngularVelocity;
					float asteroidAngularVelocity = 0;
					float asteroidInertiaWithoutDensity = 0.5f * asteroidMass * asteroid.Radius * asteroid.Radius; // TODO get Intertial value from mobile object

					MobileObject.CollisionTorque(player, asteroid, collision.point, playerMass, asteroidMass, 
						ref playerAngularVelocity, ref asteroidAngularVelocity, collision.normal);
					playerControl.AngularVelocity = playerAngularVelocity;
					if (asteroidDestroyed) {
						BreakApartAsteroid(asteroid, collision.point);
					}
				};
			}
			Action CollidePlayerAndPowerup(CollisionData collision) {
				collision.Get(out MobilePolygon player, out MobileCircle powerup);
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
					//CollisionLogic.DoCollisionLogicAndResolve(collideList, collisionRules);
					objects.ForEach(o => o.Update());
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
					objects.ForEach(o => {
						graphics.SetColor(o.Color);
						o.Draw(graphics);
					});
				}
				//spacePartition.draw(graphics, DrawFunctionCollidable);
				postDraw.ForEach(a => a.Invoke());
				graphics.PrintModifiedCharactersOnly();
				graphics.FinishedRender();
				Console.SetCursorPosition(0, (int)graphics.Size.y);
			}

			void DrawFunctionCollidable(CommandLineCanvas canvas, SpacePartitionCell<ICollidable> spacePartition, ICollidable obj) {
				Circle c = obj.GetCollisionBoundingCircle();
				canvas.DrawLine(spacePartition.Position, c.Position);
			}
		}
	}
}

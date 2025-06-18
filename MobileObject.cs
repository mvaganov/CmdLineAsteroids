using System;

namespace asteroids {
	public interface IGameObject {
		public Vec2 Direction { get; set; }
		public void Draw(CommandLineGraphicsContext graphicsContext);
		public void Update();
	}
	public interface IMobileEntity {
		public Vec2 Position { get; set; }
		public Vec2 Velocity { get; set; }
	}
	public interface ICollidable {
		public bool TryGetAABB(out Vec2 min, out Vec2 max);
		public bool TryGetCircle(out Circle circle);
	}
	public class MobileCircle : MobileObject {
		private Circle circle;
		public override Vec2 Position { get => circle.position; set => circle.position = value; }
		public override Vec2 Direction { get => Velocity.ToUnitVector(); set => throw new NotImplementedException(); }

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			circle.Draw(graphicsContext);
		}
	}

	public class MobilePolygon : MobileObject {
		private Polygon polygon;
		private bool _directionMatchesVelocity;
		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }

		public float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public bool DirectionMatchesVelocity { get => _directionMatchesVelocity; set => _directionMatchesVelocity = value; }
		public override Vec2 Velocity {
			get => base.Velocity;
			set {
				base.Velocity = value;
				if (DirectionMatchesVelocity) {
					polygon.Direction = base.Velocity.ToUnitVector();
				}
			}
		}

		public MobilePolygon(Vec2[] playerPoly) {
			polygon = new Polygon(playerPoly);
		}

		public override void Draw(CommandLineGraphicsContext graphicsContext) {
			polygon.Draw(graphicsContext);
		}
	}

	public abstract class MobileObject : IGameObject {
		public virtual Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public abstract Vec2 Position { get; set; }
		public abstract Vec2 Direction { get; set; }

		private Vec2 _velocity;
		public abstract void Draw(CommandLineGraphicsContext graphicsContext);
		public virtual void Update() {
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			float dx = _velocity.x * Time.DeltaTimeSeconds;
			float dy = _velocity.y * Time.DeltaTimeSeconds;

			moveThisFrame = new Vec2(dx, dy);
			Position += moveThisFrame;
			Console.SetCursorPosition(40, 21);
			Console.WriteLine($"V:{_velocity} * T:{Time.DeltaTimeSeconds} = M:{moveThisFrame}({dx},{dy})            ");
		}
	}
}
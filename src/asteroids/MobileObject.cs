using ConsoleMrV;
using MathMrV;
using MrV;
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
		private float _rotationRadiansPerSecond;
		private float _targetDirection = float.NaN;
		private bool _directionMatchesVelocity;
		public override Vec2 Position { get => polygon.Position; set => polygon.Position = value; }
		public override Vec2 Direction { get => polygon.Direction; set => polygon.Direction = value; }

		public float RotationDegrees { get => polygon.RotationDegrees; set => polygon.RotationDegrees = value; }
		public float RotationRadians { get => polygon.RotationRadians; set => polygon.RotationRadians = value; }
		public float RotationRadiansPerSecond { get => _rotationRadiansPerSecond; set => _rotationRadiansPerSecond = value; }
		public bool DirectionMatchesVelocity { get => _directionMatchesVelocity; set => _directionMatchesVelocity = value; }
		public override Vec2 Velocity {
			get => base.Velocity;
			set {
				base.Velocity = value;
				if (DirectionMatchesVelocity && value != Vec2.Zero) {
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
		public override void Update() {
			if (_rotationRadiansPerSecond != 0) {
				float currentRadians = Direction.UnitVectorToRadians();
				float rotationThisMoment = _rotationRadiansPerSecond * Time.DeltaTimeSeconds;
				if (!float.IsNaN(_targetDirection)) {
					UpdateTargetRotationLogic(currentRadians, rotationThisMoment);
				} else {
					RotationRadians += rotationThisMoment;
				}
			}
			base.Update();
		}
		private void UpdateTargetRotationLogic(float currentRadians, float rotationThisMoment) {
			float deltaToTarget = GetRealDeltaRotationAccountingForWrap(_targetDirection, currentRadians);
			if (MathF.Abs(rotationThisMoment) > MathF.Abs(deltaToTarget)) {
				RotationRadians = _targetDirection;
				_rotationRadiansPerSecond = 0;
				return;
			}
			RotationRadians += rotationThisMoment;
		}
		private float GetRealDeltaRotationAccountingForWrap(float aRad, float bRad) {
			aRad = Vec2.WrapRadian(aRad);
			bRad = Vec2.WrapRadian(bRad);
			float delta = aRad - bRad;
			delta = Vec2.WrapRadian(delta);
			if (delta == 0) { return 0; }
			if (MathF.Abs(delta) > MathF.PI) {
				bool isNegative = delta < 0;
				delta = isNegative ? -(MathF.PI + delta) : -(MathF.PI - delta);
			}
			return delta;
		}
		public void SmoothRotateTarget(float targetRadians, float speed) {
			_targetDirection = targetRadians;
			float currentAngle = Direction.UnitVectorToRadians();
			float deltaToTarget = GetRealDeltaRotationAccountingForWrap(_targetDirection, currentAngle);
			_rotationRadiansPerSecond = (deltaToTarget < 0)  ? - speed : speed;
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
			//Console.SetCursorPosition(40, 21);
			//Console.WriteLine($"V:{_velocity} * T:{Time.DeltaTimeSeconds} = M:{moveThisFrame}({dx},{dy})            ");
		}
	}
}
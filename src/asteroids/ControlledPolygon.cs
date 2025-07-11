using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class ControlledPolygon : MobilePolygon {
		private float _rotationRadiansPerSecond;
		private float _thrustDuration = 0;
		private float _maxSpeed = 1;
		private float _acceleration = 10;
		private float _targetDirection = float.NaN;
		private bool _directionMatchesVelocity;
		private bool _autoStopWithoutThrust = false;
		private bool _brake;
		public ControlledPolygon(Vec2[] playerPoly) : base(playerPoly) { }
		public float Speed { get => Velocity.Magnitude; set => Velocity = _directionMatchesVelocity ? Direction * value : Velocity.Normal; }
		public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
		public float Acceleration { get => _acceleration; set => _acceleration = value; }
		public float ThrustDuration { get => _thrustDuration; set => _thrustDuration = value; }
		public float TargetRotation { get => _targetDirection; set => _targetDirection = value; }
		public float RotationRadiansPerSecond { get => _rotationRadiansPerSecond; set => _rotationRadiansPerSecond = value; }
		public bool DirectionMatchesVelocity { get => _directionMatchesVelocity; set => _directionMatchesVelocity = value; }
		public bool AutoStopWithoutThrust { get => _autoStopWithoutThrust; set => _autoStopWithoutThrust = value; }
		public override Vec2 Velocity {
			get => base.Velocity;
			set {
				base.Velocity = value;
				if (DirectionMatchesVelocity && value != Vec2.Zero) {
					polygon.Direction = base.Velocity.ToUnitVector();
				}
			}
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
			bool slowDownToStop = _brake;
			float currentSpeed = Speed;
			float acceleartionThisFrame = _acceleration * Time.DeltaTimeSeconds;
			if (_thrustDuration > 0) {
				_thrustDuration -= Time.DeltaTimeSeconds;
				float newSpeed = currentSpeed + acceleartionThisFrame;
				if (currentSpeed > _maxSpeed) {
					newSpeed = currentSpeed - acceleartionThisFrame;
					if (newSpeed < _maxSpeed) {
						newSpeed = _maxSpeed;
					}
				} else if (newSpeed > _maxSpeed) {
					newSpeed = _maxSpeed;
				}
				Speed = newSpeed;
			} else {
				slowDownToStop |= _autoStopWithoutThrust;
			}
			if (slowDownToStop) {
				if (acceleartionThisFrame > currentSpeed) {
					Speed = 0;
					_brake = false;
				} else {
					Speed = currentSpeed - acceleartionThisFrame;
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
			_rotationRadiansPerSecond = (deltaToTarget < 0) ? -speed : speed;
		}
		public void ClearRotation() {
			_targetDirection = float.NaN;
			_rotationRadiansPerSecond = 0;
		}
		internal void Brakes() {
			_thrustDuration = 0;
			_brake = true;
		}
	}
}

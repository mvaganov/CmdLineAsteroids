using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class MobileObjectController : IGameObject {
		private MobileObject _target;
		private string _name;
		private float _thrustDuration = 0;
		private float _maxSpeed = 1;
		private float _acceleration = 10;
		private float _targetDirection = float.NaN;
		private bool _active;
		private bool _directionMatchesVelocity;
		private bool _autoStopWithoutThrust = false;
		private bool _brake;
		public float Speed {
			get => _target.Velocity.Length();
			set => _target.Velocity = (_directionMatchesVelocity || _target.Velocity == Vec2.Zero ? _target.Direction : _target.Velocity.Normal) * value;
		}
		public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
		public float Acceleration { get => _acceleration; set => _acceleration = value; }
		public float ThrustDuration { get => _thrustDuration; set => _thrustDuration = value; }
		public float TargetRotation { get => _targetDirection; set => _targetDirection = value; }
		public float AngularVelocity { get => _target.AngularVelocity; set => _target.AngularVelocity = value; }
		public bool DirectionMatchesVelocity { get => _directionMatchesVelocity; set => _directionMatchesVelocity = value; }
		public bool AutoStopWithoutThrust { get => _autoStopWithoutThrust; set => _autoStopWithoutThrust = value; }
		public Vec2 Velocity {
			get => _target.Velocity;
			set {
				_target.Velocity = value;
				if (DirectionMatchesVelocity && value != Vec2.Zero) {
					_target.Direction = _target.Velocity.Normal;
				}
			}
		}
		public MobileObject Target { get => _target; set => _target = value; }
		public string Name { get => _name; set => _name = value; }
		public Vec2 Position { get => _target.Position; set => _target.Position = value; }
		public Vec2 Direction { get => _target.Direction; set => _target.Direction = value; }
		public bool IsActive { get => _active; set => _active = value; }
		public bool IsVisible { get => _target.IsVisible; set => _target.IsVisible = value; }
		public ConsoleColor Color { get => _target.Color; set => _target.Color = value; }
		public float RotationDegrees { get => _target.RotationDegrees; set => _target.RotationDegrees = value; }
		public float RotationRadians { get => _target.RotationRadians; set => _target.RotationRadians = value; }

		public MobileObjectController(MobileObject target) { _target = target; }
		public void Update() {
			if (AngularVelocity != 0) {
				float currentRadians = _target.Direction.NormalToRadians();
				float rotationThisMoment = AngularVelocity * Time.DeltaTimeSeconds;
				if (!float.IsNaN(_targetDirection)) {
					UpdateTargetRotationLogic(currentRadians, rotationThisMoment);
				}
			}
			bool slowDownToStop = _brake;
			float currentSpeed = Speed;
			float acceleartionThisFrame = _acceleration * Time.DeltaTimeSeconds;
			if (_thrustDuration > 0) {
				_thrustDuration -= Time.DeltaTimeSeconds;
				float newSpeed = currentSpeed + acceleartionThisFrame;
				if (newSpeed > _maxSpeed) {
					newSpeed = _maxSpeed;
				}
				_target.Velocity = _target.Direction * newSpeed;
			} else {
				_thrustDuration = 0;
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
		}
		private void UpdateTargetRotationLogic(float currentRadians, float rotationThisMoment) {
			float deltaToTarget = GetShortDeltaRotationAccountingForWrap(_targetDirection, currentRadians);
			bool spinThisMomentWouldOvershoot = MathF.Abs(rotationThisMoment) > MathF.Abs(deltaToTarget);
			if (spinThisMomentWouldOvershoot) {
				_target.RotationRadians = _targetDirection;
				AngularVelocity = 0;
				return;
			}
			_target.RotationRadians += rotationThisMoment;
		}
		private float GetShortDeltaRotationAccountingForWrap(float destination, float source) {
			destination = Vec2.WrapRadian(destination);
			source = Vec2.WrapRadian(source);
			float delta = destination - source;
			delta = Vec2.WrapRadian(delta);
			if (delta == 0) { return 0; }
			bool shorterToSpinInOtherDirection = MathF.Abs(delta) > MathF.PI;
			if (shorterToSpinInOtherDirection) {
				bool isNegative = delta < 0;
				delta = isNegative ? -(MathF.PI + delta) : -(MathF.PI - delta);
			}
			return delta;
		}
		public void SmoothRotateTarget(float targetRadians, float speed) {
			_targetDirection = targetRadians;
			float currentAngle = _target.Direction.NormalToRadians();
			float deltaToTarget = GetShortDeltaRotationAccountingForWrap(_targetDirection, currentAngle);
			const float rotationEpsilon = 1f / (1 << 16);
			bool needToTurn = MathF.Abs(deltaToTarget) > rotationEpsilon;
			AngularVelocity = !needToTurn ? 0 : speed * MathF.Sign(deltaToTarget);
		}
		public void ClearRotationTarget() {
			_targetDirection = float.NaN;
			AngularVelocity = 0;
		}
		internal void Brakes() {
			_thrustDuration = 0;
			_brake = true;
		}
		public void Draw(CommandLineCanvas canvas) { }
	}
}

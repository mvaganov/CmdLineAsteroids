﻿using ConsoleMrV;
using MathMrV;
using MrV;
using System;

namespace asteroids {
	public abstract class MobileObject : IGameObject {
		protected string _name;
		protected bool _active = true;
		protected Vec2 _velocity;
		protected Action<CommandLineCanvas> _preDraw;
		public virtual string Name { get => _name; set => _name = value; }
		public virtual Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public virtual bool IsActive { get => _active; set => _active = value; }
		public virtual bool IsVisible { get => IsActive; set => IsActive = value; }
		public abstract Vec2 Position { get; set; }
		public abstract Vec2 Direction { get; set; }
		public Action<CommandLineCanvas> DrawSetup { get => _preDraw; set => _preDraw = value; }
		public byte TypeId { get; set; }
		public abstract void Draw(CommandLineCanvas canvas);
		public virtual void Update() {
			if (!_active) {
				return;
			}
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			float dx = _velocity.x * Time.DeltaTimeSeconds;
			float dy = _velocity.y * Time.DeltaTimeSeconds;

			moveThisFrame = new Vec2(dx, dy);
			Position += moveThisFrame;
		}
		public virtual void Copy(MobileObject other) {
			TypeId = other.TypeId;
			_active = other._active;
			_velocity = other._velocity;
			_preDraw = other._preDraw;
		}
	}
}

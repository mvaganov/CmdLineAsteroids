using asteroids.src.asteroids;
using ConsoleMrV;
using MathMrV;
using System;

namespace asteroids {
	public interface IGameObject : IDrawable {
		public Vec2 Direction { get; set; }
		public Vec2 Position { get; set; }
		public bool IsActive { get; set; }
		public byte TypeId { get; set; }
		public void Update();
	}
}

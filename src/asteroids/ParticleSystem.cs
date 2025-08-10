using MathMrV;
using MrV;
using System;

namespace asteroids {
	public class ParticleSystem {
		public ObjectPool<Particle> ParticlePool = new ObjectPool<Particle>();
		public ValueOverTime SizeOverLifetime;
		public ConsoleColor Color = ConsoleColor.White;
		public Vec2 Position;
		public RangeF ParticleLifetime;
		public RangeF ParticleSize;
		public RangeF ParticleSpeed;
		public RangeF Radius;
		public Kind kind;
		public float RestartTimer = -1;
		private float CurrentTimer;
		public int BurstSize = 10;

		public static Action<Particle> OnParticleCommission;
		public static Action<Particle> OnParticleDecommission;

		public enum Kind {
			None, Explosion
		}

		public ObjectPool<Particle>.DelegateCommission CommissionDelegate {
			get => ParticlePool.CommissionDelegate;
			set => ParticlePool.CommissionDelegate = value;
		}
		public ObjectPool<Particle>.DelegateDecommission DecommissionDelegate {
			get => ParticlePool.DecommissionDelegate;
			set => ParticlePool.DecommissionDelegate = value;
		}

		public ParticleSystem(RangeF particleLifetime, RangeF particleSize, RangeF particleSpeed, Kind kind, ConsoleColor color, RangeF radius, ValueOverTime sizeOverLifetime) {
			ParticlePool.Setup(CreateParticle, CommissionParticle, DecommissionParticle, DestroyParticle);
			this.ParticleLifetime = particleLifetime;
			this.ParticleSize = particleSize;
			this.ParticleSpeed = particleSpeed;
			this.kind = kind;
			this.Color = color;
			this.Radius = radius;
			this.SizeOverLifetime = sizeOverLifetime;
		}
		public void Update() {
			ParticlePool.Update();
			if (RestartTimer > 0) {
				CurrentTimer += Time.DeltaTimeSeconds;
				if (CurrentTimer > RestartTimer) {
					Emit(BurstSize);
					CurrentTimer = 0;
				}
			}
		}
		public void Emit(int count) {
			for (int i = 0; i < count; i++) {
				ParticlePool.Commission();
			}
		}
		public void Emit(int count, Vec2 position) {
			Vec2 lastPosition = Position;
			Position = position;
			Emit(count);
			Position = lastPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="radius"></param>
		/// <param name="particleSize"></param>
		public void Emit(int count, Vec2 position, ConsoleColor color, RangeF radius = null, RangeF particleSize = null) {
			Vec2 lastPosition = Position;
			ConsoleColor lastColor = Color;
			RangeF lastRadius = Radius;
			RangeF lastParticleSize = ParticleSize;
			Position = position;
			Color = color;
			if (radius != null) {
				Radius = radius;
			}
			if (particleSize != null) {
				ParticleSize = particleSize;
			}
			Emit(count);
			Position = lastPosition;
			Color = lastColor;
			Radius = lastRadius;
			ParticleSize = lastParticleSize;
		}
		public void Absorb(Particle p) {
			ParticlePool.DecommissionDelayed(p);
		}
		public Particle CreateParticle() => new Particle(this, Position, ParticleSize.Value(), Vec2.Zero, Color, ParticleLifetime.Value());
		public void CommissionParticle(Particle particle) {
			particle.IsActive = true;
			particle.CurrentLife = 0;
			particle.Position = Position;
			particle.StartSize = ParticleSize.Value();
			particle.Radius = particle.StartSize * GetSizeAtTime(0);
			particle.Velocity = Vec2.Zero;
			particle.Color = Color;
			particle.Lifetime = ParticleLifetime.Value();
			switch (kind) {
				case Kind.Explosion:
					Vec2 direction = Vec2.NormalFromDegrees(360 * Rand.Number);
					particle.Velocity = direction * ParticleSpeed.Value();
					particle.Position = Position + direction * Radius.Value();
					break;
			}
			OnParticleCommission?.Invoke(particle);
		}
		public void DecommissionParticle(Particle particle) {
			particle.IsActive = false;
			OnParticleDecommission?.Invoke(particle);
		}
		public void DestroyParticle(Particle particle) { }
		public float GetSizeAtTime(float lifetime) {
			if (SizeOverLifetime == null) { return 1; }
			SizeOverLifetime.TryGetValue(lifetime, out float value);
			return value;
		}
	}
}

using System;

namespace MathMrV {
	public class Rand {
		private static Rand _instance;
		public static Rand Instance => _instance != null ? _instance : _instance = new Rand();
		public Random cSharpPseudoRandomNumberGenerator = new Random();
		public Rand() {
			_instance = this;
		}
		public static float Number => (float)Instance.cSharpPseudoRandomNumberGenerator.NextDouble();
	}
}

namespace MathMrV {
	public class RangeF {
		public float Min, Max;
		public float Delta => Max - Min;
		public RangeF(float min, float max) { Min = min; Max = max; }
		public static implicit operator RangeF((float min, float max) tuple) => new RangeF(tuple.min, tuple.max);
		public static implicit operator RangeF(float value) => new RangeF(value, value);
		public float Value() {
			float d = Delta;
			if (d == 0) { return Min; }
			return Min + d * Rand.Number;
		}
	}
}

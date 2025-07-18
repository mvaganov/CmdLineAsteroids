using MrV;
using System.Collections.Generic;

namespace MathMrV {
	public class ValueOverTime {
		public bool Wrap = false;
		public IList<Vec2> curve;
		public ValueOverTime(IList<Vec2> curve) {
			this.curve = curve;
		}
		public static ValueOverTime GrowAndShrink = new ValueOverTime(new Vec2[] { new Vec2(0,0), new Vec2(0.5f, 1), new Vec2(1, 0) });
		public static ValueOverTime None = new ValueOverTime(null);
		public float GetValue(float value) {
			if (curve == null || curve.Count == 0) { return value; }
			if (Wrap) {
				value = WrapValue(value, curve[curve.Count - 1].x - curve[0].x);
			}
			int index = Algorithms.BinarySearchWithInsertionPoint(curve, value, vec2 => vec2.x);
			if (index >= 0) {
				return curve[index].y;
			}
			index = ~index;
			if (index == 0) {
				return curve[0].y;
			}
			if (index >= curve.Count) {
				return curve[curve.Count - 1].y;
			}
			Vec2 prev = curve[index - 1];
			Vec2 next = curve[index];
			float timeDelta = next.x - prev.x;
			float timeProgress = value - prev.x;
			float normalizedTimeProgress = timeProgress / timeDelta;
			float valueDelta = next.y - prev.y;
			float result = prev.Y + normalizedTimeProgress * valueDelta;
			return result;
		}
		public static float WrapValue(float value, float dist) {
			while (value > dist) { value -= dist; }
			while (value < 0) { value += dist; }
			return value;
		}
	}
}

using MrV;
using System.Collections.Generic;

namespace MathMrV {
	public class ValueOverTime : ValueOverTime<float> {
		public static ValueOverTime GrowAndShrink = new ValueOverTime(new Frame[] { new Frame(0, 0), new Frame(0.5f, 1), new Frame(1, 0) });
		public static ValueOverTime None = new ValueOverTime(null);
		public ValueOverTime(IList<Frame> curve) : base(curve) {
		}
		public override float Lerp(float percentageProgress, float start, float end) {
			float delta = end - start;
			return start + delta * percentageProgress;
		}
	}
	public abstract class ValueOverTime<T> {
		public abstract T Lerp(float t, T start, T end);
		public struct Frame {
			public float time;
			public T value;
			public Frame(float time, T value) {
				this.time = time;
				this.value = value;
			}
		}
		public bool Wrap = false;
		public IList<Frame> curve;
		public ValueOverTime(IList<Frame> curve) {
			this.curve = curve;
		}
		public bool TryGetValue(float time, out T value) {
			if (curve == null || curve.Count == 0) {
				value = default;
				return false;
			}
			if (Wrap) {
				time = WrapNumber(time, curve[curve.Count - 1].time - curve[0].time);
			}
			int index = Algorithms.BinarySearchWithInsertionPoint(curve, time, frame => frame.time);
			if (index >= 0) {
				value = curve[index].value;
				return true;
			}
			index = ~index;
			if (index == 0) {
				value = curve[0].value;
				return true;
			}
			if (index >= curve.Count) {
				value = curve[curve.Count - 1].value;
				return true;
			}
			Frame prev = curve[index - 1];
			Frame next = curve[index];
			float normalizedTimeProgress = CalculateProgress(time, prev.time, next.time);
			value = Lerp(normalizedTimeProgress, prev.value, next.value);
			return true;
		}
		public static float CalculateProgress(float t, float start, float end) {
			float timeDelta = end - start;
			float timeProgress = t - start;
			return timeProgress / timeDelta;
		}

		public static float WrapNumber(float value, float dist) {
			while (value > dist) { value -= dist; }
			while (value < 0) { value += dist; }
			return value;
		}
	}
}

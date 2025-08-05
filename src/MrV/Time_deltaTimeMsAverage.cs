using System;

namespace MrV {
	public partial class Time {
		public static float DeltaTimeMsAverage => Instance.CalculateDeltaTimeMsAverage();
		private long[] _deltaTimeMsSamples = new long[20];
		private int _deltaTimeMsSamplesIndex;
		public static int DeltaTimeSampleCount {
			get => Instance._deltaTimeMsSamples.Length;
			set => Instance.SetDeltaTimeSampleSize(value);
		}
		public float CalculateDeltaTimeMsAverage() {
			long sum = 0;
			for (int i = 0; i < _deltaTimeMsSamples.Length; ++i) {
				sum += _deltaTimeMsSamples[i];
			}
			return sum / (float)_deltaTimeMsSamples.Length;
		}
		public void SetDeltaTimeSampleSize(int sampleSize) {
			long avg = (long)CalculateDeltaTimeMsAverage();
			int oldLen = _deltaTimeMsSamples.Length;
			Array.Resize(ref _deltaTimeMsSamples, sampleSize);
			_deltaTimeMsSamplesIndex = oldLen - 1;
			for (int i = oldLen; i < _deltaTimeMsSamples.Length; ++i) {
				_deltaTimeMsSamples[i] = avg;
			}
		}
		public static void UpdateAverageDeltaTime() => Instance.UpdateAverageDeltaTimeMs();
		public void UpdateAverageDeltaTimeMs() {
			if (++_deltaTimeMsSamplesIndex >= _deltaTimeMsSamples.Length) { _deltaTimeMsSamplesIndex = 0; }
			_deltaTimeMsSamples[_deltaTimeMsSamplesIndex] = deltaTimeMs;
		}
	}
}

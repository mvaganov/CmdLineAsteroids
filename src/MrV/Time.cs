using System;
using System.Diagnostics;

namespace MrV {
	public class Time {
		private static Time _instance;
		private long _deltaTimeMs;
		private float _deltaTimeSeconds;
		private double _lastUpdateTimeSeconds;
		private long _lastUpdateTimeMs;
		private Stopwatch _timer;
		private int _deltaTimeMsSamplesIndex;
		private long[] _deltaTimeMsSamples = new long[20];
		public static Time Instance => _instance ?? (_instance = new Time());
		public static long DeltaTimeMs => Instance._deltaTimeMs;
		public static float DeltaTimeSeconds => Instance._deltaTimeSeconds;
		public static double TimeSeconds => Instance._timer.Elapsed.TotalSeconds;
		public static long TimeMs => (long)Instance._timer.Elapsed.TotalMilliseconds;
		public static void Update() => Instance.UpdateSelf();
		public static void SleepWithoutConsoleKeyPress(int ms) => Instance.ThrottleWithoutConsoleKeyPress(ms);
		public static float DeltaTimeMsAverage => Instance.CalculateDeltaTimeMsAverage();
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
			for(int i = oldLen; i < _deltaTimeMsSamples.Length; ++i) {
				_deltaTimeMsSamples[i] = avg;
			}
		}
		public Time() {
			_timer = new Stopwatch();
			_timer.Start();
			_lastUpdateTimeSeconds = _timer.Elapsed.TotalSeconds;
			_lastUpdateTimeMs = _timer.ElapsedMilliseconds;
			UpdateSelf();
		}
		private long UpdateDeltaMs => _timer.ElapsedMilliseconds - _lastUpdateTimeMs;
		private float UpdateDeltaSeconds => (float)(_timer.Elapsed.TotalSeconds - _lastUpdateTimeSeconds);
		public void UpdateSelf() {
			_deltaTimeMs = UpdateDeltaMs;
			_deltaTimeSeconds = UpdateDeltaSeconds;
			_lastUpdateTimeSeconds = _timer.Elapsed.TotalSeconds;
			_lastUpdateTimeMs = _timer.ElapsedMilliseconds;
			if (++_deltaTimeMsSamplesIndex >= _deltaTimeMsSamples.Length) { _deltaTimeMsSamplesIndex = 0; }
			_deltaTimeMsSamples[_deltaTimeMsSamplesIndex] = _deltaTimeMs;
		}
		public void ThrottleWithoutConsoleKeyPress(int ms) {
			long soon = _lastUpdateTimeMs + ms;
			while(!Console.KeyAvailable && _timer.ElapsedMilliseconds < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

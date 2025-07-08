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

		public static Time Instance => _instance ?? (_instance = new Time());
		public static long DeltaTimeMs => Instance._deltaTimeMs;
		public static float DeltaTimeSeconds => Instance._deltaTimeSeconds;
		public static double TimeSeconds => Instance._timer.Elapsed.TotalSeconds;
		public static long TimeMs => (long)Instance._timer.Elapsed.TotalMilliseconds;
		public static void Update() => Instance.UpdateSelf();
		public static void SleepWithoutConsoleKeyPress(int ms) => Instance.ThrottleWithoutConsoleKeyPress(ms);
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
		}
		public void ThrottleWithoutConsoleKeyPress(int ms) {
			long soon = _lastUpdateTimeMs + ms;
			while(!Console.KeyAvailable && _timer.ElapsedMilliseconds < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

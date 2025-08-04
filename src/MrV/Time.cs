using System;
using System.Diagnostics;

namespace MrV {
	/// <summary>
	/// Keeps track of timing specifically for frame-based update in a game loop.
	/// Maintains values for time as Milliseconds and Floating point.
	/// <list type="bullet">
	/// <item>Uses <see cref="Stopwatch"/> as cannonical timer implementation</item>
	/// <item>Floating point values are convenient for physics calculations</item>
	/// <item>Floading point timestamps are stored internally as doubles for better precision</item>
	/// <item>Time is also calculated in milliseconds, since floating points become less accurate as values increase</item>
	/// </list>
	/// </summary>
	public partial class Time {
		private Stopwatch _timer;
		private long _deltaTimeMs;
		private long _lastUpdateTimeMs;
		private float _deltaTimeSeconds;
		private double _lastUpdateTimeSeconds;
		private static Time _instance;
		public static Time Instance => _instance != null ? _instance : _instance = new Time();
		public static long DeltaTimeMs => Instance._deltaTimeMs;
		public static float DeltaTimeSeconds => Instance._deltaTimeSeconds;
		public static double TimeSeconds => Instance._timer.Elapsed.TotalSeconds;
		public static long TimeMs => (long)Instance._timer.Elapsed.TotalMilliseconds;
		public static void Update() => Instance.UpdateSelf();
		public static void SleepWithoutConsoleKeyPress(int ms) => Instance.ThrottleUpdate(ms, ()=>Console.KeyAvailable);
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
		public void ThrottleUpdate(int ms, Func<bool> interruptSleep = null) {
			long soon = _lastUpdateTimeMs + ms;
			while((interruptSleep == null || !interruptSleep.Invoke()) && _timer.ElapsedMilliseconds < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

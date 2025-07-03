using System;
using System.Diagnostics;

namespace MrV {
	public class Time {
		private static Time _instance;
		private long _deltaTimeMs;
		private float _deltaTimeSeconds;
		private Stopwatch _timer;
		public static Time Instance => _instance ?? (_instance = new Time());
		public static long DeltaTimeMs => Instance._deltaTimeMs;
		public static float DeltaTimeSeconds => Instance._deltaTimeSeconds;
		public static void Update() => Instance.UpdateSelf();
		public Time() {
			_timer = new Stopwatch();
			_timer.Start();
			UpdateSelf();
		}
		public void UpdateSelf() {
			_deltaTimeMs = _timer.ElapsedMilliseconds;
			_deltaTimeSeconds = (float)_timer.Elapsed.TotalSeconds;
			_timer.Restart();
		}
		public static void SleepWithoutConsoleKeyPress(int ms) {
			long soon = Environment.TickCount + ms;
			while(!Console.KeyAvailable && Environment.TickCount < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

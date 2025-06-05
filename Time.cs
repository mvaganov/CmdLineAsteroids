using System.Diagnostics;

namespace asteroids
{
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
			_deltaTimeSeconds = _deltaTimeMs / 1000f;
			_timer.Restart();
			System.Console.SetCursorPosition(20, 20);
			System.Console.WriteLine($".......... {_deltaTimeSeconds}");
		}
	}
}

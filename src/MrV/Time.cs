using System;
using System.Diagnostics;

namespace MrV {
	/// <summary>
	/// Keeps track of timing, specifically for frame-based update in a game loop.
	/// <list type="bullet">
	/// <item>Uses C# <see cref="Stopwatch"/> as cannonical timer implementation</item>
	/// <item>Floating point values are convenient for physics calculations</item>
	/// <item>Floating point timestamps are stored as 'double' for precision, since 'float' becomes less accurate than 1ms after 4.5 hours</item>
	/// <item>Time is also calculated in milliseconds, since all floating points (even doubles) become less accurate as values increase</item>
	/// </list>
	/// </summary>
	public partial class Time {
		protected Stopwatch timer;
		public long deltaTimeMs;
		public float deltaTimeSeconds;
		protected long lastUpdateTimeMs;
		protected double lastUpdateTimeSeconds;
		protected static Time _instance;
		public static Time Instance => _instance != null ? _instance : _instance = new Time();
		public static long DeltaTimeMs => Instance.deltaTimeMs;
		public static float DeltaTimeSeconds => Instance.deltaTimeSeconds;
		public static double TimeSeconds => Instance.timer.Elapsed.TotalSeconds;
		public static long TimeMs => (long)Instance.timer.Elapsed.TotalMilliseconds;
		public static void Update() => Instance.UpdateTiming();
		public static void ThrottleWithoutConsoleKeyPress(int ms) => Instance.ThrottleUpdate(ms, ()=>Console.KeyAvailable);
		public Time() {
			timer = new Stopwatch();
			timer.Start();
			lastUpdateTimeSeconds = timer.Elapsed.TotalSeconds;
			lastUpdateTimeMs = timer.ElapsedMilliseconds;
			UpdateTiming();
		}
		public long DeltaTimeUpdatedMs => timer.ElapsedMilliseconds - lastUpdateTimeMs;
		public float DeltaTimeUpdatedSeconds => (float)(timer.Elapsed.TotalSeconds - lastUpdateTimeSeconds);
		public void UpdateTiming() {
			deltaTimeMs = DeltaTimeUpdatedMs;
			deltaTimeSeconds = DeltaTimeUpdatedSeconds;
			lastUpdateTimeSeconds = timer.Elapsed.TotalSeconds;
			lastUpdateTimeMs = timer.ElapsedMilliseconds;
		}
		public void ThrottleUpdate(int ms, Func<bool> interruptSleep = null) {
			long soon = lastUpdateTimeMs + ms;
			while((interruptSleep == null || !interruptSleep.Invoke()) && timer.ElapsedMilliseconds < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

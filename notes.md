## process
* get VS code and C#
* Program.cs "Hello World"
```
using System;
namespace CircleCollisions {
	public class Program {
		public static int Main(string[] args) {
			Console.WriteLine("Hello World!");
			return 0;
		}
	}
}
```
* draw a rectangle
```
		public struct Vec2 {
			public float x, y;
			public Vec2(float x, float y) { this.x = x; this.y = y; }
			public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
			public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
			public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
			public override string ToString() => $"({x},{y})";
		}
		public static void DrawRectangle(Coord position, Coord size, char letterToPrint) {
			for(int row = 0; row < size.y; ++row) {
				Console.SetCursorPosition(position.x, position.y + row);
				for(int col = 0; col < size.x; ++col) {
					Console.Write(letterToPrint);
				}
			}
		}
```
* draw a circle
```
		public class Circle {
			public Vec2 position;
			public float radius;
			public static void DrawCircle(char letterToPrint, Vec2 pos, float radius) {
				Vec2 extent = (radius, radius);
				Vec2 start = pos - extent;
				Vec2 end = pos + extent;
				Vec2 coord = start;
				float r2 = radius * radius;
				for (; coord.y < end.y; coord.y += 1) {
					coord.x = start.x;
					for (; coord.x < end.x; coord.x += 1) {
						if (coord.x < 0 || coord.y < 0) { continue; }
						float dx = (coord.x - pos.x);
						float dy = (coord.y - pos.y);
						if (dx * dx + dy * dy < r2) {
							Console.SetCursorPosition((int)coord.x, (int)coord.y);
							Console.Write(letterToPrint);
						}
					}
				}
			}
		}
```
* before game/simulation loop
```
		static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			DrawRectangle('#', (2, 3), (20, 15));
			DrawCircle('.', (18, 12), 10);
		}
```
* draw the circle changing radius and position in real time, using user input
* notice the flickering
* implement a timer to see how long it takes to render things
```
			Stopwatch timer = new Stopwatch();
			// ...
			while (running) {
				timer.Restart();
				// ...
				Console.WriteLine($"{timer.ElapsedMilliseconds}   ");
				// ...
```
* create a graphics context for dirty-pixel optimization
* draw the circle for real -- explain that this integration into the graphics context is being built now because this is the second time I wrote thos program. the first time I didn't do that graphics integration until I finished experimenting without it using static functions, including drawing a polygon
* add zoom in/out to graphics buffer
* draw the polygon
```
```
* rotate functionality for the polygon
* offset functionality for the polygon
* move the polygon in real time
```
```
* refactor the game loop before adding new functionality
* create a player class, with draw method
* add velocity to player class and update method
* create an input queue and input handler
* change player velocity, triggered by input queue
* add game logic to make player wrap around

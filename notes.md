## preface
Hopefully you've come here looking for a tutorial teaching how to build a real-time simulation in C#.
This tutorial is specifically for making a space-shooter game in the C# command line console.
The idea is as old as videogames. It was the reason why the C programming language and Unix operating system were invented.
I summon that primordial motivation now, to capture your attention, as I guide you though this software implementation.
Check the description for the Github project if you want this code. Continue watching if you want to understand this code.

Before I start the tutorial content, I want to be clear about something:
this tutorial will not be worth your time if you don't write and compile the code yourself, with the intention of understanding.
If you just want to play this game, save yourself some trouble and download the code from github.

I assume you already have a C# runtime, compiler, and IDE installed.
I assume you know the basics of how to program command line applications in C#.
"""
I also assume you are using LLMs to help you write code. Importantly:
  I assume you're aware of the intellectual hazard of relying on AI while programming. AI helps you **do**, not **learn**.
  If you are not here for a
I also assume you have already navigated the intellectual hazard of using AI to write your code.
If you get used to using an LLM, you stop being able to learn.
LLMs feel like they can do stuff for you. That creates an emotional reluctance to understanding yourself.
This is poison for a computer programmer. As a computer programmer, you must understand. understanding is the source of your power.

You must want to understand. Even when it is difficult, you must want to understand.
Ideally, you want to understand **because** it is difficult. 
"""

I have a spiritual need to share this tutorial with you. please let me explain:
  creating simulations from scratch makes me feel like a God.
  when I create a universe, and craft every rule myself, I feel a kind of joy that I imagine God feels when observing our universe.
  that joy is enough to motivate me to do it. but there is more.
  the kind of joy I feel when creating software simulations makes me feel close to the divine.
  It doesn't make me feel like a peer with the God of our Universe. It lets me very concretely recognize the abstraction that separates us from God.
  The pattern of understanding a realtime simulation fools me into believing I can feel the edge of existence. Like I'm touching the glass separating us from God.
  This feeling is so profound to me that I want to try to share it with you. And it is rooted in understanding.


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
* draw the circle for real, properly scaled
```
		public static void DrawCircle(char letterToPrint, Vec2 pos, float radius, Vec2 pixelSize, Vec2 bufferOrigin) {
			Vec2 extent = (radius, radius);
			extent.Scale(pixelSize);
			Vec2 start = pos.Scaled(pixelSize) - extent;
			Vec2 end = pos.Scaled(pixelSize) + extent;
			Vec2 coord = start;
			float r2 = radius * radius;
			for (; coord.Y < end.Y; coord.Y += 1) {
				coord.X = start.X;
				for (; coord.X < end.X; coord.X += 1) {
					if (coord.X < 0 || coord.Y < 0) { continue; }
					float dx = (coord.X / pixelSize.X - pos.X);
					float dy = (coord.Y / pixelSize.Y - pos.Y);
					if (dx * dx + dy * dy < r2) {
						Console.SetCursorPosition((int)(coord.X + bufferOrigin.X), (int)(coord.Y + bufferOrigin.Y));
						Console.Write(letterToPrint);
					}
				}
			}
		}
```
* create graphics context class to handle drawing in a double buffer
```
```
* --explain that this integration into the graphics context is being built now because this is the second time I wrote thos program.
* the first time I didn't do that graphics integration until I finished experimenting without it using static functions, including drawing a polygon
* add super-sampling to the graphics context, for antialiasing
```
```
* add some code to change the circle size as a test
```
```
* add color gradient system for the antialiasing, including ConsoleColorPair and ConsoleGlyph.
  * note that ConsoleColorPair could be optimized to 8bits, but it's probably not worth it.
```
```
* create a Time class to manage deltaTime, so we can check the passage of time during updates
```
```
* make a circle that moves. which we'll call a Particle
```
	public class Particle {
		public Circle circle;
		private Vec2 _velocity;
		private ConsoleColor _color;
		public float CurrentLife;
		public float Lifetime;
		public Vec2 Position { get => circle.center; set => circle.center = value; }
		public float Radius { get => circle.Radius; set => circle.Radius = value; }
		public Vec2 Velocity { get => _velocity; set => _velocity = value; }
		public ConsoleColor Color { get => _color; set => _color = value; }
		public Particle(Vec2 position, float size, Vec2 velocity, ConsoleColor color) {
			circle = new Circle(position, size);
			_velocity = velocity;
			_color = color;
		}
		public void Draw(CommandLineCanvas canvas) {
			canvas.SetColor(Color);
			circle.Draw(canvas);
		}
		public void Update() {
			Vec2 moveThisFrame = _velocity * Time.DeltaTimeSeconds;
			Position += moveThisFrame;
		}
	}
```
* create multiple particles in a particle explosion
```
```
* create a random generator, to create random directions
```
```
* give the particles a lifetime
* change their size over time with a ValueOverTime class
* create an ObjectPool class to manage multiple particles. we can use this later for other things, like game objects
* create a specialized particle system class
```
```
* create the RangeF class for giving particles random values within a range
```
```
* trigger the particles with a key press
```
```
* we're doing a lot of input. we should create a key-input class
```
```
* add zoom in/out to graphics buffer
```
```
* draw the polygon
```
```
* rotate functionality for the polygon
* offset functionality for the polygon
* move the polygon in real time
```
```
* refactor the game loop before adding new functionality...
* design the program in broad strokes. draw diagrams. create some core interfaces. emphasize the need for imagination, and clear vision.
```
IGameObject, IDrawable, ICollidable
```
* create the core object classes
```
MobileObject, MobileCircle, MobilePolygon
```
* create a player-controlled MobilePolygon class
  - special note be careful about class design! if you do it while you are tired, you can easily regret writing a strict structure that you later need to unmake
* add velocity to player class and update method
```
```
* change player velocity, triggered by key input queue
```
```
* add code for an asteroid
```
```
* add a MemoryPool for the asteroids
```
```
* create a MemoryPool for bullets, triangle shaped MobilePolygons
```
```
* add code to shoot the bullets
```
```
* create naive collision detection that just checks circles, checking all objects in O(n^2) algorithm
```
```
* add code to destroy asteroids when they are hit by a bullet. the bullet should also go away after collision
```
```
* code complexity is about to get crazy, and we'll need debugging. create a Log class with Assert.
```
```
* add Welzl's algorithm for min bounding circle
```
```
* add sub-collision circles to the Polygon, for finer collision detection
```
```
* create a SpacePartition class, with drawing functionality for testing
* requires AABB
```
```
* 
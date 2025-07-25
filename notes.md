## preface
This is a tutorial teaching how to build a real-time simulation in C#.
The simulation is a space-shooter game, written using in the C#, in the command line console.
The idea is as old as videogames. It was the reason why the C programming language and Unix operating system were invented.
I've summoned that ancient motivation to capture your attention while I teach you foudnational concepts for writing game engines.
Check the description for the Github project if you want this code. Continue watching if you want to understand this code.

I assume you already have a C# runtime, compiler, and IDE installed.
I assume you know the basics of how to program command line applications in C#.

//I also assume you are using LLMs to help you write code. Importantly:
//  I assume you're aware of the intellectual hazard of relying on AI while programming. AI helps you **do**, not **learn**.
//  If you are not here for a
//I also assume you have already navigated the intellectual hazard of using AI to write your code.
//If you get used to using an LLM, you stop being able to learn.
//LLMs feel like they can do stuff for you. That creates an emotional reluctance to understanding yourself.
//This is poison for a computer programmer. As a computer programmer, you must understand. understanding is the source of your power.
//
//You must want to understand. Even when it is difficult, you must want to understand.
//Ideally, you want to understand **because** it is difficult. 

//I have a spiritual need to share this tutorial with you. please let me explain:
//  creating simulations from scratch makes me feel like a God.
//  when I create a universe, and craft every rule myself, I feel a kind of joy that I imagine God feels when observing our universe.
//  that joy is enough to motivate me to do it. but there is more.
//  the kind of joy I feel when creating software simulations makes me feel close to the divine.
//  It doesn't make me feel like a peer with the God of our Universe. It lets me very concretely recognize the abstraction that separates us from God.
//  The pattern of understanding a realtime simulation fools me into believing I can feel the edge of existence. Like I'm touching the glass separating us from God.
//  This feeling is so profound to me that I want to try to share it with you. And it is rooted in understanding.

Before I start the tutorial content, I want to be clear about something:
this tutorial will not be worth your time if you don't write and compile the code with the intention of understanding.
If you just want to play this game, save yourself some trouble and download the code from github.

## process
* Lets begin.
* Start your C# IDE. I'm using Visual Studio Community 2022.
* I'm going to call my project "SpaceShooterGame"
* The default Program.cs will be our entry point. Here's a basic "Hello World" program.
* Let's run it to make sure our compiler works. If your program does not compile and run, stop the video and get it working.
```
using System;
namespace SpaceShooterGame {
	public class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
		}
	}
}
```
* Lets start our game by drawing the screen where the game will be displayed.
```
			int Width = 80, Height = 25, position_x = 0, position_y = 0;
			char letterToPrint = '.';
			for(int row = 0; row < Height; ++row) {
				for(int col = 0; col < Width; ++col) {
					Console.SetCursorPosition(position_x + col, position__y + row);
					Console.Write(letterToPrint);
				}
			}
```
* this is a pretty standard nested for loop iterating over a two-dimensional space.
  * the logic here places the command line cursor exactly at each position in the rectangle before printing a character.
* we'll need a two dimensional Vector structure for many things in this game, and it will be handy in this rectangle drawing code.
```
		public struct Vec2 {
			public float x, y;
			public Vec2(float x, float y) { this.x = x; this.y = y; }
			public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
			public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
			public override string ToString() => $"({x},{y})";
			public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
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
* I spent a few weeks creating this game before starting the tutorial series.
* the steps I'm going to go through are roughly the same as what I did the first time,
	* with the extreme benefit of having made lots of implementation mistakes already.
	* I do want to be clear that this code was written slowly the first time.
	* There is a lot of code to explain, so I will be very brief about most of the code.
	* If you have questions, please ask an LLM to explain what is going on. They are actually pretty good at that.
* and let's test it
```
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			DrawRectangle('#', (2, 3), (20, 15));
		}
```
* this game will use circles for a lot of things. so we need to create that class
```
		public class Circle {
			public Vec2 position;
			public float radius;
			public static void DrawCircle(char letterToPrint, Vec2 pos, float radius) {
				Vec2 extent = (radius, radius); // Vec2 knows how to convert from a tuple of floats
				Vec2 start = pos - extent;
				Vec2 end = pos + extent;
				Vec2 coord = start;
				float r2 = radius * radius;
				for (; coord.y < end.y; coord.y += 1) {
					coord.x = start.x;
					for (; coord.x < end.x; coord.x += 1) {
						if (coord.x < 0 || coord.y < 0) { continue; }
						Vec2 d = coord - pos;
						bool pointIsInCircle = d.x * d.x + d.y * d.y < r2;
						if (pointIsInCircle) {
							Console.SetCursorPosition((int)coord.x, (int)coord.y);
							Console.Write(letterToPrint);
						}
					}
				}
			}
		}
```
* this circle class is far from complete, but it's enough to test.
```
		static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			DrawRectangle('#', (2, 3), (20, 15));
			DrawCircle('.', (18, 12), 10);
		}
```
* the circle is more interesting than the rectangle, so I'll spend a bit more time testing it.
* I want to be able to play with the position and radius. I'll add some code to read console input, and redraw the app
```
```
* this is already turning into a game engine.
* we have our core main loop, with draw code, input gathering code, and logic to update data too.
  * these three sections will continue to get flushed out during this tutorial
* draw the circle changing radius and position in real time, using user input
* notice the flickering
* drawing in the command line is actually really slow. lets implement a timer to see how long it takes to render things
```
			Stopwatch timer = new Stopwatch();
			// ...
			while (running) {
				timer.Restart();
				// ...
				Console.WriteLine($"{timer.ElapsedMilliseconds}   ");
				// ...
```
* //create a graphics context for dirty-pixel optimization
* notice the flickering. that flickering is also a source of performance problems.
* we can significantly reduce the flickering by only redrawing characters that change between frames.
* to do that, we need to be able to querey what the current frame looks like, and what the previous frame looked like
```
```
* let's test this out
```
```
* the circle is drawing more quickly now. but the sape is still not correct,
	* because the command line console's characters are not perfect squares
* lets implement a function that will draw the circle with the scale of the command line taken into account
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
// TODO <----------------------
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
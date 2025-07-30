## preface

`scene`
"If I can't create it, I don't understand it."
-Richard Feynman

`voice`
This is a tutorial teaching how to build a real-time simulation in C#, which can simulate basic physics and object interactions.
As a computer science teacher and professional developer, I'll also offer advice as best practices.
Game Development a difficult kind of programming, so this exercice will be an excellent example to discuss engineering best practices.

`scene`
demo reel of the asteroids game

`voice`
The simulation is a space-shooter game, written for the command line console.
The idea is as old as videogames. It was the reason why the C programming language and Unix operating system were invented.
I've summoned that ancient motivation to capture your attention while I teach you foundational concepts for writing game engines.
Check the description for the Github project if you want this code. Continue watching if you want to understand this code.

I spent a few weeks creating this game and writing this script before creating the tutorial series.
please do not misunderstand that this program just fell out of my head in one moment.
programming does not work that way. sometimes we wish it did, but it doesn't. your projects will take a long time to finish too.
be patient with yourself.
my guidance will follow roughly the same path I went through, but it the tutorial will be many times faster.
Now I have the extreme benefit of having made lots of implementation mistakes recently.
There is a lot of code to explain, so I will be very brief about most of the code.

`scene`
montage of code

`voice`
I'll be using C# as the programming language.
I assume you already have a C# runtime, compiler, and IDE installed.
I assume you know the basics of how to program command line applications in C#.

`scene`
montage of Unity

`voice`
The biggest reason for this language choice is the integration it has with the popular Unity game engine.
This tutorial should give you some insight into how a game engine like Unity is created.

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

//Before I start the tutorial content, I want to be clear about something:
//this tutorial will not be worth your time if you don't write and compile the code with the intention of understanding.
//If you just want to play this game, save yourself some trouble and download the code from github.

`scene`
new project window in VS Community 2022

`voice`
Lets begin. Please pause the tutorial to attempt these same steps yourself. I recommend typing everything out yourself.
it will help you learn this content.

`scene`
	 practice is the price for understanding.
understanding is the price for power in the computer.

`voice`
practicing typing yourself will dramatically increase how long this tutorial takes for you to do, probably 10x longer or more.
If you do not consider yourself a computer wizard yet, I recommend you practice now.

`scene`
new project window in VS Community 2022

`voice`
Start your C# IDE. I'm using Visual Studio Community 2022.
As of 2025, Rider is not free for commercial content, but free for personal use.
I should not use it for a tutorial video, but I recommend that as a student.

I'm going to call my project "SpaceShooterGame"

`scene`
program.cs
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

`voice`
The default Program.cs will be our entry point. This is a basic "Hello World" program.
If you are unfamiliar with Hello World in C#, please pause, and find a C# Hello World tutorial to practice before continuing.

Let's run this code to make sure everything works. If your program does not compile and run, stop the video and get it working.
Unfortuantely, most programming environments require some configuration, even with an automatic installer.

`scene`
Visual Studio Installer -> Modify -> .NET desktop development

`voice`
For example, be sure you have the .NET desktop development workload installed by the Visual Studio Installer.

`scene`
back to program.cs

`voice`
Lets start our game by drawing the screen where the game will be displayed.
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
this is a pretty standard nested for loop iterating over a two-dimensional space.
the logic here places the command line cursor exactly at each position in the rectangle before printing a character.

please take a moment to understand this code.
if this code is confusing, I highly recommend practicing loops before continuing.
the programming in this tutorial will not get simpler beyond this point.

`scene`
Vec2.cs

`voice`
we'll need a two dimensional Vector structure for many reasons in this game, and we can start using it in this rectangle drawing code.
I'll be doing this kind of code refactoring a lot during my tutorial to emphasize how important it is to do during your own programming.
I apologize for speeding it up. Please pause and rewind the video yourself as needed.
```
		public struct Vec2 {
			public float x, y;
			public Vec2(float x, float y) { this.x = x; this.y = y; }
			public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
			public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
			public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
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

`voice`
a 2 dimensional vector is a physics and math concept. The basic premise is:

`scene`
TODO diagram for each line

locations in space can be fully described by position along each dimension.
Point A can be at a known location.
Point B can be at another location.
The difference between A and B can also be described as a Vector, by subtracting the components of Point A from point B. This is typically called the Delta.
The distance between A and B can be calculated by doing the pythagorean theorum on the delta. This is typically called the Magnitude.
The direction that point B is from point A can be calculated by dividing the Delta's components by the Magnitude. This converts Delta to a point on a Unit Circle. This value is also called a Normal.
The Normal's X and Y components correspond to the Cosine and Sine of the angle between the line AB and another line going positive along the X axis.
This Normal version of an angle can also be used for rotation calculations.
The perpendicular angle can be determined by swapping the X and negative Y components into another vector.
The alignment of one vector with another can be calculated by a math operation called a Dot Product.
These mathematical relationships help us determine collision of objects, and resolve a variety of physics interactions.

There are plenty of tutorials on the internet about 2D vectors. One is linked in the description
`add to description` Two dimensional vector concept tutorial https://youtu.be/j6RI6IWd5ZU

This implementation includes the addition and subtraction operators, and also implicit tuple casting.

The data structure is small, with each float taking up only 4 bytes for a total of 8 bytes.
For this reason, I'm writing the vector as a struct, because doing so makes the vector a more efficient Value type.
If you don't understand the difference between a value type and a refernce type, I recommend doing some research about it.
This has implications in how we use the data structure, and the performance of code using it.

let's test it

`scene`
writing and compiling program.cs
```
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			DrawRectangle('#', (2, 3), new Vec2(20, 15));
		}
```

`voice`
notice I'm using tuple notation for the first vector describing position, and an explicit constructor for the size.
the form is mostly stylistic. however, in an inner-loop, using the constructor is preferred because it is slightly faster to execute.

`scene`
```
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
```

`voice`
this game will need circles for a lot of things.
let's prove that we can draw a circle in the command line before doing additional circle-related programming.
this circle drawing code uses a nested for loop like the rectangle drawing code.
it has a conditional in the inner loop testing against the equation of a circle.
```
		static void Main(string[] args) {
			Console.WriteLine("Hello World!");
			DrawRectangle('#', (2, 3), (20, 15));
			DrawCircle('.', (18, 12), 10);
		}
```

`scene`
```
		static void Main(string[] args) {
			bool running = true;
			Vec2 position = (18,12);
			float radius = 10;
			float moveIncrement = 0.125f;
			while (running) {
				DrawCircle('.', position, radius);
				char input = Console.ReadKey().keyChar;
				switch(input) {
				case 'w': position.y -= moveIncrement; break;
				case 'a': position.x -= moveIncrement; break;
				case 's': position.y += moveIncrement; break;
				case 'd': position.x += moveIncrement; break;
				case 'e': radius += moveIncrement; break;
				case 'r': radius -= moveIncrement; break;
				case (char)27: running = false; break;
				}
			}
		}
```

`voice`
the circle is more interesting than the rectangle, so I'll spend a bit more time testing it.

`scene`
run the app to test

`voice`
here is a basic interactive loop, where we can play with the circle values

this is already turning into a game engine.
a game engine has 4 main regions: initializationm, draw, input, and update

`scene`
```
		static void Main(string[] args) {
			// initialization
			bool running = true;
			Vec2 position = (18,12);
			float radius = 10;
			float moveIncrement = 0.125f;
			char input;
			while (running) {
				Draw();
				ProcessInput();
				Update();
			}
			void Draw() {
				DrawCircle('.', position, radius);
			}
			void ProcessInput() {
				input = Console.ReadKey().keyChar;
			}
			void Update() {
				switch(input) {
				case 'w': position.y -= moveIncrement; break;
				case 'a': position.x -= moveIncrement; break;
				case 's': position.y += moveIncrement; break;
				case 'd': position.x += moveIncrement; break;
				case 'e': radius += moveIncrement; break;
				case 'r': radius -= moveIncrement; break;
				case (char)27: running = false; break;
				}
			}
		}
```
C# enables us to create local functions, which help us name and organize our code.
many programmers, myself included, consider it good programming style to use small functions, with descriptive names, and only one or two levels of indentation.
lets run this after refactoring to make sure it still works

`scene`
run again

`voice`
notice the flickering
drawing a character in the command line is actually really slow.
lets implement a timer to see how long it takes to render things
and let's put the key input behind a check, so it doesn't block the game loop.

`scene`
```
			Stopwatch timer = new Stopwatch();
			// ...
			while (running) {
				timer.Restart();
				// ...
				Console.WriteLine($"{timer.ElapsedMilliseconds}   ");
				// ...
```

my computer is pretty fast, but this is really slow. it looks like it's running at around 10 frames per second.
notice the flickering. that flickering indicates a performance problem.

the code that claculates timing feels pretty bad, so before we continue, I want to implement a time-keeping class
`scene`
Time.cs

```
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
		private static Time _instance;
		private long _deltaTimeMs;
		private long _lastUpdateTimeMs;
		private float _deltaTimeSeconds;
		private double _lastUpdateTimeSeconds;
		private Stopwatch _timer;
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
```

`voice`
// TODO explain Time.cs

we can significantly reduce the flickering by only redrawing characters that change between frames.
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
* Circle.cs
* this circle class is far from complete, but it's enough to test.
```
		public class Circle {
			public Vec2 position;
			public float radius;
		}
```

* * --explain that this integration into the graphics context is being built now because this is the second time I wrote thos program.
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
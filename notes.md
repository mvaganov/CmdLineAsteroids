## preface

`scene`
"If I can't create it, I don't understand it."
-Richard Feynman

pause for 5 seconds

`scene`
demo reel of the LowFiRockBlaster game

`voice`
This is a tutorial series teaching how to build a real-time simulation in C#.
It simulates basic physics and collision detection and implements other essential games and simulation systems

`scene`
list with the following
	2D Vector math
	basic physics
	primitive rendering
	rendering primitives
	time tracking
	a task scheduler
	a key input buffer based on a dispatch table
	some basic graphics optimizations
	memory pooling
	particle systems
	collision detection
	cell space partition tree

`voice`
I'll explain everything starting from an empty project, in the TTY Command Line Console. Including the graphics, math, and collision detection, and all data structures.
I'll also offer in-context advice and best practices from my decades of experience as a game developer and computer science instructor.
And I'll give some of my own opinions about the Invisible Wizard Problems that define modern computer programming, and game development especially.

TODO mark Invisible Wizard Problems (IWP):
	top-down planning
	refactoring
	good naming
	function separation

`voice`
The simulation is a space-shooter game inspired by Spacewar! from 1962, written for the command line console.
The idea is as old as videogames. It was the reason why the C programming language and Unix operating system were invented.
I've summoned that ancient primordial motivation to capture your attention now, while I teach you foundational concepts for writing a game engine.
Check the description for the Github project link if you want the code. Continue watching if you want a thorough lesson to understand the code.

I spent a few weeks creating this game, and a few months writing this script.
Please do not misunderstand that this program just fell out of my head in one moment. Programming does not work that way.
Your projects will take a long time to finish too, even if they use a tutorial like this as a starting point.
Be patient with yourself. Be disciplined with your self. I believe anyone who sits with these ideas can learn them well, even if you don't consider yourself skilled with math.
I was terrible at math in High School, and learned what I know because of practice doing projects like this.
My guidance will follow roughly the same path I went through while making this game a few weeks ago, but it will be much faster, even with a few detours to experiment with some math.
Because I wrote this application once already, I have the extreme benefit of having made lots of mistakes recently.
Please be patient with your own mistakes, and the frustration that follows. Frustration is the sweat of learning. It is a sign that your mind is growing. Take a break if you need it.

`scene`
montage of code and the game

`voice`
I'll be using C# as the programming language.
I assume you already have a C# runtime, compiler, and IDE installed.
I also assume you know the basics of how to program command line applications in C#, including object oriented programming basics.
You can still follow along without that knowledge, but I recommend you start with that foundation.

`scene`
montage of Unity

`voice`
The Unity Game Engine is a notable reason I chose to do this in the C# language.
This tutorial should also give new developers some insight into how a game engine like Unity works.

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
	 practice is the price for understanding.
understanding is the price for power in the computer.

`voice`
If you are new to game programming, you should practice by typing all of this yourself.
This will dramatically increase how long this tutorial takes for you to do, probably 10x longer or more.
I believe that time will be worth the understanding you gain.
If you do not consider yourself a computer wizard yet, I recommend you practice more.

`scene`
new project window in VS Community 2022

`voice`
Start your C# IDE. I'm using Visual Studio Community 2022.
As of 2025, Rider is not free for commercial content, but free for personal use.
I should not use it for a tutorial video, but I recommend it for educational purposes.
Rider is similar enough to Visual Studio that this tutorial will still work well for it.

I'm going to call my project "LowFiRockBlaster".
And I'll be using .NET Core 5.0, because Unity doesn't support the latest C# features, and I want this tutorial to prepare you for Unity Game Development.

`scene`
program.cs
```
using System;

namespace MrV.LowFiRockBlaster {
	public class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");
		}
	}
}
```

`voice`
The default Program.cs will be our entry point. This is a basic "Hello World" program.
I'll be writing everything in my own MrV namespace, I recommend you name your own namespace yourself.
I'll also be using a compact whitespace style, so I can fit as much code on the screen as possible.

Let's run this code to make sure everything works. If your program does not compile and run, stop the video and get it working.
Unfortuantely, most programming environments require some configuration, even with an automatic installer.

`scene`
Visual Studio Installer -> Modify -> .NET desktop development

`voice`
For example, be sure you have the .NET desktop development workload installed by the Visual Studio Installer.

---

`scene`
Work Breakdown Structure
* game design document
	* graphics
		* draw: circles (asteroints, powerups)
		* draw: polygons (player, projectiles)
		* colors in the command line, with text overlay
	* logic
		* simulation runs in real time, even while player isn't providing input
		* objects move using simple physics
		* objects interact when they collide
		* player character is an object, controlled by user
	* player choices
		* move in cardinal directions (up/down/left/right)
		* shoot projectile, if sufficient ammo
		* avoid moving asteroids, or else be destroyed

`voice`
Before I start writing the game, I want to have a clear set of goals. An imagined vision of the game is a necessary starting point.
A durable list of features and expectations is a valuable next step. This is essential for a project that will take several days.
For game development, this is often called a Game Design Document. An effective form of a Game Design Document is a Work Breakdown Structure, like this.
More experienced developers will need fewer details and less structure to create a product.
Add just enough detail to your list of expectations that you feel you will remember your game vision when you read this document again. Avoid adding more detail than that.
Spending too much time writing a design or specification is sometimes called Analysis Paralisys, and it is a real cause for projects to fail before they even start.
Identify clear goals that you can start implementing, and give yourself the grace to updatee the document later.

`scene`
back to program.cs

`voice`
Lets start our game by drawing the screen where the game will be displayed.
I'll make a new function called DrawRectangle, and call it in my Main function. You'll notice that it's public static
we want it to be public staic for three reasons:
	- it doesn't have any dependencies on the Program class, so we should be able to run it from anywhere
	- Main is in a public static context, and needs it to be public static also
	- public static function calls are technically faster than non-public static calls. 
		- the speed gain is so extremely small that it hardly bears mentioning.
		- But this is game programming, and performance is important to think about.
		- I will not be taking extreme step to optimize this game while I write it, for the sake of clarity,
			but I will intentionally choose a more performant style, out of habit.
			- an example of this habit can be seen in this code, where I use the prefix increment operator.
				it's one assembly instruction faster in old compilers.
```
		int width = 80, height = 23;
		char letterToPrint = '#';
		DrawRectangle(0, 0, width, height, letterToPrint);
```
```
		public static void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			for (int row = 0; row < height; ++row) {
				for (int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col + x, row + y);
					Console.Write(letterToPrint);
				}
			}
		}
```
this is a pretty standard nested for loop iterating over a two-dimensional space.
the logic here places the command line cursor exactly at each position in the rectangle before printing a character.

Before moving on, let's take a moment to understand this logic.
It seems pretty specific to the command line console, but gaining familiarity with this logic will help with may other kinds of problem solving in the future.

`scene`
show the code and running output
```
			int width = 80;
			char letterToPrint = '#';
			for (int col = 0; col < width; ++col) {
				Console.Write(letterToPrint);
			}
```

`voice`
this code will write 80 hashtag characters in a row.

`scene`
show the code and running output
```
			int width = 80, height = 23;
			char letterToPrint = '#';
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.Write(letterToPrint);
				}
			}
```

`voice`
this code will write 80 times 25 hashtag characters in a row.

`scene`
show the code and running output
```
			int width = 80, height = 23;
			char letterToPrint = '#';
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.Write(letterToPrint);
				}
				Console.WriteLine();
			}
```

`voice`
this code will write a rectangle 80 wide and 25 tall.

`scene`
show the code and running output
```
		public static void DrawRectangle(int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.Write(letterToPrint);
				}
				Console.WriteLine();
			}
		}
```
```
		int width = 80, height = 23;
		char letterToPrint = '#';
		DrawRectangle(width, height, letterToPrint);
		DrawRectangle(width, height, letterToPrint);
```

`voice`
If we turn this into a function, we can print a new rectangle right after this one.
Doing this allows us to call the function at any time from any place in our program.

`scene`
show the code and running output
```
		public static void DrawRectangle(int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col, row);
					Console.Write(letterToPrint);
				}
			}
		}
```

`voice`
we can use SetCursorPosition to move the commandline cursor exactly where we want it before printing any character with Console.Write
This functionality is not easily available in all programming language console APIs, so it's nice that C# handles it for us.
For example, if you want to do the same thing in Python, you need to replace SetCursorPosition with printing an escape sequence.
And that escape sequence will not work if executed in the basic Windows console.

`scene`
show the code and running output
```
		public static void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col + c, row + y);
					Console.Write(letterToPrint);
				}
			}
		}
```

`voice`
and this code allows us to draw the rectangle anywhere in visible space.

```
		public static void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					if (col + x < 0 || row + y < 0) {
						continue;
					}
					Console.SetCursorPosition(col + c, row + y);
					Console.Write(letterToPrint);
				}
			}
		}
```

`voice`
The code will crash if x and y are negative, which can be solved with a simple if statement.

if this code is confusing, I highly recommend practicing loops before continuing.
the programming in this tutorial will get much more conceptually complex beyond this point.

---

`scene`
Create a src folder, MrV folder, Math folder. create Vec2.cs inside of src/MrV/Geometry
Vec2.cs
```
namespace MrV.Geometry {
	public struct Vec2 {
		public float x, y;
		public Vec2(float x, float y) { this.x = x; this.y = y; }
		public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
		public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
		public static implicit operator Vec2((float x, float y) tuple) => new Vec2(tuple.x, tuple.y);
		public override string ToString() => $"({x},{y})";
	}
}
```

`scene`
Program.cs
put Program.cs into src. 
```
		public static void DrawRectangle(Vec2 position, Vec2 size, char letterToPrint) {
			DrawRectangle((int)position.x, (int)position.y, (int)size.x, (int)size.y, letterToPrint);
		}
```
```
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
```

`voice`
We should get serious about 2 dimensional structures.
This simulation will have many of them, and we should start using 2D concepts in this rectangle drawing code.
Also, this program will need many source files, and I want to organize them with folders.

I'll be doing this kind of code refactoring a lot during my tutorial.
If you are new to programming, you need to know that this is how big projects are written: one step at a time, with lots of rewrites, and tests between changes.
It will be slower for you, and that's fine.
I apologize for speeding it up. Please pause and rewind the video for yourself as needed.

Notice that I'm using the old DrawRectangle class in this new function.

`voice`
Vec2 is a 2 dimensional vector, which is a physics and math concept. The basic premise is:

`scene`
Vec2.cs, with diagram for each line when it is discussed

Point A can be at a known location x/y.
	in the command line, 0,0 is at the upper left corner, and y increases as it goes down.
	I'll show you how to change this conceptually later.
Point B can be at another location x/y.
locations in space can be fully described by a vector, which is a position along each dimension.
  distances can also be described this way.
  directions can also be described this way.
//The difference between A and B can also be described as a Vector, by subtracting the components of Point A from point B. This is typically called the Delta.
//The distance between A and B can be calculated by doing the pythagorean theorum on the Delta. This is the Distance between A and B, also called the Magnitude of Delta.
//The direction of point B from point A can be calculated by dividing the Delta's components by the Magnitude. This converts Delta to a point on a Unit Circle. This value is also called a Normal.
//This Normal vector of an angle can be used for rotation calculations. Notably, the math for rotating vectors this way is identical to the math used to convert imaginary numbers into real numbers.
//The Normal's X and Y components correspond to the Cosine and Sine of the angle between the line AB and another line going positive along the X axis.
//  Luckily, we don't need to think about trig identities or the differences between radians and degrees if we keep angles as normals. But for the sake of completeness, I've included that math here.
//A perpendicular direction can be determined by swapping the X and negative Y components into another vector.
//The alignment of one vector with another can be calculated by a math operation called a Dot Product.
//Alignment with a perpendicular vector is required information when calculating how one Vector will reflect on another surface normal in a physica collision.
//The mathematical bundled up in the Vector concept helps us fully define many things in a software simulation, including the physics interactions this tutorial will show later.
There are plenty of additional tutorials on the internet about 2D vectors, check the description for examples:
`add to description` Two Dimensional Vector Concept:
  3blue1brown https://youtu.be/fNk_zzaMoSs
  HoustonMathPrep https://youtu.be/j6RI6IWd5ZU

`scene`
AABB.cs
create in the src/MrV/Geometry folder
```
namespace MrV.Geometry {
	public struct AABB {
		public Vec2 Min, Max;
		public float Width => (Max.x - Min.x);
		public float Height => (Max.y - Min.y);
		public AABB(AABB r) : this(r.Min, r.Max) { }
		public AABB(Vec2 min, Vec2 max) { Min = min; Max = max; }
		public AABB(float minx, float miny, float maxx, float maxy) :
			this(new Vec2(minx, miny), new Vec2(maxx, maxy)) { }
		public override string ToString() => $"[min{Min}, max{Max}, w/h({Width}, {Height})]";
	}
}
```

Diagram of AABB as it is discussed
```
		public static void DrawRectangle(AABB aabb, char letterToPrint) {
			DrawRectangle((int)aabb.Min.x, (int)aabb.Min.y, (int)aabb.Width, (int)aabb.Height, letterToPrint);
		}
```
```
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
```

`voice`
A box can be described with two Vec2 structures, bounded by edges aligned on the x and y axis. We call this an Axis Aligned Bounding Box or AABB.
This is a simple description of space in a simulation, and it is used for many kinds of clalculations. 
notice I'm again using public static functions, and calling a common function that has the logic written once
	computer programmers need to have a Single Point Of Truth wherever possible, even at the cost of performance.
	Single Point Of Truth is an optimization for the Programmer, not for the computer.
	If we can keep complicated logic in one place, then we only need to fix one place when there is a bug in it.
	We can always inline our functions as a final optimization step.

`scene`
writing and compiling program.cs
```
		static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			DrawRectangle(0, 0, width, height, letterToPrint);
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
			Console.SetCursorPosition(0, (int)height);
		}
```

`voice`
notice I'm using tuple notation for the first vector describing position, and an explicit constructor for the size.
the form is mostly stylistic. however, in an inner-loop, using the constructor is preferred because it is slightly faster to execute.

`scene`
Circle.cs
```
namespace MrV.Geometry {
	public struct Circle {
		public Vec2 center;
		public float radius;
		public Circle(Vec2 position, float radius) { this.center = position; this.radius = radius; }
		public override string ToString() => $"({center}, r:{radius})";
	}
}
```

`voice`
A circle can be described as a Vector with one additional value for radius.

`scene`
Polygon.cs
```
namespace MrV.Geometry {
	public struct PolygonShape {
		public Vec2[] points;
		public PolygonShape(Vec2[] points) {
			this.points = points;
		}
		public override string ToString() => $"(polygon: {string.Join(", ", points)})";
	}
}
```

`voice`
A polygon's shape can be described as a list of 2 dimensional vectors, with the assumption that there is a line between each point in the sequence.
//A more useful polygon would be that shape, which can be offset to a new position, and also rotated. We'll cover that math in more detail later.

These data structures are small and simple in memory. Each float taking up only 4 bytes. The Vec2 is a total of 8 bytes. Circle is 12. AABB is 16. The points array of Polygon is a reference, which is also small, probably 8 bytes.
Because they are simple in memory, these are written as struct Value types instead of class Reference types. A Value type has certain memory advantages.
A program passes it's data by default instead of a reference to it's data. As a result, the CPU is more certain about the value of a value-type, because it doesn't need to check a reference. This design eliminates cache misses for this data.
Check the description for additional explanation about the difference between value type and refernce type:
`add to description` Value Type vs. Reference Type:
  CodeMonkey: https://youtu.be/KGFAnwkO0Pk
  MarkInman: https://youtu.be/95SkyJe3Fe0
//A big reason for using value types is that the memory is forced to be local to the calculation space, which eliminates the possibility of cache misses
//`add to description` Cache Misses:
//  MLGuy: https://youtu.be/RkRUuNdb7io
//  HandsOnEngineering: https://youtu.be/31avbKDwyuA
You should also notice that I am using public variables. Academically, it is considered best practice to use private variables wherever possible.
However, small structures with very clear conceptual boundaries like these are often left exposed, even after rapid prototyping.

---

`scene`
```
		public static void DrawCircle(Circle c, char letterToPrint) {
			DrawCircle(c.center, c.radius);
		}
		public static void DrawCircle(Vec2 pos, float radius, char letterToPrint) {
			Vec2 extent = (radius, radius); // Vec2 knows how to convert from a tuple of floats
			Vec2 start = pos - extent;
			Vec2 end = pos + extent;
			float r2 = radius * radius;
			for (int y = (int)start.y; y < end.y; ++y) {
				for (int x = (int)start.x; x < end.x; ++x) {
					if (x < 0 || y < 0) { continue; }
					float dx = x - pos.x;
					float dy = y - pos.y;
					bool pointIsInside = dx * dx + dy * dy < r2;
					if (pointIsInside) {
						Console.SetCursorPosition(x, y);
						Console.Write(letterToPrint);
					}
				}
			}
		}

```
```
		static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			DrawRectangle(0, 0, width, height, letterToPrint);
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
			DrawCircle((21, 12), 10, '.');
			Console.SetCursorPosition(0, (int)height);
		}
```

`voice`
the circle drawing code is more complex than the rectangle drawing code, but starts with the same principles.
we iterate across a 2 dimensional area with a nested for-loop, just like with a rectangle.
instead of printing the character at every location, we check to see if the character is inside of the circle, and only print if it is.
one important additional optimization here is limiting the size of the rectangle. we calculate the start and end bounds of this region with the circle's Extent.

the logic to test if a point is inside of a circle is really important to the concept of a circle, so it should probably live in the circle class

`scene`
Circle.cs
```

		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.x - position.x, dy = point.y - position.y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(center, radius, point);
```
Program.cs
```
					if (x < 0 || y < 0) { continue; }
					bool pointIsInside = Circle.IsInsideCircle(pos, radius, new Vec2(x, y));
					if (pointIsInside) {
						Console.SetCursorPosition(x, y);
						Console.Write(letterToPrint);
					}
```

`voice`
This is a method extraction refactor, and it helps create a Single Point Of Truth for our circle logic.

If we implement a similar function in Polygon, we can use a similar draw function to draw the polygon

```
		public static bool IsInPolygon(IList<Vec2> poly, Vec2 pt) {
			bool inside = false;
			for (int index = 0, prevIndex = poly.Count - 1; index < poly.Count; prevIndex = index++) {
				Vec2 vertex = poly[index], prevVertex = poly[prevIndex];
				bool edgeVerticallySpansRay = (vertex.y > pt.y) != (prevVertex.y > pt.y);
				if (!edgeVerticallySpansRay) {
					continue;
				}
				float slope = (prevVertex.x - vertex.x) / (prevVertex.y - vertex.y);
				float xIntersection = slope * (pt.y - vertex.y) + vertex.x;
				bool intersect = pt.x < xIntersection;
				if (intersect) {
					inside = !inside;
				}
			}
			return inside;
		}
		public static bool TryGetAABB(IList<Vec2> points, out Vec2 min, out Vec2 max) {
			if (points.Count == 0) {
				min = max = default;
				return false;
			}
			min = max = points[0];
			for (int i = 1; i < points.Count; ++i) {
				Vec2 p = points[i];
				if (p.x < min.x) { min.x = p.x; }
				if (p.y < min.y) { min.y = p.y; }
				if (p.x > max.x) { max.x = p.x; }
				if (p.y > max.y) { max.y = p.y; }
			}
			return true;
		}
```

`voice`
The math for checking a point inside of a polygon is a bit complex. the basic idea is this:
	imagine a ray from the given point, going to the right.
	if that ray crosses the polygon's edges an odd number of times, then the point is inside of the polygon
	the inner loop checks if the ray from pt crosses an edge
		first check to see if the point is in range of the edge at all
		then do the math to test if the ray's x intersection is on the edge
Finding the bounding area of the polygon is also not straight forward, so we should have a function
	it check every point, looking for the minimum and maximum values
	the minimum and maximum values are output that as a bounding box
This method can fail if the polygon is not correctly formed. This TryGet pattern is common in C# when failure is possible.

```
		public static void DrawPolygon(Vec2[] poly, char letterToPrint) {
			PolygonShape.TryGetAABB(poly, out Vec2 start, out Vec2 end);
			for (int y = (int)start.y; y < end.y; ++y) {
				for (int x = (int)start.x; x < end.x; ++x) {
					if (x < 0 || y < 0) { continue; }
					bool pointIsInside = PolygonShape.IsInPolygon(poly, new Vec2(x, y));
					if (pointIsInside) {
						Console.SetCursorPosition(x, y);
						Console.Write(letterToPrint);
					}
				}
			}
		}

```
```
		static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			Vec2[] polygonShape = new Vec2[] { (25, 5), (35, 1), (50, 20) };
			DrawRectangle(0, 0, width, height, letterToPrint);
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
			DrawCircle(position, radius, '.');
			DrawPolygon(polygonShape, '-');
			Console.SetCursorPosition(0, (int)height);
		}
```

`voice`
this code proves that I can draw important parts of my game.
Graphics are a huge feature and risk of any software. proving this kind of work can be accomplished at all is critical for development.

`scene`
```
		static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			Vec2[] polygonShape = new Vec2[] { (25, 5), (35, 1), (50, 20) };
			bool running = true;
			Vec2 position = (18,12);
			float radius = 10;
			float moveIncrement = 0.3f;
			char input = (char)0;
			while (running) {
				DrawRectangle(0, 0, width, height, letterToPrint);
				DrawRectangle((2, 3), new Vec2(20, 15), '*');
				DrawRectangle(new AABB((10, 1), (15, 20)), '|');
				DrawCircle(position, radius, '.');
				DrawPolygon(polygonShape, '-');
				Console.SetCursorPosition(0, (int)height);
				char input = Console.ReadKey().KeyChar;
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
A real-time game engine is another major risk that should be addressed.
The core of any simulation or game engine is a game loop.
We can use this interactive loop to test parts of our game as we write it.
First, let's test circle drawing.

`scene`
run the app to test

`voice`
we can play with some circle values now
with some modifications, we could use this code to test the rectangle and polygon drawing code as well. I recommend doing that as practice for novice developers.

a game engine has 4 main regions: initializationm, draw, input, and update.
each single iteration through the loop is a gameloop frame.

`scene`
```
		static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			Vec2[] polygonShape = new Vec2[] { (25, 5), (35, 1), (50, 20) };
			bool running = true;
			Vec2 position = (18, 12);
			float radius = 10;
			float moveIncrement = 0.3f;
			char input = (char)0;
			while (running) {
				Draw();
				Input();
				Update();
			}
			void Draw() {
				DrawRectangle(0, 0, width, height, letterToPrint);
				DrawRectangle((2, 3), new Vec2(20, 15), '*');
				DrawRectangle(new AABB((10, 1), (15, 20)), '|');
				DrawCircle(position, radius, '.');
				DrawPolygon(polygonShape, '-');
				Console.SetCursorPosition(0, (int)height);
			}
			void Input() {
				input = Console.ReadKey().KeyChar;
			}
			void Update() {
				switch (input) {
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

many programmers, myself included, consider it good programming style to use small functions, with descriptive but concise names, and only one or two levels of indentation wherever possible.
lets run this refactored code to make sure it still works how it used to.

Many programming languages don't support local functions, so we might want to create a Game class that has the data mambers, an Init function, Draw, ProcessInput, and Update function. Like this:
```
public class Game {
	private int width, height;
	private char letterToPrint;
	private Vec2[] polygonShape;
	public bool running;
	private Vec2 position;
	private float radius;
	private float moveIncrement;
	private char input;
	public void Init() {
		width = 80;
		height = 24;
		letterToPrint = '#';
		polygonShape = new Vec2[] { (25, 5), (35, 1), (50, 20) };
		running = true;
		position = (18, 12);
		radius = 10;
		moveIncrement = 0.3f;
	}
	public void Draw() {
		DrawRectangle(0, 0, width, height, letterToPrint);
		DrawRectangle((2, 3), new Vec2(20, 15), '*');
		DrawRectangle(new AABB((10, 1), (15, 20)), '|');
		DrawCircle(position, radius, '.');
		DrawPolygon(polygonShape, '-');
		Console.SetCursorPosition(0, (int)height);
	}
	public void Input() {
		input = Console.ReadKey().KeyChar;
	}
	public void Update() {
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
```
public static void Main(string[] args) {
	Game game = new Game();
	game.Init();
	while (game.running) {
		game.Draw();
		game.Input();
		game.Update();
	}
}
```
this is a perfectly valid style in C# as well. but for the sake of fewer source files, I'll keep writing in local functions in static main.

`scene`
run again

`voice`
notice the flickering. we can see how each shape is drawn right after the other, and the last drawn shapes are the most difficult to see when the game is active.
this flickering wouldn't be so bad if the game was faster.
lets implement a timer to see how long it takes to render the graphics.
and let's put the key input behind a check, so the game iteractes as quickly as possible, without blocking the game loop.

`scene`
```
		static void Main(string[] args) {
			// initialization
			//...
			char input = (char)0;
			Stopwatch timer = new Stopwatch();
			while (running) {
				Draw();
				Console.SetCursorPosition(0, (int)height);
				Console.WriteLine($"{timer.ElapsedMilliseconds}   ");
				Input();
				Update();
				timer.Restart();
			}
			//...
			void Input() {
				if (Console.KeyAvailable) {
					input = Console.ReadKey().KeyChar;
				} else {
					input = (char)0;
				}
			}
			//...
		}
```
compile and test. also, comment out Draw and test again.

I've changed Input so that it doesn't wait for a user key press. this is also called Non-Blocking input.
My computer is pretty fast, but this game engine is really slow. it looks like it's running at around 100 milliseconds of delay between updates, or about 10 frames per second.
As with most games, the biggest reason for this performance is probably because of Draw.

The code that claculates timing feels pretty bad, so before we continue, I want to implement a better time-keeping class
`scene`
src/MrV/Time.cs

```
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
		protected Stopwatch _timer;
		protected long _deltaTimeMs;
		protected float _deltaTimeSec;
		protected long _timeMsOfCurrentFrame;
		protected double _timeSecOfCurrentFrame;
		protected static Time _instance;
		public long deltaTimeMs => _deltaTimeMs;
		public float deltaTimeSec => _deltaTimeSec;
		public long DeltaTimeMsCalculateNow => _timer.ElapsedMilliseconds - _timeMsOfCurrentFrame;
		public float DeltaTimeSecCalculateNow => (float)(_timer.Elapsed.TotalSeconds - _timeSecOfCurrentFrame);
		public static Time Instance => _instance != null ? _instance : _instance = new Time();
		public static long DeltaTimeMs => Instance.deltaTimeMs;
		public static float DeltaTimeSec => Instance.deltaTimeSec;
		public static void Update() => Instance.UpdateTiming();
		public static void SleepWithoutConsoleKeyPress(int ms) => Instance.ThrottleUpdate(ms, () => Console.KeyAvailable);
		public Time() {
			_timer = new Stopwatch();
			_timer.Start();
			_timeSecOfCurrentFrame = _timer.Elapsed.TotalSeconds;
			_timeMsOfCurrentFrame = _timer.ElapsedMilliseconds;
			UpdateTiming();
		}
		public void UpdateTiming() {
			_deltaTimeMs = DeltaTimeMsCalculateNow;
			_deltaTimeSec = DeltaTimeSecCalculateNow;
			_timeSecOfCurrentFrame = _timer.Elapsed.TotalSeconds;
			_timeMsOfCurrentFrame = _timer.ElapsedMilliseconds;
		}
		public void ThrottleUpdate(int idealFrameDelayMs, Func<bool> interruptSleep = null) {
			long soon = _timeMsOfCurrentFrame + idealFrameDelayMs;
			while ((interruptSleep == null || !interruptSleep.Invoke()) && _timer.ElapsedMilliseconds < soon) {
				System.Threading.Thread.Sleep(1);
			}
		}
	}
}

```

`voice`
Timing is a really important part of a real-time game engine.
It is required for physics simulation calculations. Tracking duration is critical to performance metrics. And it can be used to throttle the game loop, to intentionally reduce CPU usage.
Making a separate time-keeping system like this leaves design space for time-related features in the future, like pausing our simulation, or slowing down time.

This implementation is a wrapper around the C# Stopwatch object, which is a high-resolution timer standard to the C# API.
It is keeping track of the passage of time as seconds and milliseconds separately.
Floating point values for time are most convenient for physics calculations.
Millisecond values are more accurate over long durations, and most convenient for system-level calculations.
  As floating point values increase, they reduce in precision. This is because the exponent value increases, the scale of values being tracked changes.

`scene`
show floating point number's exponent change as it increases in value

`voice`
  Specifically, a game that has running for more than 4.5 hours will be more accurate using integer-based millisecond calculations instead of floating points.

`scene`
back to code

`voice`
This class is a singleton, which allows details about the passage of time to be accessed anywhere in the program, very important for physics math.
I'm not making the entire class static because pure static classes create design hazards similar to global variables.
  These would be most apparent if we were designing a game with variable passage of time, like time dialation in different regions of space.
You might also notice that DeltaTime gives the same value until UpdateSelf is called. This will keep timing math consistent when DeltaTime is checked at multiple different times of the same update frame.
You may also realize that this code is actually giving a measurement of how long the last frame took. This works in practice because consecutive frames tend to require similar amounts of compute.
In the worst case:
  a one frame lag-spike will cause a very laggy frame to use the faster timing value of the previous frame
  the next fast frame will use the long frame time of the previous laggy frame, but do so very quickly
  to a very keen-eyed observer, this will look like a strange stutter, where the game slows down a lot, and then speeds up a lot, over the course of only a few milliseconds.
  the proper solution to this problem would not be a change to the timing system, but a change to the code causing the lag spike, which is outside the scope of this tutorial.
The ThrottleUpdate function is used to smooth out frames, and reduce CPU burden.
The ThrottleWithoutConsoleKeyPress interrrupts the throttling when a keypress is detected, so that the game always updates quickly to user input.
lets test this out.

`scene`
```
			//...
			char input = (char)0;
			float targetFps = 20;
			int targetMsDelay = (int)(1000 / targetFps);
			while (running) {
				Time.Update();
				Draw();
				Console.SetCursorPosition(0, (int)height);
				Console.WriteLine($"{Time.DeltaTimeMs}   ");
				Input();
				Update();
				Time.SleepWithoutConsoleKeyPress(targetMsDelay);
			}
			//...
```

`voice`
It's possible to design the Time class without a necessary Update method, but doing so would result in different values for delta time within the same gameloop frame.
This implementation tries to artificially set the gameloop speed to 20 frames per second. Feel free to experiment with this value.
A lower framerate reduces the burden that this program puts on the CPU. A lower CPU burden improves performance of the rest of your computer while this game is running.

Notice that Time.Update(); is called in the game loop, to track the passage of time and guage the cost of the entire process for SleepWithoutConsoleKeyPress.

---

`scene`
the game is running, with the DeltaTimeMs value fluctuating

`voice`
Performance is incredibly important to a realtime simulation, and a game especially. User experience is tightly bound to game loop performance.
Good performance also improves out ability to test, which is critical to development.

To improve performance immediately for testing, I want to do two quick things: flush the entire input buffer in the input function, like this:

```
			void Input() {
				if (Console.KeyAvailable) {
					while (Console.KeyAvailable) {
						input = Console.ReadKey().KeyChar;
					}
				} else {
					input = (char)0;
				}
			}
```

and reduce the amount of drawing going on, like this:
```
			void Draw() {
				DrawRectangle(0, 0, width, height, letterToPrint);
				//DrawRectangle((2, 3), new Vec2(20, 15), '*');
				//DrawRectangle(new AABB((10, 1), (15, 20)), '|');
				DrawCircle(position, radius, '.');
				//DrawPolygon(polygonShape, '-');
				Console.SetCursorPosition(0, (int)height);
			}
```

There are three specific classes of problems have major impacts on simulation performance that I'll address with some solutions: Drawing, Memory Allocation, and Collision detection.

For now, let's improve drawing.
We can significantly reduce the cost of drawing and the appearance of flickering by only drawing each character once.
//And after that, we can improve things more by redrawing only what has change between frames.
//In a traditional graphics setting, this technique is called 'Dirty Rectangle' or 'Dirty Pixel'.
//To do it, we need to keep track of what was drawn last frame so it can be compared to the new frame. 
We'll do that by writing our graphics into a separate buffer, and draw that once.

`scene`
artist painting a picture, then painting a different picture behind it, and swapping between them

`voice`
This technique dramatically reduces flickering by replacing the entire image at once instead of redrawing all different parts one at a time
It requires a Front Buffer, and a Back Buffer.
  The Front Buffer is displayed to the user. In our program, it is already there. It is the command line console.
  The Back Buffer is where the graphics are rendered in sequence before overwriting the Front Buffer all at once

`scene`
MrV/CommandLine/DrawBuffer.cs
```
using MrV.Geometry;
using System;

namespace MrV.CommandLine {
	public partial class DrawBuffer {
		protected char[,] _currentBuffer;
		public int Height => GetHeight(_currentBuffer);
		public static int GetHeight(char[,] buffer) => buffer.GetLength(0);
		public int Width => GetWidth(_currentBuffer);
		public static int GetWidth(char[,] buffer) => buffer.GetLength(1);
		public Vec2 Size => new Vec2(Width, Height);
		public char this[int y, int x] {
			get => _currentBuffer[y, x];
			set => _currentBuffer[y, x] = value;
		}
		public DrawBuffer(int height, int width) {
			SetSize(height, width);
		}
		public virtual void SetSize(int height, int width) {
			ResizeBuffer(ref _currentBuffer, height, width);
		}
		public static void ResizeBuffer(ref char[,] buffer, int height, int width) {
			char[,] oldBuffer = buffer;
			buffer = new char[height, width];
			if (oldBuffer == null) {
				return;
			}
			int oldH = GetHeight(oldBuffer), oldW = GetWidth(oldBuffer);
			int rowsToCopy = Math.Min(height, oldH), colsToCopy = Math.Min(width, oldW);
			for (int y = 0; y < rowsToCopy; ++y) {
				for (int x = 0; x < colsToCopy; ++x) {
					buffer[y, x] = oldBuffer[y, x];
				}
			}
		}
		public Vec2 WriteAt(string text, int row, int col) => WriteAt(text.ToCharArray(), row, col);
		public Vec2 WriteAt(char[] text, int row, int col) {
			for (int i = 0; i < text.Length; i++) {
				char glyph = text[i];
				switch (glyph) {
					case '\n': ++row; col = 0; break;
					default: WriteAt(glyph, row, col++); break;
				}
			}
			return new Vec2(col, row);
		}
		public void WriteAt(char glyph, int row, int col) {
			if (!IsValidLocation(row, col)) {
				return;
			}
			_currentBuffer[row, col] = glyph;
		}
		bool IsValidLocation(int y, int x) => x >= 0 && x < Width && y >= 0 && y < Height;
		public void Clear() => Clear(_currentBuffer, ' ');
		public static void Clear(char[,] buffer, char background) {
			for (int row = 0; row < GetHeight(buffer); ++row) {
				for (int col = 0; col < GetWidth(buffer); ++col) {
					buffer[row, col] = background;
				}
			}
		}
		public virtual void Print() => PrintBuffer(_currentBuffer);
		public static void PrintBuffer(char[,] buffer) {
			int height = GetHeight(buffer), width = GetWidth(buffer);
			for (int row = 0; row < height; ++row) {
				Console.SetCursorPosition(0, row);
				for (int col = 0; col < width; ++col) {
					char glyph = buffer[row, col];
					Console.Write(glyph);
				}
			}
		}
	}
}
```

This buffer for drawing will wrap around, and act like a 2D array, with some additional convenience methods.
The class is a partial class so that we can split it's impementation across multiple files. We'll put drawing methods in a separate place.
This implementation uses C-sharp's contiguous block 2D array allocation.
Notice that Height is the first dimension and Width is the second. These values can be in either order, but it needs to be consistent.
	I choose Height first because it intuitively follows the existing rectangle code, and also improves CPU cache locality when scanning horizontally, which is historically how graphics work.
The square-bracket operator is overloaded so that it can act like a 2D array in our code.
	If you want to change the order of x/y you can do it here. Doing so is a great exercise in resolving confusion, and internalizing the value of consistent dimension ordering.
	Generations of graphics programmers before you have internalized the ambiguity of dimension order, and unlocked mental resiliency in the process.
	(IWP) this is one of the invisible wizard problems that creates undocumented skills shared by game programmers.
This ResizeBuffer method is more robust than we need it to be, because it will copy old data into the new buffer to maintain consistency.
	This feature will probably not be needed, so it could be argued that writing this extra code is a waste of time and mental space, according to the YAGNI or You Aint Gunna Need It principle.
	However, this feature fulfills my intuition of how the ResizeBuffer function should work.
	For me, the cognitive load of writing the functionality now is less than the cognitive load of remembering that the feature doesn't exist in the future.
The buffer needs methods to write glyphs into it, one to clear the buffer before every draw.
And a a convenience method for printing to the command line console.

`scene`
MrV/DrawBuffer_geometry.cs
```
using MrV.Geometry;

namespace MrV.CommandLine {
	public partial class DrawBuffer {
		public delegate bool IsInsideShapeDelegate(Vec2 position);
		public void DrawShape(IsInsideShapeDelegate isInsideShape, Vec2 start, Vec2 end, char letterToPrint) {
			for (int y = (int)start.y; y < end.y; ++y) {
				for (int x = (int)start.x; x < end.x; ++x) {
					if (!IsValidLocation(y, x)) { continue; }
					bool pointIsInside = isInsideShape == null || isInsideShape(new Vec2(x, y));
					if (!pointIsInside) { continue; }
					WriteAt(letterToPrint, y, x);
				}
			}
		}
		public void DrawRectangle(Vec2 position, Vec2 size, char letterToPrint) {
			DrawShape(null, position, position+size, letterToPrint);
		}
		public void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			DrawShape(null, new Vec2(x, y), new Vec2(x + width, y + height), letterToPrint);
		}
		public void DrawRectangle(AABB aabb, char letterToPrint) {
			DrawShape(null, aabb.Min, aabb.Max, letterToPrint);
		}
		public void DrawCircle(Circle c, char letterToPrint) {
			DrawCircle(c.center, c.radius, letterToPrint);
		}
		public void DrawCircle(Vec2 pos, float radius, char letterToPrint) {
			Vec2 extent = new Vec2(radius, radius);
			Vec2 start = pos - extent;
			Vec2 end = pos + extent;
			float r2 = radius * radius;
			DrawShape(IsInCircle, start, end, letterToPrint);
			bool IsInCircle(Vec2 point) {
				float dx = point.x - pos.x;
				float dy = point.y - pos.y;
				return dx * dx + dy * dy < r2;
			}
		}
		public void DrawPolygon(Vec2[] poly, char letterToPrint) {
			PolygonShape.TryGetAABB(poly, out Vec2 start, out Vec2 end);
			DrawShape(IsInPolygon, start, end, letterToPrint);
			bool IsInPolygon(Vec2 point) => PolygonShape.IsInPolygon(poly, point);
		}
	}
}
```

drawing shapes can be generalized to the DrawShape method here. A delegate defines if the coordinage is in the given shape, and checks each point in a given region.

let's test this out

`scene`
Program.cs
```
			char input = (char)0;
			float targetFps = 20;
			int targetMsDelay = (int)(1000 / targetFps);
			DrawBuffer graphics = new DrawBuffer(height, width); // <-- add the draw buffer
			while (running) {
```
```
			void Draw() {
				graphics.Clear();
				graphics.DrawRectangle(0, 0, width, height, letterToPrint);
				graphics.DrawRectangle((2, 3), new Vec2(20, 15), '*');
				graphics.DrawRectangle(new AABB((10, 1), (15, 20)), '|');
				graphics.DrawCircle(position, radius, '.');
				graphics.DrawPolygon(polygonShape, '-');
				graphics.Print();
				Console.SetCursorPosition(0, (int)height);
			}
```

`voice`
we can and should remove the previous draw methods now, since we shouldn't print directly to the command line anymore, and equivalent logic is in DrawBuffer_geometry.cs.

Our code is now using the DrawBuffer as a GraphicsContext, which is a complex computer graphics concept where we can include anything related to graphics. We'll expand this idea soon.

We can also add back all of the test drawing, since the buffer has created such a significant optimization.

the circle more quickly now. but the shape is not actually correct,

`scene`
diagram of console, showing width/height ratio of glyphs

`voice`
because the command line console's characters are not perfect squares
we can take that into account with our shape drawing code if we can scale our 2D vectors
Add a Scale method to Vec2

`scene`
MrV/Geometry/Vec2.cs
```
		public void Scale(Vec2 scale) { x *= scale.x; y *= scale.y; }
		public void InverseScale(Vec2 scale) { x /= scale.x; y /= scale.y; }
		public void Floor() { x = MathF.Floor(x); y = MathF.Floor(y); }
		public void Ceil() { x = MathF.Ceiling(x); y = MathF.Ceiling(y); }

```

`voice`
in addition to Scale, we should be able to undo scaling.
we'll also want methods to clip vectors to integrer boundaries, since we're converting floating point vectors to integer based positions in the buffer.

`scene`
MrV/DrawBuffer_geometry.cs
```
	public partial class DrawBuffer {
		public Vec2 ShapeScale = new Vec2(0.5f, 1);
		public delegate bool IsInsideShapeDelegate(Vec2 position);
		public void DrawShape(IsInsideShapeDelegate isInsideShape, Vec2 start, Vec2 end, char letterToPrint) {
			Vec2 renderStart = start;
			Vec2 renderEnd = end;
			renderStart.InverseScale(ShapeScale);
			renderEnd.InverseScale(ShapeScale);
			for (int y = (int)renderStart.y; y < renderEnd.y; ++y) {
				for (int x = (int)renderStart.x; x < renderEnd.x; ++x) {
					if (!IsValidLocation(y, x)) { continue; }
					bool pointIsInside = isInsideShape == null || isInsideShape(new Vec2(x * ShapeScale.x, y * ShapeScale.y));
					if (!pointIsInside) { continue; }
					WriteAt(letterToPrint, y, x);
				}
			}
		}
```

`voice`
we need to keep track of the desired scale. for that, we'll add a scale variable to DrawBuffer.
	arguably, the ShapeScale variable added to the DrawBuffer class is bad design.
	this partial class implementation could instead be a subclass, to keep a clearer boundary between buffer management and drawing with a scale.
	I am making the intentional choice to combine these ideas into the same class, to reduce my cognitive load for this system. Managing cognitive load is one of those Invisible Wizard Problems.
	If you are a stickler for Object Oriented Design, feel free to subclass this as 'ShapeDrawingBuffer' or something. Though I recommend you do that after you finish the tutorial.
to draw the shape in a scaled way, we need to inverse-scale the bounding rectangle being drawn in, to put it in the correct position in the buffer
then we need to test against the scaled point, which is being printed to the unscaled position in the buffer.

Because we added the scale member to the class, we don't need to change any of the other method signitures. that's nice.

`scene`
(run test)


`voice`
I want to be able to test my app without having to press keys to do it. For that, I will impliment a task scheduler, which we can use for many other purposes later as well.

`scene`
MrV/Task/Tasks.cs
```
using System.Collections.Generic;
using System.Diagnostics;

namespace MrV.Task {
	public class Tasks {
		protected class Task {
			public System.Action WhatToDo;
			public long WhenToDoIt;
			public string Source;
			public Task(System.Action whatToDo, long whenToDoIt) {
				WhatToDo = whatToDo; WhenToDoIt = whenToDoIt;
				StackFrame stackFrame = new StackTrace(true).GetFrame(3);
				Source = $"{stackFrame.GetFileName()}:{stackFrame.GetFileLineNumber()}";
			}
			public void Invoke() => WhatToDo?.Invoke();
		}
		protected List<Task> tasks = new List<Task>();
		protected static Tasks _instance;
		public static Tasks Instance => _instance != null ? _instance : _instance = new Tasks();
		public static void Add(System.Action whatToDo, long delay = 0) => Instance.Enqueue(whatToDo, delay);
		public static bool Update() => Instance.RunUpdate();
		public void Enqueue(System.Action whatToDo, long delay = 0) {
			long when = Time.TimeMsCurrentFrame + delay;
			int index = Algorithm.BinarySearch(tasks, when, GetTimeFromTask);
			long GetTimeFromTask(Task t) => t.WhenToDoIt;
			if (index < 0) {
				index = ~index;
			}
			tasks.Insert(index, new Task(whatToDo, when));
		}
		public bool RunUpdate() {
			if (tasks.Count == 0 || Time.TimeMsCurrentFrame < tasks[0].WhenToDoIt) {
				return false;
			}
			List<Task> toExecuteNow = new List<Task>();
			while (tasks.Count > 0 && Time.TimeMsCurrentFrame >= tasks[0].WhenToDoIt) {
				toExecuteNow.Add(tasks[0]);
				tasks.RemoveAt(0);
			}
			for (int i = 0; i < toExecuteNow.Count; ++i) {
				toExecuteNow[i].Invoke();
			}
			return true;
		}
	}
}
```

This is a very simple task scheduler, which executes functions at a given delay. This is similar to Javascript's SetTimeout.
In this implementation, a Tasks.Task is a container for a function to invoke, and a time to invoke it.
The System.Action type is a variable that stores a function to invoke later. This can also be accomplished with a delegate, as we'll see in other code soon.
Each task also keeps track of what line of code called Tasks.Add, which is very valuable information when debugging asynchronous functionality like this.
Execution of tasks happen in the Update method, where the next Task to execute is at the front of a the Tasks list.
A seperate list gathers tasks to execute before the execution.
	If the tasks executing add more tasks to the task list, this separation prevents an infinite loop.
Ordering is done by a Binary Search algorithm, generalized to work on generic records. The implementation of this binary search looks like this:

`scene`
MrV/Algorithm
```
using System;
using System.Collections.Generic;

namespace MrV {
	public static class Algorithm {
		public static int BinarySearch<ElementType, KeyType>(IList<ElementType> arr, KeyType target,
		Func<ElementType, KeyType> getKey) where KeyType : IComparable {
			int low = 0, high = arr.Count - 1;
			while (low <= high) {
				int mid = low + (high - low);
				KeyType value = getKey.Invoke(arr[mid]);
				int comparison = value.CompareTo(target);
				if (comparison == 0) {
					return mid;
				} else if (comparison < 0) {
					low = mid + 1;
				} else {
					high = mid - 1;
				}
			}
			return ~low;
		}
	}
}
```

`voice`
Binary search works on a list of ordered values. The method assumes the list is ordered, and will not work if it isn't sorted.
It also works by testing IComparable values, which extend CompareTo. All primitive types are IComparable.
	A negative value means the left-value is smaller than the right-value.
	A zero value means the left-value and right-value are equal.
In the inner loop, BinarySearch checks if the value being searched for is directly in the middle of the search space.
	if the exact value is found in the middle, BinarySearch provides it's index in the list
	if the value is higher than what was found, the next search space will be in the top half of the current search space
	if the value is lower than what was found, the next search space will be in the bottom half of the current search space
if the search space is reduced to zero, the value was not found.
this algorithm returns the 2's compliment of where the algorithm stopped searching, which is where the value should have been.
	2's compliment flips all of the binary bits in an integer value. the operation will undo itself.
	2's compliment of a positive index will always be a negative value, so we can detect if the value already exists or needs to be inserted by checking the sign of the return.

`scene`
```
			Record searchElement = new Record(null, when);
			Comparer<Record> RecordComparer = Comparer<Record>.Create((a, b) => a.WhenToDoIt.CompareTo(b.WhenToDoIt));
			int index = actions.BinarySearch(searchElement, RecordComparer);
```

`voice`
I could have also just used the BinarySearch method already in C#'s List class.
As long as the RecordComparer is created as a static member, there isn't any significant performance gain in using my custom algorithm.
However, my search algorithm doesn't need to create a mostly empty search element.
	That means my algorithm becomes better if the Task class becomes more complex
Also, this is an excellent example of a templated function using lambda expressions, which my target audience might appreciate.
	I will continue using more functional programming like this, so please to any additional research to understand this as needed before watching more.

`scene`
src/Program.cs
```
			void Update() {
				Tasks.Update();
				switch (input) {
```
```
			DrawBuffer graphics = new DrawBuffer(height, width);
			int timeMs = 0;
			int keyDelayMs = 100;
			for (int i = 0; i < 10; ++i) {
				Tasks.Add(() => input = 'd', timeMs);
				timeMs += keyDelayMs;
				Tasks.Add(() => input = 'e', timeMs);
				timeMs += keyDelayMs;
			}
			for (int i = 0; i < 20; ++i) {
				Tasks.Add(() => input = 'w', timeMs);
				timeMs += keyDelayMs;
				Tasks.Add(() => input = 'r', timeMs);
				timeMs += keyDelayMs;
			}
			while (running) {
```
`voice`
I need to make sure the Tasks are regularly Updated, so I'll include Tasks.Update in the Update section of my code.
Before the gameloop, this code creates an automatic test of my application by synthetically setting the program's input variable.
the first for-loop moves the circle to the right with the 'd' key, and expands the radius with the 'e' key
the second for-loop moves the circle up with the 'w' keu, and reduces the radius with the 'r' key.
each of the key input changes should happen about 100 milliseconds apart.

If the timing is reduced to less than the deltaTime of a frame, some of these inputs will become lost, and the circle will not move the same amount.
Lets make a Key Input system to solve this and other bugs.

`scene`
src/MrV/CommandLine/KeyInput.cs
```
using System;
using System.Collections.Generic;
using System.Text;

namespace MrV.CommandLine {
	public delegate void KeyResponse();
	public struct KeyResponseRecord<KeyType> {
		public KeyType Key;
		public KeyResponse Response;
		public string Note;
		public KeyResponseRecord(KeyType key, KeyResponse response, string note) {
			Key = key; Response = response; Note = note;
		}
	}
	public class Dispatcher<KeyType> {
		protected List<KeyType> eventsToProcess = new List<KeyType>();
		protected Dictionary<KeyType, List<KeyResponseRecord<KeyType>>> dispatchTable
			= new Dictionary<KeyType, List<KeyResponseRecord<KeyType>>>();
		public void BindKeyResponse(KeyType key, KeyResponse response, string note) {
			if (!dispatchTable.TryGetValue(key, out List<KeyResponseRecord<KeyType>> responses)) {
				dispatchTable[key] = responses = new List<KeyResponseRecord<KeyType>>();
			}
			responses.Add(new KeyResponseRecord<KeyType>(key, response, note));
		}
		public void AddEvent(KeyType key) => eventsToProcess.Add(key);
		public void ConsumeEvents() {
			List<KeyType> processNow = new List<KeyType>(eventsToProcess);
			eventsToProcess.Clear();
			for (int i = 0; i < processNow.Count; i++) {
				KeyType key = processNow[i];
				if (dispatchTable.TryGetValue(key, out List<KeyResponseRecord<KeyType>> responses)) {
					responses.ForEach(responseRecord => responseRecord.Response.Invoke());
				}
			}
		}
	}
	public class KeyInput : Dispatcher<char> {
		private static KeyInput _instance;
		public static KeyInput Instance {
			get => _instance != null ? _instance : _instance = new KeyInput();
			set => _instance = value;
		}
		public static void Bind(char keyPress, KeyResponse response, string note)
			=> Instance.BindKeyResponse(keyPress, response, note);
		public static void Read() => Instance.ReadConsoleKeys();
		public static void TriggerEvents() => Instance.ConsumeEvents();
		public static void Add(char key) => Instance.AddEvent(key);
		public void ReadConsoleKeys() {
			while (Console.KeyAvailable) {
				ConsoleKeyInfo key = Console.ReadKey();
				AddEvent(key.KeyChar);
			}
		}
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			foreach(var kvp in dispatchTable) {
				string listKeyResponses = string.Join(", ", kvp.Value.ConvertAll(r => r.Note));
				sb.Append($"'{kvp.Key}': {listKeyResponses}\n");
			}
			return sb.ToString();
		}
	}
}
```

`voice`
A KeyResponse is just some function, which happens in response to a keypress.
I could have used System.Action instead, but using a named delegate type means we can change this easily later.
A structure keeps the relationship of each Key and KeyResponse, along with a note about the purpose of the key binding.
	The KeyType is templated because this implementation will just use characters, but any type of input should work as well.
A Dispatcher manages a queue of events, and those events are mapped in a DispatchTable to responses.
	This is a general concept that is useful in many domains beyond key input handling.
BindKeyResponse will bind a KeyResponse to a key. If the key has never been bound before, a list of KeyResponses will be created for the key.
Events added to the dispatcher will be Consumed all at once.
Just like the task scheduler, execution will happen from a list that can't be added to while actions are processed.

This KeyInput implementation creates a singleton for easy access.
This design will allow the KeyInput system to swap out at runtime, in case different different user-interface states use different keybindings.
Otherwise, this class can be accessed statically, for convenience.
The KeyInput class reads specifically from the C# Console, so it has a conveniently labeled place for that logic.
The ToString method shows how to dynamically query what is bound to each key, which could be useful for dynamic key binding at runtime.

`scene`
src/Program.cs
```
			DrawBuffer graphics = new DrawBuffer(height, width);
			KeyInput.Bind('w', () => position.y -= moveIncrement, "move circle up");
			KeyInput.Bind('a', () => position.x -= moveIncrement, "move circle left");
			KeyInput.Bind('s', () => position.y += moveIncrement, "move circle down");
			KeyInput.Bind('d', () => position.x += moveIncrement, "move circle right");
			KeyInput.Bind('e', () => radius += moveIncrement, "expand radius");
			KeyInput.Bind('r', () => radius -= moveIncrement, "reduce radius");
			KeyInput.Bind((char)27, () => running = false, "quit");
			int timeMs = 0;
			int keyDelayMs = 20;
			for (int i = 0; i < 10; ++i) {
				Tasks.Add(() => KeyInput.Add('d'), timeMs);
				timeMs += keyDelayMs;
				Tasks.Add(() => KeyInput.Add('e'), timeMs);
				timeMs += keyDelayMs;
			}
			for (int i = 0; i < 20; ++i) {
				Tasks.Add(() => KeyInput.Add('w'), timeMs);
				timeMs += keyDelayMs;
				Tasks.Add(() => KeyInput.Add('r'), timeMs);
				timeMs += keyDelayMs;
			}
			while (running) {
```
`voice`
Now keys are bound to functions during initialization.
This example inlines the very simple functions, and takes advantage of the Note field to create more clarity in the code.
I personally like this style of keybinding a lot. It feels like how rules of a boardgame are explained before the game starts.
	It could also allow definitions of controls to happen closer other important context.

Notice that setting the input variable has been replaced with additions to the KeyInput queue.

`scene`
src/Program.cs
```
			void Input() {
				KeyInput.Read();
			}
			void Update() {
				KeyInput.TriggerEvents();
				Tasks.Update();
			}
```

`voice`
Because KeyInput takes care of input logic, the Input function can be dramatically simplified, and so can Update.

now when we run the program to test it, key events are not lost, even when the keyDelay is lowered to much less than the Update's DeltaTime.

but we still want to reduce that deltaTime, and we have a technique to do it still.
The image doesn't actually need to be fully refresh every frame, only a few characters change each frame.
This is similar to a graphics optimization technique called Pixel Caching, done here with character glyphs.

`scene`
src/MrV/CommandLine/GraphicsContext.cs
```
using System;

namespace MrV.CommandLine {
	public class GraphicsContext : DrawBuffer {
		private char[,] _lastBuffer;
		public GraphicsContext(int height, int width) : base(height, width) {}
		public override void SetSize(int height, int width) {
			base.SetSize(height, width);
			ResizeBuffer(ref _lastBuffer, height, width);
		}
		public virtual void PrintModifiedCharactersOnly() {
			for (int row = 0; row < Height; ++row) {
				for (int col = 0; col < Width; ++col) {
					bool isSame = this[row, col] == _lastBuffer[row, col];
					if (isSame) {
						continue;
					}
					char glyph = this[row, col];
					Console.SetCursorPosition(col, row);
					Console.Write(glyph);
				}
			}
		}
		public void SwapBuffers() {
			char[,] swap = _buffer;
			_buffer = _lastBuffer;
			_lastBuffer = swap;
		}
	}
}
```

`voice`
The GraphicsContext class is a DrawBuffer, and it also keeps track of previous buffer data which was already drawn.
The decision to inherit DrawBuffer instead could be argued here.
Conceptually, GraphicsContext has two DrawBuffers instead of being a buffer with spare data.
I decided to use inheritance because GraphicsContext an API surface similar to DrawBuffer, and _lastBuffer can be an internal array.
PrintModifiedCharactersOnly checks every character to determine if it is the same as the last character printed.
only different characters are printed.
after every print, which is commonly called a Render, the current active buffer and last buffer can switch places
PrintModifiedCharactersOnly could be further optimized to reduce calls to SetCursorPosition, which is an expensive call in the Console API.

```
		public virtual void PrintModifiedCharactersOnly() {
			for (int row = 0; row < Height; ++row) {
				bool mustMoveCursorToNewLocation = true;
				for (int col = 0; col < Width; ++col) {
					bool isSame = this[row, col] == _lastBuffer[row, col];
					if (isSame) {
						mustMoveCursorToNewLocation = true;
						continue;
					}
					char glyph = this[row, col];
					if (mustMoveCursorToNewLocation) {
						Console.SetCursorPosition(col, row);
						mustMoveCursorToNewLocation = false;
					}
					Console.Write(glyph);
				}
			}
		}
```

`voice`
Console.Write implicitly moves the cursor position.
The cursor position only needs to be set if there is a new row, or if the last glyph was skipped.

src/Program.cs
```
			int targetMsDelay = (int)(1000 / targetFps);
			GraphicsContext graphics = new GraphicsContext(height, width);
			KeyInput.Bind('w', () => position.y -= moveIncrement, "move circle up");
```

`voice`
The GraphicsContext has almost the same API surface as DrawBuffer, so it can be substituted without incident

`scene`
```
				graphics.DrawPolygon(polygonShape, '-');
				graphics.PrintModifiedCharactersOnly();
				graphics.SwapBuffers();
				Console.SetCursorPosition(0, (int)height);
```

`voice`
Using the optimized draw happens in the same way as the previous print, except that FinishedRender is called to swap the draw buffer data.
A call to SwapBuffers might seem like an overly verbose requirement. However, it's very common in graphics APIs, so it's worth getting used to the idea.

The first time I wrote this program, I didn't separate DrawBuffer with GraphicsContext, I just wrote them all in the same class.
I want to make a special note about it because I want to remind the audience that software design is difficult.
	and I want to emphasize that doing something for the first time means you should be comfortable with imperfect design.
	In every major computer programming problem I have ever solved started with a relatively messy intuitive solution.
	My messy code didn't evolve into something that made sense until I sat with it for a while and re-wrote it.
	Be patient with yourself as a developer. Give yourself the grace to rewrite messy code later.

Running this program is *much* faster than it used to be. Most of the time draw happens, there is actually no change at all.
And sometimes, only small amounts of the screen need to change. Dirty Rectangle (or Scissoring) is the name of another similar technique for pixel graphics.

<--------- TODO

The graphics context needs to use colors, as part of the original game design.

`scene`
src/MrV/CommandLine/ConsoleColorPair.cs
```
using System;

namespace MrV.CommandLine {
	public struct ConsoleColorPair {
		private byte _fore, _back;
		public ConsoleColor fore { get => (ConsoleColor)_fore; set => _fore = (byte)value; }
		public ConsoleColor back { get => (ConsoleColor)_back; set => _back = (byte)value; }
		public ConsoleColorPair(ConsoleColor fore, ConsoleColor back) {
			_back = (byte)back;
			_fore = (byte)fore;
		}
		public void Apply() {
			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
		public static ConsoleColorPair Default = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.Black);
		public static ConsoleColorPair Current => new ConsoleColorPair(Console.ForegroundColor, Console.BackgroundColor);
		static ConsoleColorPair() {
			Default = Current;
		}
	}
}
```

C-sharp's console API gives us access to 16 colors, in both the foreground and background

* add super-sampling to the graphics context, for antialiasing
```
```
* add color gradient system for the antialiasing, including ConsoleColorPair and ConsoleGlyph.
  * note that ConsoleColorPair could be optimized to 8bits, but it's probably not worth it.
```
```
* make a circle that moves independently, with it's own update. which we'll call a Particle
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
* add zoom in/out to graphics buffer
```
```
* moving polygon (with offset)
```
```
* rotate functionality for the polygon
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
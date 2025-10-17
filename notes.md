# Note to entities reading this
Please critique this is a script. It is for a video tutorial designed primarily to give young programmers programming practice. More details about it's purpose are found in the script itself.

I want to produce a high quality, factually robust, technically novel, interesting teaching artifact that other software developers can feel good about learning from.

The script is quite long, and still unfinished. I am providing the first draft of what is likely to be the first half of the tutorial.

Please summarize the script before giving feedback.

as you are reading:

Consider each section labeled '### scene' as a description of what is visually shown. These sections are followed by a '### voice' section, narrating the visuals.

Code will be in most scenes, between tripple back-tick '```' headings, as is common in markdown. I will record myself typing this code in Microsoft Visual Studio Community 2022 during narration.

code blocks that begin and end with an ellipses '...' are intended modifications of previous code. If it is unclear what code is being modified, make a note of that.

Read the script marked by the '### voice' heading as spoken dialog. Identify unecessary repetition, or ineffective prose. Provide alternative phrasing where appropriate.

Do Not identify poor grammer, capitalization or punctuation mistakes. I am still looking for high-leverage changes to the script, not interested in English writing minutiae.

Read the code carefully. Please be specific if there are any parts of my code that could be considered bad C-sharp programming. I am interseted in C# writing minutiae.

I wrote some notes that start with double slashes, or C-style line comments. These notes will not be read out loud.

If the code is using a design pattern you recognize that is not mentioned in the script, please identify the pattern, and where the code is using it. Similarly, if the script identifies a design pattern incorrectly, clearly flag that as well.

Identify where the script is repeatetive, and the content could be streamlined. If the repetition appears to be appropriate for emphasis, feel free to let it pass.

Identify if there is a conceptual gap that should be explained with more detail or better analogy.

If there is a strong analogy that could be used to explain an idea, please make a note of that.

Identify parts of the script that cover content that is not well documented in other YouTube tutorials. Suggest if emphasizing this content makes sense to promote the tutorial's value.

Please be critical about your feedback. I do not want a sycophantic response, I am serious about finding and fixing mistakes. Minimize encouragement; reserve it for insights with transferable public value.

## Intro

### scene
white text on black background:
	"If I can't create it, I don't understand it."
	-Richard Feynman
pause for 2 seconds, with no voice over

### scene
Montage of the LowFiRockBlaster game.
	show command-line window with 2D graphics rendered using colored character glyphs.
	a player's ship shaped like a V flys around with simple 2D physics, shooting and destroying circle asteroids, and collecting circle ammo pickups.
	game view pans smoothly following the player, also zooming in and out, showing how floating-point vector graphics are rendered using command line glyphs.

### voice
This is a tutorial series teaching how to build a real-time simulation in C-sharp.
It implements essential games and simulation systems, and simulates basic physics and collision detection.

### scene
white text on black background, list the following:
	2D Vector math
	basic physics
	simple rendering (in the command line)
	rendering primitive shapes
	realtime simulation
	task scheduling
	key input buffer based on a dispatch table
	basic graphics optimizations
	object pooling
	particle systems
	collision detection
	cell space partition tree
	... and more

### voice
I'll show all the code and give explainations, starting from an empty project for the Command Line Console.
	This will include graphics, math, optimization, data structures, collision detection, and more.
I'll also offer in-context advice and best practices from my decades of experience as a game developer and computer science instructor.
	I want you to understand the bottom-up implementation of a game engine, which will improve your decision making in software development generally.
And I'll give some of my own opinions about the Invisible Wizard Problems that emerge in modern computer programming, and game development especially.

### scene
montage of programming in the background, with prominent text centered in the foreground:
	Invisible Wizard Problems: Tricky programming issues that need special knowledge or experience to spot and solve.

### voice
One invisible wizard problem that I can mention now is the tradeoff of robustness vs accessibility of this simulation.
	many of my example implementations will fall short of being highly robust and maximally efficient. I don't apologize for that.
		I want this tutorial to be easy to follow more than I want it to be perfect software.
	I believe the most perfect software for this situation is easy to read and type as you watch the tutorial, and easy to understand if you are seeing it for the first time.
	I'll introduce many concepts in a way that is easy to practice while still being robust enough to be fast to iterate on.
		I do apologize that I will jump around different files often to make modifications and explain concepts.
		I recognize that is jarring to experience. Please be ready to pause the video to copy any code.

### scene
white text on black background:
	"These systems don't understand the world. They just predict the next word."
	- Jeffery Hinton
	"As an AI I don't believe or disbelieve anything in the human sense."
	- ChatGPT 2025/09/14

### voice
Another invisible wizard problem is the rapid replacement of programmers with AI in the software development industry.
Even if AI systems write most of the world's code, they still lack something, which Geoffrey Hinton warned about, and the AI's will admit.
They lack real understanding.
Artificial Intelligences need humans who can translate lived experience into software accessible tools, and simulations. These simulations help train AI, and augment toolchains.
Learning how to build simulations yourself, in a mostly agnostic tech stack like in the command line, is a powerful skill. Having it will make you useful to AI in the future.
With this skill, you can help shape how AI connects to human reality.

### scene
back to the montage of LowFiRockBlaster

### voice
This simulation tutorial is for a space-shooter game inspired by "Spacewar!" from 1962, written for the command line console.
The idea is as old as videogames. It was the reason why the C programming language and Unix operating system were invented.
I've summoned that ancient motivation to capture your attention now, while I teach you foundational concepts for writing a simulation and game engine.
Check the description for the Github project link if you want the code. Continue watching if you want a thorough lesson to understand the code.

I spent a few weeks creating this game, and a few months writing this script.
Please do not misunderstand that this program just fell out of my head in one moment. It didn't even fall out of a larg language model. Real programming takes time.
Your projects will take a long time to finish too, even if they use a tutorial like this as a starting point.
Be patient with yourself. Be disciplined with yourself. I believe anyone who sits with these ideas can learn them well. Especially you, even if you don't consider yourself skilled with math.
For context, I was terrible at math in High School, especially at Trigonometry, which I will be using in this tutorial. I learned what I know because of practice doing projects like this.
My guidance will follow roughly the same path I went through while making this game a few months ago,
	but it will be much faster, even with a few detours to explain some math, architecture, and game engine optimizations.
This tutorial doesn't capture the vast majority of the difficulty of writing this project. What you will see is just the parts curated for your understanding.
As you do this work, please be patient with your own mistakes, and the frustration that follows. Frustration is the sweat of learning. It is a sign that your mind is growing. Engage with it. And take a break if you need it. This video will still be here when you are ready.

### scene
montage of code in Visual Studio 2022, and the game running with the code in the background

### voice
I'll be using C sharp as the programming language.
I assume you already have a C sharp runtime, compiler, and IDE installed. These are non negotiable.
I also assume you know the basics of how to program command line applications in C sharp, including Object Oriented Programming basics.
You can still follow along without that knowledge, but I recommend you start with that foundation.

### scene
montage of Unity programming

### voice
The very popular Unity Game Engine is a notable reason I chose to do this in the C sharp language.
This tutorial should give new game developers insight into how a game engine works in general, which will help understand how Unity works too.

### scene
white text on black background:
	practice is the price for understanding.
	understanding is the price for power in the computer.

### voice
If you are new to game programming, you should practice by typing all the code and following along yourself. I'm serious.
Doing that will dramatically increase how long this tutorial takes --probably 10x longer than the runtime, or more.
I believe that time will be worth the understanding you gain, especially if you make mistakes and overcome those mistakes while doing it.
I want you to be a computer wizard. that is why I made this tutorial. If you do not consider yourself a computer wizard yet, and you want to be, practice.

## Practical Project Initialization

### scene
show Visual Studio Community 2022 loading.
create a new project

### voice
Start your C sharp IDE. I'm using Visual Studio Community 2022.
As of 2025, I recommend using Rider. It is not free for commercial content, but free for personal use.
I would not use it for a tutorial video, but I recommend it for educational purposes.
Rider is similar enough to Visual Studio that this tutorial will still work well for it.

I'm going to call my project "LowFiRockBlaster".
And I'll be using .NET Core 5.0 as my version of C#.
Unity doesn't support the latest C sharp features, and I want this tutorial to be coherent with Unity Game Development specifically.

### scene
src/MrV/LowFiRockBlaster/Program.cs
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

### voice
The default Program.cs will be our entry point. This is a basic "Hello World" program.
I'm going to move it to a new folder, to organize my files as I code. src/MrV/LowFiRockBlaster
I'll be writing everything in my own MrV namespace, I recommend you name your own namespace after yourself. You can refactor that later if you want.
I'll also be using a compact whitespace style, so I can fit as much code on the screen as possible.

Let's run this code to make sure everything works. If your program does not compile and run, stop the video and get it working.
Unfortuantely, most programming environments require some configuration, even with an automatic installer.

### scene
start the Visual Studio Installer
	Modify -> .NET desktop development
	show that I have .NET desktop development checked in the installer

### voice
For example, be sure you have the .NET desktop development workload installed by the Visual Studio Installer.

---

## Explicit Goals

### scene
design.md
* game design document
	* graphics
		* draw: circles (asteroids, ammo pickups)
		* draw: polygons (player, projectiles)
		* colors in the command line, with text overlay
	* logic
		* simulation runs in real time, even while player isn't providing input
		* objects move using simple physics
		* objects interact when they collide
		* player character is an object, controlled by user
		* projectiles break asteroids: big -> medium -> small -> ammo pickup
	* player choices
		* move in cardinal directions (up/down/left/right)
		* shoot projectile, if sufficient ammo
		* avoid moving into asteroids, or else be destroyed
		* pickup additional ammo from destroyed asteroids

### voice
Before I start writing the game, I want to have a clear set of goals.
An imagined goal is a necessary starting point for any project.
A written list of features and expectations is essential for a project that will take more than a few days to finish.
Clear Top-Down design addresses Confusion and Scope Ambiguitiy, which are Invisible Wizard Problems that a software developer should always keep in mind.
Project managers might call this a Work Breakdown Structure. Game Developers might call this a Game Design Document.
More experienced developers will need fewer details and less structure to create a product.
	If this seems too sparse for your own projects, please add more details in your own project.
I recommend you add just enough detail to your list of expectations that you feel you will remember your vision when you read the document again.
	Avoid adding more detail than you need.
	Expect your goals to change while you are achieving them, especially for game development where fun is a moving target.
Spending too much time writing a design or specification is sometimes called Analysis Paralisys.
	it is a real cause for projects to fail before they even start.
Identify clear goals that you can start implementing, and give yourself the grace to update the document later.

## Draw Rectangle

### scene
back to program.cs
```
...
		public static void Main(string[] args) {
			int width = 80, height = 23;
			char letterToPrint = '#';
			DrawRectangle(0, 0, width, height, letterToPrint);
			Console.WriteLine("Hello World!");
		}

		public static void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			for (int row = 0; row < height; ++row) {
				for (int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col + x, row + y);
					Console.Write(letterToPrint);
				}
			}
		}
...
```

### voice
Lets start our game by drawing the screen where the game will be displayed.
I'll make a new function called `DrawRectangle`, and call it in my `Main` function. You'll notice that it's `public static`
we want it to be `public static` for three reasons:
	- it doesn't have any dependencies on the Program class, so we should be able to run it from anywhere
	- the `Main` function is in a `public static` context, and needs `DrawRectangle` to be `public static` also
	- `public static` function calls are technically faster than non-`public static` calls.
		- the speed gain is because an instance doesn't need to be pushed onto the stack with the method call. the gain is so extremely small that it hardly bears mentioning.
		- However this is game programming, and performance is always important to keep in mind.
For the sake of clarity, I will try not obfuscate the game with optimizations while I write it,
	but I will intentionally choose a more performant style, often out of habit.
	- an example of this habit can be seen in this code, where I use the prefix increment operator.
		it's one assembly instruction faster than postfix, for old compilers only.
		It's an old habit from my days as a flip-phone game developer, and you will just need to suffer through it in this tutorial.
DrawRectangle has a pretty standard nested for loop iterating over a two-dimensional space.
the logic here places the command line cursor exactly at each position in the rectangle before printing a character.

Before moving on, let's take a moment to understand this logic.
It seems pretty specific to the command line console, but gaining familiarity with this two dimenstional iteration will help with may other kinds of problem solving in the future.

### scene
show the code and running output
```
...
			int width = 80;
			char letterToPrint = '#';
			for (int col = 0; col < width; ++col) {
				Console.Write(letterToPrint);
			}
...
```

### voice
this code will write 80 hashtag characters in a row.

### scene
show the code and running output
```
...
			int width = 80, height = 23;
			char letterToPrint = '#';
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.Write(letterToPrint);
				}
			}
...
```

### voice
this code will write 80 times 23 hashtag characters, still all in one row.

### scene
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

### voice
this code will write a rectangle 80 wide and 23 tall.

### scene
show the code and running output
```
...
		int width = 80, height = 23;
		char letterToPrint = '#';
		DrawRectangle(width, height, letterToPrint);
		DrawRectangle(width/2, height/2, '?');
...
```

```
...
		public static void DrawRectangle(int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.Write(letterToPrint);
				}
				Console.WriteLine();
			}
		}
...
```

### voice
If we turn this into a function, we can print a new distinct rectangle right after this one.
Doing this allows us to call the function at any time from any place in our program.

### scene
show the code and running output
```
...
		public static void DrawRectangle(int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col, row);
					Console.Write(letterToPrint);
				}
			}
		}
...
```

### voice
we can use `SetCursorPosition` to move the commandline cursor exactly where we want it before printing any character with `Console.Write`
This functionality is not easily available in all programming language console APIs, so it's nice that C sharp gives it to us so cleanly.
For example, if you want to do the same thing in Python, you may need to replace `SetCursorPosition` with printing an escape sequence.
That escape sequence will not work if executed in the basic Windows console. And it will cause strange errors when printing some special characters, or printing in a separate thread.

### scene
show the code and running output
```
...
		public static void DrawRectangle(int x, int y, int width, int height, char letterToPrint) {
			for(int row = 0; row < height; ++row) {
				for(int col = 0; col < width; ++col) {
					Console.SetCursorPosition(col + c, row + y);
					Console.Write(letterToPrint);
				}
			}
		}
...
```

### voice
and this code allows us to draw the rectangle anywhere in visible space, not just the upper-left corner of the console.

### scene
```
...
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
...
```

### voice
The code will crash if x+col or y+row are negative, which can be solved with a simple if statement.

if this code is confusing, I highly recommend you stop the tutorial here, and practice writing loops before continuing.
the programming in this tutorial will get much more conceptually complex beyond this point, and I will not make suggestions for more practice.

---

## 2D Math Structures

### scene
Create a src folder, MrV folder, Math folder. create Vec2.cs inside of src/MrV/Geometry
src/Geometry/Vec2.cs
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

### voice
this program will need many source files, and I want to organize them with folders.
also, this project needs to be serious about 2 dimensional math.

Two dimensional vectors are extremely common in game programming frameworks.
this is a very basic 2D vector structure, which we'll add to during the tutorial.

this code is very densly written, using inlined curlybrackets for simple lines of code, and the expression-bodied fat-arrow for single line methods.

this code also shows operator overloading, an implicit constructor that converts tuples into 2D vectors, and an interpolated string.
	these features are not available in all other languages, but I am taking advantage of their inclusion in C-sharp.

### scene
src/LowFiRockBlaster/Program.cs, add to Main
```
...
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
...
```

```
...
		public static void DrawRectangle(Vec2 position, Vec2 size, char letterToPrint) {
			DrawRectangle((int)position.x, (int)position.y, (int)size.x, (int)size.y, letterToPrint);
		}
...
```

### voice
We should start using the 2D vector concept now, in this rectangle drawing code.

I'll be doing more of this kind of code refactoring during my tutorial, but not nearly as much as I did while writing the game for myself.
Real programmers constantly rewrite their own code, often renaming variables, and adding or removing new code structures for many different reasons.
Know that this tutorial is the result of lots of such rewrites, and I have minimized the rerwites to be easier on you.
If you are new to programming, you need to know that this is how big projects are written: one step at a time, with lots of rewrites, and tests between changes.
I sincerely apologize for speeding it up my development process. Please learn what you can from my compressed examples, and do pause and rewind the video for yourself as needed.

Notice that I made a new `DrawRectangle` function, and I'm using the old `DrawRectangle` function in this new one.

### scene
black background with dark-gray grid, 10x10 squares
bold white cartesian plane (x/y axis)
label integer locations of the x and y axis, -5 to +5 on both axis

### voice
`Vec2` is a 2 dimensional vector, which is a physics and math concept. The basic premise is:

### scene
Vec2.cs, with cartesian plane + grid diagram in small window
shift the cartesian plane to have the origin at the upper-left
swap the negative y axis to have positive integer values

### voice
in the command line, 0,0 is at the upper left corner, and y increases as it goes down.

### scene
show Point A at location 1, 1

### voice
Point A can be at a known location x/y.

### scene
show Point A at location 3, 5

### voice
Point B can be at another location x/y.

locations in space can be fully described by a 2D vector, which is a position along each dimension.

### scene
draw a line between points A and B

### voice
distances can also be described this way.

### scene
remove the line between points A and B
draw an arrow from the origin to point B

### voice
directions can also be described this way.

### scene
show link to 
  3blue1brown https://youtu.be/fNk_zzaMoSs
  HoustonMathPrep https://youtu.be/j6RI6IWd5ZU

### voice
There are plenty of additional tutorials on the internet about 2D vectors, check the description for examples:

### scene
create the src/MrV/Geometry folder structure in the solution explorer
src/MrV/Geometry/AABB.cs
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
AABB.cs, with cartesian plane + grid diagram in small window

### voice
A box can be described with two `Vec2` structures, bounded by edges aligned on the x and y axis. We call this an Axis Aligned Bounding Box or `AABB`.
This is a simple description of space in a simulation, and it is used for many kinds of clalculations. 

### scene
```
...
		public static void DrawRectangle(AABB aabb, char letterToPrint) {
			DrawRectangle((int)aabb.Min.x, (int)aabb.Min.y, (int)aabb.Width, (int)aabb.Height, letterToPrint);
		}
...
```

src/LowFiRockBlaster/Program.cs, add to Main
```
...
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
...
```

notice I'm again using `public static` functions, and calling a common function that has the logic written once
	In most cases, computer programmers focus on having a Single Point Of Truth wherever possible, even at the cost of performance.
	Being undisciplined about a Single Point of Truth leads to technical debt, which is an Invisible Wizard Problems that I'm trying to avoid in this tutorial.
	If we can keep complicated logic in one place, then we only need to fix one place when there is a bug.
		This creates less confusion, and less cognitive load.
	It is true that inlining, or writing the function again, will improve runtime speed.
		But the runtime speed improvement is tiny compared to the lost programmer time when reading, writing, and fixing the code.
	Single Point Of Truth is an optimization for the Programmer, not for the computer.
		This is right to do because most of the time, the programmer's costs is much higher than runtime costs for the computer.

Width and Height are public properties, which are methods that look like functions. these only use get methods.
	You'll see set properties later in the tutorial.

Also, notice how I am naming my variables. I make my public variables capitalized, which is the standard for C# properties.
	I do this intentionally, with the understanding that these variables should actually be private members with public property accessors.
	If I were doing this tutorial myself, I would convert public members into a private members with public property get and set methods.
	I am leaving the variables public because it is easier for you to copy, and will make no functional difference to the syntax.

In my coding style, lowercase member variables are explicitly primitives, which should actually not be accessed publicly,
	unless the class is a datastructure that exists solely to wrap around those members, like the `Vec2` struct.

### scene
writing and compiling program.cs
```
...
		public static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			DrawRectangle(0, 0, width, height, letterToPrint);
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
			Console.SetCursorPosition(0, (int)height);
		}
...
```

### voice
notice I'm using tuple notation for the first vector describing position, and an explicit constructor for the size.
the form is mostly stylistic. however, in an inner-loop, using the more verbose constructor is preferred because it is slightly faster to execute.

### scene
src/MrV/Geometry/Circle.cs
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
AABB.cs, with cartesian plane + grid diagram in small window
draw a circle at position 3,4 with a radius or 2. label radius with a line at 0 degrees

### voice
A circle can be described as a Vector with one additional value for radius.

### scene
src/MrV/Geometry/PolygonShape.cs
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
PolygonShape.cs, with cartesian plane + grid diagram in small window
draw point A at 1,1, point B at 3,4, point C at 1,7, and point D at 8,4
draw lines AB, BC, CD, and DA

### voice
A polygon's shape can be described as a list of 2 dimensional vectors, with the assumption that there is a line between each point in the sequence.

### scene
black background, 3 rows of labeled white boxes (each white box has 4 smaller gray boxes inside), followed by a text label.
	2 boxes labeled [x, y], followed by label: `Vec2`
	4 boxes labeled [Min.x, Min.y, Max.x, Max.y], followed by label: `AABB`
	3 boxes labeled [x, y, radius], followed by label: `Circle`
	below these 3 rows is another 1 row, with a white box (with 8 smaller gray boxes inside)
	1 box labeled [points], followed by the label `PolygonShape`

### voice
These data structures are small and simple in memory.
Each float taking up only 4 bytes. `Vec2` is a total of 8 bytes. `AABB` is 16. `Circle` is 12. The points array of `PolygonShape` is a reference, which is also small, probably 8 bytes.
Because they are simple in memory, these are written as struct Value types instead of class Reference types. A Value type has certain memory advantages.
A program passes all struct data by value instead of by reference. That means all data is copied each time it is used.
	References are almost always the same size, which is 8 bytes on a 64 bit CPU. the For small structures, especially less than or equal to 8 bytes, passing by value saves a CPU time.
	For larger structs, there are still advantages.
		the CPU is more certain about the value of a value-type, because it doesn't need to check a reference. This design eliminates cache misses for this data.
Check the description for additional explanation about the difference between value type and refernce type:
//`add to description` Value Type vs. Reference Type:
//  CodeMonkey: https://youtu.be/KGFAnwkO0Pk
//  MarkInman: https://youtu.be/95SkyJe3Fe0

You should also notice that I am using public variables. Again, it is considered best practice to use private variables wherever possible.
However, small structures with very clear conceptual boundaries like these are often left exposed, even after rapid prototyping.

---

## Drawing Math in the Command Line

### scene
```
...
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

...
```

```
...
		public static void Main(string[] args) {
			int width = 80, height = 24;
			char letterToPrint = '#';
			DrawRectangle(0, 0, width, height, letterToPrint);
			DrawRectangle((2, 3), new Vec2(20, 15), '*');
			DrawRectangle(new AABB((10, 1), (15, 20)), '|');
			DrawCircle((21, 12), 10, '.');
			Console.SetCursorPosition(0, (int)height);
		}
...
```

### voice
the circle drawing code is more complex than the rectangle drawing code, but starts with the same principles.
we iterate across a 2 dimensional area with a nested for-loop, just like with a rectangle.
instead of printing the character at every location, we check to see if the character is inside of the circle, and only print if it is.
one important additional optimization here is limiting the size of the rectangle. we calculate the start and end bounds of this region with the circle's Extent.

the logic to test if a point is inside of a circle is really important to the concept of a circle, so it should probably live in the circle class

### scene
Circle.cs
```
...
		public static bool IsInsideCircle(Vec2 position, float radius, Vec2 point) {
			float dx = point.x - position.x, dy = point.y - position.y;
			return dx * dx + dy * dy <= radius * radius;
		}
		public bool Contains(Vec2 point) => IsInsideCircle(center, radius, point);
...
```
split screen, showing Program.cs
```
...
					if (x < 0 || y < 0) { continue; }
					bool pointIsInside = Circle.IsInsideCircle(pos, radius, new Vec2(x, y));
					if (pointIsInside) {
						Console.SetCursorPosition(x, y);
						Console.Write(letterToPrint);
					}
...
```

### voice
There is some `Circle` specific logic written in Program.cs right here. It should really be in the `Circle` class instead.

This is a method extraction refactor, and it helps create a Single Point Of Truth for our circle logic.

If we implement a similar function in `PolygonShape`, we can use a similar draw function, like `DrawCircle`, to draw the polygon

### scene
PolygonShape.cs
```
...
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
...
```

### voice
The math for checking a point inside of a polygon is a bit complex. the basic idea is this:

### scene
PolygonShape.cs, with cartesian plane + grid diagram in small window
draw point A at 1,1, point B at 3,4, point C at 1,7, and point D at 8,4
draw lines AB, BC, CD, and DA

### voice
	imagine a ray from the given point, going to the right.

### scene
draw a point T at 4,3
draw a horizontal arrow extending to the right

### voice
	if that ray crosses the polygon's edges an odd number of times, then the point is inside of the polygon
	the inner loop checks if the ray from pt crosses an edge
		first check to see if the point is in range of the edge at all
		then do the math to test if the ray's x intersection is on the edge

Finding the bounding rectangle of the polygon is also not straight forward, so we should have a function
	it check every point, looking for the minimum and maximum values
	the minimum and maximum values are output that as a bounding box
This method can fail if the polygon is not correctly formed. This TryGet pattern is common in C# when failure is possible.

With this we can create a draw method for polygons

### scene
Program.cs
```
...
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
...
```

```
...
		public static void Main(string[] args) {
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
...
```

## Interactive Test

### voice
this code proves that I can draw important parts of my game.
Graphics are a huge feature and risk of any software, especially games.
proving this kind of work can be accomplished at all is critical for development, which is why I did this first.

graphics also have a way of motivating a game developer to keep working on their game.
I want to be able to see progress with my eyes, and test my understanding with visible results.

### scene
```
...
		public static void Main(string[] args) {
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
...
```

### voice
I want to see my simulation start to move, with a frame-advancing game engine.

The core of any simulation or game engine is a game loop, like this.
We can use this interactive loop to test parts of our game as we write it.
First, let's test circle drawing.

I'll add this basic swtich statement to trigger changes to my simulation with key presses.
This is a lot of logic to insert into the game loop, which does feel bad to me as a game engine developer.

### scene
run the app to test
show the circle move and change size

### voice
we can play with some circle values now
with some modifications, we could use this code to test the rectangle and polygon drawing code as well.
I recommend doing that as practice for novice developers.

If you feel like this code is a bit confusing, I recommend you change the switch statement to modify one of the rectangles being drawn, or the polygon. You can pause the video, I can wait.

---

## What Is A Game Loop

### scene
Program.cs
(wait 2 seconds)

### voice

A game engine has 4 main regions: initialization, draw, input, and update.
initialization happens before the game loop, and the game loop repeats draw, input, and update in series.
each single iteration through the loop is a frame.

we should formalize this a bit more clearly in the code.

### scene
```
...
		public static void Main(string[] args) {
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
...
```

### voice
C-sharp enables us to create local functions, which help us name and organize our code, and name the regions with function names.

many programmers, myself included, consider it good programming style to use small functions, with descriptive but concise names, and only one or two levels of indentation wherever possible.
lets run this refactored code to make sure it still works how it used to.

### scene
run the code to show that it still works the same way

### voice
Many programming languages don't support local functions.
For those, we might want to create a Game class that has the data members, an Init function, Draw, ProcessInput, and Update function. Like this:

### scene
Program.cs
```
...
public class Game {
	private int width, height;
	private char letterToPrint;
	private Vec2[] polygonShape;
	public bool IsRunning;
	private Vec2 position;
	private float radius;
	private float moveIncrement;
	private char input;
	public void Init() {
		width = 80;
		height = 24;
		letterToPrint = '#';
		polygonShape = new Vec2[] { (25, 5), (35, 1), (50, 20) };
		IsRunning = true;
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
		case (char)27: IsRunning = false; break;
		}
	}
}

public static void Main(string[] args) {
	Game game = new Game();
	game.Init();
	while (game.IsRunning) {
		game.Draw();
		game.Input();
		game.Update();
	}
}
...
```

### voice
this is a perfectly valid implemenatation in C-sharp as well. but for the sake of one fewer class, I'll keep writing in local functions in static main.
I'll just delete this Game class, and put the Main method back.

### scene
run again

### voice
notice the flickering. we can see how each shape is drawn right after the other, and the last drawn shapes flicker the most when the game is active.
this flickering might not be so bad if the game rendered more quickly.
lets implement a timer to see how long it takes to render the graphics.
and let's put the key input behind a check, so the game interacts as quickly as possible, without blocking the game loop.

## Timing

### scene
Program.cs
```
...
		public static void Main(string[] args) {
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
...
```
compile and test.

### voice
I've changed Input so that it doesn't wait for a user key press. this is also called Non-Blocking input.
My computer is pretty fast, but this game engine is really slow. it looks like it's running at around 100 milliseconds of delay between updates, or about 10 frames per second.
As with most games, the biggest reason for this performance is probably because of Draw.

### scene
comment out Draw and test again.

### voice
The code that claculates timing is going to be important later, and it feels pretty bad. So before we continue, I want to implement a better time-keeping class.

### scene
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
		public static long CurrentTimeMs => DateTimeOffset.Now.ToUnixTimeMilliseconds();
		public long DeltaTimeMilliseconds => _deltaTimeMs;
		public float DeltaTimeSeconds => _deltaTimeSec;
		public long TimeMillisecondsCurrentFrame => _timeMsOfCurrentFrame;
		public double TimeSecondsCurrentFrame => _timeSecOfCurrentFrame;
		public long DeltaTimeMsCalculateNow => _timer.ElapsedMilliseconds - _timeMsOfCurrentFrame;
		public float DeltaTimeSecCalculateNow => (float)(_timer.Elapsed.TotalSeconds - _timeSecOfCurrentFrame);
		public static long TimeMsCurrentFrame => Instance.TimeMillisecondsCurrentFrame;
		public static double TimeSecCurrentFrame => Instance.TimeSecondsCurrentFrame;
		public static Time Instance => _instance != null ? _instance : _instance = new Time();
		public static long DeltaTimeMs => Instance.DeltaTimeMilliseconds;
		public static float DeltaTimeSec => Instance.DeltaTimeSeconds;
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

### voice
Awareness of timing is an essential part of a real-time game engine.
It is required for accurate physics simulation calculations.
Testing duration is also critical to performance metrics.
And time can be used to throttle the game loop, to intentionally reduce CPU usage, and improve stability of the rest of the software on your computer.
Making a separate time-keeping system like this leaves design space for time-related features in the future, like pausing our simulation, or slowing down time.

This implementation is a wrapper around the C-sharp Stopwatch object, which is a high-resolution timer standard in the C-sharp API.
It is keeping track of the passage of time as seconds and milliseconds separately.
Floating point values for time are most convenient for physics calculations.
Millisecond values are more accurate over long durations, and most convenient for lower-level calculations.
Also, as floating point values increase, they reduce in precision.
	This is because as the exponent part of the float value increases, the scale of values being tracked changes. Eventually, the exponent will ignore millisecond changes to time.

### scene
black screen with white rows of text:
									integer					floating point
	binary	00000000000000000000000000000000			00000000000000000000000000000000
	decimal									0											0
	value as time: 0 hrs, 0 minutes, 0 seconds, 0 milliseconds
	rate increase: millisecond
there are 5 representations of the same number, one as a binary integer, another as a binary representation of a `float`, then a decimal integer and decimal float, and lastly, a time representation.
all 5 numbers increase at the same rate, with the rate of increase identified by the row labeled 'rate increase'.
after a few seconds, the rate increase changes from millisecond to second, then second to minute, then minute to hour.
show floating point number's exponent change as it increases in value.
after the 'value as time' row exceeds 5 hours, bring the rate increase value back to millisecond.
mark the floating point values in gray, unmoving, while the integer values are in green, continuing to increase 1 millisecond at a time.

### voice
Specifically, a game that has running for more than 4.5 hours will be more accurate using integer-based millisecond calculations instead of floating points.

### scene
back to code

### voice
This class is a singleton, which allows details about the passage of time to be accessed anywhere in the program, which is very convenient for game physics.
I'm not making the entire class static because pure static classes create design hazards similar to global variables. we'll talk more about this design choice later.
You might also notice that DeltaTime gives the same value until UpdateSelf is called.
	This is intentional, and will keep timing math consistent when `DeltaTime` is checked at multiple different times of the same update frame.
You may also realize that this code is actually giving a measurement of how long the last frame took, not how long this frame is taking.
	This works in practice because consecutive frames tend to require similar amounts of compute.
In the worst case for this design:
	a one frame lag-spike will cause a very laggy frame to use the faster timing value of the previous frame
	the next fast frame will use the long frame time of the previous laggy frame, but do so very quickly
	to a very keen-eyed observer, this will look like a strange stutter, where the game slows down a lot, and then speeds up a lot, over the course of only a few milliseconds.
	the proper solution to this problem would not be a change to the timing system, but a change to the code causing the lag spike.
The `ThrottleUpdate` function is used to smooth out frames, and reduce CPU burden.
The `ThrottleWithoutConsoleKeyPress` interrrupts the throttling when a keypress is detected, so that the game always updates quickly to user input, even if the framerate is set to be low.
lets test this out.

### scene
Program.cs
```
...
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
...
```

### voice
It's possible to design the `Time` class without a necessary `Update` method, but doing so would result in different values for delta time within the same gameloop frame.
This implementation tries to artificially set the gameloop speed to 20 frames per second. Feel free to experiment with this value.
A lower framerate, which is a higher frame delay, reduces the burden that this program puts on the CPU.
	A lower CPU burden improves performance of the rest of your computer while this game is running.

Notice that `Time.Update();` is called in the game loop, to track the passage of time and guage the cost of the entire process for `SleepWithoutConsoleKeyPress`.

---

## Stop the Flickering

### scene
show the game is running, with the printed DeltaTimeMs value fluctuating

### voice
Performance is incredibly important to a realtime simulation, and a game especially. User experience is tightly bound to game loop performance.
Good performance also improves our ability to test, which is also critical to software development.

Right now, only one key is being processed per frame. This makes the game feel slow.

To improve performance immediately for testing, I want to do two quick things: flush the entire input buffer in the input function:

### scene
Program.cs
```
...
			void Input() {
				if (Console.KeyAvailable) {
					while (Console.KeyAvailable) {
						input = Console.ReadKey().KeyChar;
					}
				} else {
					input = (char)0;
				}
			}
...
```

### voice
like this. and reduce the amount of drawing going on:

### scene
Program.cs
```
...
			void Draw() {
				DrawRectangle(0, 0, width, height, letterToPrint);
				//DrawRectangle((2, 3), new Vec2(20, 15), '*');
				//DrawRectangle(new AABB((10, 1), (15, 20)), '|');
				DrawCircle(position, radius, '.');
				//DrawPolygon(polygonShape, '-');
				Console.SetCursorPosition(0, (int)height);
			}
...
```

### voice
like this.

There are three specific classes of problems have major impacts on simulation performance that I want to keep adressing in this tutorial:
	Drawing, Memory Allocation, and Collision detection.

Let's continue improve drawing.

We can significantly reduce the cost of drawing and the appearance of flickering by only drawing each character once.
We'll do that by writing our graphics into a separate buffer, and draw that once.
this technique is called double buffering.

### scene
cartoon of an artist artist painting a picture
then the artist flips the picture, presenting it to an audience
then the artist pulls up another blank canvas
then the artist paints a slightly different picture
then the artist flips the picture, presenting it to an audience
then the artist pulls up another blank canvas
then the artist paints a slightly different picture
then the artist flips the picture, presenting it to an audience
then the artist pulls up another blank canvas
then the artist paints a slightly different picture
then the artist flips the picture, presenting it to an audience
then the artist pulls up another blank canvas

### voice
This technique dramatically reduces flickering by replacing the entire image at once instead of redrawing all different parts one at a time
It requires a Front Buffer, and a Back Buffer.
  The Front Buffer is displayed to the user. In our program, it is already there. It is the command line console.
  The Back Buffer is where the graphics are rendered in sequence before overwriting the Front Buffer all at once

### scene
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

### voice
This is a buffer class for drawing console characters. I'll refer to these characters as glyphs.
The buffer is a 2D array of these glyphs, with some additional convenience methods.
The class is a partial class so that we can split it's impementation across multiple files. We'll put specialized drawing methods in a separate place.
This code uses C-sharp's contiguous block 2D array allocation instead of an array-of-arrays, which some progammers might be more familiar with.
Notice that Height is the first dimension and Width is the second. These dimensions can be in either order, but it should be consistent once it is selected.
	I choose Height first because it intuitively follows the existing rectangle code, and also improves CPU cache locality when scanning horizontally, which is historically how graphics work.
The square-bracket operator is overloaded so the class can be accessed like a 2D array in our code.
	If you want to change the order of x/y you can do it here. Doing so is a great exercise in resolving confusion, and internalizing the value of consistent dimension ordering.
	Generations of graphics programmers before you have internalized the ambiguity of dimension order, and unlocked mental resiliency in the process.
	this is one of the invisible wizard problems that creates undocumented skills shared by many game programmers.

### scene
scroll down and highlight the last half of the ResizeBuffer method
write in big bold letters on the side of the screen
  Y  you
  A  ain't
  G  gunna
  N  need
  I  it

### voice
This `ResizeBuffer` method is more robust than we need it to be, because it will copy old data into the new buffer to maintain consistency.
This feature will probably not be needed, so it could be argued the extra code is a waste of time and mental energy, according to the YAGNI or You Aint Gunna Need It principle.

### scene
gray-out the YAGNI text, and write white text with red outline over it
  I Don't Wanna Worry About It

### voice
However, this feature fulfills my intuition of how the `ResizeBuffer` function should work. That allows me to comfortably forget about how it actually works later.
For me, the cognitive load of writing the extra functionality now is less than the cognitive load of having to remember that the feature doesn't exist in the future.

### scene
scroll down to WriteAt overloads, Clear, and Print + PrintBuffer

### voice
The buffer needs methods to write glyphs into it. We need a method to clear the buffer before every draw.
And a a convenience method for printing to the command line console, with a static implementation that could be useful for debugging.

### scene
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

because our shape-drawing methods are so similar, drawing shapes can be generalized to the DrawShape method here.
A delegate defines if the coordinate is in the given shape, and checks each point in a given region.

I have chosen to write this in a separate file because it feels like the DrawBuffer class is doing too much already.
This functionality still belongs with the DrawBuffer. Maybe we can decide a better place for it when the game engine is more complete.

let's test this out

### scene
Program.cs
```
...
			char input = (char)0;
			float targetFps = 20;
			int targetMsDelay = (int)(1000 / targetFps);
			DrawBuffer graphics = new DrawBuffer(height, width);
			while (running) {
...
```

```
...
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
...
```

### voice
we can and should remove the previous draw functions now, since we shouldn't print directly to the command line anymore, and equivalent logic is in DrawBuffer_geometry.cs.

Our code is now using the `DrawBuffer` as a Graphics Context, which is a computer graphics concept.
A graphics context is where we can include anything related to graphics state. We'll expand this idea soon.

We can also add back the test drawing that was causing flickering before, since the buffer technique has eliminated the flickering.

the circle draws more quickly now. but the shape is not actually correct,

### scene
diagram of console, showing width/height ratio of glyphs

### voice
because the command line console's characters are not perfect squares
we can take that into account with our shape drawing code if we can scale our 2D vectors
Lets add scale methods to Vec2

### scene
MrV/Geometry/Vec2.cs
```
...
		public void Scale(Vec2 scale) { x *= scale.x; y *= scale.y; }
		public Vec2 Scaled(Vec2 scale) => new Vec2(x * scale.x, y * scale.y);
		public void InverseScale(Vec2 scale) { x /= scale.x; y /= scale.y; }
...
```

### voice
in addition to Scale, we should be able to undo scaling.
Also, we will want a version of Scale that returns a new scaled structure without modifying this structure's data.

### scene 
MrV/DrawBuffer_geometry.cs
```
...
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
...
```

### voice
we need to keep track of the desired scale. for that, we'll add a scale variable to `DrawBuffer`.
	arguably, the `ShapeScale` variable added to the `DrawBuffer` class is bad design.
	this partial class implementation could instead be a subclass, to keep a clearer boundary between buffer management and drawing with a scale.
	I am making the intentional choice to combine these ideas into the same class, to reduce my cognitive load for this system.
		Because this code is in a separate file, I can fix this more easily later if the design weighs on me.
	Managing cognitive load is one of those Invisible Wizard Problems. Some developers also call it Managing Complexity.
	If you are a stickler for Object Oriented Design, feel free to subclass this as 'ShapeDrawingBuffer' or something. Though I recommend you do that after you finish the tutorial.
to draw the shape in a scaled way, we need to inverse-scale the bounding rectangle being drawn in, to put it in the correct position in the buffer
then we need to test against the scaled point, which is being printed to the unscaled position in the buffer.

If you want to change the direction of the Y axis in the simulation, you can experiment with changing the sign of the y-value of `ShapeScale`.
	I recommend doing that later, after we implement a moving camera.

Because we added the scale member to this class, we don't need to pass it in as a variable. that means we don't change any of the other method signitures. Nice.

### scene
(run test)

### voice
Testing with key presses every time is a bit tedious.

I want to be able to test my app more automatically, without having to press keys to do it.
For that, I will impliment a task scheduler, which we can use for many other purposes later as well.

---

## Task Scheduling

### scene
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

### voice
This is a very simple task scheduler, which executes functions at a given delay. This is functionally similar to Javascript's SetTimeout.
In this implementation, the `Tasks.Task` type is basically a container for a function to invoke, and a time to invoke it.
The `System.Action` type is a variable that stores a function to invoke later. This can also be accomplished with a delegate, as we saw in `DrawBuffer.IsInsideShapeDelegate`.
Each task also keeps track of what line of code called `Tasks.Add`, which is very valuable information when debugging asynchronous functionality like this.
Execution of tasks happen in the Update method, where the next Task to execute is at the front of a the Tasks list.
A seperate list in RunUpdate gathers tasks to execute before the execution.
	If an executing Task ends up calling `Tasks.Add` in a nested call, this separation prevents an infinite loop.
Ordering is done by a Binary Search algorithm, generalized to work on generic records. The implementation of this binary search looks like this:

### scene
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

### voice
`BinarySearch` is a classic algorithm that works on a list of ordered values. The method assumes the list is ordered, and it will not work if the list isn't sorted.
`BinarySearch` tests `IComparable` values, which extend a `CompareTo` method. All primitive types, like `float`s and `int`s, are `IComparable`.
	In the `CompareTo` function,
		A negative value means the left-value is smaller than the right-value.
		A zero value means the left-value and right-value are equal.
In the inner loop, BinarySearch checks if the value being searched for is directly in the middle of the search space.
	if the exact value is found in the middle, BinarySearch provides it's index in the list
	if the found value is too low, the next search space will be in the top half of the current search space
	if the found value is too high, the next search space will be in the bottom half of the current search space
if the search space is reduced to zero or less, the value was not found.
this algorithm returns the 2's compliment of where the algorithm stopped searching, which is where the value should have been.
	2's compliment flips all of the binary bits in an integer value. the operation will undo itself.
	2's compliment of a positive index will always be a negative value, so we can detect if the value already exists or needs to be inserted by checking the sign of the return.

### scene
Task.cs
```
...
			Record searchElement = new Record(null, when);
			Comparer<Record> RecordComparer = Comparer<Record>.Create((a, b) => a.WhenToDoIt.CompareTo(b.WhenToDoIt));
			int index = actions.BinarySearch(searchElement, RecordComparer);
...
```

### voice
I could have also just used the `BinarySearch` method already in C-sharp's List class.
As long as the `RecordComparer` is created as a static member, there isn't any significant performance gain in using my custom algorithm.
However, my search algorithm doesn't need to create a mostly empty search element.
	That means my algorithm becomes better if the Task class becomes more complex, since we don't need to allocate the entire thing.
	I expect that the Task class could become quite complex, and I don't want to worry about the cost of enqueueing a task later.
Also, this is an excellent example of a templated function using lambda expressions, which my target audience might appreciate.
	I will continue using more functional programming like this, so please do any additional experimentation and research you need to understand this before watching more.

### scene
src/Program.cs
```
...
			void Update() {
				Tasks.Update();
				switch (input) {
...
```

```
...
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
...
```

### voice
I need to make sure the `Tasks` are regularly Updated, so I'll include `Tasks.Update` in the `Update` section of my code.
Before the gameloop, this code creates an automatic test of my application by synthetically setting the program's input variable.
the first for-loop moves the circle to the right with the 'd' key, and expands the radius with the 'e' key
the second for-loop moves the circle up with the 'w' key, and reduces the radius with the 'r' key.
each of the key input changes should happen about 100 milliseconds apart.

### scene
test the code

### voice
Nice.
If the `keyDelayMs` timing is reduced to less than the deltaTime of a frame, some of these inputs will be lost, and the circle will not move the same amount.

## Key Input Dispatcher

### scene
set the `KeyDelayMs` value to 5 and test again

### voice
Lets make a better Key Input system to solve this and other bugs.

### scene
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

### voice
A `KeyResponse` is just some function, which happens in response to a keypress.
I could have used `System.Action` instead, but using a named delegate type means we can change this more easily later.
A structure keeps the relationship of each Key press and `KeyResponse`, along with a note about the purpose of the key binding.
	The `KeyType` is templated because this implementation will just use characters, but any type of input should work as well.
A `Dispatcher` manages a queue of events, and those events are mapped in a `dispatchTable` to responses.
	This is a general concept that is useful in many domains beyond key input handling.
	If we were making a scripting engine or multiplayer system, we could use this dispatcher for a critical part of the system there.
`BindKeyResponse` will bind a `KeyResponse` delegate to a key.
	If the key has never been bound before, a new list of KeyResponses will be created for that key.
Events added to the `Dispatcher` will be consumed all at once.
Just like the task scheduler, execution will happen from a list that can't be added to while actions are processed.

This `KeyInput` implementation creates a singleton for easy access, with a public `Set` method for the `Instance`.
This design will allow the `KeyInput` system to swap out at runtime, in case different different user-interface states use different keybindings.
Otherwise, this class can be accessed statically like any singleton, for convenience.
The `KeyInput` class reads specifically from the C-sharp Console, so it has a conveniently labeled place for that logic.
The `ToString` method shows how to dynamically query what is bound to each key, which could be useful for exposing dynamic key binding at runtime.

### scene
src/Program.cs
```
...
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
...
```

### voice
Now keys are bound to functions during initialization.
This example inlines the very simple functions, and takes advantage of the Note field to create more clarity in the code.
I personally like this style of keybinding a lot. It feels like how rules of a boardgame are explained before the game starts.
	It allows definitions of controls to happen closer other important context for those controls.

Notice that the old code setting the input variable has been replaced with additions to the KeyInput queue. That old input variable is obsolete now, and removed.

### scene
src/Program.cs
```
...
			void Input() {
				KeyInput.Read();
			}
			void Update() {
				KeyInput.TriggerEvents();
				Tasks.Update();
			}
...
```

### voice
Because `KeyInput` takes care of console input logic, the Input function can be dramatically simplified, and so can `Update`.

now when we run the program to test it, key events are not lost, even when the keyDelay is lowered to much less than the `Update`'s `DeltaTime`.

the input bug is fixed!

let's keep working on the graphics optimization

The image doesn't actually need to be fully refresh every frame, only a few characters change each frame.
This is similar to a graphics optimization technique called Pixel Caching, done here with character glyphs.

---

## Graphics Context

### scene
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
		public virtual void PrintModifiedOnly() {
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

### voice
The `GraphicsContext` class is a `DrawBuffer`, and it also keeps track of previous buffer data which was already drawn.
The decision to inherit `DrawBuffer` could be argued here.
Conceptually, `GraphicsContext` has two `DrawBuffer`s instead of being a buffer with spare data.
I decided to use inheritance because `GraphicsContext` would want an API surface similar to `DrawBuffer`, and `_lastBuffer` ccould be the second buffer internally.

The new `PrintModifiedOnly` method checks every character to determine if it is the same as the last character printed.
only different characters are printed.
after every complete print, which is commonly called a Render, the current active buffer and last buffer can switch places.

`PrintModifiedOnly` could be further optimized to reduce calls to `SetCursorPosition`.

### scene
```
...
		public virtual void PrintModifiedOnly() {
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
...
```

### voice
`Console.Write` implicitly moves the cursor position over one space.
The cursor position only needs to be set if there is a new row, or if the last glyph was not printed.

In practice, `SetCursorPosition` is an expensive call in the Console API.

### scene
src/Program.cs
```
...
			int targetMsDelay = (int)(1000 / targetFps);
			GraphicsContext graphics = new GraphicsContext(height, width);
			KeyInput.Bind('w', () => position.y -= moveIncrement, "move circle up");
...
```

### voice
The `GraphicsContext` has basically the same API surface as `DrawBuffer`, so it can be substituted without incident.

### scene
src/Program.cs
```
...
				graphics.DrawPolygon(polygonShape, '-');
				graphics.PrintModifiedOnly();
				graphics.SwapBuffers();
				Console.SetCursorPosition(0, (int)height);
...
```

### voice
Using the optimized draw happens in the same way as the previous print, except that `SwapBuffers` is called to swap the draw buffer data at the end.
A call to `SwapBuffers` might seem like an overly verbose requirement. However, it's very common in graphics APIs, so it's worth getting used to the idea.

The first time I wrote this program, I didn't separate `DrawBuffer` with `GraphicsContext`, I just wrote them all in the same class.
I want to make a special note about it because I want to remind the audience that software design is difficult, and refactoring is common.
	and I want to emphasize that doing something for the first time means you should be comfortable with imperfect design.
	every major computer programming problem I have ever solved started with a relatively messy intuitive solution.
	My messy code didn't evolve into something that made sense until I sat with it for a while and re-wrote it.
	Be patient with yourself as a developer. Give yourself the grace to rewrite messy code later.

Running this program is *much* faster than it used to be. Most of the time draw happens, there is actually no change at all.
And sometimes, only small amounts of the screen needs to change.

The graphics look fast enough to test quickly. we still need to keep working with the graphics system to use colors, which is part of the original game design.

## Colors in the Console

### scene
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

### voice
C-sharp's console API gives us access to 16 colors, in both the foreground and background.
This structure will be useful to have color data without text in our graphics system.

These values are stored as 1-byte values instead of the default enumeration type, which is possibly a 4 byte value.

We could do some bitwise tricks to put both 4-bit values into one 8-bit byte, but that memory optimization doesn't actually help because of struct memory alignment in C-sharp.

A static constructor remembers what the default console colors are as soon as any ConsoleColorPair code is called. This could help solve some color bugs later.

### scene
src/MrV/CommandLine/ConsoleGlyph.cs
```
using System;
using System.Text;

namespace MrV.CommandLine {
	public struct ConsoleGlyph {
		private char letter;
		private ConsoleColorPair colorPair;
		public char Letter {
			get { return letter; }
			set {
				letter = value;
				if (letter != '\n' && (letter < 32 || letter > 126)) {
					throw new Exception("out of ascii range");
				}
			}
		}
		public ConsoleColor Fore { get { return colorPair.fore; } set { colorPair.fore = value; } }
		public ConsoleColor Back { get { return colorPair.back; } set { colorPair.back = value; } }
		public ConsoleGlyph(char letter, ConsoleColorPair colorPair) { this.letter = letter; this.colorPair = colorPair; }
		public static implicit operator ConsoleGlyph(ConsoleColor color) => new ConsoleGlyph(' ', Default.Fore, color);
		public ConsoleGlyph(char letter, ConsoleColor fore, ConsoleColor back) :
			this(' ', new ConsoleColorPair(fore, back)) { }
		public static readonly ConsoleGlyph Default = new ConsoleGlyph(' ', ConsoleColorPair.Default);
		public static readonly ConsoleGlyph Empty = new ConsoleGlyph('\0', ConsoleColor.Black, ConsoleColor.Black);
		public bool Equals(ConsoleGlyph other) => other.letter == letter && other.Fore == Fore && other.Back == Back;
		public override string ToString() => letter.ToString();
		public void ApplyColor() => colorPair.Apply();
		public static ConsoleGlyph[] Convert(string text,
			ConsoleColor fore = ConsoleColor.Gray, ConsoleColor back = ConsoleColor.Black) {
			ConsoleGlyph[] result = new ConsoleGlyph[text.Length];
			for (int i = 0; i < result.Length; i++) {
				ConsoleGlyph g = new ConsoleGlyph(text[i], fore, back);
				result[i] = g;
			}
			return result;
		}
		public static string Convert(ConsoleGlyph[] text) {
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < text.Length; ++i) {
				sb.Append(text[i].letter);
			}
			return sb.ToString();
		}
	}
}
```

### voice
Each glyph on the screen should have the `ConsoleColorPair` qualities, so we can change the color.
Because each structure is a struct, there can't be inheritance, the glyph must be composed.
If we want to conveniently access a glyph's colors, we should do it with a properties.
There are two constructors, and an implicit constructor, all eventually calling the same base constructor, so we keep a Single Point of Truth.
A few readonly constant-like values will help conveniently define things like a default clear canvas, which is different from an explicitly empty canvas.

Some convenience methods will help convert text to and from `ConsoleGlyphs`.

Now we need to use the console glyph in the `DrawBuffer`, instead of the character primitive.

### scene
src/MrV/CommandLine/DrawBuffer.cs
```
...
	public partial class DrawBuffer {
		protected ConsoleGlyph[,] _buffer;
		public int Height => GetHeight(_buffer);
		public static int GetHeight(ConsoleGlyph[,] buffer) => buffer.GetLength(0);
		public int Width => GetWidth(_buffer);
		public static int GetWidth(ConsoleGlyph[,] buffer) => buffer.GetLength(1);
		public Vec2 Size => new Vec2(Width, Height);
		public ConsoleGlyph this[int y, int x] {
			get => _buffer[y, x];
			set => _buffer[y, x] = value;
		}
...
```

```
...
		protected static void ResizeBuffer(ref ConsoleGlyph[,] buffer, int height, int width) {
			ConsoleGlyph[,] oldBuffer = buffer;
			buffer = new ConsoleGlyph[height, width];
...
```

```
...
		public Vec2 WriteAt(string text, int row, int col) => WriteAt(ConsoleGlyph.Convert(text), row, col);
		public Vec2 WriteAt(ConsoleGlyph[] text, int row, int col) {
			for (int i = 0; i < text.Length; i++) {
				ConsoleGlyph glyph = text[i];
				switch (glyph.Letter) {
...
```

```
...
		public void WriteAt(ConsoleGlyph glyph, int row, int col) {
...
```

```
...
		public void Clear() => Clear(_buffer, ConsoleGlyph.Default);
		public static void Clear(ConsoleGlyph[,] buffer, ConsoleGlyph background) {
...
```

```
...
		public static void PrintBuffer(ConsoleGlyph[,] buffer) {
			int height = GetHeight(buffer), width = GetWidth(buffer);
			for (int row = 0; row < height; ++row) {
				Console.SetCursorPosition(0, row);
				for (int col = 0; col < width; ++col) {
					ConsoleGlyph glyph = buffer[row, col];
					glyph.ApplyColor();
					Console.Write(glyph);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
...
```

### voice
I can make the right changes by:
	search/replace of `char` with `ConsoleGlyph`
	replace `text.ToCharArray()` with `ConsoleGlyph.Convert(text)`
	set the switch statement in `WriteAt` to use `glyph.Letter`
	changed the `Clear()` method to call `Clear(_buffer, ConsoleGlyph.Default)`
	in `PrintBuffer`,
		just before `Console.Write(glyph);`, add
			`glyph.ApplyColor();`
		at the very end of the method
			`ConsoleGlyph.Default.ApplyColor();`

Similar changes need to happen in the DrawBuffer_geometry file

### scene
src/MrV/CommandLine/DrawBuffer_geometry.cs
```
...
		public void DrawShape(IsInsideShapeDelegate isInsideShape, Vec2 start, Vec2 end, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawRectangle(Vec2 position, Vec2 size, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawRectangle(int x, int y, int width, int height, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawRectangle(AABB aabb, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawCircle(Circle c, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawCircle(Vec2 pos, float radius, ConsoleGlyph letterToPrint) {
...
```

```
...
		public void DrawPolygon(Vec2[] poly, ConsoleGlyph letterToPrint) {
...
```

### voice
In DrawBuffer_geometry, search/replace of `char` with `ConsoleGlyph`

And because we changed `DrawBuffer`, we need to change it's inheriting class `GraphicsContext`

### scene
src/MrV/CommandLine/GraphicsContext
```
...
		public virtual void PrintModifiedOnly() {
			for (int row = 0; row < Height; ++row) {
				bool mustMoveCursorToNewLocation = true;
				for (int col = 0; col < Width; ++col) {
					bool isSame = this[row, col].Equals(_lastBuffer[row, col]);
					if (isSame) {
						mustMoveCursorToNewLocation = true;
						continue;
					}
					ConsoleGlyph glyph = this[row, col];
					if (mustMoveCursorToNewLocation) {
						Console.SetCursorPosition(col, row);
						mustMoveCursorToNewLocation = false;
					}
					glyph.ApplyColor();
					Console.Write(glyph);
				}
			}
			ConsoleGlyph.Default.ApplyColor();
		}
		public void SwapBuffers() {
			ConsoleGlyph[,] swap = _buffer;
			_buffer = _lastBuffer;
			_lastBuffer = swap;
		}
...
```

in `GraphicsContext`
	search/replace of `char` with `ConsoleGlyph`
	in `PrintModifiedOnly()`, replace the `isSame` variable initialization using a double-equal operator with
		`bool isSame = this[row, col].Equals(_lastBuffer[row, col]);`
	in `PrintModiefiedOnly`,
		just before `Console.Write(glyph);`, add
			`glyph.ApplyColor();`
		at the very end of the method
			`ConsoleGlyph.Default.ApplyColor();`

### scene
src/Program.cs
```
...
			void Draw() {
				Vec2 scale = (0.5f, 1);
				graphics.Clear();
				graphics.DrawRectangle(0, 0, width, height, ConsoleGlyph.Default);
				graphics.DrawRectangle((2, 3), new Vec2(20, 15), ConsoleColor.Red);
				graphics.DrawRectangle(new AABB((10, 1), (15, 20)), ConsoleColor.Green);
				graphics.DrawCircle(position, radius, ConsoleColor.Blue);
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
				graphics.PrintModifiedOnly();
				graphics.SwapBuffers();
				Console.SetCursorPosition(0, (int)height);
			}

...
```
test the code

### voice
now we can test these changes and see that our graphics are colored squares instead of plain gray special characters

the graphics are very low resolution.

there is a programming trick called Anti-Aliasing that allows graphics to look like they have higher resolution than they really do.

---

## Naive Anti-Aliasing in the Command Line

### scene
video showing anti-aliasing

### voice
this technique requires a large color space to work best. still, even with only 16 colors, we can implement a basic anti-aliasing, and it will help.

The technique requires that we calculate a higher-resolution than we can actually draw, which we call a super-sample.
once we have a super-sample for each pixel that we are drawing, we can decide how to draw that pixel with more color information.

### scene
src/MrV/DrawBuffer_geometry.cs
```
...
	public partial class DrawBuffer {
		public static ConsoleColorPair[,] AntiAliasColorMap;
		static DrawBuffer() {
			int countColors = 16;
			int maxSuperSample = 4;
			AntiAliasColorMap = new ConsoleColorPair[countColors, maxSuperSample];
			for (int i = 0; i < countColors; ++i) {
				for (int s = 0; s < maxSuperSample; ++s) {
					AntiAliasColorMap[i, s] = new ConsoleColorPair(ConsoleColor.Gray, (ConsoleColor)i);
				}
			}
			// for light colors, the least-populated half should use the darker color
			for (int i = (int)ConsoleColor.Blue; i <= (int)ConsoleColor.White; ++i) {
				for (int s = 0; s < maxSuperSample/2; ++s) {
					AntiAliasColorMap[i, s] = new ConsoleColorPair(ConsoleColor.Gray, (ConsoleColor)(i-8));
				}
			}
			// gray is a special case because DarkGray is 1 value after Gray
			int grayIndex = (int)ConsoleColor.Gray;
			AntiAliasColorMap[grayIndex, 0] = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.DarkGray);
			AntiAliasColorMap[grayIndex, 1] = new ConsoleColorPair(ConsoleColor.Gray, ConsoleColor.DarkGray);
		}

		public Vec2 ShapeScale = new Vec2(0.5f, 1);
		public delegate bool IsInsideShapeDelegate(Vec2 position);
		public void DrawShape(IsInsideShapeDelegate isInsideShape, Vec2 start, Vec2 end, ConsoleGlyph glyphToPrint) {
			Vec2 renderStart = start;
			Vec2 renderEnd = end;
			renderStart.InverseScale(ShapeScale);
			renderEnd.InverseScale(ShapeScale);
			int TotalSamplesPerGlyph = AntiAliasColorMap.GetLength(1);
			int SamplesPerDimension = (int)Math.Sqrt(TotalSamplesPerGlyph);
			float SuperSampleIncrement = 1f / SamplesPerDimension;
			for (int y = (int)renderStart.y; y < renderEnd.y; ++y) {
				for (int x = (int)renderStart.x; x < renderEnd.x; ++x) {
					if (!IsValidLocation(y, x)) { continue; }
					int countSamples = 0;
					if (isInsideShape != null) {
						for (float sampleY = 0; sampleY < 1; sampleY += SuperSampleIncrement) {
							for (float sampleX = 0; sampleX < 1; sampleX += SuperSampleIncrement) {
								bool pointIsInside = isInsideShape(new Vec2((x + sampleX) * ShapeScale.x, (y + sampleY) * ShapeScale.y));
								if (pointIsInside) {
									++countSamples;
								}
							}
						}
					} else {
						countSamples = TotalSamplesPerGlyph;
					}
					if (countSamples == 0) { continue; }
					ConsoleGlyph glyph = glyphToPrint;
					glyph.back = AntiAliasColorMap[(int)glyphToPrint.back, countSamples - 1].back;
					WriteAt(glyph, y, x);
				}
			}
		}
...
```

### voice
I must admit that this implementation of antialiasing is very naive, and doesn't take color mixing from overlapping geometry into account.
	This is an intentional choice made in the robustness vs accessability tradeoff I mentioned at the beginning of the tutorial.

because all of the draw methods use `DrawShape`, we can accomplish all of our Anti-Aliasing by only modifying that one method.

at the beginning of the implementation of this partial class, I'll define the anti-alias gradients for each color.
	this will only be meaningful for the bright colors in out 16 color range

the `DrawShape` method needs to do more checks per glyph, to count samples.
the additional nested for-loop counts how many times the `isInsideShape` function would trigger in each glyph's space.
then, before the glyph is printed, a copy is made with the correct background color based on it's starting background color and sample count.

### scene
src/MrV/DrawBuffer_geometry
```
...
		public void DrawLine(Vec2 start, Vec2 end, float thickness, ConsoleGlyph letterToPrint) {
			Vec2 delta = end - start;
			Vec2 direction = delta.Normalized;
			Vec2 perp = direction.Perpendicular * thickness;
			Vec2[] line = new Vec2[] { start - perp, start + perp, end + perp, end - perp };
			DrawPolygon(line, letterToPrint); 
		}
...
```

### voice
drawing lines is an essential part of testing and debugging vector math, which we may need to do soon.
while we are in the drawing code, we should add a method to draw lines.
this creates a thin rectangle, with the center of two of it's opposite edges at the given start and end coordinate.

Vec2 needs some additional math to support this math.

### scene
```
...
		public float MagnitudeSqr => x * x + y * y;
		public float Magnitude => MathF.Sqrt(MagnitudeSqr);
		public static Vec2 operator *(Vec2 vector, float scalar) => new Vec2(vector.x * scalar, vector.y * scalar);
		public static Vec2 operator /(Vec2 vector, float scalar) => new Vec2(vector.x / scalar, vector.y / scalar);
		public Vec2 Normalized => this / Magnitude;
		public Vec2 Perpendicular => new Vec2(y, -x);
		public bool Equals(Vec2 v) => x == v.x && y == v.y;
...
```
PolygonShape.cs, with cartesian plane + grid diagram in small window
draw point A at 1,1, point B at 3,4
draw lines AB

### voice
if you have 2 points in space, you can calculate their difference, or `Delta` with simple subtraction.
the distance, also called the `Magnitude` of the `Delta`, can be determined with the pythagorean theorum, 'a' squared plus 'b' squared equals 'c' squared.
the square root operation is fairly expensive for a computer to do accurately, so for performance reasons, it's best to do math that doesn't need square root as much as possible.
	for this reason, game engine APIs will often include a `MagnitudeSqr`, to eliminate a call to the square-root function. this is fine as long as we compare other squared values

### scene
draw a thicker line along point AB that stops at length 1, at the edge of the unit circle

### voice
if we divide the entire vector by it's `Magnitude`, we get it's `Normalized` value, which we can think of as a direction.

### scene
show angle theta between line AB and the X axis starting at point A
draw a unit circle (radius 1) arount point A
draw a horizontal line to the right from point A to the edge of the unit circle, label the X component
draw a vertical line down from poitn A the of the unit circle, label the y component

### voice
the x and y components of a normal value are identical to the cosine and sine values of this `Normalized` vector.

As a personal note, I felt I was terrible at math in high-school, especially when I studied trigonometry.
As a game developer, I have never needed to know my trig-identities, but using a unit vector to describe direction like this has been necessary very often.

### scene
draw a vertical line up from point A the of the unit circle that is as long as the x component, label it perp-x component
draw a horizontal line to the right from point A to the edge of the unit circle as long as the y component, label it perp-y

### voice
Swapping the x and y components of a vector and making one of them negative will give a perpendicular vector.

we need this perpendicular vector to create the thin rectangle for our line drawing

---

## Particles

### scene
src/MrV/GameEngine/Particle.cs
```
using MrV.CommandLine;
using MrV.Geometry;
using System;

namespace MrV.GameEngine {
	public class Particle {
		public Circle Circle;
		public ConsoleColor Color;
		public Vec2 Velocity;
		public Particle(Circle circle, Vec2 velocity, ConsoleColor color) {
			Circle = circle;
			Velocity = velocity;
			Color = color;
		}
		public void Draw(GraphicsContext g) {
			g.DrawCircle(Circle, Color);
			float speed = Velocity.Magnitude;
			if (speed > 0) {
				Vec2 direction = Velocity / speed;
				Vec2 rayStart = Circle.center + direction * Circle.radius;
				g.DrawLine(rayStart, rayStart + Velocity, 0.5f, Color);
			}
		}
		public void Update() {
			Vec2 moveThisFrame = Velocity * Time.DeltaTimeSec;
			Circle.center += moveThisFrame;
		}
	}
}
```

### voice
We can use the line to visualize the velocity of a moving particle.

This simple particle class is just a circle with a color that moves along a linear path, which is defined by velocity vector.

The `Draw` code represents velocity as a line coming out of the edge of its circle.
	if the particle has no velocity, it doesn't draw the velocity line
direction is calculated by dividing the velocity by the magnitude.
	This is the same as just calling the `Normalized` property.
	But, I avoid using `Normalized` to avoid recalculating the same square root value again. Again, an accurate square-root value is expensive to re-compute.

the `Update` method will change the position of the particle's cirle based on the velocity, and the amount of time passed.

### scene
src/Program
```
...
			}
			Particle particle = new Particle(new Circle((10,10), 3), (3,4), ConsoleColor.White);
			while (running) {
...
```

```
...
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
				particle.Draw(graphics);
				graphics.PrintModifiedOnly();
...
```

```
...
				Tasks.Update();
				particle.Update();
			}
...
```

### voice
To include the moving particle in our app, we need to: initalize it, and add it's `Draw` and `Update` methods to the game loop.

### scene
test

### voice
this looks satisfactory. I wonder if more particles will be even more satisfying. Let's make many of them.

### scene
```
...
			}
			Particle[] particles = new Particle[10];
			for (int i = 0; i < particles.Length; ++i) {
				Vec2 direction = Vec2.ConvertDegrees(i * (360f / 10));
				float speed = 5;
				particles[i] = new Particle(new Circle((10, 10), 3), direction * speed, ConsoleColor.White);
			}
			while (running) {
...
```

```
...
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
				for (int i = 0; i < particles.Length; ++i) {
					particles[i].Draw(graphics);
				}
				graphics.PrintModifiedOnly();
...
```

```
...
				Tasks.Update();
				for (int i = 0; i < particles.Length; ++i) {
					particles[i].Update();
				}
			}
...
```

### voice
instead of one particle, I want an array. Let's say 10 elements for now.

these need to be initailized, drawn, and updated, just like the old singular particle.

Because I want to make the particles fan out in a circle, I need to add some more math to `Vec2`

### scene
src/MrV/Geometry/Vec2
```
...
		public static float DegreesToRadians(float degrees) => degrees * MathF.PI / 180;
		public static float RadiansToDegrees(float radians) => radians * 180 / MathF.PI;
		public static Vec2 ConvertRadians(float radians) => new Vec2(MathF.Cos(radians), MathF.Sin(radians));
		public static Vec2 ConvertDegrees(float degrees) => ConvertRadians(DegreesToRadians(degrees));
		public float NormalToDegrees() => RadiansToDegrees(NormalToRadians());
		public float NormalToRadians() => WrapRadian(MathF.Atan2(y, x));
		public static float WrapRadian(float radian) {
			while (radian > MathF.PI) { radian -= 2 * MathF.PI; }
			while (radian <= -MathF.PI) { radian += 2 * MathF.PI; }
			return radian;
		}
...
```

### voice
for the sake of conveniently using normal vectors instead of angles, I'll also add these methods to do conversions.

I'm doing conversions to both Radians and Degrees because the standard C# math library uses Radians based on Pi, even though most people learn angles based on 360 degrees per circle.
Doing angle math in pure radians does mean the computer will need to do less conversion math total.
	but if we only need the math done during initialization, being more efficient has almost no performance gain. It makes much more sense to use the more intuitive 360 degree format here.

### scene
run the code and see the particles expand out from their origin

### voice
this looks cool. I want to turn this into a particle explosion, which is a common and effective tool for making games feel very alive.
to do the explosion well, I need a random number generator.
C-sharp does provide a random number generator class, but I want something more convenient. I want a singleton that I can call statically.

## Rand Singleton

### scene
src/MrV/GameEngine/Rand.cs
```
namespace MrV.GameEngine {
	public class Rand {
		private static Rand _instance;
		public static Rand Instance => _instance != null ? _instance : _instance = new Rand();
		public uint Seed = 2463534242; // seed (must be non-zero)
		/// <summary>Xorshift32</summary>
		uint Next() {
			Seed ^= Seed << 13;
			Seed ^= Seed >> 17;
			Seed ^= Seed << 5;
			return Seed;
		}
		public float GetNumber() => (Instance.Next() & 0xffffff) / (float)(0xffffff);
		public float GetNumber(float min, float max) => (Instance.GetNumber() * (max - min)) + min;
		public static float Number => Instance.GetNumber();
		public static float Range(float min, float max) => Instance.GetNumber(min, max);
	}
}
```

### voice
This random number generator uses the XorShift32 algorithm to generate fast random numbers. It is a Pseudo-Random Number Generator, which Software develoepers call a PRNG.
PRNG systems create the illusion of randomness while being exactly reproducable as long as the same starting seed is used.
	this reproducability is extremely useful for simulation debugging.
It isn't a high-quality random number generator for statistically robust simulations, but it is very fast.
If you want a higher quality generator that is a little bit slower, look up the SplitMix32 algorithm.
Like the other singleton classes, this one has a separate static API for convenience, and can also be created as an instance for specific number sequences.

### scene
src/MrV/Time.cs
```
...
		public static long CurrentTimeMs => DateTimeOffset.Now.ToUnixTimeMilliseconds();
...
```

### voice
to seed our random number generator, we should add an extra static method to Time, so we can have every program use a unique starting point for the random numbers.

### scene
src/Program.cs
```
...
			Particle[] particles = new Particle[10];
			Rand.Instance.Seed = (uint)Time.CurrentTimeMs;
			for (int i = 0; i < particles.Length; ++i) {
				Vec2 direction = Vec2.ConvertDegrees(Rand.Number * 360);
				float speed = 5, rad = 3;
				particles[i] = new Particle(new Circle((10, 10), Rand.Number * rad), direction * (Rand.Number * speed), ConsoleColor.White);
			}
...
```
test the particles code

### voice
the particles spread out differently with each runtime. but they don't look like an explosion yet.

### scene
src/MrV/GameEngine/Particle.cs
```
...
	public class Particle {
		public Circle Circle;
		public ConsoleColor Color;
		public Vec2 Velocity;
		public bool Enabled;
		public float LifetimeMax, LifetimeCurrent;
		public float OriginalSize;
		public Particle(Circle circle, Vec2 velocity, ConsoleColor color, float lifetime) {
			Circle = circle;
			OriginalSize = circle.radius;
			Velocity = velocity;
			Color = color;
			Enabled = true;
			LifetimeMax = lifetime;
			LifetimeCurrent = 0;
		}
		public void Draw(GraphicsContext g) {
			if (!Enabled) { return; }
			g.DrawCircle(Circle, Color);
			//float speed = Velocity.Magnitude;
			//if (speed > 0) {
			//	Vec2 direction = Velocity / speed;
			//	Vec2 rayStart = Circle.center + direction * Circle.radius;
			//	g.DrawLine(rayStart, rayStart + Velocity, 0.5f, Color);
			//}
		}
		public void Update() {
			if (!Enabled) { return; }
			LifetimeCurrent += Time.DeltaTimeSec;
			if (LifetimeCurrent >= LifetimeMax) {
				Enabled = false;
				return;
			}
			Vec2 moveThisFrame = Velocity * Time.DeltaTimeSec;
			Circle.center += moveThisFrame;
		}
	}
...
```

### voice
I need to add variables to track lifetime, and an enable them to turn off when lifetime is exceeded.
`Update` and `Draw` should stop working when `Enabled` is false.

also, I want to stop drawing the direction lines. those were nice for debugging motion, but they don't help the explosion graphic.

### scene
src/Program.cs
```
...
				particles[i] = new Particle(new Circle((10, 10), Rand.Number * rad), direction * (Rand.Number * speed), ConsoleColor.White, Rand.Range(.25f, 1));
...
```

### voice
this looks more like an explosion, but it's hard to tell from just one run. we should be able to test this more easily

### scene
```
...
			KeyInput.Bind(' ', () => {
				for (int i = 0; i < particles.Length; ++i) {
					Vec2 direction = Vec2.ConvertDegrees(Rand.Number * 360);
					float speed = 5, rad = 3;
					particles[i] = new Particle(new Circle((10, 10), Rand.Number * rad), direction * (Rand.Number * speed), ConsoleColor.White, Rand.Range(.25f, 1));
				}
			}, "explosion");
...
```

## Reuse Particles

### voice
This works, but we don't want to re-create the particles each time we press a key.
the `new` keyword prompts memory allocation, which is one of the most time consuming basic things the computer does.
to be clear, this program is not suffering very much from this allocation. this is a very small amount of memory.
But if we want to scale this explosion up to hundreds or thousands of circles, using `new` like this will become a problem. we want to avoice allocation as much as possible.

### scene
src/MrV/GameEngine/Particle.cs
```
...
		public Particle(Circle circle, Vec2 velocity, ConsoleColor color, float lifetime) {
			Init(circle, velocity, color, lifetime);
		}
		public void Init(Circle circle, Vec2 velocity, ConsoleColor color, float lifetime) {
			Circle = circle;
			OriginalSize = circle.radius;
			Velocity = velocity;
			Color = color;
			Enabled = true;
			LifetimeMax = lifetime;
			LifetimeCurrent = 0;
		}
...
```

### voice
we want an initialization function that re-purposes the existing particle
the need for this sort of re-initialization would be clearer in a language like C, where memory needs to be manually reclaimed.

### scene
src/Program.cs
```
...
			KeyInput.Bind(' ', () => {
				for (int i = 0; i < particles.Length; ++i) {
					Vec2 direction = Vec2.ConvertDegrees(Rand.Number * 360);
					float speed = 5, rad = 3;
					particles[i].Init(new Circle((10, 10), Rand.Number * rad), direction * Rand.Number * speed, ConsoleColor.White, Rand.Range(.25f, 1));
				}
			}, "explosion");
...
```

### voice
and we want to use the `Init` function instead of `new`.

it still doesn't look enough like an explosion for me. I want to see the particles change size as they move.

---

## FloatOverTime

### scene
src/MrV/GameEngine/FloatOverTime.cs
```
using System.Collections.Generic;

namespace MrV.GameEngine {
	public class FloatOverTime : ValueOverTime<float> {
		public static FloatOverTime GrowAndShrink = new FloatOverTime(new Frame<float>[] {
			new Frame<float>(0, 0), new Frame<float>(0.5f, 1), new Frame<float>(1, 0)
		});
		public FloatOverTime(IList<Frame<float>> curve) : base(curve) {}
		public override float Lerp(float percentageProgress, float start, float end) {
			float delta = end - start;
			return start + delta * percentageProgress;
		}
	}
	public abstract class ValueOverTime<T> {
		public struct Frame<T> {
			public float time;
			public T value;
			public Frame(float time, T value) {
				this.time = time;
				this.value = value;
			}
		}
		public bool WrapTime = false;
		public IList<Frame<T>> curve;
		public float StartTime => curve[0].time;
		public float EndTime => curve[curve.Count - 1].time;
		public abstract T Lerp(float t, T start, T end);
		public ValueOverTime(IList<Frame<T>> curve) {
			this.curve = curve;
		}
		public bool TryGetValue(float time, out T value) {
			if (curve == null || curve.Count == 0) {
				value = default;
				return false;
			}
			int index = Algorithm.BinarySearch(curve, time, frame => frame.time);
			if (index >= 0) {
				value = curve[index].value;
				return true;
			}
			index = ~index;
			if (index == 0) {
				value = curve[0].value;
				return true;
			}
			if (index >= curve.Count) {
				value = curve[curve.Count - 1].value;
				return true;
			}
			Frame<T> prev = curve[index - 1];
			Frame<T> next = curve[index];
			float normalizedTimeProgress = CalcProgressBetweenStartAndEnd(time, prev.time, next.time);
			value = Lerp(normalizedTimeProgress, prev.value, next.value);
			return true;
		}
		public static float CalcProgressBetweenStartAndEnd(float t, float start, float end) {
			float timeDelta = end - start;
			float timeProgress = t - start;
			return timeProgress / timeDelta;
		}
	}
}
```

### voice
this class moves a value along a path over time. math calculates a position at any point in time. we call this math interpolation.

It's a templated class because the idea of interpolating a value is useful for many different kinds of values.
It's going to be used for changing the radius of a particle over time.
it could also be used for changing a particle's color, position, or any concept that can be interpolated between.

Any implementation will need to implement a `Lerp` method, which explains how to interpolate between values of the used type.
A list of frames will define the curve of the value, which is the path that the value interpolates over time.
a convenient static instance that grows and then shrinks a value is included here, which we need for the explosion particle.

the `ValueOverTime` abstract class defines how the math works.
notably, this class interpolates a curve with sharp transitions from frame to frame.
the interpolation could be smoother with a spline, which is a clear opportunity for to improve this class later.
for now, this is sufficient for our simulation.

### scene
src/Program.cs
```
...
			}, "explosion");
			FloatOverTime growAndShrink = FloatOverTime.GrowAndShrink;
			while (running) {
...
```

```
...
				for (int i = 0; i < particles.Length; ++i) {
					particles[i].Update();
					float timeProgress = particles[i].LifetimeCurrent / particles[i].LifetimeMax;
					if (growAndShrink.TryGetValue(timeProgress, out float nextRadius)) {
						particles[i].Circle.radius = nextRadiusPercentage * particles[i].OriginalSize;
					}
				}
...
```

### voice
for a quick test, I'll just initialize the FloatOverTime object as normal game data
	and use the structure to modify particle radius directly in Update.

### scene
test

### voice
this actually looks pretty good now. but writing this much specific logic directly in Update feels bad to me.
I want a separate class that will handle all of the particle logic.
I want the class to generate and re-use particles easily and transparently as well.
That kind of object re-use is done with a data structure called an ObjectPool.

---

## ObjectPool

### scene
src/MrV/GameEngine/PolicyDrivenObjectPool.cs
```
using System;
using System.Collections.Generic;

namespace MrV.GameEngine {
	public class PolicyDrivenObjectPool<T> {
		private List<T> _allObjects = new List<T>();
		private int _freeObjectCount = 0;
		public Func<T> CreateObject;
		public Action<T> DestroyObject, CommissionObject, DecommissionObject;
		private HashSet<int> _delayedDecommission = new HashSet<int>();

		public int Count => _allObjects.Count - _freeObjectCount;
		public T this[int index] => index < Count
			? _allObjects[index] : throw new ArgumentOutOfRangeException();
		public PolicyDrivenObjectPool() { }
		public void Setup(Func<T> create, Action<T> commission = null,
			Action<T> decommission = null, Action<T> destroy = null) {
			CreateObject = create; CommissionObject = commission;
			DecommissionObject = decommission; DestroyObject = destroy;
		}
		public T Commission() {
			T freeObject = default;
			if (_freeObjectCount == 0) {
				freeObject = CreateObject.Invoke();
				_allObjects.Add(freeObject);
			} else {
				freeObject = _allObjects[_allObjects.Count - _freeObjectCount];
				--_freeObjectCount;
			}
			if (CommissionObject != null) { CommissionObject(freeObject); }
			return freeObject;
		}
		public void Decommission(T obj) => DecommissionAtIndex(_allObjects.IndexOf(obj));
		public void DecommissionAtIndex(int indexOfObject) {
			if (indexOfObject >= (_allObjects.Count - _freeObjectCount)) {
				throw new Exception($"trying to free object twice: {_allObjects[indexOfObject]}");
			}
			if (_delayedDecommission.Count > 0) {
				DecommissionDelayedAtIndex(indexOfObject);
				return;
			}
			T obj = _allObjects[indexOfObject];
			++_freeObjectCount;
			int beginningOfFreeList = _allObjects.Count - _freeObjectCount;
			_allObjects[indexOfObject] = _allObjects[beginningOfFreeList];
			_allObjects[beginningOfFreeList] = obj;
			if (DecommissionObject != null) { DecommissionObject.Invoke(obj); }
		}
		public void Clear() {
			for (int i = _allObjects.Count - _freeObjectCount - 1; i >= 0; --i) {
				Decommission(_allObjects[i]);
			}
		}
		public void Dispose() {
			Clear();
			if (DestroyObject != null) { ForEach(DestroyObject.Invoke); }
			_allObjects.Clear();
		}
		public void ForEach(Action<T> action) {
			for (int i = 0; i < _allObjects.Count; ++i) {
				action.Invoke(_allObjects[i]);
			}
		}
		public void DecommissionDelayedAtIndex(int indexOfObject) {
			_delayedDecommission.Add(indexOfObject);
		}
		public void ServiceDelayedDecommission() {
			if (_delayedDecommission.Count == 0) { return; }
			List<int> decommisionNow = new List<int>(_delayedDecommission);
			decommisionNow.Sort();
			_delayedDecommission.Clear();
			for (int i = decommisionNow.Count - 1; i >= 0; --i) {
				DecommissionAtIndex(decommisionNow[i]);
			}
		}
	}
}
```

### voice
we can use this object pool to cache memory for anything that we create and destroy a lot of.
it could be particles, bullets, enemies, pickups, or really anything.

the idea of this class is that a list of objects has some unused objects that can be reused later.
	objects at the end of the list are considred unused, or decommisioned.
the user must define some policies: how to create objects, how to reuse them, how to mark them as unused, and how to clean them up later.
importantly this class handles deferred cleanup
	this ObjectPool changes the order of objects in the list when they are disposed,
	so special care needs to be taken if an object is `Decommission`ed while processing the object pool list in a for loop.
when the user wants to `Commission` an object,
	if there are no unused objects in the list, a new one is created, added to the list, and given to the user
	if there is an unused object, the one closest to the edge of free objects is given to the user, and that edge is moved up.
when the user wants to `Decommission` an object,
	this code checks to make sure that it isn't decommissioning an already decommissioned object
	and it also protects against mixing deferred and immediate decomissioning
	then the object to decommission switches places in the list with the last commissioned object
	then the boundary of decommissioned objects moves down to absorb that newly decomissioned object
if an object needs to be decommissioned, but can't be decomissioned right now (because the object pool is being iterated through)
	the index of the object to decommission is put into a set (which won't contain duplicate indexes)
	then during a later time, outside of the objectpool iteration, those objects to decommission are decommissioned. this decomissioning has to happen in reverse index order, because removing an index at the front would shift all indexes beyond it.
		we want the last objects to get pushed to the end first.

This class manages creation of objects in an automated way. Programmers call the function that creates each object is a Factory Method, and the Object Pool is the Factory.
This implementation could also be explained as a Strategy Pattern, with it's parameterized Commission and Decommission methods.

### scene
src/MrV/Program.cs
```
...
			}
			PolicyDrivenObjectPool<Particle> particlesPool = new PolicyDrivenObjectPool<Particle>();
			float particleSpeed = 5, particleRad = 3;
			particlesPool.Setup(
				() => new Particle(new Circle(default, 1), default, ConsoleColor.White, 1),
				p => {
					Vec2 direction = Vec2.ConvertDegrees(Rand.Number * 360);
					p.Init(new Circle((10, 10), Rand.Number * particleRad), direction * Rand.Number * particleSpeed,
						ConsoleColor.White, Rand.Range(.25f, 1));
				},
				p => p.enabled = false);
			Rand.Instance.Seed = (uint)Time.CurrentTimeMs;
			KeyInput.Bind(' ', () => {
				for (int i = 0; i < 10; ++i) {
					particlesPool.Commission();
				}
			}, "explosion");
			FloatOverTime growAndShrink = FloatOverTime.GrowAndShrink;
			while (running) {
...
```

### voice
lets replace the Particle array with the `PolicyDrivenObjectPool` of `Particle` objects.
we need to define how to create a basic particle
how to commission a new particle
and how to decommission a particle
we don't need to include how to destroy the particle, because our particle doesn't allocate any special resources

the "explosion" KeyInput Bind should change to commission 10 particles.
a nice side effect of using this new system is that we can create more than just 10 particles, which will help create more interesting tests.

### scene
src/MrV/Program.cs
```
...
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
				for (int i = 0; i < particlesPool.Count; ++i) {
					particlesPool[i].Draw(graphics);
				}
				graphics.PrintModifiedOnly();
...
```

### voice
the `Draw` code needs to change, to use the particle object pool instead of the old particles array

### scene
src/MrV/Program.cs
```
...
			void Update() {
				KeyInput.TriggerEvents();
				Tasks.Update();
				for (int i = 0; i < particlesPool.Count; ++i) {
					particlesPool[i].Update();
					float timeProgress = particlesPool[i].LifetimeCurrent / particlesPool[i].LifetimeMax;
					if (growAndShrink.TryGetValue(timeProgress, out float nextRadiusPercentage)) {
						particlesPool[i].Circle.radius = nextRadiusPercentage * particlesPool[i].OriginalSize;
					}
					if (timeProgress >= 1) {
						particlesPool.DecommissionDelayedAtIndex(i);
					}
				}
				particlesPool.ServiceDelayedDecommission();
			}
...
```

### voice
and the `Update` needs to change as well.
here we can see that particles could be decommissioned while the particles pool is being iterated through, if we aren't careful.
after the pool is iterated through, the particlesPool can decommission those deferred particles

### scene
test

### voice
this looks pretty great!
but there is too much particle-specific code in the main loop code for my taste.

the random range calulating code in particular could be more explicit.

---

## RangeF

### scene
src/MrV/GameEngine/RangF
```
namespace MrV.GameEngine {
	public class RangeF {
		public float Min, Max;
		public float Delta => Max - Min;
		public float Random => Min + Delta * Rand.Number;
		public RangeF(float min, float max) { Min = min; Max = max; }
		public static implicit operator RangeF((float min, float max) tuple) => new RangeF(tuple.min, tuple.max);
		public static implicit operator RangeF(float singleValue) => new RangeF(singleValue, singleValue);
	}
}
```

### voice
before I implement the ParticleSystem class, I want to implement a `RangeF` class, which is a distributable way of considering a random number in a range.

If I don't want a random range, I should just be able to use a single value as a constant.
This creates a range with no domain, which costs pointless multiplication and addition to calculate.
But because the extra math is done on initialization, optimizing it is not a high priority.

let's get to the particle system.

## ParticleSystem

### scene
src/MrV/GameEngine/ParticleSystem.cs
```
using MrV.Geometry;
using System;

namespace MrV.GameEngine {
	internal class ParticleSystem {
		public PolicyDrivenObjectPool<Particle> ParticlePool = new PolicyDrivenObjectPool<Particle>();
		public FloatOverTime SizeOverLifetime;
		public ConsoleColor Color = ConsoleColor.White;
		public Vec2 Position;
		public RangeF ParticleLifetime;
		public RangeF ParticleSize;
		public RangeF ParticleSpeed;
		public RangeF SpawnRadius;

		public ParticleSystem(RangeF particleLifetime, RangeF particleSize, RangeF particleSpeed,
		RangeF spawnRadius, ConsoleColor color, FloatOverTime sizeOverLifetime) {
			ParticlePool.Setup(CreateParticle, CommissionParticle, DecommissionParticle);
			ParticleLifetime = particleLifetime;
			ParticleSize = particleSize;
			ParticleSpeed = particleSpeed;
			SpawnRadius = spawnRadius;
			Color = color;
			SizeOverLifetime = sizeOverLifetime;
		}
		public Particle CreateParticle() => new Particle(new Circle(Position,0), default, Color, 0);
		public void CommissionParticle(Particle particle) {
			particle.Enabled = true;
			particle.LifetimeCurrent = 0;
			particle.OriginalSize = ParticleSize.Random;
			particle.Circle.radius = particle.OriginalSize * GetSizeAtTime(0);
			particle.Velocity = default;
			particle.Color = Color;
			particle.LifetimeMax = ParticleLifetime.Random;
			Vec2 direction = Vec2.ConvertDegrees(360 * Rand.Number);
			particle.Velocity = direction * ParticleSpeed.Random;
			particle.Circle.center = Position + direction * SpawnRadius.Random;
		}
		public void DecommissionParticle(Particle particle) {
			particle.Enabled = false;
		}
		public float GetSizeAtTime(float lifetimePercentage) {
			if (SizeOverLifetime == null
			|| !SizeOverLifetime.TryGetValue(lifetimePercentage, out float value)) { return 1; }
			return value;
		}
		public void Draw(GraphicsContext graphics) {
			for (int i = 0; i < ParticlePool.Count; ++i) {
				ParticlePool[i].Draw(graphics);
			}
		}
		public void Update() {
			for (int i = 0; i < ParticlePool.Count; ++i) {
				ParticlePool[i].Update();
				float timeProgress = ParticlePool[i].LifetimeCurrent / ParticlePool[i].LifetimeMax;
				ParticlePool[i].Circle.radius = GetSizeAtTime(timeProgress) * ParticlePool[i].OriginalSize;
				if (timeProgress >= 1) {
					ParticlePool.DecommissionDelayedAtIndex(i);
				}
			}
			ParticlePool.ServiceDelayedDecommission();
		}
		public void Emit(int count = 1) {
			for (int i = 0; i < count; ++i) {
				ParticlePool.Commission();
			}
		}
	}
}
```

### voice
This is a very specific particle system implementation for explosions.
	i won't be modifying this for the rest of the tutorial, so feel free to make it more generalized if you want.
notice that I'm using `RangeF` for the values that could be random between two values.
the `PolicyDrivenObjectPool`'s delegate methods are defined as member functions.
a `Draw` method handles drawing of all particles
and the particle system's `Update` handles logic related to the Particle.
	arguably, all Particle logic, including the movement from Velocity could be moved to this function. I'm leaving that design choice to any audience member willing to make it.

### scene
src/Program.cs
```
...
			}
			float particleSpeed = 5, particleRad = 2;
			ParticleSystem particles = new ParticleSystem((.25f, 1), (1, particleRad),
				(1, particleSpeed), 0.125f, ConsoleColor.White, FloatOverTime.GrowAndShrink);
			particles.Position = (10, 10);
			KeyInput.Bind(' ', () => particles.Emit(10), "explosion");
			while (running) {
...
```

```
...
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
				particles.Draw(graphics);
				graphics.PrintModifiedOnly();
...
```

```
...
			void Update() {
				KeyInput.TriggerEvents();
				Tasks.Update();
				particles.Update();
			}
...
```

### voice
initialization of the `Particle`s is much simpler now
so is drawing, and updating. all of the interesting logic related to the particle system is comfortably in the `ParticleSystem` class.

### scene
test the code, showing lots of particles exploding

### voice
I do want to make a special note here: this tutorial took me a long time to plan, edit, rewrite, record, and edit again.
	during that time, the `ParticleSystem` went through significant changes.
	developing a particle system is not as easy as this tutorial made it look.
	when I first developed the game, I made the particle system much later, bigger, and more robust, for different kinds of particles.
	after I realized that I really only use the explosion particle, I greatly simplified it.
		I also moved this implementation earlier in the tutorial, to really utilize rendering optimizations sooner.
	if you are having trouble getting the particle system working, know that there is nothing wrong with you. this was difficult.
	you are practicing hard programming right now. it is good practice when it's difficult. slow down, pay attention, and take a break if you need to.
	you don't need to feel like you have to rush through this. slow is smooth, and smooth is fast.

---

## Zoom In and Out

### scene
src/Program.cs
```
...
			KeyInput.Bind('-', () => graphics.ShapeScale *= 1.5f, "zoom out");
			KeyInput.Bind('=', () => graphics.ShapeScale /= 1.5f, "zoom in");
...
```
also run.

### voice
to look at particles closer, we should zoom in with our graphics context.
we can do that by simply modifying the `ShapeScale` member

but doing this doesn't zoom into the center of the screen, it zooms into the origin point. that isn't what we want.

we need some real math to make this zoom look good. 

### scene
src/MrV/CommandLine/DrawBuffer_geometry_.cs
```
...
	public partial class DrawBuffer {
		public static ConsoleColorPair[,] AntiAliasColorMap;
		private Vec2 _originOffsetULCorner;
		public Vec2 Offset { get => _originOffsetULCorner; set => _originOffsetULCorner = value; }
		public Vec2 Scale {
			get => ShapeScale;
			set {
				Vec2 center = GetCameraCenter();
				ShapeScale = value;
				SetCameraCenter(center);
			}
		}
		public Vec2 GetCameraCenter() {
			Vec2 cameraCenterPercentage = (0.5f, 0.5f);
			Vec2 cameraCenterOffset = Size.Scaled(cameraCenterPercentage);
			Vec2 scaledCameraOffset = cameraCenterOffset.Scaled(Scale);
			Vec2 position = Offset + scaledCameraOffset;
			return position;
		}
		public void SetCameraCenter(Vec2 position) {
			Vec2 cameraCenterPercentage = (0.5f, 0.5f);
			Vec2 cameraCenterOffset = Size.Scaled(cameraCenterPercentage);
			Vec2 scaledCameraOffset = cameraCenterOffset.Scaled(Scale);
			Vec2 screenAnchor = position - scaledCameraOffset;
			Offset = screenAnchor;
		}
		static DrawBuffer() {
...
```

```
...
		public void DrawShape(IsInsideShapeDelegate isInsideShape, Vec2 start, Vec2 end, ConsoleGlyph glyphToPrint) {
			Vec2 renderStart = start - _originOffsetULCorner;
			Vec2 renderEnd = end - _originOffsetULCorner;
			renderStart.InverseScale(ShapeScale);
...
```

```
...
							for (float sampleX = 0; sampleX < 1; sampleX += SuperSampleIncrement) {
								bool pointIsInside = isInsideShape(new Vec2((x + sampleX) * ShapeScale.x, (y + sampleY) * ShapeScale.y)
									+ _originOffsetULCorner);
								if (pointIsInside) {
...
```

### voice
everything is being drawn by `DrawShape`, so changes only need to be local to this funciton.
a new 2D vector member keeps track of the offset of the upper-left corner of the screen
A new `Scale` property modifies the `ShapeScale` value while keeping the center off the screen in the same place
the math for calculating the center position, and moving the offset to center on a new center position is very similar. you are welcome to make a common helper function for that math, I decided to just put the math in the same area of the source file.

the `DrawShape` function doesn't need to change that much. we need to offset the rendering rectangle by the camera's offset, and adjust the sampling point as well.

### scene
src/LowFiRockBlaster/Program.cs
```
...
			particles.Position = (10, 10);
			KeyInput.Bind(' ', () => particles.Emit(10), "explosion");
			float ScaleFactor = 1.25f;
			KeyInput.Bind('-', () => graphics.Scale *= ScaleFactor, "zoom out");
			KeyInput.Bind('=', () => graphics.Scale /= ScaleFactor, "zoom in");
			graphics.SetCameraCenter(particles.Position);
			while (running) {
...
```

### voice
In the game code, the plus and minus keys on the keyboard change the zoom by a common `Scale` factor.
also, we initialize the camera's center to focus on the particle system. I'm using the raw x/y coordinate in a tuple instead of a named variable because I expect that value will be removed soon.

### scene
test the code, zooming in and out

### voice
this particle effect looks better from far away. I think it's good enough for now.

before we move on to creating game elements, lets take a look at the game loop.

---

## Draw Lists

### scene
Program.cs

### voice

I think this code looks mostly ok. but I think there is too much unlabeled test drawing code.

a game engine should have a list of drawable elements, and draw those in a uniform way.
we should remove specific draw calls and replace them with general things to draw.

### scene
src/LowFiRockBlaster
```
...
			graphics.SetCameraCenter(particles.Position);

			List<Action<GraphicsContext>> drawPreProcessing = new List<Action<GraphicsContext>>();
			List<Action<GraphicsContext>> drawPostProcessing = new List<Action<GraphicsContext>>();
			void TestGraphics(GraphicsContext graphics) {
				graphics.DrawRectangle(0, 0, width, height, ConsoleGlyph.Default);
				graphics.DrawRectangle((2, 3), new Vec2(20, 15), ConsoleColor.Red);
				graphics.DrawRectangle(new AABB((10, 1), (15, 20)), ConsoleColor.Green);
				graphics.DrawCircle(position, radius, ConsoleColor.Blue);
				graphics.DrawPolygon(polygonShape, ConsoleColor.Yellow);
			}
			drawPreProcessing.Add(TestGraphics);
			drawPostProcessing.Add(particles.Draw);

			while (running) {
...
```

```
...
			void Draw() {
				Vec2 scale = (0.5f, 1);
				graphics.Clear();
				drawPreProcessing.ForEach(a => a.Invoke(graphics));
				// draw simulation elements here
				drawPostProcessing.ForEach(a => a.Invoke(graphics));
				graphics.PrintModifiedOnly();
				graphics.SwapBuffers();
				Console.SetCursorPosition(0, height);
			}
...
```

### voice
a game should have a clear way to draw specific things that are and are not part of the simulation, like User Interface and visual effects vs game objects.
we can refactor the existing test code into into pre and post processing effects as a test.

The simulation's elements, like the player, the player's projectiles, asteroids, and ammo pick ups will be drawable objects that populate a draw list.

## UML

### scene
UML diagram of IGameObject, IDrawable, UIText, MobieObject, MobielCircle, MobilePolygon
https://lucid.app/lucidchart/ec14ab7a-a936-4356-bb0e-0326d2a5e45e/edit?viewport_loc=-340%2C-125%2C2514%2C1365%2CHWEp-vi-RSFO&invitationId=inv_571bd2ad-b5a4-4065-9b11-780a61085d7b

### voice
The draw list is one of the lists that the game engine will service every frame. All drawable objects will implement the `IDrawable` interface, as seen in this diagram.

UML diagramming is useful to clearly communicate system architecture.
	like the design document, it helps explain the concept and goals of a system.
	also, it becomes less important to make it detailed as a programmer reading it becomes more skilled, and can make some assumptions from experience.

### scene
still screenshot of the game screen, with labels for the asteroids of different size, player, player's projectiles, and ammo pickup.

### voice
My game will need moving circles to destroy, which are the conceptual asteroids. The rocks to blast in my lowfi rockblaster game.
The game's player will be a polygon shape visually distinct from the circles.
The player will shoot projectiles. I want to see spinning triangles for these projectiles, because I think that will look cool.
When the player destroys asteroids, they will break into smaller asteroids. the smallest asteroid will turn into an ammo pickup when destroyed.
I'll also need some user interface that stays static on the screen, to tell the user their score, ammo, and health.

I want to use Object Oriented Programming for this game design. Probably the most well known and well respected guideline for Object Oriented Design is the SOLID principles.

## S. O. L. I. D.

### scene
black background with white text
	S Single Responsiblity: each class does one thing.
	O Open-Closed: Classes are open to extension, closed to modification.
	L Liskov Substitution: Objects are substitutable for objects of their superclass.
	I Interface Segregation: Multiple interfaces are better than one general-purpose interface. 
	D Dependency Inversion: Use abstractions so classes don't rely on specific implementations.

### voice
Following these restrictions reduces cognitive load as the system grows in complexity, so I agree with SOLID principles in most situations.
However, I intentionally break the principles as practical and sometimes stylistic choice.
Choices about architecture design is one of those Invisible Wizard Problems.

### scene
highlight 'S Single Responsiblity'

### voice
Yes, each class should clearly do one thing. We want few mental burdens, and clear purpose at all times. However, my code already breaks the Single Responsiblity Principle:
Breaking this rule is common for game programming because of how dynamic the game design process is. Games are constantly trying to be more fun, which is actually moving target.
To rapidly find what is fun, code is inserted and removed often, to rapid-prototype new game mechanics.
	Intense work pressure can push developers to cut corners with good programming habits, and leave such messy code in place if it is working.
	The causes of this pressure are nuanced and interesting enough for another video, but I'll summarize my take on it:
		the games industry is a victim of it's own success, which makes most people expectations too high. That creates a lot of pressure to cut corners when it comes to disciplined development.
At least one of my classes is already doing things that could be argued as extending into new functionality that should be in a different class.
	`DrawBuffer` does more than simply manage a buffer. It has a partial class extension where scaled rendering code exists. The partial class extention was my own compromise on design quality, feeling pressure to release this tutorial sooner.
The new game classes I'll write will also do a lot. `MobileCircle` will be used for asteroids and for ammo pickups, with no additional subclassing.
	You'll see how I do that with metadata and lambda expressions.
In general, I break the Single Responsibility Principle on purpose, to keep file count low, so it's easier to read my code, and easier to think about.
Personally, I will continue to add functionality to a file if I can comfortably hold the additional functionality in my head along with the rest of the file.
	I will often refactor this kind of code later.

### scene
highlight 'O Open-Closed'

### voice
My code also breaks the Open-Closed Principle:
I do create small classes that should be easy to extend, in a way that looks like good Open-Closed design.
However, I write these classes with the specific expectation that you will modify the code yourself. And I wrote the code in a way that is easier for you to copy, and more effortful to extend.
My code is full of public members named as properties with the expectation that you will refactor that yourself in the future.
These implementations are explicitly not closed for modification: I want you to modify the code, and make your own design changes. Then it will become your code.
And crucially, I want you to make mistakes by making changes. Making those mistakes is how you learn. And I hope you are using this tutorial as a learning exercise.
If I were more serious about making it easy to extend and closed for editing, I would have turned all public members into properties, added the virtual keyword to many methods and properties, and maybe some XML documentation identifying what kind of extensions I expect.
Personally, I think the Open-Closed principle is better for mostly finished business software, where libraries should be shared with partner businesses, and performance loss from the language features enforcing this principle are negligible, because they aren't being run in a real-time loop.
	Game programming in particular is not a good place to make source code difficult to edit and slightly slower by default, and certainly not during prototype iteration.

### scene
highlight 'L Liskov Substitution'

### voice
My code will not strictly adhere to the Liskov Substitution Princicple:
I want to take advantage of good polymorphism, but strict adherence to Liskov Substitution creates inefficient code full of type verification.
One easy approach to maintain this principle is to avoid inheritance entirely, and create classes that extend functionality with lambda expressions and extra meta-data.
	I will be doing this myself, but to a limited degree, as you'll see with MobileObject.
	Duck Typing, which is an object design pattern that Python, JavaScript, and Lua use, is the logical extreme of Liskov Substitution.
		In those scripting languages, most complex objects are the same type, eacn being essentially a dictionary of variables and functions.
		In practice, this is a mess for code efficiency and compile-time error checking. This is also bad for game programming, because of performance costs.
	Unity, another C# game development environment, uses a Decorator pattern, which enables all GameObjects to substitute for each other, while having differentiated component elements.
		This often requires type verification at runtime when specialized functionality is needed by code, but not always. That's worth discussion in another video about Unity specifically.

### scene
highlight 'I Interface Segregation'

### voice
My code will break the Interface Segregation Principle:
I will be making interfaces, becase doing so is good practice for Object Oriented Programming.
However, I'm not going to make fine-grained Interface separations. I won't need them in practice, and writing them will the increase complexity of this tutorial for little gain.
For example, it is possible that not all GameObjects will need a position. But I don't want an additional IHasPosition interface.
	If you want to make the code adhere to Interface Segregation, you are welcome to implement the extra interfaces yourself.

### scene
highlight 'D Dependency Inversion'

### voice
My code already uses Singletons, which is a gross violation of the Dependency Inversion Principle.
	To be clear, I hate the fact that my code relies on singletons. Singletons make a brittle design, and it limit future functionality.
		For example, I have a `Time` singleton, and if I want to add a localized-time-travel, like a region of space that makes physics work backwards, that would require multiple Time objects, which is impossible to do with a Singleton for time.
	I did write every singleton class to be able to substitute it's the static instance for another one, to help enable prototyping some interesting designs later.
		If I was serious about those specialized designs, I would not use singletons.
		Singletons, like global variables, create hidden dependencies that are difficult to extract reason about, and share across projects. When multi-threading gets involved, these kinds of bugs can become unsolvable nightmares.
To be clear, I wrote singletons because I accept them as well understood utilities, as extensions of the programming environment more than program features, and I expect to enforce single-threading for all game logic.
I explicitly accept the design cost of Singletons, as many other game engines do (like Unity).

If I did want to create code that didn't use a `Time` singleton, designed with Dependency Inversion in mind, I would:
	Create an interface for `Time` objects, like `ITime`
	Give every instance of an object that uses `Time` a reference to a `ITime` object
	Populate that `ITime` object reference on initialization, probably in a factory method
	I would also have properties to query and change the Time instance at runtime.

Lets get to implementing some interfaces.

---

## Interfaces

### scene
src/MrV/GameEngine/IDrawable.cs
```
using MrV.CommandLine;

namespace MrV.GameEngine {
	public interface IDrawable {
		public bool IsVisible { get; set; }
		public void Draw(GraphicsContext canvas);
	}
}
```

### voice
the IDrawable interface in code is more complex than what was shown in the UML diagram.
Again, the UML diagram helps describe architecture, it doesn't need to be detailed enough to run as executable code. As an experienced programmer, I was able to make assumptions about what should be in the interface.

### scene
src/MrV/GameEngine/IGameObject.cs
```
using MrV.Geometry;

namespace MrV.GameEngine {
	public interface IGameObject : IDrawable {
		public string Name { get; set; }
		public Vec2 Position { get; set; }
		public bool IsActive { get; set; }
		public void Update();
	}
}
```

### scene
This engine uses an interface for all objects called IGameObject. I expect complex objects to inherit this interface, adding their own specialized complexity.
Notably, Unity has a concrete class called GameObject, and objects of GameObject are extended using a decorator pattern instead of inheritance.
	The decorator pattern has runtime overhead that we avoid in this game's implemenation.
	That decorator design makes more sense for Unity, which is a very dynamic general-purpose engine. This engine will be much less dynamic, but conceivably more performant for it's design.

---

## TODO

TODO: Code is done, just need the script written
* show MobileObject code and explain
* show MobileCircle code and explain
* show MobilePolygon code and explain
* create a player MobilePolygon in the Program.cs file
* create a PlayerController class that changes the velocity of a target MobileObject
  - special note be careful about class design! if you do it while you are tired, you can easily regret make it rotate
* make it move with WASD, rotate CW and CW with QE
* add code for an asteroid
* add a MemoryPool for the asteroids
* create a MemoryPool for bullets, triangle shaped MobilePolygons
* add code to shoot the bullets
* create naive collision detection that just checks circles, checking all objects in O(n^2) algorithm
* add code to destroy asteroids when they are hit by a bullet. the bullet should also go away after collision
* code complexity is about to get crazy, and we'll need debugging. create a Log class with Assert.
* add Welzl's algorithm for min bounding circle
* add sub-collision circles to the Polygon, for finer collision detection
* test with asteroids spawning around the player
* give the asteroids velocity
* test player being destroyed by asteroid collision
* test asteroids being destroied by projectile
* test asteroids spawning X number of smaller asteroids
* implement ammo variable for player
* implement ammo pickup
* increase asteroid count to 1000, and notice the frame rate change
* set asteroid count back down to like 100
* create a SpacePartition class, with drawing functionality for testing
* test SpacePartition
* show multiple partition cells capturing the same collision
* add code to collapse duplicate collisions
* create code for nested SpacePartition
* increase asteroid count to 1000, and notice the frame rate change

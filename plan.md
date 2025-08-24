# LowFiCollider

## Intro and Hello World
* intro
* preview
* start project

## initial code
* hello world
* draw rectangle
* details of rectangle drawing in the command line

## introduce Vec2 math structs
* Vec2
* Circle
* AABB
* Polygon

## writing objects and Drawing with struct arguments
* DrawRectangle -> DrawRectangle
  - two methods for convenience
  - code is very cheap, thinking is expensive
  - premature optimization is the root of all evil
* DrawCircle

## Basic GameLoop
* test circle with variable controls in a gameloop

## Gameloop refactor
* refactor gameloop
* test performance with Timer

## Timing
* implement Time class
* calculate cost of calculation
* throttle code

## Shape drawing
* create the draw method with delegate

## Scaled shape drawing
* add scale member, modify drawing code

## Automatic testing with queued events

## Value Over Time To Test Circle Radius
* ValueOverTime class
* BinarySearchWithInsertionPoint
* test circle

## Vec2 Lerp
* Lerp math for 2D Vectors

## DrawLine
* determine if width or height of line is longer
* iterate over the longer dimension, use Lerp to put down points

# abstract class and Templates
* template ValueOverTime
* create abstract method
* implement two child classes
* move circle position over time

## Proper Scale

## Draw buffering
* graphics context class with draw buffer

## Task scheduler

## KeyInput Class

## Dirty Console Glyph

## Anti Aliasing With Super Sampling Using ASCII

## Draw Line antialiased

## Antialiasing With Console Color
* ConsoleColorPair
* ConsoleGlyph

## Color Gradient

## Write Text At

## Polygon
* Polygon class
* Draw polygon
* Draw rotated polygon

## Testing
* Proof Of Life
* This is not Test Driven Development, this is Game Development.

---

## Particle Class, to show moving circle
* velocity
* particle update

## Basic Particle Explosion
* create multiple particles
* do it again, dynamically, in a for-loop

## Particle Death Over Time
* lifetime variables
* Update
* disabled flag

## ObjectPool

## Particle Change Size Over Time With Upadte
* use ValueOverTime class

## RangeF and Rand Class For Particle Randomness

## Particle System class

## Use KeyInput For Different Particle Triggers

## Game Engine Refactor
* IDrawable list
* IGameObject list
* Key input triggers set during init

---

## Circle
* Circle class
* MobileObject class
* Create multiple asteroids using ObjectPool

## Basic Mobile Polygon
* MobilePolygon class
* Welzl's Algorithm

## CharacterController
* Character Controller class

## Shooting Projectiles
* triangle mobile polygon
* spin
* create using ObjectPool

## Basic Collision
* ICollidable interface
* projectile and mobile circle

## Better Collision
* multi-stage: detect, resolve later.

## Limits of Naive Collision
* show lag From O(n^2) Collision Detection

## Quad Tree
* draw the tree

## Cell Space Partition
* draw the tree

## Circle Collision instead of point
* draw the membership
* note circles that belong to multiple cells

## Debugging
* test collision
* discover duplicate collisions

## Resolve Duplicate Collisions
* use tests to identify and verify issue
* break apart collision detection

## ObjectPool for Space Partition
* add shared memory space used by SpacePartition

## Next Steps

---

## 0 Basic MCP Connection in C#

## 1 MCP Functionality

## 2 Connection To LLM

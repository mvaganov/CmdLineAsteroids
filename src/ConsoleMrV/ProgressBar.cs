using MathMrV;
using System.Collections.Generic;

namespace ConsoleMrV {
	public class ProgressBar {
		public float progress;
		public ConsoleGlyph back;
		public ConsoleGlyph fore;
		public Vec2 start;
		public Vec2 end;
		public List<float> specialMarkers = new List<float>();
		public ProgressBar(Vec2 start, Vec2 end) {
			this.start = start;
			this.end = end;
			back = new ConsoleGlyph('.', System.ConsoleColor.Black, System.ConsoleColor.DarkBlue);
			fore = new ConsoleGlyph('.', System.ConsoleColor.Green, System.ConsoleColor.DarkGreen);
		}
		public void Draw(CommandLineCanvas canvas) {
			Vec2 delta = end - start;
			int width = (int)(delta.X > delta.Y ? delta.X : delta.Y);
			Vec2 increment = delta / width;
			Vec2 cursor = start;
			int progressInt = (int)(progress * width);
			int[] specialPositions = new int[specialMarkers.Count];
			for (int i = 0; i < specialMarkers.Count; i++) {
				specialPositions[i] = (int)(specialMarkers[i] * width);
			}
			int specialIndex = 0;
			ConsoleGlyph glyph = back;
			for(int i = 0; i < width; ++i) {
				if (i < progressInt) {
					glyph = fore;
				} else {
					glyph = back;
				}
				if (specialIndex < specialPositions.Length && i >= specialPositions[specialIndex]) {
					glyph.Letter = '|';
					++specialIndex;
				}
				canvas.WriteAt(glyph, (int)cursor.X, (int)cursor.Y);
				//canvas.WriteAt(glyph.Letter, (int)cursor.x, (int)cursor.y);
				cursor += increment;
			}
		}
	}
}

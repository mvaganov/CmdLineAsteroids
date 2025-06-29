#define KeepLogs
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConsoleMrV {
	public static class Log {
		public static Code visibility = Code.Verbose;
		public static Visibility[] Visibilities;
		private static string VisibilityEscapeCodes;
		public static string LogFile = "log.txt";
		public static bool IdentifySourceCode = true;
		public static bool AssertThrowsException = true;
		public enum Code {
			/// <summary>
			/// Never print
			/// </summary>
			None = 0,
			/// <summary>
			/// failed asserts: something has gone very wrong.
			/// </summary>
			Critical = 1,
			/// <summary>
			/// Log.e: when things are about to go very wrong
			/// </summary>
			Error = 2,
			/// <summary>
			/// Log.w: when things could go wrong
			/// </summary>
			Warning = 3,
			/// <summary>
			/// Log.i: helpful markers through a process
			/// </summary>
			Info = 4,
			/// <summary>
			/// Log.d: additional info for a programmer debugging something, likely temporary
			/// </summary>
			Debug = 5,
			/// <summary>
			/// Log.v: even more additional info for a programmer debugging. debug messages that are useful enough to keep forever, but not for everyone
			/// </summary>
			Verbose = 6,
			/// <summary>
			/// successful asserts: if you're using Asserts well, prepare for so much spam.
			/// </summary>
			SuccessfulAsserts = 8
		}
		public class Visibility {
			public Code code;
			public ConsoleColorPair color;
			public char escapeCode;
			public Visibility(Code code, ConsoleColorPair color, char escapeCode) {
				this.code = code;
				this.color = color;
				this.escapeCode = escapeCode;
			}
		}
		static Log() {
			GenerateVisiblities();
			File.WriteAllText(LogFile, "");
		}
		public static char UnhighlightChar => Visibilities[0].escapeCode;
		public static string Unhighlight => Visibilities[0].escapeCode.ToString();
		public static void GenerateVisiblities() {
			Visibilities = new Visibility[] {
				new Visibility(Code.None, ConsoleColorPair.Default, '\x01'),
				new Visibility(Code.Critical, ConsoleColorPair.Default + ConsoleColor.Magenta, '\x02'),
				new Visibility(Code.Error, ConsoleColorPair.Default + ConsoleColor.Red, '\x03'),
				new Visibility(Code.Warning, ConsoleColorPair.Default + ConsoleColor.Yellow, '\x04'),
				new Visibility(Code.Info, ConsoleColorPair.Default + ConsoleColor.White, '\x05'),
				new Visibility(Code.Debug, ConsoleColorPair.Default + ConsoleColor.Gray, '\x06'),
				new Visibility(Code.Verbose, ConsoleColorPair.Default + ConsoleColor.DarkGray, '\x0b'),
				new Visibility(Code.SuccessfulAsserts, ConsoleColorPair.Default + ConsoleColor.DarkMagenta, '\x0c'),
			};
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < Visibilities.Length; i++) {
				sb.Append(Visibilities[i].escapeCode);
			}
			VisibilityEscapeCodes = sb.ToString();
		}
		public static void PrintWithVisibility(string msg, Code minimumVisibility, bool endline)
			=> PrintWithVisibilityInternal(msg, minimumVisibility, endline);
		private static void PrintWithVisibilityInternal(string msg, Code visibility, bool endline) {
#if KeepLogs
			if (Log.visibility < visibility) { return; }
			if (IdentifySourceCode) {
				ShowSourceCodeMetaData(visibility);
			}
			ForcePrintWithVisibility(msg, visibility, endline);
#endif
		}
		public static void e(string msg) => PrintWithVisibilityInternal(msg, Code.Error, true);
		public static void w(string msg) => PrintWithVisibilityInternal(msg, Code.Warning, true);
		public static void i(string msg) => PrintWithVisibilityInternal(msg, Code.Info, true);
		public static void d(string msg) => PrintWithVisibilityInternal(msg, Code.Debug, true);
		public static void v(string msg) => PrintWithVisibilityInternal(msg, Code.Verbose, true);
		public static void Assert(bool condition, string message) {
			if (condition) {
				PrintWithVisibilityInternal(message, Code.SuccessfulAsserts, true);
			} else {
				PrintWithVisibilityInternal(message, Code.Critical, true);
				// TODO print additional stack info
				if (AssertThrowsException) {
					throw new Exception(message);
				}
			}
		}
		public static void ForcePrintWithVisibility(string message, Code visiblity, bool endLine) {
			ConsoleColorPair oldColor = ConsoleColorPair.Current;
			Visibilities[(int)visiblity].color.Apply();
			PrintWithColorEscape(message);
			oldColor.Apply();
			if (endLine) {
				Write("\n");
			}
		}
		public static void PrintWithColorEscape(string message) {
			bool escapeCodeUsed = false;
			int start = 0;
			for (int i = 0; i < message.Length; i++) {
				char c = message[i];
				int escapeCode = VisibilityEscapeCodes.IndexOf(c);
				if (escapeCode >= 0) {
					escapeCodeUsed = true;
					string text = message.Substring(start, i - start);
					Write(text);
					Visibilities[escapeCode].color.Apply();
					start = i + 1;
				}
			}
			if (!escapeCodeUsed) {
				Write(message);
			} else {
				Write(message.Substring(start, message.Length - start));
			}
		}
		public static void Print(string message) {
			ConsoleColorPair oldColor = ConsoleColorPair.Current;
			PrintWithColorEscape(message);
			oldColor.Apply();
		}
		public static string StackPosition(int framesBack = 1, bool includeFullPath = false) {
			string currentFile = new StackTrace(true).GetFrame(framesBack).GetFileName();
			if (!includeFullPath) {
				int lastFolder = currentFile.LastIndexOf('/');
				if (lastFolder < 0) {
					lastFolder = currentFile.LastIndexOf('\\');
				}
				lastFolder += 1;
				int fileSuffix = currentFile.LastIndexOf('.');
				if (fileSuffix < 0) {
					fileSuffix = currentFile.Length;
				}
				int suffixSize = currentFile.Length - fileSuffix;
				currentFile = currentFile.Substring(lastFolder, currentFile.Length - lastFolder - suffixSize);
			}
			int currentLine = new StackTrace(true).GetFrame(framesBack).GetFileLineNumber();
			return $"{currentFile}:{currentLine}";
		}
		private static void ShowSourceCodeMetaData(Code visibility) {
			ConsoleColorPair oldColor = ConsoleColorPair.Current;
			Visibilities[(int)visibility].color.Invert().Apply();
			PrintWithColorEscape(Log.StackPosition(4));
			oldColor.Apply();
		}
		private static void Write(string text) {
			Console.Write(text);
			AppendToLog(text);
		}
		private static void AppendToLog(string text) {
			if (string.IsNullOrEmpty(LogFile)) {
				return;
			}
			File.AppendAllText(LogFile, text);
		}
	}
}

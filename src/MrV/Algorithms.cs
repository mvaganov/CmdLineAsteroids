using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MrV {
	public static partial class Algorithms {
		public static long current_milli_time() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		public static bool LessThanLong(long a, long b) => a < b;
		public static bool LessThanInt(int a, int b) => a < b;
		public static bool LessThanFloat(float a, float b) => a < b;
		public static bool LessThanDouble(double a, double b) => a < b;
		public static bool LessThanByte(byte a, byte b) => a < b;

		public static int binary_search_with_insertion_point<T>(IList<T> arr, long target, Func<T, long> getValue)
			=> binary_search_with_insertion_point(arr, target, getValue, LessThanLong);
		public static int binary_search_with_insertion_point<T>(IList<T> arr, int target, Func<T, int> getValue)
			=> binary_search_with_insertion_point(arr, target, getValue, LessThanInt);
		public static int binary_search_with_insertion_point<T>(IList<T> arr, float target, Func<T, float> getValue)
			=> binary_search_with_insertion_point(arr, target, getValue, LessThanFloat);
		public static int binary_search_with_insertion_point<T>(IList<T> arr, double target, Func<T, double> getValue)
			=> binary_search_with_insertion_point(arr, target, getValue, LessThanDouble);
		public static int binary_search_with_insertion_point<T>(IList<T> arr, byte target, Func<T, byte> getValue)
			=> binary_search_with_insertion_point(arr, target, getValue, LessThanByte);

		public static int binary_search_with_insertion_point<T,V>(
		IList<T> arr, V target, Func<T, V> getValue, Func<V,V,bool> lessThan) {
			int low = 0, high = arr.Count - 1;
			while (low <= high) {
				int mid = low + (high - low);
				V value = getValue.Invoke(arr[mid]);
				if (value.Equals(target)) {
					return mid;
				} else if (lessThan.Invoke(value, target)) {
					low = mid + 1;
				} else {
					high = mid - 1;
				}
			}
			return ~low;
		}

		public static string ConvertEscapeCodes(string str) {
			StringBuilder sb = new StringBuilder();
			int start = 0, end;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				if (c == '\\' && i < str.Length - 1) {
					end = i;
					char next = str[++i];
					switch (next) {
						case 'a': c = '\a'; break;
						case 'b': c = '\b'; break;
						case 't': c = '\t'; break;
						case 'n': c = '\n'; break;
						case 'r': c = '\r'; break;
						case '\\': c = '\\'; break;
						case '\'': c = '\''; break;
						case '\"': c = '\"'; break;
						case 'x': throw new NotSupportedException();
					}
					sb.Append(str.Substring(start, end - start));
					sb.Append(c);
					start = i + 1;
				}
			}
			end = str.Length;
			sb.Append(str.Substring(start, end - start));
			return sb.ToString();
		}
		public static string[] Concat(string[] a, string b) {
			string[] result = new string[a.Length + 1];
			Array.Copy(a, result, a.Length);
			result[a.Length] = b;
			return result;
		}
		public static void AppendToFile(string path, string data) {
			File.AppendAllText(path, data);
		}
	}
}

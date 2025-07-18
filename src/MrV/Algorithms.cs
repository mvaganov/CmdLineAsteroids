using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MrV {
	public static partial class Algorithms {
		public static long current_milli_time() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		public static bool DelegateLessThanLong(long a, long b) => a < b;
		public static bool DelegateLessThanInt(int a, int b) => a < b;
		public static bool DelegateLessThanFloat(float a, float b) => a < b;
		public static bool DelegateLessThanDouble(double a, double b) => a < b;
		public static bool DelegateLessThanByte(byte a, byte b) => a < b;
		public static bool DelegateLessThanShort(short a, short b) => a < b;

		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, long target, Func<T, long> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanLong);
		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, int target, Func<T, int> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanInt);
		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, float target, Func<T, float> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanFloat);
		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, double target, Func<T, double> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanDouble);
		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, byte target, Func<T, byte> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanByte);
		public static int BinarySearchWithInsertionPoint<T>(IList<T> arr, short target, Func<T, short> getValue)
			=> BinarySearchWithInsertionPoint(arr, target, getValue, DelegateLessThanShort);

		public static int BinarySearchWithInsertionPoint<T,V>(
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

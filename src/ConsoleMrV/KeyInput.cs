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
			BindKeyResponse(new KeyResponseRecord<KeyType>(key, response, note));
		}
		public void BindKeyResponse(KeyResponseRecord<KeyType> keyResponseRecord) {
			if (!dispatchTable.TryGetValue(keyResponseRecord.Key, out List<KeyResponseRecord<KeyType>> responses)) {
				dispatchTable[keyResponseRecord.Key] = responses = new List<KeyResponseRecord<KeyType>>();
			}
			responses.Add(keyResponseRecord);
		}
		public void UnbindKeyResponse(KeyResponseRecord<KeyType> keyResponseRecord) {
			if (!dispatchTable.TryGetValue(keyResponseRecord.Key, out List<KeyResponseRecord<KeyType>> responses)) {
				return;
			}
			for(int i = responses.Count-1; i >= 0; --i) {
				if (responses[i].Note == keyResponseRecord.Note) {
					responses.RemoveAt(i);
				}
			}
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
		public void Add(Dispatcher<KeyType> child) {
			foreach(var kvp in child.dispatchTable) {
				kvp.Value.ForEach(BindKeyResponse);
			}
		}
		public void Remove(Dispatcher<KeyType> child) {
			foreach (var kvp in child.dispatchTable) {
				kvp.Value.ForEach(UnbindKeyResponse);
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
			foreach (var kvp in dispatchTable) {
				string listKeyResponses = string.Join(", ", kvp.Value.ConvertAll(r => r.Note));
				sb.Append($"'{kvp.Key}': {listKeyResponses}\n");
			}
			return sb.ToString();
		}
	}
}

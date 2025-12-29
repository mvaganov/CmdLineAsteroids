using System;
using System.Collections;
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
		public static implicit operator KeyResponseRecord<KeyType>((KeyType key, KeyResponse response, string note) tuple)
			=> new KeyResponseRecord<KeyType>(tuple.key, tuple.response, tuple.note);
	}
	public class Dispatcher<KeyType> : IEnumerable<KeyResponseRecord<KeyType>> {
		public List<KeyType> EventsToProcess = new List<KeyType>();
		public Dictionary<KeyType, List<KeyResponseRecord<KeyType>>> DispatchTable
			= new Dictionary<KeyType, List<KeyResponseRecord<KeyType>>>();
		public Dispatcher() { }
		public Dispatcher(IEnumerable<KeyResponseRecord<KeyType>> events) { BindKeyResponse(events); }
		public void BindKeyResponse(IEnumerable<KeyResponseRecord<KeyType>> records) {
			foreach (var record in records) { BindKeyResponse(record); }
		}
		public void UnbindKeyResponse(IEnumerable<KeyResponseRecord<KeyType>> records) {
			foreach (var record in records) { UnbindKeyResponse(record); }
		}
		public void BindKeyResponse(KeyType key, KeyResponse response, string note) {
			BindKeyResponse(new KeyResponseRecord<KeyType>(key, response, note));
		}
		public void BindKeyResponse(KeyResponseRecord<KeyType> record) {
			if (!DispatchTable.TryGetValue(record.Key, out List<KeyResponseRecord<KeyType>> responses)) {
				DispatchTable[record.Key] = responses = new List<KeyResponseRecord<KeyType>>();
			}
			responses.Add(record);
		}
		public void UnbindKeyResponse(KeyResponseRecord<KeyType> keyResponseRecord) {
			if (!DispatchTable.TryGetValue(keyResponseRecord.Key, out List<KeyResponseRecord<KeyType>> responses)) {
				return;
			}
			for(int i = responses.Count-1; i >= 0; --i) {
				if (responses[i].Note == keyResponseRecord.Note) {
					responses.RemoveAt(i);
				}
			}
		}
		public void AddEvent(KeyType key) => EventsToProcess.Add(key);
		public void ConsumeEvents() {
			if (EventsToProcess.Count == 0) { return; }
			List<KeyType> processNow = new List<KeyType>(EventsToProcess);
			EventsToProcess.Clear();
			for (int i = 0; i < processNow.Count; i++) {
				KeyType key = processNow[i];
				if (DispatchTable.TryGetValue(key, out List<KeyResponseRecord<KeyType>> responses)) {
					responses.ForEach(responseRecord => responseRecord.Response.Invoke());
				}
			}
		}
		public IEnumerator<KeyResponseRecord<KeyType>> GetEnumerator() {
			foreach(var eventsPerKey in DispatchTable) {
				foreach(var e in eventsPerKey.Value) {
					yield return e;
				}
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	public class KeyInput : Dispatcher<char> {
		private static KeyInput _instance;
		public static KeyInput Instance {
			get => _instance != null ? _instance : _instance = new KeyInput();
			set => _instance = value;
		}
		public KeyInput() { }
		public KeyInput(IEnumerable<KeyResponseRecord<char>> events) : base(events) {}
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
			foreach (var kvp in DispatchTable) {
				string listKeyResponses = string.Join(", ", kvp.Value.ConvertAll(r => r.Note));
				sb.Append($"'{kvp.Key}': {listKeyResponses}\n");
			}
			return sb.ToString();
		}
	}
}

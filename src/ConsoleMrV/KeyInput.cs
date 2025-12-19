using System;
using System.Collections.Generic;

namespace ConsoleMrV {
	public class KeyInput {
		public static KeyInput Instance;
		public List<char> Keys = new List<char>();
		public Dictionary<char, List<Action<KeyInput>>> keyBinding = new Dictionary<char, List<Action<KeyInput>>>();
		private List<List<Action<KeyInput>>> toExecuteThisFrame = new List<List<Action<KeyInput>>>();

		public KeyInput() {
			if (Instance == null) {
				Instance = this;
			}
		}
		public int Count => Keys.Count;
		public char this[int index] => GetChar(index);
		public char GetChar(int index) => Keys[index];
		public KeyInput get_instance() {
			if (Instance != null) {
				return Instance;
			}
			Instance = new KeyInput();
			return Instance;
		}
		public void Update() {
			UpdateKeyInput();
			TriggerKeyBinding();
		}
		public void UpdateKeyInput() {
			ClearKeys();
			while (Console.KeyAvailable) {
				ConsoleKeyInfo key = Console.ReadKey();
				Keys.Add(key.KeyChar);
			}
		}
		public void TriggerKeyBinding() {
			for(int i = 0; i < Keys.Count; i++) {
				if (keyBinding.TryGetValue(Keys[i], out List<Action<KeyInput>> actions)) {
					toExecuteThisFrame.Add(actions);
				}
			}
			toExecuteThisFrame.ForEach(actions => actions.ForEach(a => a.Invoke(this)));
			toExecuteThisFrame.Clear();
		}
		public void ClearKeys() { Keys.Clear(); }
		public bool HasKey(char keyChar) {
			return GetKeyIndex(keyChar) != -1;
		}
		public int GetKeyIndex(char keyChar) {
			for (int i = 0; i < Keys.Count; ++i) {
				if (Keys[i] == keyChar) {
					return i;
				}
			}
			return -1;
		}
		public List<Action<KeyInput>> BindKey(char key, Action<KeyInput> action) {
			if (!keyBinding.TryGetValue(key, out List<Action<KeyInput>> actions)) {
				actions = new List<Action<KeyInput>>();
				keyBinding[key] = actions;
			}
			actions.Add(action);
			return actions;
		}
		/// <param name="key"></param>
		/// <param name="action">if null, removes all actions bound to this key</param>
		/// <returns></returns>
		public int Unbind(char key, Action<object> action) {
			int removedCount = 0;
			if (!keyBinding.TryGetValue(key, out List<Action<KeyInput>> actions)) {
				return removedCount;
			}
			for(int i = actions.Count - 1; i >= 0; --i) {
				if (action != null && actions[i] != action) {
					continue;
				}
				actions.RemoveAt(i);
				removedCount += 1;
			}
			return removedCount;
		}
	}
}

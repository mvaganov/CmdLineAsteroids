using System;
using System.Collections.Generic;

namespace ConsoleMrV {
  public class KeyInput {
    public static KeyInput Instance;
    public List<char> keys = new List<char>();
    public Dictionary<char, List<Action<KeyInput>>> keyBinding = new Dictionary<char, List<Action<KeyInput>>>();
    private List<List<Action<KeyInput>>> toExecuteThisFrame = new List<List<Action<KeyInput>>>();

    public KeyInput() {
      if (Instance == null) {
        Instance = this;
      }
    }
    public int Count => keys.Count;
    public char this[int index] => GetChar(index);
    public char GetChar(int index) => this.keys[index];
    public KeyInput get_instance() {
      if (Instance != null) {
        return Instance;
      }
      Instance = new KeyInput();
      return Instance;
    }
    public void Update() {
      this.ClearKeys();
      while (Console.KeyAvailable) {
        ConsoleKeyInfo key = Console.ReadKey();
        this.keys.Add(key.KeyChar);
        if (this.keyBinding.TryGetValue(key.KeyChar, out List<Action<KeyInput>> actions)) {
          this.toExecuteThisFrame.Add(actions);
        }
      }
      this.toExecuteThisFrame.ForEach(actions => actions.ForEach(a => a.Invoke(this)));
      this.toExecuteThisFrame.Clear();
    }
    public void ClearKeys() { this.keys.Clear(); }
    public bool HasKey(char keyChar) {
      return GetKeyIndex(keyChar) != -1;
    }
    public int GetKeyIndex(char keyChar) {
      for (int i = 0; i < this.keys.Count; ++i) {
        if (this.keys[i] == keyChar) {
          return i;
        }
      }
      return -1;
    }
    public List<Action<KeyInput>> BindKey(char key, Action<KeyInput> action) {
      if (!this.keyBinding.TryGetValue(key, out List<Action<KeyInput>> actions)) {
        actions = new List<Action<KeyInput>>();
        this.keyBinding[key] = actions;
      }
      actions.Add(action);
      return actions;
    }
    /// <param name="key"></param>
    /// <param name="action">if null, removes all actions bound to this key</param>
    /// <returns></returns>
    public int Unbind(char key, Action<object> action) {
      int removedCount = 0;
      if (!this.keyBinding.TryGetValue(key, out List<Action<KeyInput>> actions)) {
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

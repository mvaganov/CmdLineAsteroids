using System;
using System.Collections.Generic;

namespace MrV {
	public class ActionQueue {
		public List<Action> actions = new List<Action>();
		private List<Action> toExecuteNow = new List<Action>();
		private static ActionQueue _instance;
		public static ActionQueue Instance => _instance != null ? _instance : _instance = new ActionQueue();
		public ActionQueue() { _instance = this; }
		private void SwapQueues() {
			List<Action> swap = actions;
			actions = toExecuteNow;
			toExecuteNow = swap;
		}
		public static void Update() { Instance.UpdateSelf(); }
		public void UpdateSelf() {
			SwapQueues();
			toExecuteNow.ForEach(action => action.Invoke());
			toExecuteNow.Clear();
		}
		public void Enqueue(Action action) {
			actions.Add(action);
		}
		public void EnqueueDelayed(float delay, Action action) {
			double soon = Time.TimeSeconds + delay;
			void DoThisSoon() {
				if (Time.TimeSeconds < soon) {
					Enqueue(DoThisSoon);
				} else {
					action.Invoke();
				}
			};
			Enqueue(DoThisSoon);
		}
	}
}

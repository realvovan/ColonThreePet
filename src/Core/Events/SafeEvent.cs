namespace ColonThreePet.Core.Events;

/// <summary>
/// An object that runs connected callbacks upon a specific occurance.
/// An exception inside a callback will not propagate and stop the program.
/// </summary>
/// <typeparam name="TDelegate">Signature of the callback</typeparam>
public abstract class SafeEventBase<TDelegate> where TDelegate : Delegate {
	private class Entry(TDelegate handler, bool once) {
		public readonly TDelegate Handler = handler;
		public readonly bool Once = once;
	}
	private class Subscription(SafeEventBase<TDelegate> o, TDelegate e) : IDisposable {
		public void Dispose() => o.Remove(e);
	}

	private readonly List<Entry> callbacks = [];
	private Entry[] snapshot = [];
	private bool isDirty = false;

	public IDisposable Subscribe(TDelegate handler, bool once = false) {
		this.isDirty = true;
		this.callbacks.Add(new Entry(handler, once));
		return new Subscription(this, handler);
	}

	public void Remove(TDelegate handler) {
		lock (this.callbacks) {
			bool found = false;
			int index;
			for (index = 0; index < this.callbacks.Count; index++) {
				if (this.callbacks[index].Handler == handler) {
					found = true;
					break;
				}
			}
			if (!found) return;
			this.isDirty = true;
			this.callbacks.RemoveAt(index);
		}
	}

	public void Clear() {
		this.callbacks.Clear();
		this.isDirty = true;
	}

	protected void invoke<TState>(TState state, Action<TDelegate, TState> call) {
		if (this.isDirty) {
			lock (this.snapshot) {
				this.snapshot = this.callbacks.ToArray();
				this.isDirty = false;
			}
		}
		foreach (var i in this.snapshot) {
			try {
				call(i.Handler, state);
			} catch (Exception e) {
				// Add handling idk
				Console.WriteLine(e.Message);
			}
			if (i.Once) this.Remove(i.Handler);
		}
	}


}

public sealed class SafeEvent : SafeEventBase<Action> {
	public void Invoke() => invoke(0, static (h, _) => h());
}

public sealed class SafeEvent<T> : SafeEventBase<Action<T>> {
	public void Invoke(T a) => invoke(a, static (h, s) => h(s));
}

// add more definitions as needed

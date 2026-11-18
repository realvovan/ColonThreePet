using ColonThreePet.Core.Interfaces;
using ColonThreePet.Core.Events;
using System.Runtime.CompilerServices;

namespace ColonThreePet.Core.Instances;

/// <summary>
/// The base class for all classes in the parent-child hiararchy.
/// </summary>
public abstract class Instance {
	// property fields
	private Instance? parent;
	private string name;
	//

	private bool isDestroyed = false;
	/// key: Instance.Name, value: bucket of instances
	private Dictionary<string,LinkedList<Instance>> children = new();

	/// <summary>
	/// Fires whenever any property gets assigned a new value.
	/// The name of the changed property is provided in <see cref="PropertyChangedEventArgs.PropertyName"/>
	/// </summary>
	public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;
	/// <summary>
	/// Fires after a descendant is either added or removed.
	/// Propagates to all descendants, not just the direct children.
	/// </summary>
	public event EventHandler<DescendantChangedArgs> DescendantsChanged = static (sender,args) => {
		// recursively propagate the event to all ancestors
		if (sender is Instance instance && instance.Parent is not null) {
			instance.Parent.DescendantsChanged.Invoke(instance.Parent,args);
		}
	};
	/// <summary>
	/// Fires when the <see cref="Instance.Parent"/> property of this object or any of its ancestors is changed.
	/// </summary>
	public event EventHandler<AncestryChangedArgs> AncestryChanged = static (sender,args) => {
		// recursively propagate the event to all descendants
		if (sender is Instance instance) {
			instance.ForEachChild(c => {
				c.AncestryChanged.Invoke(c,args);
			});
		}
	};

	public Instance this[string name] {
		get {
			if (this.children.TryGetValue(name, out var bucket)) {
				return bucket.First!.Value;
			}
			throw new KeyNotFoundException($"No child instance with name '{name}' found.");
		}
	}

	/// <summary>
	/// Determines the hierarchical parent of the <see cref="Instance"/>.
	/// </summary>
	public Instance Parent {
		get => this.parent!;
		set {
			if (this.isDestroyed) {
				throw new InvalidOperationException($"{this} is destroyed and cannot be parented.");
			}
			if (this.isParentLocked) {
				throw new InvalidOperationException($"Cannot change parent of {this} because it is locked");
			}
			if (parent is EngineInstance) {
				throw new InvalidOperationException("Parent cannot be the engine instance");
			}
			if (value is not null && this is IValidatesParent validator && !validator.isValidParent(value)) {
				throw new InvalidOperationException($"{this.GetType().Name} cannot be parents to {value.GetType().Name}");
			}
			this.forceSetParent(value);
		}
	}
	/// <summary>
	/// A non-unique identifier of the <see cref="Instance"/>
	/// </summary>
	public string Name {
		get => this.name;
		set => this.setPropertyAndNotify(ref this.name, value);
	}
	protected virtual bool isParentLocked => false;

	/// <summary>
	/// Called after the direnct parent of the <see cref="Instance"/> is changed.
	/// Child classes may override this method to implement some functionality instead of using <see cref="Instance.AncestryChanged"/>
	/// </summary>
	protected virtual void onParentChange(Instance? oldParent) { }

	/// <summary>
	/// Sets the <see cref="Instance.Parent"/> property to <c>null</c>, locks the <see cref="Instance.Parent"/> property,
	/// disconnects all events, and calls <c>Destroy()</c> on all children.
	/// </summary>
	public virtual void Destroy() {
		if (this.isDestroyed) return;
		this.isDestroyed = true;
		this.forceSetParent(null!);
		this.ForEachChild(c => c.Destroy());
		this.children.Clear();
		this.PropertyChanged = null;
		this.DescendantsChanged = null!;
		this.AncestryChanged = null!;
	}

	/// <summary>
	/// Returns the first child of the <see cref="Instance"/> with a given name, or null if no instance is found.
	/// </summary>
	/// <param name="name"><see cref="Instance.Name"/> to be searched for</param>
	public Instance? FindFirstChild(string name) {
		if (this.children.TryGetValue(name, out var bucket)) {
			return bucket.First?.Value;
		}
		return null;
	}

	/// <summary>
	/// Returns <c>true</c> if the <see cref="Instance"/> is an ancestor of the given descendant.
	/// </summary>
	public bool IsAncestorOf(Instance ancestor) {
		var parent = this.Parent;
		while (parent is not null) {
			if (parent == ancestor) return true;
			parent = parent.Parent;
		}
		return false;
	}

	/// <summary>
	/// Loops over all the children of the <see cref="Instance"/> and executes a provided <see cref="Action"/>
	/// </summary>
	public void ForEachChild(Action<Instance> action) {
		foreach (var bucket in this.children.Values) {
			var child = bucket.First;
			while (child is not null) {
				action(child.Value);
				child = child.Next;
			}
		}
	}

	/// <summary>
	/// Sets the parent of the <see cref="Instance"/> bypassing all validations.
	/// It is preferable to directly set <see cref="Instance.Parent"/> over this method
	/// </summary>
	internal void forceSetParent(Instance? parent) {
		// If the parent is the same, do nothing
		if (this.parent == parent) return;

		// Remove from old parent
		if (this.parent is not null && this.parent.children.TryGetValue(this.Name, out var bucket)) {
			bucket.Remove(this);
			this.parent.DescendantsChanged.Invoke(this.parent, new DescendantChangedArgs(this, DescendantChangedArgs.ChangeType.Removed));
			if (bucket.Count == 0) this.parent.children.Remove(this.Name); // Remove the bucket if it's empty
		}

		if (parent is not null) {
			if (parent.children.TryGetValue(this.Name,out bucket)) {
				bucket.AddLast(this);
			} else {
				bucket = new LinkedList<Instance>();
				bucket.AddLast(this);
				parent.children.Add(this.Name,bucket);
			}
		}

		var oldParent = this.parent;

		this.setPropertyAndNotify(ref this.parent, parent, nameof(Parent));
		this.onParentChange(oldParent);
		this.AncestryChanged.Invoke(this, new AncestryChangedArgs(this, parent!));
		parent?.DescendantsChanged.Invoke(parent, new DescendantChangedArgs(this, DescendantChangedArgs.ChangeType.Added));
	}

	protected void setPropertyAndNotify<T>(ref T property, T value, [CallerMemberName] string? propertyName = null) {
		if (Object.Equals(property, value)) return;
		property = value;
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName!));
	}
	// this exists in case you cannot use setPropertyAndNotify<T>(...)
	protected void notifyPropertyChange([CallerMemberName] string? propertyName = null) {
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName!));
	}

	public override string ToString() => $"[{this.GetType().Name}] {this.Name}";

	protected Instance() {
		this.name = this.GetType().Name;
	}
}

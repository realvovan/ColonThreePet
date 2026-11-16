using ColonThreePet.Core.Events;
using System.Windows;
using System.Windows.Controls;

namespace ColonThreePet.Core.Instances;

public abstract class RenderComponent : Instance {
	// property fields
	private bool isEnabled = true;
	private bool isActive = false;
	//

	public bool IsEnabled {
		get => this.isEnabled;
		set {
			this.setPropertyAndNotify(ref this.isEnabled, value);
			this.updateIsActive();
		}
	}
	public bool IsActive {
		get => this.isActive;
		private set => this.setPropertyAndNotify(ref this.isActive, value);
	}

	public abstract FrameworkElement GetDrawable();
	
	protected virtual void updateDrawable() {
		if (this.Parent is not WorldInstance parent) return;
		var drawable = this.GetDrawable();
		drawable.Width = parent.Size.X;
		drawable.Height = parent.Size.Y;
		Canvas.SetLeft(drawable,parent.Position.X);
		Canvas.SetTop(drawable,parent.Position.Y);
	}

	protected virtual void parentPropertyChangeSignal(object? sender,PropertyChangedEventArgs args) {
		switch (args.PropertyName) {
			case "Position":
				Engine.Workspace.DeferToRender(this, args.PropertyName, () => {
					var position = ((WorldInstance)sender!).Position;
					Canvas.SetLeft(this.GetDrawable(),position.X);
					Canvas.SetTop(this.GetDrawable(),position.Y);
				});
				break;
			case "Size":
				Engine.Workspace.DeferToRender(this, args.PropertyName, () => {
					var size = ((WorldInstance)sender!).Size;
					this.GetDrawable().Width = size.X;
					this.GetDrawable().Height = size.Y;
				});
				break;
		}
	}

	private void updateIsActive() {
		this.IsActive = this.IsEnabled && this.Parent is WorldInstance && this.IsAncestorOf(Engine.Workspace);
	}

	protected override void onParentChange(Instance? oldParent) {
		Engine.Workspace.DeferToRender(this, nameof(this.Parent), this.updateDrawable);
		if (oldParent is WorldInstance) oldParent.PropertyChanged -= parentPropertyChangeSignal;
		if (this.Parent is WorldInstance) this.Parent.PropertyChanged += parentPropertyChangeSignal;
	}

	protected RenderComponent() {
		this.AncestryChanged += (sender,args) => this.updateIsActive();
	}
}
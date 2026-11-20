using ColonThreePet.Core.Events;
using System.Windows;

namespace ColonThreePet.Core.Instances;

/// <summary>
/// Holds all the objects that are meant to be rendered onto the screen.
/// Objects not descending from it will not be rendered or physically interect with the scene.
/// </summary>
public sealed class Workspace : ServiceInstance {
	// property fields
	private double gravity = 500d, airDrag = 1.5d, bounceThreshold = 5d;
	//

	private bool isStarted = false;
	private readonly MainScene mainScene;
	private readonly Application application;
	private readonly Dictionary<(object, string), Action> onRenderActionsQueue = new();
	private readonly List<PhysicsComponent> physicsComponentsIndex = new();

	/// <inheritdoc cref="Application.Exit"/>
	public event EventHandler<ExitEventArgs>? ApplicationExit;

	public double Gravity {
		get => this.gravity;
		set => this.setPropertyAndNotify(ref this.gravity, value);
	}

	public double AirDrag {
		get => this.airDrag;
		set => this.setPropertyAndNotify(ref this.airDrag, value);
	}

	public double BounceThreshold {
		get => this.bounceThreshold;
		set => this.setPropertyAndNotify(ref this.bounceThreshold, value);
	}

	// technically isnt safe since you can cast it to a list
	// but i dont want to reallocate it on every read
	internal IReadOnlyList<PhysicsComponent> PhysicsComponentsIndex => this.physicsComponentsIndex;

	internal void start(bool disableWpf) {
		if (this.isStarted) throw new InvalidOperationException("Main scene has already been launched.");
		this.isStarted = true;
		Engine.GetService<RunService>().Render += dt => {
			lock (this.onRenderActionsQueue) {
				foreach (Action i in this.onRenderActionsQueue.Values) {
					i.Invoke();
				}
				this.onRenderActionsQueue.Clear();
			}
		};
		Engine.GetService<RunService>().Heartbeat += dt => {
			PhysicsComponent.stepWorld(dt);
		};

		if (!disableWpf) {
			this.application.MainWindow = this.mainScene;
			this.application.Run(this.mainScene);
		}
	}

	internal void stop() {
		this.application.Dispatcher.Invoke(this.application.Shutdown);
	}

	private void onDescendantsChanged(object? sender, DescendantChangedArgs args) {
		if (args.Descendant is RenderComponent renderable) {
			if (args.TypeOfChange == DescendantChangedArgs.ChangeType.Added) {
				renderable.PropertyChanged += this.onRenderablePropertyChanged;
				if (renderable.IsActive) this.DeferToRender(renderable, nameof(renderable.IsActive), () => this.safeAddDrawable(renderable.GetDrawable()));
			} else {
				renderable.PropertyChanged -= this.onRenderablePropertyChanged;
				this.DeferToRender(renderable, nameof(renderable.IsActive), () => this.mainScene.MainCanvas.Children.Remove(renderable.GetDrawable()));
			}
		} else if (args.Descendant is PhysicsComponent pc) {
			if (args.TypeOfChange == DescendantChangedArgs.ChangeType.Added) {
				this.physicsComponentsIndex.Add(pc);
			} else {
				this.physicsComponentsIndex.Remove(pc);
			}
		}
	}

	private void safeAddDrawable(UIElement drawable) {
		if (this.mainScene.MainCanvas.Children.Contains(drawable)) return;
		this.mainScene.MainCanvas.Children.Add(drawable);
	}

	private void onRenderablePropertyChanged(object? sender, PropertyChangedEventArgs args) {
		if (args.PropertyName != "IsActive") return;
		var renderable = (RenderComponent)sender!;
		if (renderable.IsActive) {
			this.DeferToRender(renderable, args.PropertyName, () => this.safeAddDrawable(renderable.GetDrawable()));
		} else {
			this.DeferToRender(renderable, args.PropertyName, () => this.mainScene.MainCanvas.Children.Remove(renderable.GetDrawable()));
		}
	}

	/// <summary>
	/// Defers the execution of an action to the next rendered frame.
	/// Ideal when you need to make changes to WPF's <see cref="UIElement"/> from a different thread.
	/// In most cases you do not need to use this method to make changes to <see cref="Instance"/> properties,
	/// unless you have a specific reason to do so.
	///<br/> When a new call with the same <paramref name="owner"/> and <paramref name="key"/> within the same frame is made,
	/// any older calls with this <paramref name="owner"/> and <paramref name="key"/> are ignored.
	/// </summary>
	/// <param name="owner">
	///		Owner of the defer request. Different owners may have requests with the same <paramref name="key"/>.
	///		If this parameter is omitted, the owner is set to <see cref="Workspace"/>
	/// </param>
	/// <param name="key">String key of the defer request</param>
	/// <param name="action">The action to be executed next frame during rendering</param>
	public void DeferToRender(object owner, string key, Action action) {
		this.onRenderActionsQueue[(owner, key)] = action;
	}
	/// <inheritdoc cref="Workspace.DeferToRender(object, string, Action)"/>
	public void DeferToRender(string key, Action action) {
		this.onRenderActionsQueue[(this, key)] = action;
	}

	public override void Destroy() {
		base.Destroy();
		this.ApplicationExit = null;
	}

	internal Workspace() : base() {
		this.mainScene = new MainScene();
		this.DescendantsChanged += onDescendantsChanged;
		this.application = new Application {
			ShutdownMode = ShutdownMode.OnMainWindowClose
		};
		this.application.Exit += (sender, args) => {
			this.ApplicationExit?.Invoke(sender, args);
		};
	}
}

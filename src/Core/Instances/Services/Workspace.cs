using ColonThreePet.Core.Events;
using System.Windows;

namespace ColonThreePet.Core.Instances;
public sealed class Workspace : ServiceInstance {
	private bool isStarted = false;
	private readonly MainScene mainScene;
	private readonly Application application;
	private readonly EventHandler<PropertyChangedEventArgs> onRenderablePropertyChanged;
	private readonly Dictionary<(object, string), Action> onRenderActionsQueue = new();
	internal void start() {
		if (this.isStarted) throw new InvalidOperationException("Main scene has already been launched.");
		this.isStarted = true;
		Engine.GetService<RunService>().Render += dt => {
			lock (this.onRenderActionsQueue) {
				foreach (KeyValuePair<(object, string), Action> kv in this.onRenderActionsQueue) {
					kv.Value.Invoke();
				}
				this.onRenderActionsQueue.Clear();
			}
		};
		this.application.Run(this.mainScene);
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
		}
	}

	private void safeAddDrawable(UIElement drawable) {
		if (this.mainScene.MainCanvas.Children.Contains(drawable)) return;
		this.mainScene.MainCanvas.Children.Add(drawable);
	}

	public void DeferToRender(object owner, string key, Action action) {
		this.onRenderActionsQueue[(owner, key)] = action;
	}
	public void DeferToRender(string key, Action action) {
		this.onRenderActionsQueue[(this, key)] = action;
	}

	internal Workspace() : base() {
		this.mainScene = new MainScene();
		this.DescendantsChanged += onDescendantsChanged;
		this.onRenderablePropertyChanged = (sender,args) => {
			if (args.PropertyName != "IsActive") return;
			var renderable = (RenderComponent)sender!;
			if (renderable.IsActive) {
				this.DeferToRender(renderable, args.PropertyName, () => this.safeAddDrawable(renderable.GetDrawable()));
			} else {
				this.DeferToRender(renderable, args.PropertyName, () => this.mainScene.MainCanvas.Children.Remove(renderable.GetDrawable()));
			}
		};
		this.application = new Application {
			ShutdownMode = ShutdownMode.OnMainWindowClose
		};
	}
}

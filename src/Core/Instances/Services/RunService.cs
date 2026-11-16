using System.Windows.Media;

namespace ColonThreePet.Core.Instances;

public class RunService : ServiceInstance {
	public event Action<double>? Heartbeat;
	public event Action<double>? Render;

	internal void start() {
		CompositionTarget.Rendering += onRender;
	}
	internal void stop() {
		CompositionTarget.Rendering -= onRender;
		this.Heartbeat = null;
		this.Render = null;
	}

	private void onRender(object? sender, EventArgs args) {
		// TODO calculate actual delta time here
		this.Render?.Invoke(0.16);
	}
}
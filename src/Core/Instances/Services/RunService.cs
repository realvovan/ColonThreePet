using ColonThreePet.Core.RunServiceUtils;
using System.Diagnostics;
using System.Windows.Media;

namespace ColonThreePet.Core.Instances;

/// <summary>
/// Encapsulates a method that runs every frame
/// </summary>
/// <param name="dt">Time between the previous and current frames (delta time)</param>
public delegate void Tick(double dt);

/// <summary>
/// Service responsible for runtime activies
/// </summary>
public sealed class RunService : ServiceInstance {
	private TimeSpan lastRenderTime;
	private Thread? physicsThread;
	private CancellationTokenSource? physicsCts;

	/// <summary>
	/// Fires after every physics simulation step
	/// </summary>
	public event Tick? Heartbeat;
	/// <summary>
	/// Fires just before objects are rendered
	/// </summary>
	public event Tick? Render;

	internal void start() {
		if (this.physicsThread is not null) return;
		CompositionTarget.Rendering += onRender;

		this.physicsCts = new CancellationTokenSource();
		this.physicsThread = new Thread(() => this.runPhysics(this.physicsCts.Token)) {
			Name = "Physics",
			IsBackground = true,
			Priority = ThreadPriority.AboveNormal
		};
		this.physicsThread.Start();
	}
	internal void stop() {
		if (this.physicsThread is null) return;
		CompositionTarget.Rendering -= onRender;

		this.physicsCts!.Cancel();
		this.physicsThread.Join();
		this.physicsCts.Dispose();
		this.physicsThread = null;
		this.physicsCts = null;

		this.Heartbeat = null;
		this.Render = null;
	}

	private void onRender(object? sender, EventArgs args) {
		var e = (RenderingEventArgs)args;

		// skips duplicate events
		//if (e.RenderingTime == lastRenderTime) return;
		// test if this makes a difference and then decide whether to keep this in the code

		double dt = (e.RenderingTime - lastRenderTime).TotalSeconds;
		lastRenderTime = e.RenderingTime;

		try {
			this.Render?.Invoke(dt);
		} catch (Exception ex) {
			// TODO add proper error handling
			Console.WriteLine(ex.Message);
		}
	}

	private void runPhysics(CancellationToken ct) {
		using var timer = new HighResTimer();
		var sw = Stopwatch.StartNew();
		double next = sw.Elapsed.TotalSeconds;

		double loop() {
			const double STEP = 1d / 60d;

			try {
				this.Heartbeat?.Invoke(STEP);
			} catch (Exception ex) {
				// TODO add proper error handling
				Console.WriteLine(ex.Message);
			}
			
			next += STEP;
			double now = sw.Elapsed.TotalSeconds;
			if (now - next > 0.25) {
				// fell far behind, resync
				next = now;
				return -1d;
			}
			return next - now;
		}

		if (timer.IsAvailable) {
			while (!ct.IsCancellationRequested) {
				double waitingTime = loop();
				timer.Wait(waitingTime);
			}
		} else {
			Console.WriteLine("Could not create a High resolution timer. Falling back to Thread.Sleep");
			while (!ct.IsCancellationRequested) {
				double waitingTime = loop();
				if (waitingTime != -1d) Thread.Sleep((int)(waitingTime * 1000));
			}
		}
	}
}
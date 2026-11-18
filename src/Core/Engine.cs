using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core;

public sealed class EngineInstance : Instance {
	private bool isRunning = false;
	private Dictionary<Type,ServiceInstance> services = new();

	protected override bool isParentLocked => true;

	public void Start() {
		if (this.isRunning) return;
		this.isRunning = true;

		this.GetService<Workspace>().ApplicationExit += (sender, args) => {
			Console.WriteLine("Stopping services");
			this.stopServices();
		};

		this.GetService<RunService>().start();
		this.GetService<Workspace>().start();
	}

	public T GetService<T>() where T : ServiceInstance {
		if (!this.services.TryGetValue(typeof(T), out var service)) {
			throw new InvalidOperationException($"Service of type {typeof(T).Name} does not exist.");
		}
		return (T)service;
	}

	private void registerServices() {
		void addService(ServiceInstance service) {
			service.forceSetParent(this);
			this.services.Add(service.GetType(), service);
		}
		addService(new Workspace());
		addService(new RunService());
	}

	private void stopServices() {
		this.GetService<RunService>().stop();
	}

	internal EngineInstance() {
		this.registerServices();
	}
}

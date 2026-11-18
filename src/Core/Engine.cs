using ColonThreePet.Core.Exceptions;
using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core;

/// <summary>
/// The root of the project's parent-child hierarchy. Its direnct children are <see cref="ServiceInstance"/>s,
/// that act as the fundamental components of any project.
/// </summary>
public sealed class EngineInstance : Instance {
	private bool isRunning = false;
	private Dictionary<Type,ServiceInstance> services = new();

	protected override bool isParentLocked => true;

	/// <summary>
	/// Starts all the services including physics simulation and rendering.
	/// It should only be called once inside your project.
	/// </summary>
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

	/// <summary>
	/// Returns a service of a specifed type.
	///<br/>Throws <see cref="ServiceNotFoundException"/> if a specifed service does not exist
	/// </summary>
	/// <exception cref="ServiceNotFoundException"/>
	public T GetService<T>() where T : ServiceInstance {
		if (!this.services.TryGetValue(typeof(T), out var service)) {
			throw new ServiceNotFoundException(typeof(T));
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

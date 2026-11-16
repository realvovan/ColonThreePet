using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core;

public static class Engine {
	public readonly static EngineInstance Instance = new();
	public readonly static Workspace Workspace = GetService<Workspace>();

	public static T GetService<T>() where T : ServiceInstance {
		return Instance.GetService<T>();
	}
}
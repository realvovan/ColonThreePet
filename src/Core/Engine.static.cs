using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core;

/// <summary>
/// Provides access to a singleton <see cref="EngineInstance"/> instnace
/// </summary>
public static class Engine {
	/// <inheritdoc cref="EngineInstance"/>
	public readonly static EngineInstance Instance = new();
	/// <inheritdoc cref="Instances.Workspace"/>
	public readonly static Workspace Workspace = GetService<Workspace>();

	/// <inheritdoc cref="EngineInstance.GetService{T}"/>
	public static T GetService<T>() where T : ServiceInstance {
		return Instance.GetService<T>();
	}
}
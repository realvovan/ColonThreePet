using ColonThreePet.Core.Interfaces;

namespace ColonThreePet.Core.Instances;

/// <summary>
/// The superclass to all Services that are direct children of <see cref="EngineInstance"/>.
/// </summary>
public abstract class ServiceInstance : Instance, IValidatesParent {
	protected override bool isParentLocked => true;

	public bool isValidParent(Instance parent) {
		return parent is EngineInstance;
	}
}

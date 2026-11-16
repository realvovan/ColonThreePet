using ColonThreePet.Core.Interfaces;

namespace ColonThreePet.Core.Instances;
public abstract class ServiceInstance : Instance, IValidatesParent {
	protected override bool isParentLocked => true;

	public bool isValidParent(Instance parent) {
		return parent is EngineInstance;
	}
}

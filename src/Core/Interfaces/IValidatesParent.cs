using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core.Interfaces;

public interface IValidatesParent {
	bool isValidParent(Instance parent);
}
using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core.Events;

public class AncestryChangedArgs(Instance child, Instance parent) : EventArgs {
	public Instance Child { get; } = child;
	public Instance Parent { get; } = parent;
}
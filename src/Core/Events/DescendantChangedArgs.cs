using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core.Events;

public class DescendantChangedArgs(Instance descendant,DescendantChangedArgs.ChangeType changeType) : EventArgs {
	public enum ChangeType {
		Added,
		Removed
	}

	public Instance Descendant { get; } = descendant;
	public ChangeType TypeOfChange { get; } = changeType;
}

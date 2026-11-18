using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core.Interfaces;

public interface IValidatesParent {
	/// <summary>
	/// Returns <c>true</c> if the implementing <see cref="Instance"/> can be parented to a provided parent
	/// </summary>
	bool isValidParent(Instance parent);
}
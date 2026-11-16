namespace ColonThreePet.Core.Events;

public class PropertyChangedEventArgs(string propertyName) : EventArgs {
	public string PropertyName { get; } = propertyName;
}
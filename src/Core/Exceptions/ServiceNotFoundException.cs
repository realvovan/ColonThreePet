namespace ColonThreePet.Core.Exceptions;

public class ServiceNotFoundException : Exception {
	public ServiceNotFoundException() : base("Provided service does not exist.") { }
	public ServiceNotFoundException(string message) : base(message) { }
	public ServiceNotFoundException(Type serviceType) : base($"Service of type {serviceType.Name} does not exist.") { }
}
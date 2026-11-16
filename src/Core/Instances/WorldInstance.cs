using ColonThreePet.Core.Math;

namespace ColonThreePet.Core.Instances;

public sealed class WorldInstance : Instance {
	// property field
	private Vector2d position = Vector2d.Zero;
	private Vector2d size = new Vector2d(100d, 100d);
	//
	public Vector2d Position {
		get => this.position;
		set => this.setPropertyAndNotify(ref this.position, value);
	}
	public Vector2d Size {
		get => this.size;
		set => this.setPropertyAndNotify(ref this.size, value);
	}
}
using ColonThreePet.Core.Math;

namespace ColonThreePet.Core.Instances;

public sealed class WorldInstance : Instance {
	// property field
	private Vector2d position = Vector2d.Zero;
	private Vector2d size = new Vector2d(100d, 100d);
	//
	public Vector2d Position {
		get => this.position;
		set => this.setPropertyAndNotify(ref this.position, clampToMinVector(value));
	}
	public Vector2d Size {
		get => this.size;
		set => this.setPropertyAndNotify(ref this.size, clampToMinVector(value));
	}

	private static Vector2d clampToMinVector(Vector2d x)
		=> new Vector2d(System.Math.Max(0.001, x.X), System.Math.Max(0.001, x.Y));
}
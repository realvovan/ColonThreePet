using System.Windows;

namespace ColonThreePet.Core.Math;

public readonly struct Vector2d(double x, double y) {
	public static readonly Vector2d Zero = new(0, 0);

	public readonly double X = x;
	public readonly double Y = y;

	public override int GetHashCode() => HashCode.Combine(this.X, this.Y);
	public override bool Equals(object? obj) => obj is Vector2d other && this.X == other.X && this.Y == other.Y;

	internal Point toPoint() => new Point(this.X, this.Y);

	public static Vector2d operator +(Vector2d a, Vector2d b) => new(a.X + b.X, a.Y + b.Y);
	public static Vector2d operator -(Vector2d a, Vector2d b) => new(a.X - b.X, a.Y - b.Y);
	public static bool operator ==(Vector2d a, Vector2d b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2d a, Vector2d b) => !(a == b);


	// vector2d with vector2f operations
	public static bool operator ==(Vector2d a, Vector2f b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2d a, Vector2f b) => !(a == b);
	public static explicit operator Vector2f(Vector2d self) => new Vector2f((float)self.X, (float)self.Y);
}
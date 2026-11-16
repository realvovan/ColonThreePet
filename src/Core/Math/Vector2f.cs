namespace ColonThreePet.Core.Math;

public readonly struct Vector2f(float x, float y) {
	public static readonly Vector2f Zero = new(0, 0);

	public readonly float X = x;
	public readonly float Y = y;

	public override int GetHashCode() => HashCode.Combine(this.X,this.Y);
	public override bool Equals(object? obj) => obj is Vector2f other && this.X == other.X && this.Y == other.Y;

	public static Vector2f operator +(Vector2f a,Vector2f b) => new(a.X + b.X,a.Y + b.Y);
	public static Vector2f operator -(Vector2f a,Vector2f b) => new(a.X - b.X,a.Y - b.Y);
	public static bool operator ==(Vector2f a,Vector2f b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2f a,Vector2f b) => !(a == b);

	// vector2f with vector2d operations
	public static bool operator ==(Vector2f a, Vector2d b) => a.X == b.X && a.Y == b.Y;
	public static bool operator !=(Vector2f a, Vector2d b) => !(a == b);
}
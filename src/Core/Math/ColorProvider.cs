using System.Windows.Media;

namespace ColonThreePet.Core.Math;

public abstract class ColorProvider {
	protected internal abstract Brush ToBrush();
	public abstract override int GetHashCode();
	public abstract override bool Equals(object? obj);

	public static bool operator ==(ColorProvider a,ColorProvider b) => a.Equals(b);
	public static bool operator !=(ColorProvider a, ColorProvider b) => !(a.Equals(b));
}
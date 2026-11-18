using System.Windows.Media;

namespace ColonThreePet.Core.Math;

public sealed class GradientStopCollectionBuilder {
	private GradientStopCollection gradientStops = new();

	public bool Built { get; private set; } = false;

	public GradientStopCollectionBuilder AddColor(Color3 color, double offset) {
		if (this.Built) throw new InvalidOperationException("Cannot add color to a built GradientStopCollectionBuilder");
		this.gradientStops.Add(new GradientStop(Color.FromRgb(color.R, color.G, color.B), offset));
		return this;
	}

	public GradientStopCollection Build() {
		this.Built = true;
		if (this.gradientStops.Count == 0)
			this.gradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0));
		return this.gradientStops;
	}
}

public abstract class ColorGradient : ColorProvider {
	protected readonly GradientStopCollection gradientStops;

	public override int GetHashCode() {
		var hash = new HashCode();
		foreach (var i in this.gradientStops) hash.Add(i);
		return hash.ToHashCode();
	}

	public override bool Equals(object? obj) {
		if (obj is ColorGradient otherGradient) return this == otherGradient;
		else if (obj is Color3 otherColor) return this == otherColor;
		return false;
	}

	public ColorGradient(Color3 color) {
		this.gradientStops = new GradientStopCollectionBuilder().AddColor(color, 0d).Build();
	}
	public ColorGradient(GradientStopCollection colors) {
		this.gradientStops = colors;
	}
	public ColorGradient(GradientStopCollectionBuilder colorBuilder) {
		this.gradientStops = colorBuilder.Build();
	}

	public static bool operator ==(ColorGradient a, ColorGradient b) {
		if (a.gradientStops.Count != b.gradientStops.Count) return false;
		for (int i = 0; i < a.gradientStops.Count; i++) {
			var stopA = a.gradientStops[i];
			var stopB = b.gradientStops[i];
			if (stopA.Color != stopB.Color || stopA.Offset != stopB.Offset) return false;
		}
		return true;
	}
	public static bool operator !=(ColorGradient a, ColorGradient b) => !(a == b);

	public static bool operator ==(ColorGradient a, Color3 b) {
		if (a.gradientStops.Count != 1) return false;
		var color = a.gradientStops[0].Color;
		return b.R == color.R && b.G == color.G && b.B == color.B;
	}
	public static bool operator !=(ColorGradient a, Color3 b) => !(a == b);

	public static bool operator ==(Color3 a, ColorGradient b) => b == a;
	public static bool operator !=(Color3 a, ColorGradient b) => !(b == a);
}
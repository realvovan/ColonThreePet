using System.Windows.Media;

namespace ColonThreePet.Core.Math;

public class RadialGradientColor : ColorGradient {
	public Vector2d Center { get; init; } = new Vector2d(0.5, 0.5);
	public Vector2d Radius { get; init; } = new Vector2d(0.5, 0.5);
	protected internal override Brush ToBrush() => new RadialGradientBrush {
															GradientStops = this.gradientStops,
															Center = this.Center.toPoint(),
															GradientOrigin = this.Center.toPoint(),
															RadiusX = this.Radius.X,
															RadiusY = this.Radius.Y,
														};

	public RadialGradientColor(Color3 color) : base(color) { }
	public RadialGradientColor(GradientStopCollection colors) : base(colors) { }
	public RadialGradientColor(GradientStopCollectionBuilder colorBuilder) : base(colorBuilder) { }
}
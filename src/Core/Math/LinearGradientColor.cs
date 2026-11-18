using System.Windows.Media;

namespace ColonThreePet.Core.Math;

public enum LinearGradientDirection {
	LeftRight,
	RightLeft,
	TopBottom,
	BottomTop,
	Custom
}

public class LinearGraidnetColor : ColorGradient {
	public Vector2d StartPoint { get; init; }
	public Vector2d EndPoint { get; init; }
	protected internal override Brush ToBrush() => new LinearGradientBrush {
		GradientStops = this.gradientStops,
		StartPoint = this.StartPoint.toPoint(),
		EndPoint = this.EndPoint.toPoint()
	};

	public LinearGraidnetColor(Color3 color) : base(color) { }
	public LinearGraidnetColor(GradientStopCollection colors, LinearGradientDirection direction = LinearGradientDirection.TopBottom) : base(colors) {
		(this.StartPoint, this.EndPoint) = parseRotation(direction);
	}
	public LinearGraidnetColor(GradientStopCollectionBuilder colorBuilder, LinearGradientDirection direction = LinearGradientDirection.TopBottom)
		: base(colorBuilder)
	{
		(this.StartPoint, this.EndPoint) = parseRotation(direction);
	}

	private static (Vector2d start, Vector2d end) parseRotation(LinearGradientDirection direction) {
		switch (direction) {
			case LinearGradientDirection.LeftRight:
				return (new Vector2d(0d, 0.5), new Vector2d(1d, 0.5));
			case LinearGradientDirection.RightLeft:
				return (new Vector2d(1d, 0.5), new Vector2d(0d, 0.5));
			case LinearGradientDirection.TopBottom:
				return (new Vector2d(0.5, 0d), new Vector2d(0.5, 1d));
			case LinearGradientDirection.BottomTop:
				return (new Vector2d(0.5, 1d), new Vector2d(0.5, 0d));
			case LinearGradientDirection.Custom:
				return (Vector2d.Zero, Vector2d.Zero);
			default: throw new ArgumentException("Invalid rotation enum value", nameof(direction));
		}
	}
}
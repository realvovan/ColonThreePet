using ColonThreePet.Core.Math;
using System.Windows;
using System.Windows.Shapes;

namespace ColonThreePet.Core.Instances;

public sealed class RectangleRenderComponent : RenderComponent {
	// property fields
	private ColorProvider backgroundColor = Color3.FromRGB(0,0,0);
	//

	private readonly Rectangle rect = new();

	public ColorProvider BackgroundColor {
		get => this.backgroundColor;
		set {
			this.setPropertyAndNotify(ref this.backgroundColor, value);
			Engine.Workspace.DeferToRender(this, nameof(this.BackgroundColor), () => this.rect.Fill = value.ToBrush());
		}
	}

	protected override void updateDrawable() {
		base.updateDrawable();
		this.rect.Fill = this.backgroundColor.ToBrush();
	}

	public override FrameworkElement GetDrawable() => this.rect;
}
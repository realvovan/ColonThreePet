using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColonThreePet.Core.Instances;

public class ImageRenderComponent : RenderComponent {
	private readonly Image image = new() {
		Stretch = Stretch.Fill
	};

	public ImageSource ImageSource {
		get => this.image.Source;
		set => Engine.Workspace.DeferToRender(this, nameof(this.ImageSource), () => {
			if (Object.Equals(this.image.Source,value)) return;
			this.image.Source = value;
			this.notifyPropertyChange();
		});
	}
	public void SetImage(string path) => this.ImageSource = new BitmapImage(new Uri(path));

	public override FrameworkElement GetDrawable() => this.image;
}
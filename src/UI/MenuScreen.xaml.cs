using System.Windows;

namespace ColonThreePet;

public partial class MenuScreen : Window {
	bool isPetOpen = false;
    public MenuScreen() {
        InitializeComponent();
    }

	void StartButtonClick(object sender,RoutedEventArgs args) {
		if (this.isPetOpen) return;
		this.isPetOpen = true;
		new PetApp(!this.DisableHungerBox.IsChecked ?? false).Show();
		this.Close();
	}
}

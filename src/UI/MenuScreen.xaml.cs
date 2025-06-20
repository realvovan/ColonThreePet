using System.Windows;

namespace ColonThreePet;

public readonly struct PetConfig {
	/// <summary>
	/// How much the Y velocity component is increased by every frame
	/// </summary>
	public readonly double VerticalAcceleration { get; init; }
	/// <summary>
	/// How much is velocity decreased by every frame, when the pet is NOT touching the floor
	/// </summary>
	public readonly double AirFriction { get; init; }
	/// <summary>
	/// How much is velocity decreased by every frame, when the pet is touching the floor
	/// </summary>
	public readonly double GroundFriction { get; init; }
	/// <summary>
	/// Force of throwing the pet with the mouse pointer
	/// </summary>
	public readonly double ThrowForce { get; init; }
	/// <summary>
	/// How much is the Y velocity component multiplied by when the pet touches the floor
	/// </summary>
	public readonly double Elasticity { get; init; }
	/// <summary>
	/// Time in seconds before the pet falls asleep if the user has been idle
	/// </summary>
	public readonly int InactivityTimeout { get; init; }
	/// <summary>
	/// Whether or not the pet loses hunger
	/// </summary>
	public readonly bool EnableHunger { get; init; }
}
public partial class MenuScreen : Window {
	bool isPetOpen = false;
	public MenuScreen() {
		InitializeComponent();
	}

	void StartButtonClick(object sender,RoutedEventArgs args) {
		if (this.isPetOpen) return;
		this.isPetOpen = true;
		new PetApp(
			new PetConfig {
				VerticalAcceleration = this.AccelerationSlider.Value,
				AirFriction = this.AirFrictionSlider.Value,
				GroundFriction = this.GroundFrictionSlider.Value,
				ThrowForce = this.ThrowForceSlider.Value,
				EnableHunger = !this.DisableHungerBox.IsChecked ?? false,
				Elasticity = 0.35,
				InactivityTimeout = 450,
			}
		).Show();
		this.Close();
	}

	void ToggleSettingsClick(object sender,RoutedEventArgs args) {
		this.SettingsPanel.Visibility = this.SettingsPanel.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
	}
}

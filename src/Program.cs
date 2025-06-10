using System.Windows;

namespace ColonThreePet;

static class Program {
	[STAThread]
	static void Main() {
		new Application().Run(new MenuScreen());
	}
}
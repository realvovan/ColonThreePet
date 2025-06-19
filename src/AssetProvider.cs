using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColonThreePet;

static class AssetProvider {
	public static class Images {
		public static readonly ImageSource CatIdle = getImage("CatIdle.png");
		public static readonly ImageSource CatMoving = getImage("CatMoving.png");
		public static readonly ImageSource TootieQuiet = getImage("TootieQuiet.png");
		public static readonly ImageSource TootieScream = getImage("TootieScream.png");
		public static readonly ImageSource CatStarving = getImage("CatStarving.png");
		public static readonly ImageSource CatHungry = getImage("CatHungry.png");
		public static readonly ImageSource CatIdle2 = getImage("CatIdle2.png");
		public static readonly ImageSource CatSpeak = getImage("CatNerd.png");
		public static readonly ImageSource CatSleep = getImage("CatSleeping.png");
		public static readonly ImageSource CatSecretIdle = getImage("CatAlienGoober.png");
		public static readonly ImageSource CatHit = getImage("CatHit.png");

		private static ImageSource getImage(string name) => new BitmapImage(new Uri("/assets/Images/" + name,UriKind.Relative));
	}
	public static class Audios {
		public static readonly MediaPlayer TootieScream = getSound("TootieMeow.wav");
		public static readonly MediaPlayer EatingSound = getSound("EatingSfx.wav");
		public static readonly MediaPlayer Meow = getSound("Meow.wav");
		public static readonly MediaPlayer StartupSound = getSound("Startup.wav");
		public static readonly MediaPlayer ShutdownSound = getSound("VanishSFX.wav");
		public static readonly MediaPlayer SleepSound = getSound("SleepSFX.wav");
		public static readonly MediaPlayer HitSound = getSound("CatHitSFX.wav");
		private static MediaPlayer getSound(string name) {
			var player = new MediaPlayer();
			player.Open(new Uri("assets/Sounds/" + name,UriKind.Relative));
			return player;
		}
	}

	public static void PlayFromStart(this MediaPlayer player) {
		player.Stop();
		player.Position = TimeSpan.Zero;
		player.Play();
	}
}
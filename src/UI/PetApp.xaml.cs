using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace ColonThreePet;

public enum PetStates {
    /// <summary>
    /// Pet is not moving and isn't being affected by gravity
    /// </summary>
    Idle,
    /// <summary>
    /// Pet is falling down due to gravity
    /// </summary>
    Freefall,
    /// <summary>
    /// Pet is being dragged around by the user's mouse pointer
    /// </summary>
    MouseDrag,
    /// <summary>
    /// Pet is navigating towards a random point on the screen
    /// </summary>
    Navigating,
    /// <summary>
    /// Pet begging for food when you're dragging some
    /// </summary>
    Begging
}

public partial class PetApp : Window {
    const double VERTICAL_ACCELERATION = 1.1d;
    const double AIR_FRICTION = 0.9d;
    const double GROUND_FRICTION = AIR_FRICTION * 3d;
    const double ELASTICITY = 0.35d;

    public int Hunger { get; private set; } = 100;
    public PetStates State { get; private set; } = PetStates.Freefall;

    bool isMouseInPet = false;
    int gotoX = -1;
    double floorY = getScreenSize().Height;
    Stopwatch lastWalkStopwatch = new Stopwatch();
    Stopwatch lastFoodAppearStopwatch = new Stopwatch();
    Random rng = new Random();
    Vector velocity = new Vector(0d,1d);
    Vector nextPosition;
    Size screenSize = getScreenSize();
    public PetApp() {
        InitializeComponent();
        this.nextPosition = new Vector(this.PetDecalPos.X,this.PetDecalPos.Y) + this.velocity;
        this.PetDecal.Source = AssetProvider.Images.CatIdle;
        this.HungerImage.Visibility = Visibility.Hidden;
        this.FoodDecal.Visibility = Visibility.Hidden;

        if (rng.Next(0,100) < 94) {
            this.Tootie.Visibility = Visibility.Hidden;
        } else {
            ((TranslateTransform)this.Tootie.RenderTransform).X = this.screenSize.Width - this.Tootie.Width;
            ((TranslateTransform)this.Tootie.RenderTransform).Y = this.screenSize.Height - this.Tootie.Height;
            this.Tootie.Visibility = Visibility.Visible;
            this.Tootie.MouseLeftButtonDown += async (sender,args) => {
                this.Tootie.Source = AssetProvider.Images.TootieScream;
                AssetProvider.Audios.TootieScream.PlayFromStart();
                await Task.Delay(500);
                this.Tootie.Source = AssetProvider.Images.TootieQuiet;
            };
        }

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1d / 60d);
        timer.Tick += physicsLoop;
        timer.Start();
    }
    void physicsLoop(object? sender,EventArgs args) {
        //update positions
        this.PetDecalPos.X = this.nextPosition.X;
        this.PetDecalPos.Y = this.nextPosition.Y;
        //calculate next positions
        this.nextPosition.X = this.PetDecalPos.X + this.velocity.X;
        this.nextPosition.Y = this.PetDecalPos.Y + this.velocity.Y;
        if (this.State == PetStates.Idle) {
            if (this.Hunger < 1) {
                this.Hunger = 0;
                this.PetDecal.Source = AssetProvider.Images.CatStarving;
            }
            //make him go to random positions
            if (this.Hunger > 0 && lastWalkStopwatch.Elapsed() > rng.Next(5,12)) {
                this.gotoX = rng.Next(0,(int)(this.screenSize.Width - this.PetDecal.Width));
                this.PetDecal.Source = AssetProvider.Images.CatMoving;
                this.PetScale.ScaleX = this.gotoX < this.nextPosition.X ? 1d : -1d;
                this.Hunger -= rng.Next(2,6);
                this.State = PetStates.Navigating;
            }
        } else if (this.State == PetStates.Navigating) {
			if (Math.Abs(this.nextPosition.X - this.gotoX) < 1d) {
                //cat reached its destination
                this.lastWalkStopwatch.Reset();
				this.PetDecal.Source = AssetProvider.Images.CatIdle;
                this.PetScale.ScaleX = 1d;
                this.State = PetStates.Idle;
				return;
			}
			this.nextPosition.X += this.gotoX < this.nextPosition.X ? -1d : 1d;
            //no point of doing further calculations if the guy has a predefined goto location
			return;
		}

        if (this.nextPosition.X < 0) {
            this.velocity.X = -this.velocity.X;
            this.nextPosition.X = 0;
        } else if (this.nextPosition.X + this.PetDecal.Width > this.screenSize.Width) {
            this.velocity.X = -this.velocity.X;
            this.nextPosition.X = this.screenSize.Width - this.PetDecal.Width;
        }

        if (
            (this.State == PetStates.Freefall || this.State == PetStates.Begging)
            && this.nextPosition.Y + this.PetDecal.Height >= this.floorY
        ) {
            //he's touching the floor but has velocity
            this.velocity.X = absoluteSubtract(this.velocity.X,GROUND_FRICTION,0d);
            this.velocity.Y *= -ELASTICITY;
            this.nextPosition.Y = this.floorY - this.PetDecal.Height;
            //check if velocity is near 0 and make him idle
            if (this.velocity.X == 0d && Math.Abs(this.velocity.Y) < 0.6d) {
                this.velocity.Y = 0d;
                this.lastWalkStopwatch.Reset();
                this.State = PetStates.Idle;
            }
        } else if (this.nextPosition.Y < 0d) {
            //he's touching the screen top
            this.velocity.Y = -this.velocity.Y;
            this.nextPosition.Y = 0d;
        } else {
            //make him fall and apply less friction to X velocity
            this.velocity.X = absoluteSubtract(this.velocity.X,AIR_FRICTION,0d);
            if (this.State == PetStates.Freefall || this.State == PetStates.Begging) this.velocity.Y += VERTICAL_ACCELERATION;
        }
        //make food appear
        if (this.lastFoodAppearStopwatch.Elapsed() > 10) {
            this.lastFoodAppearStopwatch.Reset();
            if (this.FoodDecal.Visibility == Visibility.Hidden && rng.Next(100) < 75) {
                this.FoodPos.X = rng.Next(0,(int)(this.screenSize.Width - this.FoodDecal.Width));
                this.FoodPos.Y = rng.Next(0,(int)(this.screenSize.Height - this.FoodDecal.Height));
                this.FoodDecal.Visibility = Visibility.Visible;
            }
        }
	}
	async void foodOnMouseDown(object sender,MouseButtonEventArgs args) {
        var clickPosOnFood = (Vector)args.GetPosition(this.FoodDecal);
        var lastState = this.State;
        this.State = PetStates.Begging;
        this.PetDecal.Source = AssetProvider.Images.CatHungry;
        while (args.LeftButton == MouseButtonState.Pressed) {
            Vector currentPos = new Vector(this.FoodPos.X,this.FoodPos.Y);
            Vector nextPos = currentPos + (getMousePosition() - clickPosOnFood - currentPos) * 0.25;
            this.FoodPos.X = nextPos.X;
            this.FoodPos.Y = nextPos.Y;
            await Task.Delay(10);
        }
        if (
            this.FoodPos.X > this.PetDecalPos.X && this.FoodPos.X < this.PetDecalPos.X + this.PetDecal.Width
            && this.FoodPos.Y > this.PetDecalPos.Y && this.FoodPos.Y < this.PetDecalPos.Y + this.PetDecal.Height
        ) {
            AssetProvider.Audios.EatingSound.PlayFromStart();
            this.Hunger = Math.Min(100,this.Hunger + rng.Next(10,35));
            this.FoodDecal.Visibility = Visibility.Hidden;
        }
        this.State = lastState;
        this.PetDecal.Source = lastState == PetStates.Navigating ? AssetProvider.Images.CatMoving : AssetProvider.Images.CatIdle;
	}
	async void petOnMouseDown(object sender,MouseButtonEventArgs args) {
        this.State = PetStates.MouseDrag;
        this.velocity = new Vector(0d,0d);
        this.isMouseInPet = false;
        this.HungerImage.Visibility = Visibility.Hidden;
        this.PetScale.ScaleX = 1d;
		this.PetDecal.Source = this.Hunger > 0 ? AssetProvider.Images.CatIdle : AssetProvider.Images.CatStarving;
		Vector oldAbsPos;
        var clickPosOnPet = (Vector)args.GetPosition(this.PetDecal);
        while (args.LeftButton == MouseButtonState.Pressed) {
            oldAbsPos = getMousePosition();
            this.PetRotation.Angle = 10 * Math.Sin(DateTimeOffset.Now.ToUnixTimeMilliseconds() * 0.004);
            this.nextPosition = this.nextPosition + (oldAbsPos - clickPosOnPet - this.nextPosition) * 0.25; //lerping to mouse pos by 25%
			this.PetDecalPos.X = this.nextPosition.X;
			this.PetDecalPos.Y = this.nextPosition.Y;
			await Task.Delay(10);
        }
        this.PetRotation.Angle = 0;
        this.velocity = getMousePosition() - oldAbsPos;
        this.screenSize = getScreenSize();
        this.lastWalkStopwatch.Reset();
        this.Hunger--;
        this.State = PetStates.Freefall;
    }
	async void petOnMouseIn(object sender,MouseEventArgs args) {
        if (this.State == PetStates.MouseDrag) return;
        this.isMouseInPet = true;
		var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime < 1000) await Task.Delay(50);
        if (!this.isMouseInPet) return;
		this.HungerImage.Visibility = Visibility.Visible;
        while (this.isMouseInPet) {
			this.HungerLabel.Content = $"{this.Hunger}%";
            ((TranslateTransform)this.HungerImage.RenderTransform).X = Math.Max(0,this.PetDecalPos.X - this.HungerImage.Width + 40);
			((TranslateTransform)this.HungerImage.RenderTransform).Y = this.PetDecalPos.Y - this.HungerImage.Height + 15;
			await Task.Delay(50);
        }
	}
	void petOnMouseOut(object sender,MouseEventArgs args) {
        this.isMouseInPet = false;
        this.HungerImage.Visibility = Visibility.Hidden;
	}
    public Vector GetPetVelocity() => this.velocity;
	static Size getScreenSize() => new Size(SystemParameters.WorkArea.Width,SystemParameters.WorkArea.Height);
    static Vector getMousePosition() => new Vector(System.Windows.Forms.Cursor.Position.X,System.Windows.Forms.Cursor.Position.Y);
    static double absoluteSubtract(double num1,double num2) => num1 - Math.Sign(num1) * num2;
    /// <summary>
    /// Subtracts num2 from the absolute value of num1, if abs(num1) > num2 returns clamp
    /// </summary>
    static double absoluteSubtract(double num1,double num2,double clamp) {
        if (Math.Abs(num1) < num2) return clamp;
        return num1 - Math.Sign(num1) * num2;
    }
}
namespace ColonThreePet;

class Stopwatch {
	int startTime = Now();
	public int Elapsed() => Now() - this.startTime;
	public void Reset() => this.startTime = Now();
	public static int Now() => (int)DateTimeOffset.Now.ToUnixTimeSeconds();
}
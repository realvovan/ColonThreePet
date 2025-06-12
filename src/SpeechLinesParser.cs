using System.IO;

namespace ColonThreePet;

public class SpeechLinesParser(string pathToFile) {
	private string[] lines = File.ReadAllLines(pathToFile);
	private Random rng = new Random();
	private int currentLine = 0;

	public string GetRandomLine() => this.lines[rng.Next(this.lines.Length)];
	public string GetLine(int index) {
		this.currentLine = index;
		return this.lines[index];
	}
	public string GetNextLine() {
		var line = this.lines[this.currentLine];
		this.currentLine = this.currentLine + 1 < this.lines.Length ? ++this.currentLine : 0;
		return line;
	}
}
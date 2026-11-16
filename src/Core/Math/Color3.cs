using System.Windows.Media;

namespace ColonThreePet.Core.Math;

public class Color3(byte r, byte g, byte b) : ColorProvider {
	public byte R { get; } = r;
	public byte G { get; } = g;
	public byte B { get; } = b;

	protected internal override Brush ToBrush() => new SolidColorBrush(Color.FromRgb(this.R, this.G, this.B));

	public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);
	public override bool Equals(object? obj) => obj is Color3 other && this == other;
	public override string ToString() => $"{this.R}, {this.G}, {this.B}";

	public static Color3 FromRGB(byte r,byte g,byte b) => new Color3(r,g,b);
	public static Color3 FromRGB(int r, int g, int b) => new Color3((byte)r, (byte)g, (byte)b);

	public static Color3 FromHex(string hex) {
		byte r = Convert.ToByte(hex.Substring(1,2), 16);
		byte g = Convert.ToByte(hex.Substring(3,2),16);
		byte b = Convert.ToByte(hex.Substring(5,2),16);
		return new Color3(r,g,b);
	}
	public static Color3 FromHex(int hex) {
		int r = (hex >> 16) & 0xff;
		int g = (hex >> 8) & 0xff;
		int b = hex & 0xff;
		return Color3.FromRGB(r, g, b);
	}

	public static bool operator ==(Color3 a, Color3 b) => a.R == b.R && a.G == b.G && a.B == b.B;
	public static bool operator !=(Color3 a, Color3 b) => !(a == b);
}
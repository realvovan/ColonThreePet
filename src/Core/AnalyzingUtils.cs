using ColonThreePet.Core.Instances;

namespace ColonThreePet.Core;

public static class AnalyzingUtils {
	public static void PrintAncestryTree(Instance root, int indentLevel = 0) {
		root.ForEachChild(c => {
			Console.WriteLine($"{new string(' ', indentLevel * 4)} {c}");
			PrintAncestryTree(c, indentLevel + 1);
		});
	}
}
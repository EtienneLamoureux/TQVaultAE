using System.Drawing;
using TQVaultAE.Domain.Actions;

namespace TQVaultAE.Application.Character;

/// <summary>Reads the numeric variables of a game record (name to all its values).</summary>
public static class GameValues
{
	/// <summary>The first value of a variable; null when it is missing or has no value.</summary>
	public static double? First(this IReadOnlyDictionary<string, IReadOnlyList<double>> values, string name)
		=> values.TryGetValue(name, out var all) && all.Count > 0 ? all[0] : null;

	/// <summary>
	/// A grid size two variables give in pixels, in cells (<see cref="SackLayout.CellsOf"/>); null when either is missing.
	/// </summary>
	public static Size? PixelGrid(this IReadOnlyDictionary<string, IReadOnlyList<double>> values, string widthName, string heightName)
		=> values.First(widthName) is { } width && values.First(heightName) is { } height
			? SackLayout.CellsOf(width, height)
			: null;
}

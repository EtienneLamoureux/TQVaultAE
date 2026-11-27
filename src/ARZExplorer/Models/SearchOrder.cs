namespace ArzExplorer.Models;

/// <summary>
/// Order in which Find Next and Find Previous visit the tree nodes.
/// </summary>
internal static class SearchOrder
{
	/// <summary>
	/// Indices of <paramref name="count"/> nodes, starting next to the last match and wrapping around,
	/// so the last match comes last and is found again when it is the only one.
	/// </summary>
	/// <param name="count">Number of nodes.</param>
	/// <param name="lastMatch">Index of the last match; -1 (forward) or <paramref name="count"/> (backward) to start from an end.</param>
	/// <param name="forward">Find Next when true, Find Previous when false.</param>
	internal static IEnumerable<int> Indices(int count, int lastMatch, bool forward)
	{
		if (count <= 0)
			yield break;

		// Past an end (a search the other way found nothing): start from the other end
		if (forward && lastMatch >= count)
			lastMatch = -1;
		else if (!forward && lastMatch < 0)
			lastMatch = count;
		for (int step = 1; step <= count; step++)
		{
			int index = forward ? lastMatch + step : lastMatch - step;
			yield return ((index % count) + count) % count;
		}
	}
}

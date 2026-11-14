namespace TQVaultAE.Application;

/// <summary>
/// Rules of the shared stashes (transfer stash and relic vault): each custom map (mod) has its own.
/// </summary>
public static class SharedStashes
{
	/// <summary>
	/// The maps whose shared stashes are read and watched: the maps of the Immortal Throne characters, each once
	/// (<see cref="GameMaps.Same"/>) in their order, and the base game, put first when no character plays it; none
	/// without such characters.
	/// </summary>
	/// <param name="characterMaps">The map of each Immortal Throne character.</param>
	public static List<string> Maps(IEnumerable<string> characterMaps)
	{
		var maps = new List<string>();
		foreach (var map in characterMaps)
		{
			if (!maps.Any(m => GameMaps.Same(m, map)))
				maps.Add(map);
		}

		if (maps.Count != 0 && !maps.Any(m => GameMaps.Same(m, string.Empty)))
			maps.Insert(0, string.Empty);

		return maps;
	}
}

using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>
/// Texts of the stashes shared by the characters of a map: the transfer stash and the relic vault.
/// </summary>
public static class SharedStashText
{
	/// <summary>
	/// Gets the translated name of a shared stash.
	/// </summary>
	/// <param name="type">The transfer stash or the relic vault.</param>
	/// <returns>Its name, or null for another stash type.</returns>
	public static string? Name(StashType? type) => type switch
	{
		StashType.TransferStash => Resources.GlobalTransferStash,
		StashType.RelicVaultStash => Resources.GlobalRelicVaultStash,
		_ => null,
	};

	/// <summary>
	/// Translates a container name the services built from a shared stash's neutral name (<see cref="SharedStashNames"/>),
	/// such as "Transfer Stash [mod]" or "Relic Vault - Immortal Throne": the neutral name becomes the translated one and
	/// the rest stays. Any other name is returned as it is.
	/// </summary>
	/// <param name="name">The container name.</param>
	/// <param name="type">The stash type of the container, or null when it is not a stash.</param>
	public static string Localize(string name, StashType? type)
		=> SharedStashNames.Of(type) is { } neutral && Name(type) is { } text && name.StartsWith(neutral, StringComparison.Ordinal)
			? string.Concat(text, name.AsSpan(neutral.Length))
			: name;
}

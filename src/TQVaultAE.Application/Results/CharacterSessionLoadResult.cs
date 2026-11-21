namespace TQVaultAE.Application.Results;

/// <summary>
/// What a character selection loads: the character, and its stashes when it has some.
/// A Titan Quest original character has no player, transfer or relic vault stash.
/// </summary>
/// <param name="Player">The character load.</param>
/// <param name="PlayerStash">The character's stash load; null for a Titan Quest original character.</param>
/// <param name="TransferStash">The transfer stash load; null when it was not asked for or the character has no stashes.</param>
/// <param name="RelicVaultStash">The relic vault stash load; null when it was not asked for or the character has no stashes.</param>
public sealed record CharacterSessionLoadResult(
	PlayerLoadResult Player
	, StashLoadResult? PlayerStash
	, StashLoadResult? TransferStash
	, StashLoadResult? RelicVaultStash)
{
	/// <summary>
	/// Indicates whether the character has stashes (Immortal Throne).
	/// When false, the player, transfer and relic vault stashes shown are to be cleared.
	/// </summary>
	public bool HasStashes => this.PlayerStash is not null;
}

using System.Diagnostics.CodeAnalysis;

namespace TQVaultAE.Application.Results;

/// <summary>Why a loadout name is refused.</summary>
public enum LoadoutNameError
{
	/// <summary>The name can be used.</summary>
	None,

	/// <summary>No name, or only spaces.</summary>
	Empty,

	/// <summary>Another loadout of the character has this name (case ignored).</summary>
	AlreadyUsed,
}

/// <summary>Outcome of a change to a character's loadouts (add, update, rename, copy, delete).</summary>
public sealed class LoadoutChange
{
	private LoadoutChange(LoadoutNameError nameError, Exception? error, bool notFound)
	{
		this.NameError = nameError;
		this.Error = error;
		this.NotFound = notFound;
	}

	/// <summary>The refused name; <see cref="LoadoutNameError.None"/> otherwise.</summary>
	public LoadoutNameError NameError { get; }

	/// <summary>The failure to write the file; null otherwise.</summary>
	public Exception? Error { get; }

	/// <summary>True when the loadout asked for does not exist (or the file could not be read, so nothing is written).</summary>
	public bool NotFound { get; }

	/// <summary>True when the change is written.</summary>
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this.Error is null && this.NameError == LoadoutNameError.None && !this.NotFound;

	public static readonly LoadoutChange Succeeded = new(LoadoutNameError.None, null, false);

	public static readonly LoadoutChange Missing = new(LoadoutNameError.None, null, true);

	public static LoadoutChange Refused(LoadoutNameError error) => new(error, null, false);

	public static LoadoutChange Failed(Exception error) => new(LoadoutNameError.None, error, false);
}

/// <summary>How a loadout load ended.</summary>
public enum LoadoutLoadOutcome
{
	/// <summary>The equipment is the loadout's.</summary>
	Loaded,

	/// <summary>The equipment already was the loadout's: nothing changed.</summary>
	AlreadyWorn,

	/// <summary>The bag lacks room for the pieces taken off: nothing changed.</summary>
	NoRoom,

	/// <summary>No such loadout, or a character, equipment or bag that cannot change: nothing changed.</summary>
	Refused,
}

/// <summary>Outcome of loading a loadout.</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="ToBag">Pieces taken off and stored in the bag (or that would have been, for <see cref="LoadoutLoadOutcome.NoRoom"/>).</param>
/// <param name="Moved">Worn pieces that changed slot.</param>
/// <param name="Copied">Loadout pieces copied in.</param>
public sealed record LoadoutLoadResult(LoadoutLoadOutcome Outcome, int ToBag = 0, int Moved = 0, int Copied = 0);

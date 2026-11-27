using System.Diagnostics.CodeAnalysis;

namespace TQVaultAE.Application.Results;

/// <summary>Outcome of a change to a character's loadouts (add, update, rename, copy, delete).</summary>
public sealed class LoadoutChange
{
	private LoadoutChange(SetNameError nameError, Exception? error, bool notFound)
	{
		this.NameError = nameError;
		this.Error = error;
		this.NotFound = notFound;
	}

	/// <summary>The refused name; <see cref="SetNameError.None"/> otherwise.</summary>
	public SetNameError NameError { get; }

	/// <summary>The failure to write the file; null otherwise.</summary>
	public Exception? Error { get; }

	/// <summary>True when the loadout asked for does not exist (or the file could not be read, so nothing is written).</summary>
	public bool NotFound { get; }

	/// <summary>True when the change is written.</summary>
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => this.Error is null && this.NameError == SetNameError.None && !this.NotFound;

	public static readonly LoadoutChange Succeeded = new(SetNameError.None, null, false);

	public static readonly LoadoutChange Missing = new(SetNameError.None, null, true);

	public static LoadoutChange Refused(SetNameError error) => new(error, null, false);

	public static LoadoutChange Failed(Exception error) => new(SetNameError.None, error, false);
}

/// <summary>How a gear swap ended.</summary>
public enum LoadoutSwapOutcome
{
	/// <summary>The pieces were exchanged.</summary>
	Swapped,

	/// <summary>The loadout and the equipment are both empty, with the same weapon set in hand: nothing changed.</summary>
	NothingToSwap,

	/// <summary>No such loadout, a file that could not be read, or a character or equipment that cannot change: nothing changed.</summary>
	Refused,
}

/// <summary>How emptying a loadout into a bag ended.</summary>
public enum LoadoutEmptyOutcome
{
	/// <summary>The pieces are in the bag; the loadout is empty.</summary>
	Emptied,

	/// <summary>Some pieces are in the bag; the others, without room, stay in the loadout.</summary>
	PartlyEmptied,

	/// <summary>The loadout holds no piece: nothing changed.</summary>
	NothingToEmpty,

	/// <summary>No piece has room in the bag (or the bag takes none of them): they all stay in the loadout.</summary>
	NoRoom,

	/// <summary>No such loadout, a file that could not be read, or a bag that cannot change: nothing changed.</summary>
	Refused,
}

/// <summary>Outcome of emptying a loadout (<see cref="Contracts.Services.ILoadoutService.Empty"/>).</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="Pieces">Pieces moved to the bag.</param>
/// <param name="Left">Pieces left in the loadout for want of room.</param>
public sealed record LoadoutEmptyResult(LoadoutEmptyOutcome Outcome, int Pieces = 0, int Left = 0)
{
	/// <summary>The pieces now in the bag.</summary>
	public IReadOnlyList<Domain.Entities.Item> Moved { get; init; } = [];
}

/// <summary>Outcome of a gear swap (<see cref="Contracts.Services.ILoadoutService.Swap"/>).</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="PutOn">Loadout pieces now worn.</param>
/// <param name="TakenOff">Worn pieces now in the loadout.</param>
public sealed record LoadoutSwapResult(LoadoutSwapOutcome Outcome, int PutOn = 0, int TakenOff = 0);

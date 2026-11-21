using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// The class sets of the character edited in the character editor, changed in memory: sets saved, renamed or deleted
/// and the set selected are written on OK (<see cref="Contracts.Services.IClassSetService.Write"/>) and dropped by
/// Cancel. The selected set is the one applied or saved last; edits in the editor do not change it, they only make the
/// build shown differ from it (<see cref="Status"/>).
/// </summary>
public sealed class ClassSetDraft
{
	private readonly ClassSet _opening;
	private readonly string? _openingSelected;
	private readonly List<ClassSet> _sets;
	private bool _setsChanged;

	/// <summary>True while the selected set is the one remembered and the save read at the opening already differed from it.</summary>
	private bool _changedInGame;

	/// <summary>The build the selected set gave when it could not be applied whole, or null.</summary>
	private ClassSet? _appliedInPart;

	/// <summary>Initializes a new instance of the <see cref="ClassSetDraft"/> class.</summary>
	/// <param name="book">The character's class sets, as read.</param>
	/// <param name="canChange">False for a read-only character: the sets are listed and applied, never changed.</param>
	/// <param name="opening">The character's build when the editor opens.</param>
	public ClassSetDraft(ClassSetBook book, bool canChange, ClassSet opening)
	{
		this._opening = opening;
		this._sets = [.. book.Sets];
		this.CanChange = canChange && !book.ReadFailed;

		// The set remembered stays selected while the character keeps its masteries; its points may have changed since.
		if (this.Find(book.LastApplied) is { } last && last.SameMasteries(opening))
		{
			this.Selected = last.Name;
			this._changedInGame = !last.SameBuild(opening);
		}

		this._openingSelected = this.Selected;
	}

	/// <summary>The class sets, in the order of the file.</summary>
	public IReadOnlyList<ClassSet> Sets => this._sets;

	/// <summary>The names of the class sets.</summary>
	public IReadOnlyList<string> Names => [.. this._sets.Select(s => s.Name)];

	/// <summary>False for a read-only character or a file that could not be read.</summary>
	public bool CanChange { get; }

	/// <summary>The set applied or saved last, or null.</summary>
	public string? Selected { get; private set; }

	/// <summary>True when OK has something to write: a set saved, renamed or deleted, or another set selected than at the opening.</summary>
	public bool Changed => this._setsChanged || !string.Equals(this.Selected, this._openingSelected, StringComparison.OrdinalIgnoreCase);

	/// <summary>The set of that name (case ignored), or null.</summary>
	public ClassSet? Find(string? name)
		=> this._sets.FirstOrDefault(s => string.Equals(s.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

	/// <summary>Selects a set to apply it.</summary>
	/// <returns>The set, or null when there is none of that name.</returns>
	public ClassSet? Select(string name)
	{
		var set = this.Find(name);
		if (set is not null)
		{
			this.Selected = set.Name;
			this._changedInGame = false;
			this._appliedInPart = null;
		}

		return set;
	}

	/// <summary>The selected set could not be applied whole: the build it gave is marked as such until it changes.</summary>
	/// <param name="shown">The build shown after the apply.</param>
	public void AppliedInPart(ClassSet shown) => this._appliedInPart = this.Selected is null ? null : shown;

	/// <summary>Save as: a new set holding the build shown, which becomes the selected set.</summary>
	/// <param name="name">Its name.</param>
	/// <param name="build">The build shown; its name is not used.</param>
	public SetNameError SaveAs(string name, ClassSet build)
	{
		if (!this.CanChange)
			return SetNameError.None;
		if (SetNames.Validate(this.Names, name) is var error and not SetNameError.None)
			return error;

		this._sets.Add(build with { Name = name.Trim() });
		this.Selected = name.Trim();
		this._changedInGame = false;
		this._appliedInPart = null;
		this._setsChanged = true;
		return SetNameError.None;
	}

	/// <summary>Save: the selected set takes the build shown; it keeps its name and place.</summary>
	/// <param name="build">The build shown; its name is not used.</param>
	/// <returns>False with no set selected, or when the sets cannot change.</returns>
	public bool Save(ClassSet build)
	{
		if (!this.CanChange || this.Find(this.Selected) is not { } set)
			return false;

		this._sets[this._sets.IndexOf(set)] = build with { Name = set.Name };
		this._changedInGame = false;
		this._appliedInPart = null;
		this._setsChanged = true;
		return true;
	}

	/// <summary>Renames a set; the selection follows it.</summary>
	public SetNameError Rename(string name, string newName)
	{
		if (!this.CanChange || this.Find(name) is not { } set)
			return SetNameError.None;
		if (SetNames.Validate(this.Names, newName, set.Name) is var error and not SetNameError.None)
			return error;

		this._sets[this._sets.IndexOf(set)] = set with { Name = newName.Trim() };
		if (this.Selected == set.Name)
			this.Selected = newName.Trim();
		this._setsChanged = true;
		return SetNameError.None;
	}

	/// <summary>Deletes a set; deleting the selected one leaves none selected.</summary>
	/// <returns>False when there is no such set or the sets cannot change.</returns>
	public bool Delete(string name)
	{
		if (!this.CanChange || this.Find(name) is not { } set)
			return false;

		this._sets.Remove(set);
		if (this.Selected == set.Name)
		{
			this.Selected = null;
			this._changedInGame = false;
			this._appliedInPart = null;
		}

		this._setsChanged = true;
		return true;
	}

	/// <summary>How the build shown differs from the selected set, or null when it holds it (or none is selected).</summary>
	/// <param name="current">The build shown.</param>
	public ClassSetStatus? Status(ClassSet current)
		=> this.Find(this.Selected) is { } set && !set.SameBuild(current)
			? new ClassSetStatus(this.ChangeOf(current), current.DifferenceFrom(set))
			: null;

	// The game or a partial apply while the build shown is still the one they gave; any edit since makes it the editor's.
	private ClassSetChange ChangeOf(ClassSet current)
		=> this._appliedInPart is { } partial && current.SameBuild(partial) ? ClassSetChange.AppliedInPart
			: this._changedInGame && current.SameBuild(this._opening) ? ClassSetChange.InGame
			: ClassSetChange.InEditor;
}

/// <summary>The build shown differs from the selected class set.</summary>
/// <param name="Change">Where the difference comes from.</param>
/// <param name="Difference">What the build shown has more or less than the set.</param>
public sealed record ClassSetStatus(ClassSetChange Change, ClassSetDifference Difference);

/// <summary>Where the difference between the build shown and the selected class set comes from.</summary>
public enum ClassSetChange
{
	/// <summary>Edits in the editor since the set was applied or saved.</summary>
	InEditor,

	/// <summary>The save already differed at the opening: points spent in the game since the set was saved.</summary>
	InGame,

	/// <summary>The set was applied in part: too few points or mastery slots.</summary>
	AppliedInPart,
}

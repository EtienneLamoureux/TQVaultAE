using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// The class sets of the character edited in the character editor, changed in memory: added, renamed, deleted or
/// updated sets are written on OK (<see cref="Contracts.Services.IClassSetService.Write"/>) and dropped by Cancel. The
/// selected set is the one applied last: later edits in the editor do not change it.
/// </summary>
public sealed class ClassSetDraft
{
	private readonly List<ClassSet> _sets;

	/// <summary>Initializes a new instance of the <see cref="ClassSetDraft"/> class.</summary>
	/// <param name="book">The character's class sets, as read.</param>
	/// <param name="canChange">False for a read-only character: the sets are listed and applied, never changed.</param>
	public ClassSetDraft(ClassSetBook book, bool canChange)
	{
		this._sets = [.. book.Sets];
		this.CanChange = canChange && !book.ReadFailed;
	}

	/// <summary>The class sets, in the order of the file.</summary>
	public IReadOnlyList<ClassSet> Sets => this._sets;

	/// <summary>The names of the class sets.</summary>
	public IReadOnlyList<string> Names => [.. this._sets.Select(s => s.Name)];

	/// <summary>False for a read-only character or a file that could not be read.</summary>
	public bool CanChange { get; }

	/// <summary>True once a set was added, renamed, deleted or updated: the file is written on OK.</summary>
	public bool Changed { get; private set; }

	/// <summary>The set applied last, or null.</summary>
	public string? Selected { get; private set; }

	/// <summary>The set of that name (case ignored), or null.</summary>
	public ClassSet? Find(string? name)
		=> this._sets.FirstOrDefault(s => string.Equals(s.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

	/// <summary>Selects a set to apply it.</summary>
	/// <returns>The set, or null when there is none of that name.</returns>
	public ClassSet? Select(string name)
	{
		var set = this.Find(name);
		if (set is not null)
			this.Selected = set.Name;

		return set;
	}

	/// <summary>Selects the first set holding the same build as the character, or none: what the editor shows at its opening.</summary>
	/// <param name="current">The character's build, captured.</param>
	public void SelectMatching(ClassSet current)
		=> this.Selected = this._sets.FirstOrDefault(s => s.SameBuild(current))?.Name;

	/// <summary>Adds an empty set (no mastery, no attribute point); the selection stays.</summary>
	public SetNameError Add(string name)
	{
		if (!this.CanChange)
			return SetNameError.None;
		if (SetNames.Validate(this.Names, name) is var error and not SetNameError.None)
			return error;

		this._sets.Add(ClassSet.Empty(name.Trim()));
		this.Changed = true;
		return SetNameError.None;
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
		this.Changed = true;
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
			this.Selected = null;
		this.Changed = true;
		return true;
	}

	/// <summary>
	/// Writes a build (the character's now) into another set than the selected one, which keeps its name and place.
	/// </summary>
	/// <param name="target">The set written.</param>
	/// <param name="build">The build; its name is not used.</param>
	/// <returns>False when the target is the selected set, missing, or the sets cannot change.</returns>
	public bool Update(string target, ClassSet build)
	{
		if (!this.CanChange || this.Find(target) is not { } set || set.Name == this.Selected)
			return false;

		this._sets[this._sets.IndexOf(set)] = build with { Name = set.Name };
		this.Changed = true;
		return true;
	}
}

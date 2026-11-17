using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>What the user asked the class set bar for.</summary>
public enum ClassSetBarAction
{
	/// <summary>A class set is picked in the list: it is applied to the character edited.</summary>
	Select,

	/// <summary>Adds an empty class set ([+] button).</summary>
	Add,

	/// <summary>Writes the character's build now into another class set than the selected one (menu: the set picked in its submenu).</summary>
	Update,

	/// <summary>Renames the selected class set.</summary>
	Rename,

	/// <summary>Deletes the selected class set.</summary>
	Delete,
}

/// <summary>
/// The class sets of the character in the character editor (saved builds: masteries, skill points, attribute points),
/// in the look of the loadout bar (<see cref="GameListBar{TAction}"/>). The selected set is the one applied last: none
/// at the opening unless one holds the character's build.
/// </summary>
public sealed class ClassSetBar : GameListBar<ClassSetBarAction>
{
	public ClassSetBar(IUIService uiService, IFontService fontService)
		: base(uiService, fontService)
	{
	}

	protected internal override ClassSetBarAction SelectAction => ClassSetBarAction.Select;

	protected internal override ClassSetBarAction AddAction => ClassSetBarAction.Add;

	protected override string NoneText => Resources.ClassSetNone;

	protected override string PlaceholderText => Resources.ClassSetPlaceholder;

	protected override string AddToolTip => Resources.ClassSetAdd;

	protected override string ReadOnlyText => Resources.ClassSetReadOnly;

	protected override bool SelectsFirst => false;

	/// <summary>The class sets Update can write: all but the selected one.</summary>
	internal IReadOnlyList<string> UpdateTargets => [.. this.Names.Where(n => n != this.Selected)];

	/// <summary>Shows the class sets of the character.</summary>
	/// <param name="names">Their names.</param>
	/// <param name="selected">The one applied last, or null.</param>
	/// <param name="canChange">False for a read-only character: the sets can be applied, not changed.</param>
	public void ShowClassSets(IReadOnlyList<string> names, string? selected, bool canChange)
		=> this.SetList(names, selected, canChange);

	/// <inheritdoc/>
	public override bool CanAct(ClassSetBarAction action) => action switch
	{
		ClassSetBarAction.Select => this.Names.Count > 0,
		ClassSetBarAction.Add => this.CanChange,
		ClassSetBarAction.Update => this.CanChange && this.UpdateTargets.Count > 0,
		_ => this.CanChange && this.Selected is not null,
	};

	/// <summary>Why an action is greyed: a read-only character; for Update, no other set to write into.</summary>
	public override string? LockedReason(ClassSetBarAction action)
		=> action == ClassSetBarAction.Update && this.CanChange && !this.CanAct(action) ? Resources.ClassSetUpdateNone
		: base.LockedReason(action);

	protected override void FillActions(ToolStripItemCollection items)
		=> items.AddRange(
		[
			this.SubMenu(Resources.ClassSetUpdate, this.CanAct(ClassSetBarAction.Update),
				this.UpdateTargets.Select(name => new ToolStripMenuItem(name) { Tag = new ListBarTarget<ClassSetBarAction>(ClassSetBarAction.Update, name) }),
				this.LockedReason(ClassSetBarAction.Update) ?? Resources.ClassSetUpdateTip),
			this.Entry(Resources.ClassSetRename, ClassSetBarAction.Rename),
			this.Entry(Resources.ClassSetDelete, ClassSetBarAction.Delete),
			new ToolStripSeparator(),
			this.Entry(Resources.ClassSetAddMenu, ClassSetBarAction.Add),
		]);
}

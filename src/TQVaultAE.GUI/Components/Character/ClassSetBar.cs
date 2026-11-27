using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>What the user asked the class set bar for.</summary>
public enum ClassSetBarAction
{
	/// <summary>A class set is picked in the list: it is applied to the character edited.</summary>
	Select,

	/// <summary>Save as: a new class set holding the build shown ([+] button, menu).</summary>
	SaveAs,

	/// <summary>Save: the selected class set takes the build shown.</summary>
	Save,

	/// <summary>Renames the selected class set.</summary>
	Rename,

	/// <summary>Deletes the selected class set.</summary>
	Delete,
}

/// <summary>
/// The class sets of the character in the character editor (saved builds: masteries, skill points, attribute points),
/// in the look of the loadout bar (<see cref="GameListBar{TAction}"/>). The selected set is the one applied or saved
/// last; a mark in the box tells when the build shown differs from it.
/// </summary>
public sealed class ClassSetBar : GameListBar<ClassSetBarAction>
{
	public ClassSetBar(IUIService uiService, IFontService fontService)
		: base(uiService, fontService)
	{
	}

	protected internal override ClassSetBarAction SelectAction => ClassSetBarAction.Select;

	protected internal override ClassSetBarAction AddAction => ClassSetBarAction.SaveAs;

	protected override string NoneText => Resources.ClassSetNone;

	protected override string PlaceholderText => Resources.ClassSetPlaceholder;

	protected override string AddToolTip => Resources.ClassSetAdd;

	protected override string ReadOnlyText => Resources.ClassSetReadOnly;

	protected override bool SelectsFirst => false;

	/// <summary>Shows the class sets of the character.</summary>
	/// <param name="names">Their names.</param>
	/// <param name="selected">The one applied or saved last, or null.</param>
	/// <param name="canChange">False for a read-only character: the sets can be applied, not changed.</param>
	/// <param name="statusToolTip">How the build shown differs from the selected set (the mark's tool tip), or null when it holds it.</param>
	public void ShowClassSets(IReadOnlyList<string> names, string? selected, bool canChange, string? statusToolTip)
	{
		this.SetList(names, selected, canChange);
		this.SetMark(statusToolTip);
	}

	/// <inheritdoc/>
	public override bool CanAct(ClassSetBarAction action) => action switch
	{
		ClassSetBarAction.Select => this.Names.Count > 0,
		ClassSetBarAction.SaveAs => this.CanChange,
		_ => this.CanChange && this.Selected is not null,
	};

	/// <summary>Why an action is greyed: a read-only character; for Save, no set selected.</summary>
	public override string? LockedReason(ClassSetBarAction action)
		=> action == ClassSetBarAction.Save && this.CanChange && !this.CanAct(action) ? Resources.ClassSetSaveNone
		: base.LockedReason(action);

	protected override void FillActions(ToolStripItemCollection items)
		=> items.AddRange(
		[
			this.Entry(Resources.ClassSetSave, ClassSetBarAction.Save),
			this.Entry(Resources.ClassSetSaveAs, ClassSetBarAction.SaveAs),
			this.Entry(Resources.ClassSetRename, ClassSetBarAction.Rename),
			this.Entry(Resources.ClassSetDelete, ClassSetBarAction.Delete),
		]);
}

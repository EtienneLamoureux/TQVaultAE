using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The class sets of the character editor (saved builds: masteries, skill points, attribute points), in the bar under
/// the mastery pages: picking a set applies it to the editor; Save, Save as, Rename and Delete change the sets in memory,
/// written on OK with the character's edits (and the set selected, shown at the next opening) and dropped by Cancel. A
/// mark in the box tells when the build shown differs from the selected set.
/// </summary>
internal sealed partial class CharacterEditorUserControl
{
	private ClassSetBar _classSetBar = null!;
	private ClassSetBook _classSetBook = null!;
	private ClassSetDraft _classSets = null!;

	/// <summary>The class set bar, for the automation endpoint.</summary>
	internal ClassSetBar ClassSetBar => this._classSetBar;

	/// <summary>The class sets as edited, for the automation endpoint.</summary>
	internal ClassSetDraft ClassSets => this._classSets;

	/// <summary>The class sets as read, for the automation endpoint.</summary>
	internal ClassSetBook ClassSetBook => this._classSetBook;

	/// <summary>
	/// Reads the character's class sets and builds the bar; the set selected at the last OK shows as selected while the
	/// character keeps its masteries. Called once the masteries and the attributes are shown.
	/// </summary>
	private void InitClassSets()
	{
		var folder = Path.GetDirectoryName(this.PlayerCollection.PlayerFile) ?? string.Empty;
		this._classSetBook = this.Services.ClassSets.Read(folder);
		this._classSets = new ClassSetDraft(this._classSetBook, canChange: !this.PlayerCollection.IsReadOnly, this.CaptureClassSet(string.Empty));

		this._classSetBar = new ClassSetBar(this.UIService, this.FontService) { Name = "ClassSetBar" };
		this._classSetBar.ActionRequested += (_, e) => this.ClassSetAction(e.Action, e.Name);
		this._content.Controls.Add(this._classSetBar);
		this.ShowClassSets();
	}

	/// <summary>Shows the sets, the selected one and the mark: after each action and each change of the build shown.</summary>
	private void ShowClassSets()
	{
		if (this._classSets is null)
			return;

		var status = this._classSets.Status(this.CaptureClassSet(string.Empty));
		this._classSetBar.ShowClassSets(this._classSets.Names, this._classSets.Selected, this._classSets.CanChange,
			status is null ? null : ClassSetText.StatusToolTip(status, this.MasteryName));
	}

	/// <summary>The name of a mastery in the game's language.</summary>
	private string MasteryName(Masteries mastery)
		=> this._skills?.KnownMasteries.FirstOrDefault(t => t.Mastery == mastery) is { } tree
			? this.TranslationService.TranslateOr(tree.NameTag, mastery.ToString())
			: mastery.ToString();

	/// <summary>The build shown in the editor now: masteries with their skill levels, attribute points.</summary>
	internal ClassSet CaptureClassSet(string name)
		=> new(name, this._skills?.CaptureClassSet() ?? [],
			ClassSetAttributeRules.PointsOf([.. this.AttributeBoxes.Select(b => Convert.ToInt32(b.Value))]));

	private void ClassSetAction(ClassSetBarAction action, string? name)
	{
		switch (action)
		{
			case ClassSetBarAction.Select when name is not null:
				if (this._classSets.Select(name) is { } set)
					this.ApplyClassSet(set);
				break;
			case ClassSetBarAction.SaveAs:
				if (this.AskClassSetName(string.Empty) is { } added)
					this._classSets.SaveAs(added, this.CaptureClassSet(added));
				break;
			case ClassSetBarAction.Save when name is not null:
				if (this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.ClassSetSaveConfirm, name), Resources.ClassSetTitle, MessageBoxIcon.Question))
					this._classSets.Save(this.CaptureClassSet(name));
				break;
			case ClassSetBarAction.Rename when name is not null:
				if (this.AskClassSetName(name, name) is { } renamed)
					this._classSets.Rename(name, renamed);
				break;
			case ClassSetBarAction.Delete when name is not null:
				if (this.Services.MessageBoxes.Confirm(string.Format(CultureInfo.CurrentCulture, Resources.ClassSetDeleteConfirm, name), Resources.ClassSetTitle, MessageBoxIcon.Question))
					this._classSets.Delete(name);
				break;
		}

		this.ShowClassSets();
	}

	/// <summary>Asks a class set name until it is valid or the dialog is cancelled.</summary>
	private string? AskClassSetName(string proposed, string? current = null)
		=> InputDialog.AskSetName(this.Services, Resources.ClassSetNamePrompt, Resources.ClassSetTitle, proposed
			, name => SetNames.Validate(this._classSets.Names, name, current)
			, (name, error) => error == SetNameError.AlreadyUsed
				? string.Format(CultureInfo.CurrentCulture, Resources.ClassSetNameUsed, this.PlayerCollection.PlayerName, this._classSets.Find(name)?.Name ?? name)
				: Resources.ClassSetNameEmpty);

	/// <summary>
	/// Applies a class set to the editor (<see cref="ClassSetApplication.Apply"/>, as the read-only panels do): the
	/// masteries and their skills, then the attributes, the lowered ones first so that their points pay for the raised
	/// ones. Every point the set does not spend stays in the unspent pools. OK applies it to the character, Cancel drops it.
	/// </summary>
	private void ApplyClassSet(ClassSet set)
	{
		if (this._skills is not { } skills)
			return;

		var boxes = this.AttributeBoxes;
		var values = boxes.Select(b => Convert.ToInt32(b.Value)).ToArray();
		var pool = Convert.ToInt32(this.attributeNumericUpDown.Value);
		var applied = ClassSetApplication.Apply(skills, values, pool, set);
		var targets = applied.Attributes;
		this.RefreshSkills();

		// The pool box takes every point given back, as for Reset Attributes: a value past its maximum would be dropped.
		var total = pool + PlayerLevel.GetInvestedAttributePoints(values);
		if (total > this.attributeNumericUpDown.Maximum)
			this.attributeNumericUpDown.Maximum = total;

		foreach (var i in PlayerLevel.AttributeUndoOrder(values, targets))
		{
			if (targets[i] > boxes[i].Maximum)
				boxes[i].Maximum = targets[i];
			boxes[i].Value = Math.Max(targets[i], Convert.ToInt32(boxes[i].Minimum));
		}

		if (!applied.Complete)
			this._classSets?.AppliedInPart(this.CaptureClassSet(string.Empty));

		this.InvalidateStats();
		if (!applied.Complete)
			this.Services.MessageBoxes.Inform(string.Format(CultureInfo.CurrentCulture, Resources.ClassSetPartlyApplied, set.Name), Resources.ClassSetTitle);
	}

	/// <summary>Writes the class sets changed in the editor, on OK.</summary>
	/// <returns>False when the file could not be written (the error is shown; the editor stays open).</returns>
	private bool SaveClassSets()
	{
		if (!this._classSets.Changed)
			return true;

		try
		{
			this.Services.ClassSets.Write(this._classSetBook, this._classSets.Sets, this._classSets.Selected);
			return true;
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to write the class sets {File}", this._classSetBook.FilePath);
			this.Services.MessageBoxes.Inform(string.Format(CultureInfo.CurrentCulture, Resources.ClassSetWriteFailed, this.PlayerCollection.PlayerName, ex.Message), Resources.ClassSetTitle, MessageBoxIcon.Error);
			return false;
		}
	}

	/// <summary>
	/// The class set bar in the room under the frame between Reset Masteries and OK, level with the buttons, in the
	/// statistics panel's look.
	/// </summary>
	private void PlaceClassSetBar(int buttonsTop, int gap)
	{
		var scale = this.GameScale * ColumnScale;
		var left = this.ResetMasteriesScalingButton.Right + gap;
		var room = Math.Max(0, this.ok.Left - gap - left);
		var width = Math.Min(room, (int)Math.Round(260 * scale));
		var height = GameListBar.HeightAt(scale);
		this._classSetBar.BarScale = scale;
		this._classSetBar.TextFont = this._stats.TextFont;
		this._classSetBar.Bounds = new Rectangle(left + ((room - width) / 2), buttonsTop + ((this.ok.Height - height) / 2), width, height);
	}
}

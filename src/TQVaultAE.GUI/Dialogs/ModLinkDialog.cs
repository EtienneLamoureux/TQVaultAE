using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Links a custom map character to an installed mod, or removes its link. It can suggest the mod from the records the
/// character uses, but only the user links.
/// </summary>
public class ModLinkDialog : Form
{
	private readonly IModLinkService ModLinkService;
	private readonly IGameMapService GameMapService;
	private readonly IMessageBoxService MessageBoxes;

	private readonly Label labelCharacter;
	private readonly Label labelState;
	private readonly Label labelMods;
	private readonly ListBox listBoxMods;
	private readonly Label labelFolder;
	private readonly Label labelSuggestion;
	private readonly Button buttonSuggest;
	private readonly Button buttonLink;
	private readonly Button buttonUnlink;
	private readonly Button buttonClose;

	private PlayerSave? _save;

	/// <summary>
	/// The character whose link is managed. Set before showing the dialog.
	/// </summary>
	public PlayerSave? Save
	{
		get => this._save;
		set
		{
			this._save = value;
			if (value is not null)
				this.ShowCharacter();
		}
	}

	/// <summary>
	/// True when the dialog linked or unlinked the character.
	/// </summary>
	public bool LinkChanged { get; private set; }

	public ModLinkDialog(IModLinkService modLinkService, IGameMapService gameMapService, IMessageBoxService messageBoxes)
	{
		this.ModLinkService = modLinkService;
		this.GameMapService = gameMapService;
		this.MessageBoxes = messageBoxes;

		const int width = 480;
		Label NewLabel(string name) => new()
		{
			Name = name,
			AutoSize = true,
			MaximumSize = new Size(width, 0),
			Margin = new Padding(0, 0, 0, 6),
		};

		this.labelCharacter = NewLabel(nameof(this.labelCharacter));
		this.labelCharacter.Font = new Font(this.Font, FontStyle.Bold);
		this.labelState = NewLabel(nameof(this.labelState));
		this.labelMods = NewLabel(nameof(this.labelMods));
		this.labelMods.Text = Resources.ModLinkInstalledMods;
		this.listBoxMods = new ListBox()
		{
			Name = nameof(this.listBoxMods),
			Width = width,
			Height = 140,
			IntegralHeight = false,
			Margin = new Padding(0, 0, 0, 4),
		};
		this.listBoxMods.SelectedIndexChanged += (_, _) => this.UpdateButtons();
		this.listBoxMods.DoubleClick += (_, _) => this.LinkSelected();
		this.labelFolder = NewLabel(nameof(this.labelFolder));
		this.labelFolder.ForeColor = SystemColors.GrayText;
		this.labelSuggestion = NewLabel(nameof(this.labelSuggestion));

		Button NewButton(string name, string text) => new()
		{
			Name = name,
			Text = text,
			AutoSize = true,
			MinimumSize = new Size(75, 23),
			UseVisualStyleBackColor = true,
		};

		this.buttonSuggest = NewButton(nameof(this.buttonSuggest), Resources.ModLinkSuggest);
		this.buttonSuggest.Click += (_, _) => this.SuggestMod();
		this.buttonLink = NewButton(nameof(this.buttonLink), Resources.ModLinkLink);
		this.buttonLink.Click += (_, _) => this.LinkSelected();
		this.buttonUnlink = NewButton(nameof(this.buttonUnlink), Resources.ModLinkUnlink);
		this.buttonUnlink.Click += (_, _) => this.UnlinkCharacter();
		this.buttonClose = NewButton(nameof(this.buttonClose), Resources.GlobalClose);
		this.buttonClose.DialogResult = DialogResult.Cancel;

		var buttons = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, 6, 0, 0),
			WrapContents = false,
		};
		buttons.Controls.AddRange([this.buttonSuggest, this.buttonLink, this.buttonUnlink, this.buttonClose]);

		var layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
		};
		layout.Controls.AddRange([this.labelCharacter, this.labelState, this.labelMods, this.listBoxMods, this.labelFolder, this.labelSuggestion, buttons]);

		this.SuspendLayout();
		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.AutoSize = true;
		this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		this.CancelButton = this.buttonClose;
		this.Controls.Add(layout);
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = nameof(ModLinkDialog);
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = Resources.ModLinkDialogTitle;
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	private InstalledMod? SelectedMod => this.listBoxMods.SelectedItem as InstalledMod;

	/// <summary>
	/// Fills the dialog for <see cref="Save"/>: the installed mods, read again, and its link.
	/// </summary>
	private void ShowCharacter()
	{
		var save = this._save!;
		this.labelCharacter.Text = string.Format(CultureInfo.CurrentCulture, Resources.ModLinkCharacter, save.Name);

		var mods = this.ModLinkService.GetInstalledMods(refresh: true);
		this.listBoxMods.BeginUpdate();
		this.listBoxMods.Items.Clear();
		foreach (var mod in mods)
			this.listBoxMods.Items.Add(mod);
		this.listBoxMods.EndUpdate();

		this.labelMods.Text = mods.Count == 0 ? Resources.ModLinkNoMods : Resources.ModLinkInstalledMods;
		this.labelSuggestion.Text = string.Empty;
		this.ShowState(selectLinked: true);
	}

	/// <summary>
	/// Shows the character's link, after it was read again.
	/// </summary>
	private void ShowState(bool selectLinked)
	{
		var save = this._save!;
		var mod = this.GameMapService.ModOf(save);

		// Without a link to an installed mod, a character is loaded with the base game data.
		string loadedWith = Resources.ModLinkBaseGameData;

		this.labelState.Text = mod.State switch
		{
			CharacterModState.BaseGame => Resources.ModLinkStateBaseGame,
			CharacterModState.Linked => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateLinked, mod.Mod!.DisplayName),
			CharacterModState.LinkedModMissing => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateMissing, mod.Link!.Mod, loadedWith),
			_ => string.Format(CultureInfo.CurrentCulture, Resources.ModLinkStateUnlinked, loadedWith),
		};

		if (selectLinked)
			this.listBoxMods.SelectedItem = mod.Mod;

		this.listBoxMods.Enabled = save.IsCustom;
		this.buttonSuggest.Enabled = save.IsCustom;
		this.buttonUnlink.Enabled = mod.Link is not null;
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		var mod = this.GameMapService.ModOf(this._save!);
		var selected = this.SelectedMod;
		this.labelFolder.Text = selected?.Folder ?? string.Empty;
		this.buttonLink.Enabled = this._save!.IsCustom && selected is not null && !Equals(selected, mod.Mod);
	}

	private void LinkSelected()
	{
		if (this.SelectedMod is not { } mod || !this.buttonLink.Enabled)
			return;

		var result = this.ModLinkService.Link(this._save!.Folder, mod);
		this.AfterChange(result);
	}

	private void UnlinkCharacter()
	{
		var result = this.ModLinkService.Unlink(this._save!.Folder);
		this.AfterChange(result);
	}

	private void AfterChange(Application.Results.ModLinkResult result)
	{
		if (!result.IsSuccess)
		{
			this.MessageBoxes.ShowException(result.Error, Resources.ModLinkSaveError, MessageBoxIcon.Error);
			return;
		}

		this.LinkChanged = true;
		this.GameMapService.Refresh(this._save!);
		this.ShowState(selectLinked: true);
	}

	/// <summary>
	/// Reads the character's records and selects the mod that has the most of those the base game lacks. Links nothing.
	/// </summary>
	private void SuggestMod()
	{
		var cursor = this.Cursor;
		this.Cursor = Cursors.WaitCursor;
		ModSuggestion suggestion;
		try
		{
			suggestion = this.ModLinkService.Suggest(this._save!.Folder);
		}
		finally
		{
			this.Cursor = cursor;
		}

		if (suggestion.OnlyBaseGameRecords)
			this.labelSuggestion.Text = Resources.ModLinkSuggestionBaseOnly;
		else if (suggestion.Matches.Count == 0)
			this.labelSuggestion.Text = string.Format(CultureInfo.CurrentCulture, Resources.ModLinkSuggestionNone, suggestion.ModOnlyRecords);
		else if (suggestion.Best is { } best)
		{
			this.labelSuggestion.Text = string.Format(CultureInfo.CurrentCulture, Resources.ModLinkSuggestion, best.Mod.DisplayName, best.Matched, suggestion.ModOnlyRecords);
			this.listBoxMods.SelectedItem = best.Mod;
		}
		else
		{
			int top = suggestion.Matches[0].Matched;
			var tied = suggestion.Matches.TakeWhile(m => m.Matched == top).Select(m => m.Mod.DisplayName);
			this.labelSuggestion.Text = string.Format(CultureInfo.CurrentCulture, Resources.ModLinkSuggestionTie, string.Join(", ", tied));
		}
	}
}

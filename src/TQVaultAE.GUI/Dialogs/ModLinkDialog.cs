using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Links a custom map character to an installed mod, or removes its link. It can suggest the mod from the records the
/// character uses, but only the user links. A TQ window like the other dialogs: marble, the mod list in a caravan frame,
/// gold buttons; laid out in code by a table, at the user's zoom, and following the DPI and the base font.
/// </summary>
internal sealed class ModLinkDialog : VaultForm
{
	/// <summary>Width of the mod list and of the text lines, at zoom 1.</summary>
	private const int ContentWidth = 600;

	private readonly IModLinkService ModLinkService;
	private readonly IGameMapService GameMapService;
	private readonly IMessageBoxService MessageBoxes;

	private readonly ScalingLabel labelCharacter;
	private readonly ScalingLabel labelState;
	private readonly ScalingLabel labelMods;
	private readonly ListBox listBoxMods;
	private readonly ScalingLabel labelFolder;
	private readonly ScalingLabel labelSuggestion;
	private readonly ScalingButton buttonSuggest;
	private readonly ScalingButton buttonLink;
	private readonly ScalingButton buttonUnlink;
	private readonly ScalingButton buttonClose;

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

	public ModLinkDialog(MainForm instance, IModLinkService modLinkService, IGameMapService gameMapService) : base(instance.Services)
	{
		this.Owner = instance;
		this.ModLinkService = modLinkService;
		this.GameMapService = gameMapService;
		this.MessageBoxes = this.Services.MessageBoxes;

		// Sizes at zoom 1 and 96 dpi: the zoom is applied here, the DPI by Windows Forms (AutoScaleMode.Dpi).
		float zoom = this.UIService.Zoom;
		int Zoomed(int value) => (int)Math.Round(value * zoom);
		int width = Zoomed(ContentWidth);

		ScalingLabel NewLabel(string name) => new()
		{
			Name = name,
			AutoSize = true,
			MaximumSize = new Size(width, 0),
			Margin = new Padding(0, 0, 0, Zoomed(6)),
		};

		this.labelCharacter = NewLabel(nameof(this.labelCharacter));
		this.labelCharacter.Font = this.FontService.GetFontLight(13F, FontStyle.Bold, zoom);
		this.labelState = NewLabel(nameof(this.labelState));
		this.labelMods = NewLabel(nameof(this.labelMods));
		this.labelMods.Text = Resources.ModLinkInstalledMods;

		// The list draws its own content from its corner: the frame goes around it, on the table (CaravanFrame.FrameAround).
		this.listBoxMods = new ListBox()
		{
			Name = nameof(this.listBoxMods),
			BackColor = Color.Black,
			BorderStyle = BorderStyle.None,
			ForeColor = TQColor.Silver.Color(),
			IntegralHeight = false,
			Size = new Size(width - (2 * CaravanFrame.Border), Zoomed(150)),
		};
		this.listBoxMods.SelectedIndexChanged += (_, _) => this.UpdateButtons();
		this.listBoxMods.DoubleClick += (_, _) => this.LinkSelected();
		this.labelFolder = NewLabel(nameof(this.labelFolder));
		this.labelFolder.ForeColor = TQColor.Silver.Color();
		// A folder has no space to wrap at: a longer one ends with an ellipsis.
		this.labelFolder.AutoEllipsis = true;
		this.labelSuggestion = NewLabel(nameof(this.labelSuggestion));

		ScalingButton NewButton(string name, string text)
		{
			var button = new ScalingButton()
			{
				Name = name,
				BackColor = Color.Transparent,
				FlatStyle = FlatStyle.Flat,
				Font = this.FontService.GetFontLight(12F, zoom),
				ForeColor = Theme.ButtonText,
				Margin = new Padding(Zoomed(3), 0, Zoomed(3), 0),
				Size = new Size(Zoomed(132), Zoomed(30)),
				SizeToGraphic = false,
				UseCustomGraphic = true,
				UseVisualStyleBackColor = false,
			};
			button.FlatAppearance.BorderSize = 0;
			button.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
			button.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
			// After the font: the button fits its text to its width.
			button.Text = text;
			return button;
		}

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
			Anchor = AnchorStyles.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, Zoomed(8), 0, 0),
			WrapContents = false,
		};
		buttons.Controls.AddRange([this.buttonSuggest, this.buttonLink, this.buttonUnlink, this.buttonClose]);

		var layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Dock = DockStyle.Fill,
			Margin = new Padding(0),
		};
		layout.Controls.AddRange([this.labelCharacter, this.labelState, this.labelMods, this.listBoxMods, this.labelFolder, this.labelSuggestion, buttons]);
		CaravanFrame.FrameAround(this.listBoxMods);

		this.SuspendLayout();
		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.AutoSize = true;
		this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		this.BackColor = Color.FromArgb(46, 31, 21);
		this.CancelButton = this.buttonClose;
		this.Font = this.FontService.GetFontLight(11F, zoom);
		this.ForeColor = Color.White;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.Name = nameof(ModLinkDialog);
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = Resources.ModLinkDialogTitle;
		var border = CustomBorderThickness;
		int gap = Zoomed(10);
		this.Padding = new Padding(border.Left + gap, border.Top + Zoomed(4), border.Right + gap, border.Bottom + Zoomed(6));
		this.Controls.Add(layout);
		this.DrawCustomBorder = true;
		CaravanLook.Apply(this, layout, buttons);
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

		this.labelState.Text = ModLinkText.State(mod);

		// A link by a name several installed mods have: the user links again to the right one.
		if (mod is { Ambiguous: true, Link: { } link })
			this.labelState.Text += Environment.NewLine + string.Format(CultureInfo.CurrentCulture, Resources.ModAmbiguous, link.Mod);

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
		this.buttonLink.Enabled = mod.CanLinkTo(this._save!.IsCustom, selected);
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

		this.labelSuggestion.Text = ModLinkText.Suggestion(suggestion);
		if (!suggestion.OnlyBaseGameRecords && suggestion.Best is { } best)
			this.listBoxMods.SelectedItem = best.Mod;
	}
}

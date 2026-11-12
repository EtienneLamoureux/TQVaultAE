﻿using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

public partial class ComboBoxCharacterItem : UserControl
{
	/// <summary>
	/// Default Height for this control
	/// </summary>
	internal const int BASE_HEIGHT = 60;

	// Set by Init: the drop down creates the item before giving it its save.
	private ICharacterEditService CharacterEditService = null!;
	private ITagService TagService = null!;
	private ITranslationService TranslationService = null!;
	private IFontService FontService = null!;

	/// <summary>
	/// a <see cref="PlayerSave"/> or a <see cref="string"/>
	/// </summary>
	public object Item { get; private set; } = string.Empty;

	/// <summary>Background of the selected character's box when it is archived.</summary>
	internal static readonly Color ArchivedBackColor = Color.Silver;

	/// <summary>Background of the selected character's box when it is read-only: an icy blue.</summary>
	internal static readonly Color ReadOnlyBackColor = Color.FromArgb(206, 232, 248);

	/// <summary>
	/// "Read-only" with the other status badges of a read-only character (<see cref="PlayerSave.IsReadOnly"/>).
	/// </summary>
	private readonly ScalingLabel labelReadOnly = new()
	{
		Name = "labelReadOnly",
		Text = Presentation.Resources.CharacterReadOnly,
		BackColor = Color.LightSkyBlue,
		AutoSize = true,
		Margin = new Padding(0, 0, 3, 0),
		Visible = false,
	};

	public ComboBoxCharacterItem()
	{
		InitializeComponent();

		this.scalingLabelTags.Text = $"{Presentation.Resources.GlobalTags} : ";
		this.scalingLabelArchived.Text = Presentation.Resources.GlobalArchived;
		this.scalingLabelModded.Text = Presentation.Resources.CharacterModded;

		// The status badges (Archived, Read-only, Modded and its link state) on the right; the tags line keeps the tags.
		this.bufferedFlowLayoutPanelTags.Controls.Remove(this.scalingLabelArchived);
		this.scalingLabelArchived.Margin = new Padding(0, 0, 3, 0);
		this.bufferedFlowLayoutPanelMod.Controls.Add(this.scalingLabelArchived);
		this.bufferedFlowLayoutPanelMod.Controls.Add(this.labelReadOnly);
		this.bufferedFlowLayoutPanelMod.Controls.SetChildIndex(this.scalingLabelArchived, 0);
		this.bufferedFlowLayoutPanelMod.Controls.SetChildIndex(this.labelReadOnly, 1);

		this.ProcessAllControls(ctr =>
		{
			ctr.Click += Ctr_Click;
			ctr.MouseEnter += Ctr_MouseEnter;
			ctr.MouseLeave += Ctr_MouseLeave;
		});
	}

	private void Ctr_MouseLeave(object? sender, EventArgs e)
	{
		this.BackColor = Color.White;
	}

	private void Ctr_MouseEnter(object? sender, EventArgs e)
	{
		this.BackColor = SystemColors.MenuHighlight;
	}

	public event EventHandler? SelectedItemChanged;
	private void Ctr_Click(object? sender, EventArgs e)
	{
		SelectedItemChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Init(object item, ITranslationService translationService, IFontService fontService, ICharacterEditService characterEditService, ITagService tagService)
	{
		this.Item = item;
		this.TranslationService = translationService;
		this.FontService = fontService;
		this.CharacterEditService = characterEditService;
		this.TagService = tagService;

		this.scalingLabelCharName.Font = FontService.GetFont(this.scalingLabelCharName.Font.Size);
		this.scalingLabelPlayerHeader.Font =
			this.scalingLabelLevel.Font =
				this.scalingLabelCLassName.Font = FontService.GetFont(this.scalingLabelCLassName.Font.Size);
		this.scalingLabelTags.Font =
			this.scalingLabelMasteries.Font =
				this.scalingLabelModName.Font =
					this.scalingLabelModded.Font =
					this.scalingLabelModState.Font =
					this.labelReadOnly.Font =
					this.scalingLabelArchived.Font = FontService.GetFontLight(this.scalingLabelArchived.Font.Size);

		this.scalingLabelArchived.Visible =
			this.bufferedFlowLayoutPanelTags.Visible =
				this.bufferedFlowLayoutPanelSubTitle.Visible =
					this.bufferedFlowLayoutPanelMod.Visible =
					this.scalingLabelModName.Visible =
						this.scalingLabelArchived.Visible = false;

		this.scalingLabelCharName.Text =
			this.scalingLabelPlayerHeader.Text =
				this.scalingLabelLevel.Text =
					this.scalingLabelCLassName.Text = string.Empty;
	}

	public void RefreshContent()
	{
		this.Invoke(_RefreshContent);
	}

	private void _RefreshContent()
	{
		if (this.Item is PlayerSave ps)
		{
			this.scalingLabelCharName.Text = ps.Name;
			this.scalingLabelPlayerHeader.Text = ps.Info?.HeaderVersion.ToString();

			this.scalingLabelLevel.Text = ps.Info?.CurrentLevel is null
				? string.Empty
				: $"{TranslationService.TranslateXTag("tagMenuImport05")} : {ps.Info.CurrentLevel}";

			this.scalingLabelCLassName.Text = ps.Info?.Class is null
				? string.Empty
				: TranslationService.TranslateXTag(ps.Info.Class, true, true);

			// Masteries
			this.scalingLabelMasteries.Text = PlayerInfoText.MasteryNames(ps.Info, this.CharacterEditService, this.TranslationService);
			if (ps.Info?.ActiveMasteriesRecordNames is { Length: > 0 })
				this.bufferedFlowLayoutPanelSubTitle.Visible = this.scalingLabelMasteries.Visible = true;

			bool modded = this.RefreshMod(ps);

			// Status badges on the right. Decided from the states, not from the badges' Visible, which reads false while
			// their panel is hidden.
			this.scalingLabelArchived.Visible = ps.IsArchived;
			this.labelReadOnly.Visible = ps.IsReadOnly;
			this.bufferedFlowLayoutPanelMod.Visible = ps.IsArchived || ps.IsReadOnly || modded;

			#region Tags

			this.bufferedFlowLayoutPanelTags.Visible = false;

			this.ClearTagList();

			if (ps.Tags.Count != 0)
			{
				// refresh assigned tag list
				int idx = 0;
				foreach (var tag in ps.Tags)
				{
					var ctr = new ScalingLabel()
					{
						Name = ComboBoxCharacter.TAGKEY + idx++,
						Text = tag.Key,
						BackColor = tag.Value,
						Font = FontService.GetFontLight(this.scalingLabelArchived.Font.Size),
						AutoSize = true,
					};
					ctr.MouseEnter += Ctr_MouseEnter;
					ctr.MouseLeave += Ctr_MouseLeave;
					this.bufferedFlowLayoutPanelTags.Controls.Add(ctr);
				}
			}

			if (ps.Tags.Count != 0)
				this.scalingLabelTags.Visible = this.bufferedFlowLayoutPanelTags.Visible = true;

			#endregion
		}
		else if (this.Item is string str)
		{
			this.scalingLabelCharName.Text = str;
			this.scalingLabelPlayerHeader.Text =
				this.scalingLabelLevel.Text =
					this.scalingLabelCLassName.Text = string.Empty;
			this.bufferedFlowLayoutPanelMod.Visible = this.scalingLabelModName.Visible = false;
		}
	}

	/// <summary>
	/// Marks a custom map character as modded: its mod beside its name, and the Modded tag with the link state on the
	/// tags line.
	/// </summary>
	/// <returns>true when the character is modded (its badges are to be shown).</returns>
	private bool RefreshMod(PlayerSave ps)
	{
		var state = ps.IsCustom ? ps.ModState : CharacterModState.BaseGame;
		bool modded = state != CharacterModState.BaseGame;
		this.scalingLabelModded.Visible =
			this.scalingLabelModState.Visible = modded;
		this.scalingLabelModName.Visible = modded && !string.IsNullOrEmpty(ps.ModName);
		if (!modded)
			return false;

		this.scalingLabelModName.Text = $"[{ps.ModName}]";

		this.scalingLabelModState.Text = Presentation.ModLinkText.CharacterBadge(state);
		this.scalingLabelModState.BackColor = state switch
		{
			CharacterModState.Linked => Color.LightGreen,
			CharacterModState.LinkedModMissing => Color.LightSalmon,
			_ => Color.Khaki,
		};
		return true;
	}

	private void ClearTagList()
	{
		this.bufferedFlowLayoutPanelTags.Controls.OfType<ScalingLabel>()
			.Where(ctr => ctr.Name.StartsWith(ComboBoxCharacter.TAGKEY, StringComparison.Ordinal))
			.ToList().ForEach(ctr => this.bufferedFlowLayoutPanelTags.Controls.Remove(ctr));
	}
}
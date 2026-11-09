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

	public ComboBoxCharacterItem()
	{
		InitializeComponent();

		this.scalingLabelTags.Text = $"{Presentation.Resources.GlobalTags} : ";
		this.scalingLabelArchived.Text = Presentation.Resources.GlobalArchived;
		this.scalingLabelModded.Text = Presentation.Resources.CharacterModded;

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
					this.scalingLabelArchived.Font = FontService.GetFontLight(this.scalingLabelArchived.Font.Size);

		this.scalingLabelArchived.Visible =
			this.bufferedFlowLayoutPanelTags.Visible =
				this.bufferedFlowLayoutPanelSubTitle.Visible =
					this.bufferedFlowLayoutPanelMod.Visible =
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

			this.RefreshMod(ps);

			#region Tags

			this.scalingLabelArchived.Visible =
				this.bufferedFlowLayoutPanelTags.Visible = false;

			// Is Archived
			this.scalingLabelArchived.Visible = ps.IsArchived;

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

			if (ps.Tags.Count != 0 || ps.IsArchived)
				this.scalingLabelTags.Visible = this.bufferedFlowLayoutPanelTags.Visible = true;

			#endregion
		}
		else if (this.Item is string str)
		{
			this.scalingLabelCharName.Text = str;
			this.scalingLabelPlayerHeader.Text =
				this.scalingLabelLevel.Text =
					this.scalingLabelCLassName.Text = string.Empty;
			this.bufferedFlowLayoutPanelMod.Visible = false;
		}
	}

	/// <summary>
	/// Marks a custom map character as modded, with its mod and whether the link to it works.
	/// </summary>
	private void RefreshMod(PlayerSave ps)
	{
		var state = ps.IsCustom ? ps.ModState : CharacterModState.BaseGame;
		this.bufferedFlowLayoutPanelMod.Visible = state != CharacterModState.BaseGame;
		if (state == CharacterModState.BaseGame)
			return;

		this.scalingLabelModName.Text = ps.ModName ?? string.Empty;
		this.scalingLabelModName.Visible = !string.IsNullOrEmpty(ps.ModName);

		(this.scalingLabelModState.Text, this.scalingLabelModState.BackColor) = state switch
		{
			CharacterModState.Linked => (Presentation.Resources.CharacterModLinked, Color.LightGreen),
			CharacterModState.LinkedModMissing => (Presentation.Resources.CharacterModNotInstalled, Color.LightSalmon),
			_ => (Presentation.Resources.CharacterModNotLinked, Color.Khaki),
		};
	}

	private void ClearTagList()
	{
		this.bufferedFlowLayoutPanelTags.Controls.OfType<ScalingLabel>()
			.Where(ctr => ctr.Name.StartsWith(ComboBoxCharacter.TAGKEY, StringComparison.Ordinal))
			.ToList().ForEach(ctr => this.bufferedFlowLayoutPanelTags.Controls.Remove(ctr));
	}
}
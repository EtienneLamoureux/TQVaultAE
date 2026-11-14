//-----------------------------------------------------------------------
// <copyright file="MainFormUserControl.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The main content of the main window: the vault list and panel, the character list with the player, stash and
/// secondary vault panels, the highlight search and the notification line. The main window places it under its
/// toolbar, fills the rest of the window with it, and shows another view (the settings) in its place when asked.
/// </summary>
/// <remarks>
/// The view builds, wires and drives its panels and lists; the main window asks it what it needs from the window
/// (zoom, keys, toolbar) and answers its events.
/// </remarks>
public partial class MainFormUserControl : UserControl
{
	private readonly ILogger Log;
	private readonly GuiServices Services;
	private readonly SessionContext userContext;
	private readonly IHighlightService highlightService;
	private readonly ITranslationService translationService;
	private readonly ITagService tagService;

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services.
	public MainFormUserControl() => this.InitializeComponent();
#pragma warning restore CS8618
#endif

	/// <summary>
	/// Initializes a new instance of the MainFormUserControl class.
	/// </summary>
	public MainFormUserControl(GuiServices services, ITagService tagService)
	{
		this.tagService = tagService;
		this.Services = services;
		this.Log = services.Logger<MainFormUserControl>();
		this.userContext = services.Session;
		this.highlightService = services.HighlightService;
		this.translationService = services.TranslationService;

		this.InitializeComponent();

		this.vaultListComboBox.SelectedIndexChanged += this.VaultListComboBoxSelectedIndexChanged;
		this.secondaryVaultListComboBox.SelectedIndexChanged += this.SecondaryVaultListComboBoxSelectedIndexChanged;
		this.comboBoxCharacter.SelectedIndexChanged += this.CharacterComboBoxSelectedIndexChanged;
	}

	/// <summary>
	/// Sets up the view's own controls once it is in the window: the vault pictures and the character list, whose
	/// menus reach the window.
	/// </summary>
	internal void Initialize()
	{
		var toolTip = new ToolTip(this.components);

		this.scalingLabelHighlight.Text = Resources.MainFormHighlightLabelText;

		this.vaultPictureBox.Image = Images.Majestic_Chest_small;
		this.vaultPictureBox.Size = Images.Majestic_Chest_small.Size;
		toolTip.SetToolTip(this.vaultPictureBox, Resources.MainFormLabel2);

		this.pictureBoxSecondVault.Image = Images.Majestic_Chest_small;
		this.pictureBoxSecondVault.Size = Images.Majestic_Chest_small.Size;
		toolTip.SetToolTip(this.pictureBoxSecondVault, Resources.MainForm2ndVault);

		this.comboBoxCharacter.Init(
			this.Services.UIService
			, this.Services.FontService
			, this.translationService
			, this.tagService
			, this.Services.PlayerService
			, this.Services.Settings
			, () => this.DuplicateCharacter()
			, this.OpenModLinkDialog
			, this.ToggleCharacterReadOnly
		);
	}

	/// <summary>
	/// Shows or hides the cell borders of the layout panels, to see the layout while debugging.
	/// </summary>
	internal void ShowLayoutBorders(bool visible)
	{
		var border = visible ? BorderStyle.Fixed3D : BorderStyle.None;
		this.flowLayoutPanelRightComboBox.BorderStyle =
		this.flowLayoutPanelVaultSelector.BorderStyle =
		this.flowLayoutPanelRightPanels.BorderStyle =
		this.bufferedFlowLayoutPanelsecondaryVaultList.BorderStyle =
		this.pictureBoxSecondVault.BorderStyle =
		this.vaultPictureBox.BorderStyle =
		this.comboBoxCharacter.BorderStyle = border;
		this.tableLayoutPanelMain.CellBorderStyle = visible ? TableLayoutPanelCellBorderStyle.Single : TableLayoutPanelCellBorderStyle.None;
	}

	/// <summary>
	/// The fonts of the view's own controls at zoom 1: the window's zoom scales them afterwards.
	/// </summary>
	internal void ApplyFonts()
	{
		var fonts = this.Services.FontService;
		var fontZoom = this.Services.UIService.FontZoom;
		this.NotificationText.Font = fonts.GetFontLight(11F, FontStyle.Bold, fontZoom);
		this.vaultListComboBox.Font = fonts.GetFontLight(13F, fontZoom);
		this.secondaryVaultListComboBox.Font = fonts.GetFontLight(13F, fontZoom);
		this.ApplyHighlightFonts();
		this.scalingTextBoxHighlight.Font = fonts.GetFontLight(10F, fontZoom);
	}

	/// <summary>The fonts of the highlight link and of its filters panel at the current zoom.</summary>
	internal void ApplyHighlightFonts()
	{
		this.scalingLabelHighlight.Font = this.Services.FontService.GetFontLight(10F, this.Services.UIService.FontZoom);
		this.highlightFilters.ApplyFonts(this.Services.FontService, this.Services.UIService.FontZoom);
	}

	/// <summary>
	/// Shows a message on the notification line, in <paramref name="color"/> when given.
	/// </summary>
	internal void ShowNotification(string message, Color? color = null)
	{
		if (color is { } foreColor)
			this.NotificationText.ForeColor = foreColor;

		this.NotificationText.Text = message;
	}

	/// <summary>The size the panels' table needs, to size the window around it.</summary>
	internal Size PanelsPreferredSize => this.tableLayoutPanelMain.PreferredSize;

	/// <summary>
	/// Gives the focus to the highlight search box (the window is shown, or the main content again).
	/// </summary>
	/// <param name="dock">Docks the box in its cell first (when the window is first shown).</param>
	internal void FocusHighlightSearch(bool dock)
	{
		if (dock)
			this.scalingTextBoxHighlight.Dock = DockStyle.Fill;

		this.scalingTextBoxHighlight.Focus();
	}

	/// <summary>
	/// The setting of the character edition changed: the stash panel's player information shows it.
	/// </summary>
	internal void UpdatePlayerInfo() => this.stashPanel?.UpdatePlayerInfo();

	/// <summary>
	/// Makes the main content ready to be hidden: the item held on the cursor goes back to its place, the tool tips close.
	/// </summary>
	/// <returns>False when the held item cannot go back (a modified item), which the user is told: the content stays.</returns>
	internal bool PrepareToHide()
	{
		if (this.DragInfo.IsActive)
		{
			if (!this.DragInfo.CanBeCanceled)
			{
				this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
				return false;
			}

			this.DragInfo.Cancel();
		}

		Tooltips.ItemTooltip.HideTooltip();
		Tooltips.BagButtonTooltip.HideTooltip();
		Tooltips.BagButtonLabelTooltip.HideTooltip();
		Tooltips.FoundResultsTooltip.HideTooltip();
		return true;
	}

	#region HighlightItems

	private void typeAssistant_Idled(object? sender, EventArgs e)
	{
		var value = (scalingTextBoxHighlight.Text ?? string.Empty).Trim();

		this.highlightService.HighlightSearch = value;
		this.highlightService.FindHighlight();

		// The highlight is drawn by every panel of the window.
		var window = (Control?)this.FindForm() ?? this;
		this.Invoke(new System.Windows.Forms.MethodInvoker(window.Refresh));
	}

	private void scalingTextBoxHighlight_TextChanged(object? sender, EventArgs e)
	{
		/// Wait for the end of typing by delaying the call to <see cref="typeAssistant_Idled"/>
		this.typeAssistant.TextChanged();
	}

	private void scalingLabelHighlight_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
		=> this.ToggleHighlightFilters();

	/// <summary>
	/// Shows the highlight filters over their link, or hides them: what a click on the link does.
	/// </summary>
	internal void ToggleHighlightFilters()
	{
		var link = this.scalingLabelHighlight;
		if (highlightFilters.Visible)
		{
			highlightFilters.Visible = false;
			return;
		}

		// Show it
		if (highlightFilters.UserContext is null)
		{ // First time
			highlightFilters.UserContext = userContext;
			highlightFilters.TranslationService = translationService;
			highlightFilters.HighlightService = highlightService;
			highlightFilters.ResetAll();
			highlightFilters.InitializeFilters();
			highlightFilters.Link = link;
		}

		highlightFilters.SendToBack();// To avoid flickering
		highlightFilters.Visible = true;
		highlightFilters.BringToFront();
	}

	#endregion
}

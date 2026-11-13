//-----------------------------------------------------------------------
// <copyright file="MainFormUserControl.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The main content of the main window: the vault list and panel, the character list with the player, stash and
/// secondary vault panels, the highlight search and the notification line. The main window places it under its
/// toolbar, fills the rest of the window with it, and shows another view (the settings) in its place when asked.
/// </summary>
/// <remarks>
/// The panels are built and driven by the main window, which reaches these controls by name
/// (<see cref="Forms.MainForm"/>); the highlight search is the view's own.
/// </remarks>
public partial class MainFormUserControl : UserControl
{
	private readonly SessionContext userContext;
	private readonly IHighlightService highlightService;
	private readonly ITranslationService translationService;
	private readonly IFontService fontService;

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services.
	public MainFormUserControl() => this.InitializeComponent();
#pragma warning restore CS8618
#endif

	/// <summary>
	/// Initializes a new instance of the MainFormUserControl class.
	/// </summary>
	public MainFormUserControl(GuiServices services, SessionContext sessionContext)
	{
		this.userContext = sessionContext;
		this.highlightService = services.HighlightService;
		this.translationService = services.TranslationService;
		this.fontService = services.FontService;

		this.InitializeComponent();
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
			highlightFilters.FontService = fontService;
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

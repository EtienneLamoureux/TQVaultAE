using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The forge of the main content, shown in place of the character's panels and of the character list.
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>The visibility of the panels the forge hides, given back when it closes.</summary>
	private (
		bool playerPanel
		, bool stashPanel
		, bool secondaryVaultPanel
		, bool flowLayoutPanelRightComboBox
	) lastVisibility;

	/// <summary>Raised when the forge is cancelled from its own panel.</summary>
	internal event EventHandler? ForgeCancelled;

	/// <summary>Raised when an item was forged.</summary>
	internal event EventHandler? ForgeForged;

	/// <summary>Whether the forge is shown.</summary>
	internal bool ForgeShown => this.forgePanel.Visible;

	/// <summary>Forges from the window's shortcut key, as the forge's button does.</summary>
	internal void ForgeFromShortcut() => this.forgePanel.ForgeFromShortcut();

	private ForgePanel CreateForgePanel()
	{
		forgePanel = new ForgePanel(DragInfo, Services);

		flowLayoutPanelRightPanels.Controls.Add(forgePanel);
		forgePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		forgePanel.Visible = false;
		forgePanel.CancelAction = () => this.ForgeCancelled?.Invoke(this, EventArgs.Empty);
		forgePanel.ForgeAction = () => this.ForgeForged?.Invoke(this, EventArgs.Empty);

		// The forge takes the height of the panels it stands for: the character's bags and the stash under them.
		forgePanel.HeightLimit = () => this.playerPanel.Height + this.stashPanel.Height - forgePanel.Margin.Vertical;
		this.playerPanel.SizeChanged += (_, _) => forgePanel.FitHeight();
		this.stashPanel.SizeChanged += (_, _) => forgePanel.FitHeight();
		return forgePanel;
	}

	/// <summary>
	/// Shows the forge in place of the character's panels and of the character list.
	/// </summary>
	internal void ShowForge()
	{
		// Save UI visibility
		lastVisibility = (
			playerPanel: playerPanel.Visible
			, stashPanel: stashPanel.Visible
			, secondaryVaultPanel: secondaryVaultPanel.Visible
			, flowLayoutPanelRightComboBox: flowLayoutPanelRightComboBox.Visible
		);

		// Hide right panels
		flowLayoutPanelRightComboBox.Visible =
		playerPanel.Visible =
		stashPanel.Visible =
		secondaryVaultPanel.Visible =
		false;

		// Show the forge
		forgePanel.Dock = DockStyle.Left;
		forgePanel.Visible = true;
		forgePanel.FitHeight();

		// Detailed tooltips while the forge is open, as session state: the setting itself is not changed, so closing the
		// app with the forge open cannot save the forced value.
		this.userContext.ForceDetailedTooltipView = true;
	}

	/// <summary>
	/// Hides the forge and shows the panels it hid.
	/// </summary>
	internal void HideForge()
	{
		// Restore UI visibility
		flowLayoutPanelRightComboBox.Visible = lastVisibility.flowLayoutPanelRightComboBox;
		playerPanel.Visible = lastVisibility.playerPanel;
		stashPanel.Visible = lastVisibility.stashPanel;
		secondaryVaultPanel.Visible = lastVisibility.secondaryVaultPanel;
		// Hide the forge
		forgePanel.Dock = DockStyle.None;
		forgePanel.Visible = false;

		this.userContext.ForceDetailedTooltipView = false;
	}
}

using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	private ForgePanel forgePanel = null!; // Created while the form loads.

	/// <summary>The text of the forge button while the forge is closed; the button reads "Vault" while it is open.</summary>
	private string? forgeButtonText;

	private (
		bool configureButton
		, bool showVaulButton
		, bool searchButton
		, bool saveButton
		, bool playerPanel
		, bool stashPanel
		, bool secondaryVaultPanel
		, bool flowLayoutPanelRightComboBox
	) lastVisibility;

	private ForgePanel CreateForgePanel()
	{
		forgePanel = new ForgePanel(DragInfo, Services);

		flowLayoutPanelRightPanels.Controls.Add(forgePanel);
		forgePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		forgePanel.Visible = false;
		forgePanel.CancelAction = ForgeActionCanceled;
		forgePanel.ForgeAction = ForgeActionForged;

		// The space follows the panels the forge stands for: a zoom while the forge is open refits it.
		this.playerPanel.SizeChanged += (_, _) => this.RefitOpenForge();
		this.stashPanel.SizeChanged += (_, _) => this.RefitOpenForge();
		return forgePanel;
	}

	private void scalingButtonForge_Click(object? sender, EventArgs e)
	{
		if (!forgePanel.Visible)
		{
			ForgeShowUI();

			return;
		}

		ForgeHideUI();
	}

	private void ForgeShowUI()
	{
		SoundService.PlayRandomMetalHit();

		// Save UI visibility
		lastVisibility = (
			configureButton: configureButton.Visible
			, showVaulButton: showVaulButton.Visible
			, searchButton: searchButton.Visible
			, saveButton: saveButton.Visible
			, playerPanel: playerPanel.Visible
			, stashPanel: stashPanel.Visible
			, secondaryVaultPanel: secondaryVaultPanel.Visible
			, flowLayoutPanelRightComboBox: flowLayoutPanelRightComboBox.Visible
		);

		// Hide buttons
		configureButton.Visible =
		showVaulButton.Visible =
		searchButton.Visible =
		saveButton.Visible =

		// Hide right panels
		flowLayoutPanelRightComboBox.Visible =
		playerPanel.Visible =
		stashPanel.Visible =
		secondaryVaultPanel.Visible =
		false;

		// Show the forge
		forgePanel.Dock = DockStyle.Left;
		forgePanel.Visible = true;
		this.FitForgePanel();

		// The button now leads back to the vaults: it says so.
		this.forgeButtonText = forgeButton.Text;
		forgeButton.Text = TQVaultAE.Presentation.Resources.ResultsContainerVault;

		// Detailed tooltips while the forge is open, as session state: the setting itself is not changed, so closing the
		// app with the forge open cannot save the forced value.
		this.userContext.ForceDetailedTooltipView = true;
	}

	/// <summary>The forge takes the height of the panels it stands for: the character's bags and the stash under them.</summary>
	private void FitForgePanel()
		=> forgePanel.FitHeight(this.playerPanel.Height + this.stashPanel.Height - forgePanel.Margin.Vertical);

	private void RefitOpenForge()
	{
		if (forgePanel.Visible)
			this.FitForgePanel();
	}

	private void ForgeActionForged()
	{
		ForgeHideUI();
	}

	private void ForgeActionCanceled()
	{
		SoundService.PlayRandomCancel();
		ForgeHideUI();
	}

	private void ForgeHideUI()
	{
		SoundService.PlayRandomCancel();

		// Restore UI visibility
		configureButton.Visible = lastVisibility.configureButton;
		showVaulButton.Visible = lastVisibility.showVaulButton;
		searchButton.Visible = lastVisibility.searchButton;
		saveButton.Visible = lastVisibility.saveButton;
		flowLayoutPanelRightComboBox.Visible = lastVisibility.flowLayoutPanelRightComboBox;
		playerPanel.Visible = lastVisibility.playerPanel;
		stashPanel.Visible = lastVisibility.stashPanel;
		secondaryVaultPanel.Visible = lastVisibility.secondaryVaultPanel;
		// Hide the forge
		forgePanel.Dock = DockStyle.None;
		forgePanel.Visible = false;
		if (this.forgeButtonText is not null)
			forgeButton.Text = this.forgeButtonText;

		this.userContext.ForceDetailedTooltipView = false;

		Refresh();
	}
}

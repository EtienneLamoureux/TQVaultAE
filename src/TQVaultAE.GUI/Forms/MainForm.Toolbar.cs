using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The toolbar of the main window: which buttons it shows, the save button, the panel selection button and the
/// forge button.
/// </summary>
public partial class MainForm
{
	/// <summary>
	/// The forge and save buttons show when their setting is on; Exit stays last.
	/// </summary>
	private void AdjustMenuButtonVisibility()
	{
		this.forgeButton.Visible = USettings.AllowItemEdit;
		this.saveButton.Visible = USettings.EnableHotReload;
		// Get last position
		var flowctr = this.flowLayoutPanelMenuButtons.Controls;
		var lastctr = flowctr[flowctr.Count - 1];
		var lastidx = flowctr.GetChildIndex(lastctr);
		// Force "Exit" button in last position
		flowctr.SetChildIndex(this.exitButton, lastidx + 1);
	}

	private void saveButton_Click(object? sender, EventArgs e) => this.SaveAllModifiedFiles();

	/// <summary>
	/// Handler for clicking the panel selection button.
	/// Switches between the player panel and seconday vault panel.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs strucure</param>
	private void PanelSelectButtonClick(object? sender, EventArgs e)
		=> this.mainView.ShowSecondaryVault(!this.mainView.SecondaryVaultShown);

	/// <summary>
	/// The panel selection button names the panel it switches to.
	/// </summary>
	private void MainView_SecondaryVaultShownChanged(object? sender, EventArgs e)
		=> this.showVaulButton.Text = this.mainView.SecondaryVaultShown ? Resources.MainFormBtnShowPlayer : Resources.MainFormBtnPanelSelect;

	/// <summary>The text of the forge button while the forge is closed; the button reads "Show Vault" while it is open.</summary>
	private string? forgeButtonText;

	/// <summary>The visibility of the toolbar buttons the forge hides, given back when it closes.</summary>
	private (
		bool configureButton
		, bool showVaulButton
		, bool searchButton
		, bool saveButton
	) lastVisibility;

	private void scalingButtonForge_Click(object? sender, EventArgs e)
	{
		if (!this.mainView.ForgeShown)
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
		);

		// Hide buttons
		configureButton.Visible =
		showVaulButton.Visible =
		searchButton.Visible =
		saveButton.Visible =
		false;

		// Hide the right panels and show the forge
		this.mainView.ShowForge();

		// The button now leads back to the vaults: it says so.
		this.forgeButtonText = forgeButton.Text;
		forgeButton.Text = Resources.MainFormBtnPanelSelect;
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

		// Hide the forge
		this.mainView.HideForge();
		if (this.forgeButtonText is not null)
			forgeButton.Text = this.forgeButtonText;

		Refresh();
	}
}

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
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
		forgeButton.Text = TQVaultAE.Presentation.Resources.MainFormBtnPanelSelect;
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

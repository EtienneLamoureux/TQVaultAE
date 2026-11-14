using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
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
}

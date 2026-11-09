using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Debug-only access to the mod link of the characters for the automation endpoint.
/// </summary>
public partial class MainForm
{
	internal IGameMapService AutomationGameMapService => this.gameMapService;

	internal IReadOnlyList<PlayerSave> AutomationPlayerSaves => [.. this.comboBoxCharacter.Items.OfType<PlayerSave>()];

	internal PlayerSave? AutomationSelectedSave => this.comboBoxCharacter.SelectedItem as PlayerSave;

	/// <summary>
	/// Opens the mod link dialog of the selected character, like its context menu entry. Runs after the call returns,
	/// since the dialog is modal.
	/// </summary>
	internal void AutomationOpenModLinkDialog(PlayerSave save)
		=> this.BeginInvoke(() => this.OpenModLinkDialog(save));
}

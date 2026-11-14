using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Debug-only access to the mod link of the characters for the automation endpoint.
/// </summary>
public partial class MainForm
{
	internal IGameMapService AutomationGameMapService => this.gameMapService;

	internal IReadOnlyList<PlayerSave> AutomationPlayerSaves => [.. this.mainView.comboBoxCharacter.Items.OfType<PlayerSave>()];

	internal PlayerSave? AutomationSelectedSave => this.mainView.comboBoxCharacter.SelectedItem as PlayerSave;
}

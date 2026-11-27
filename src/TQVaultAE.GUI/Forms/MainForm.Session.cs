using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The state of the session kept in the user settings: shown vault, selected character, zoom.
/// </summary>
public partial class MainForm
{
	/// <summary>
	/// Updates configuration settings
	/// </summary>
	private void SaveConfiguration()
	{
		var configuration = this.CurrentSessionConfiguration();
		this.settingsPersistenceService.SaveConfiguration(configuration.VaultName, configuration.CharacterName, configuration.Zoom);
	}

	/// <summary>
	/// Gets the state of the session to store in the user settings: shown vault, selected character, zoom.
	/// </summary>
	private SessionConfiguration CurrentSessionConfiguration()
	{
		return new SessionConfiguration(this.mainView.SelectedVaultName, this.mainView.SelectedCharacterKey, this.NominalZoom);
	}
}

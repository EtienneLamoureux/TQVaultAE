using TQVaultAE.Application;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// After a switch of map: the watchers of the shared stash files (the new map may not be watched yet) and the
	/// panels, whose pictures and texts now come from the new map's data.
	/// </summary>
	private void GameMapService_ActiveMapChanged(object? sender, EventArgs e)
	{
		if (this.loadingComplete)
			this.WatchSharedStashFiles();

		this.Invalidate(true);
	}
}

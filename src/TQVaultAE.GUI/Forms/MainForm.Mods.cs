using TQVaultAE.Application;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// Makes a character's custom map (mod) the active one before it loads. The first switch to a map reads its database,
	/// which takes a few seconds.
	/// </summary>
	/// <returns>True when the active map changed.</returns>
	private bool ActivateMapFor(PlayerSave save)
	{
		var cursor = this.Cursor;
		this.Cursor = Cursors.WaitCursor;
		try
		{
			return this.gameMapService.ActivateFor(save);
		}
		finally
		{
			this.Cursor = cursor;
		}
	}

	/// <summary>
	/// After a switch of map: the watchers of the shared stash files (one set per map) and the panels, whose pictures and
	/// texts now come from the new map's data.
	/// </summary>
	private void GameMapService_ActiveMapChanged(object? sender, EventArgs e)
	{
		if (this.loadingComplete)
			this.WatchSharedStashFiles();

		this.Invalidate(true);
	}

	/// <summary>
	/// Watches the active map's shared stash files (transfer stash and relic vault) when hot reload is on.
	/// </summary>
	private void WatchSharedStashFiles()
	{
		if (!USettings.EnableHotReload)
			return;

		var relicPath = GamePathResolver.RelicVaultStashFileFullPath;
		var transferPath = GamePathResolver.TransferStashFileFullPath;

		this.fileSystemWatcherRelicStash.EnableRaisingEvents = false;
		if (FileIO.Exists(relicPath))
		{
			this.fileSystemWatcherRelicStash.Path = PathIO.GetDirectoryName(relicPath);
			this.fileSystemWatcherRelicStash.Filter = PathIO.GetFileName(relicPath);
			this.fileSystemWatcherRelicStash.EnableRaisingEvents = true;
		}

		this.fileSystemWatcherTransferStash.EnableRaisingEvents = false;
		if (FileIO.Exists(transferPath))
		{
			this.fileSystemWatcherTransferStash.Path = PathIO.GetDirectoryName(transferPath);
			this.fileSystemWatcherTransferStash.Filter = PathIO.GetFileName(transferPath);
			this.fileSystemWatcherTransferStash.EnableRaisingEvents = true;
		}
	}

	/// <summary>
	/// Opens the mod link dialog of a character (the character list's context menu). When the link of the character
	/// shown changed, it is loaded again with its new mod's data.
	/// </summary>
	private void OpenModLinkDialog(PlayerSave save)
	{
		bool changed;
		using (var dialog = this.Services.Forms.Create<ModLinkDialog>())
		{
			dialog.Save = save;
			dialog.ShowDialog(this);
			changed = dialog.LinkChanged;
		}

		if (!changed)
			return;

		if (this.comboBoxCharacter.SelectedItem == save)
			this.LoadPlayerAndStashes(save);

		// After the load: the class name may come from the new mod's text.
		this.comboBoxCharacter.RefreshItem(save);

		this.Refresh();
	}
}

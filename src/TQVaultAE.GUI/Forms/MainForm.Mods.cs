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
	/// After a switch of map: the watchers of the shared stash files (the new map may not be watched yet) and the
	/// panels, whose pictures and texts now come from the new map's data.
	/// </summary>
	private void GameMapService_ActiveMapChanged(object? sender, EventArgs e)
	{
		if (this.loadingComplete)
			this.WatchSharedStashFiles();

		this.Invalidate(true);
	}

	/// <summary>
	/// Opens the mod link dialog of a character (the character list's context menu). When the link of the character
	/// shown changed, it is loaded again with its new mod's data.
	/// </summary>
	private void OpenModLinkDialog(PlayerSave save)
	{
		// The link file belongs to the character: a read-only one keeps its link.
		if (this.NotifyIfReadOnly(save))
			return;

		bool changed;
		using (var dialog = this.Services.Forms.Create<ModLinkDialog>())
		{
			dialog.Save = save;
			dialog.ShowDialog(this);
			changed = dialog.LinkChanged;
		}

		if (!changed)
			return;

		// A link to a mod no other character uses brings its shared stash files in use.
		this.WatchSharedStashFiles();

		if (this.comboBoxCharacter.SelectedItem == save)
			this.LoadPlayerAndStashes(save);

		// After the load: the class name may come from the new mod's text.
		this.comboBoxCharacter.RefreshItem(save);

		this.Refresh();
	}
}

using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// Makes a character read-only, or writable again (the character list's tools menu): the read-only attribute of its
	/// save files changes, then the list and the panels show the new state. Refused, with a notification, while the
	/// character has unsaved changes; a failure is told as well.
	/// </summary>
	private void ToggleCharacterReadOnly(PlayerSave save)
	{
		bool readOnly = !save.IsReadOnly;

		// An item held from this character would leave it once dropped: it goes back first.
		if (readOnly && this.DragInfo.IsActive && this.DragInfo.CanBeCanceled)
			this.DragInfo.Cancel();

		var result = this.Services.CharacterReadOnlyService.SetReadOnly(save, readOnly);

		string? problem = result.Change switch
		{
			ReadOnlyChange.UnsavedChanges => string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyUnsaved, save.Name),
			ReadOnlyChange.Failed => string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyFailed, save.Name, result.Error?.Message),
			_ => null,
		};
		if (problem is not null)
			this.UIService.NotifyUser(problem, TQColor.Red);

		this.comboBoxCharacter.RefreshItem(save);

		if (this.comboBoxCharacter.SelectedItem == save)
		{
			// A selection made before cannot be acted on any more.
			this.ClearAllItemsSelectedCallback(this, new Models.SackPanelEventArgs(null, null));
			this.playerPanel.Refresh();
			this.stashPanel.Refresh();
		}
	}

	/// <summary>
	/// Tells the user that a read-only character cannot change, when <paramref name="save"/> is one.
	/// </summary>
	/// <returns>Whether the character is read-only.</returns>
	private bool NotifyIfReadOnly(PlayerSave save)
	{
		if (!this.Services.CharacterReadOnlyService.IsReadOnly(save.Folder))
			return false;

		this.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, save.Name), TQColor.Red);
		return true;
	}
}

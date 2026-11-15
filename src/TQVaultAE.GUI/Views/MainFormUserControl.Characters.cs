using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The character list of the main content and the loading of the character selected: the player panel, its stash and
/// the shared stashes of its map, with the messages of each load; the list's tools (duplicate, mod link, read-only).
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>
	/// Raised when the mod link of a character changed: its mod's shared stash files may now be in use.
	/// </summary>
	internal event EventHandler? ModLinkChanged;

	/// <summary>
	/// The listed characters: each is read with its own map's database.
	/// </summary>
	internal PlayerSave[] ListedCharacters()
		=> [.. this.comboBoxCharacter.Items.OfType<PlayerSave>()];

	/// <summary>The character selected in the list; null when none is.</summary>
	internal PlayerSave? SelectedCharacter => this.comboBoxCharacter.SelectedItem as PlayerSave;

	/// <summary>
	/// The key of the character selected, to store in the user settings: empty when none is.
	/// </summary>
	internal string? SelectedCharacterKey
	{
		get
		{
			// The key, not the name: a main campaign and a custom map character can have the same name.
			string? characterName = this.comboBoxCharacter.SelectedItem is PlayerSave ps
				? ps.Key
				: this.comboBoxCharacter.SelectedItem?.ToString();

			// No character is selected
			if (characterName == Resources.MainFormSelectCharacter)
				characterName = string.Empty;

			return characterName;
		}
	}

	/// <summary>
	/// Fills the character list, with its first entry selected (no character).
	/// </summary>
	private void ListCharacters(PlayerSave[] characters)
	{
		// Initialize the character combo-box; the saves listed before go, with their file watchers.
		foreach (var listed in this.comboBoxCharacter.Items.OfType<PlayerSave>())
			listed.Dispose();
		this.comboBoxCharacter.Items.Clear();

		if (characters.Length == 0)
			this.comboBoxCharacter.Items.Add(Resources.MainFormNoCharacters);
		else
		{
			this.comboBoxCharacter.Items.Add(Resources.MainFormSelectCharacter);

			foreach (var chara in characters)
				this.comboBoxCharacter.Items.Add(chara);
		}

		this.comboBoxCharacter.SelectedIndex = 0;
	}

	/// <summary>
	/// The characters may have been loaded since they were listed: the list shows them as loaded.
	/// </summary>
	internal void RefreshCharacters() => this.comboBoxCharacter.RefreshItems();

	/// <summary>
	/// Selects a character in the list, which loads it.
	/// </summary>
	internal void SelectCharacter(PlayerSave save) => this.comboBoxCharacter.SelectedItem = save;

	/// <summary>
	/// Selects the character given on the command line: a character key (IT/User/HeroA) or the start of a name in the list.
	/// </summary>
	internal void SelectAutomaticPlayer(string player)
	{
		if (this.ListedCharacters().FirstOrDefault(ps => ps.Key.Equals(player, StringComparison.OrdinalIgnoreCase)) is { } save)
			this.comboBoxCharacter.SelectedItem = save;
		else if (this.comboBoxCharacter.FindString(player) is var index and not -1)
			this.comboBoxCharacter.SelectedIndex = index;
	}

	/// <summary>
	/// Handler for changing the Character drop down selection.
	/// </summary>
	/// <param name="sender">Sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CharacterComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		// Hmm. We can load a character now!
		var selected = this.comboBoxCharacter.SelectedItem;
		var selectedSave = selected as PlayerSave;
		var selectedText = selected?.ToString() ?? string.Empty;

		// See if they actually changed their selection and ignore "No TQ characters detected"
		if (selectedText.Equals(Resources.MainFormSelectCharacter, StringComparison.Ordinal) || selectedText.Equals(Resources.MainFormNoCharacters, StringComparison.Ordinal)
			|| selectedText.Equals(Resources.MainFormNoCustomChars, StringComparison.Ordinal))
		{
			// no char selected
			this.ClearPlayer();
		}
		else if (selectedSave is not null)
		{
			this.LoadPlayerAndStashes(selectedSave);
		}
		this.RefreshWindow();
	}

	/// <summary>
	/// Redraws the whole window: what the main content shows changed.
	/// </summary>
	private void RefreshWindow() => ((Control?)this.FindForm() ?? this).Refresh();

	/// <summary>
	/// Clears out the selected player
	/// Changed by VillageIdiot to a separate function.
	/// </summary>
	private void ClearPlayer()
	{
		if (this.playerPanel.Player is not null)
		{
			this.playerPanel.Player = null;
			this.stashPanel.Player = null;
			this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;

			if (this.stashPanel.Stash is not null)
				this.stashPanel.Stash = null;
		}
	}

	/// <summary>
	/// Loads a player using the drop down list, with its stash and the shared stashes not loaded yet.
	/// </summary>
	/// <param name="selectedSave">Player from the drop down list.</param>
	private void LoadPlayerAndStashes(PlayerSave selectedSave)
	{
		// The character's mod data first: its database, archives and shared stash files.
		bool mapChanged = this.ActivateMapFor(selectedSave);

		var session = this.Services.PlayerService.LoadCharacterSession(
			selectedSave
			, loadTransferStash: mapChanged || this.stashPanel.TransferStash is null
			, loadRelicVaultStash: mapChanged || this.stashPanel.RelicVaultStash is null);

		this.ShowPlayer(selectedSave, session.Player, false);
		this.ShowPlayerStash(selectedSave, session.PlayerStash, false);

		if (!session.HasStashes)
		{
			this.stashPanel.RelicVaultStash = null;
			this.stashPanel.TransferStash = null;
			return;
		}

		if (session.RelicVaultStash is not null)
			this.ShowRelicVaultStash(session.RelicVaultStash, false);

		if (session.TransferStash is not null)
			this.ShowTransferStash(session.TransferStash, false);
	}

	/// <summary>
	/// Makes a character's custom map (mod) the active one before it loads. The first switch to a map reads its database,
	/// which takes a few seconds.
	/// </summary>
	/// <returns>True when the active map changed.</returns>
	private bool ActivateMapFor(PlayerSave save)
	{
		var window = (Control?)this.FindForm() ?? this;
		var cursor = window.Cursor;
		window.Cursor = Cursors.WaitCursor;
		try
		{
			return this.Services.GameMapService.ActivateFor(save);
		}
		finally
		{
			window.Cursor = cursor;
		}
	}

	/// <summary>
	/// Reads a character's file again after the game wrote it (player or player stash), and shows it when the character
	/// is the one shown.
	/// </summary>
	/// <param name="folder">The character's folder.</param>
	/// <param name="fileName">The file written.</param>
	internal void ReloadCharacterFile(string folder, string? fileName)
	{
		var gamePathResolver = this.Services.GamePathService;

		// retrieve PlayerSave
		var playerSave = this.comboBoxCharacter.Items.OfType<PlayerSave>().FirstOrDefault(ps => ps.Folder == folder);
		if (playerSave is null) return;

		// Reload player file
		PlayerLoadResult? playerResult = null;
		if (string.Equals(fileName, gamePathResolver.PlayerSaveFileName, StringComparison.OrdinalIgnoreCase))
		{
			playerResult = this.Services.PlayerService.LoadPlayer(playerSave, true);
			this.ShowPlayer(playerSave, playerResult, true);
		}

		StashLoadResult? stashResult = null;
		if (string.Equals(fileName, gamePathResolver.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
		{
			stashResult = this.Services.StashService.LoadCharacterStash(playerSave, true);
			this.ShowPlayerStash(playerSave, stashResult, true);
		}

		// Refresh if is current displayed character
		if (this.comboBoxCharacter.SelectedItem == playerSave)
		{
			if (playerResult is not null)
			{
				this.playerPanel.Player = playerResult.Player;
				this.stashPanel.Player = playerResult.Player;
			}

			if (stashResult is not null)
				this.stashPanel.Stash = stashResult.Stash;
		}
	}

	/// <summary>
	/// Shows a loaded player: its messages, then the player in the panels (unless reloaded by the file watcher).
	/// </summary>
	private void ShowPlayer(PlayerSave selectedSave, PlayerLoadResult result, bool fromFileWatcher)
		=> this.ShowLoaded(
			LoadMessages.ForPlayer(result)
			, result.PlayerFile
			, Resources.MainFormPlayerReadError
			, () =>
			{
				if (!fromFileWatcher)
				{
					this.playerPanel.Player = result.Player;
					this.stashPanel.Player = result.Player;
					this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;
				}
				this.comboBoxCharacter.RefreshItem(selectedSave);
			}
			, () =>
			{
				this.playerPanel.Player = null;
				this.comboBoxCharacter.SelectedIndex = 0;
			});

	/// <summary>
	/// Shows a loaded character stash: its messages, then the stash in the panel (unless reloaded by the file watcher).
	/// </summary>
	/// <param name="result">The load; null when the character has no stash, which clears the panel.</param>
	private void ShowPlayerStash(PlayerSave selectedSave, StashLoadResult? result, bool fromFileWatcher)
	{
		if (result is null)
		{
			this.stashPanel.Stash = null;
			return;
		}

		this.ShowLoaded(
			LoadMessages.ForPlayerStash(result, selectedSave)
			, result.StashFile
			, Resources.MainFormStashReadError
			, () =>
			{
				if (!fromFileWatcher)
					this.stashPanel.Stash = result.Stash;
			}
			, () => this.stashPanel.Stash = null);
	}

	/// <summary>
	/// Loads and shows the transfer and relic vault stashes, unless the selected character is a Titan Quest original one.
	/// </summary>
	internal void LoadSharedStashes()
	{
		var selectedSave = this.comboBoxCharacter.SelectedItem as PlayerSave;
		this.ShowTransferStash(this.Services.StashService.LoadTransferStashFor(selectedSave), false);
		this.ShowRelicVaultStash(this.Services.StashService.LoadRelicVaultStashFor(selectedSave), false);
	}

	/// <summary>
	/// Reads a shared stash again after the game wrote it: in the search, and in the stash panel when it is the active
	/// map's.
	/// </summary>
	internal void ReloadSharedStash(StashType type, string map)
	{
		var stashResult = this.Services.StashService.ReloadSharedStash(type, map);
		if (stashResult is null)
			return;

		// The panel shows the active map's shared stashes, unless the character shown has none (Titan Quest original).
		var selected = this.comboBoxCharacter.SelectedItem as PlayerSave;
		bool shown = GameMaps.Same(map, this.Services.GameMapService.ActiveMap) && (selected is null || selected.IsImmortalThrone);
		if (type == StashType.TransferStash)
		{
			this.ShowTransferStash(stashResult, true);
			if (shown)
				this.stashPanel.TransferStash = stashResult.Stash;
		}
		else
		{
			this.ShowRelicVaultStash(stashResult, true);
			if (shown)
				this.stashPanel.RelicVaultStash = stashResult.Stash;
		}
	}

	/// <summary>
	/// Shows the loaded transfer stash: its messages, then the stash in the panel (unless reloaded by the file watcher).
	/// </summary>
	/// <param name="result">The load; null when the character has no transfer stash, which clears the panel.</param>
	private void ShowTransferStash(StashLoadResult? result, bool fromFileWatcher)
	{
		if (result is null)
		{
			this.stashPanel.TransferStash = null;
			return;
		}

		this.ShowLoaded(
			LoadMessages.ForTransferStash(result)
			, result.StashFile
			, Resources.MainFormStashReadError
			, () =>
			{
				if (!fromFileWatcher)
					this.stashPanel.TransferStash = result.Stash;
			}
			, () => this.stashPanel.TransferStash = null);
	}

	/// <summary>
	/// Shows the loaded relic vault stash: its messages, then the stash in the panel (unless reloaded by the file watcher).
	/// </summary>
	/// <param name="result">The load; null when the character has no relic vault stash, which clears the panel.</param>
	private void ShowRelicVaultStash(StashLoadResult? result, bool fromFileWatcher)
	{
		if (result is null)
		{
			this.stashPanel.RelicVaultStash = null;
			return;
		}

		this.ShowLoaded(
			LoadMessages.ForRelicVaultStash(result)
			, result.StashFile
			, Resources.MainFormStashReadError
			, () =>
			{
				if (!fromFileWatcher)
					this.stashPanel.RelicVaultStash = result.Stash;
			}
			, () => this.stashPanel.RelicVaultStash = null);
	}

	/// <summary>
	/// Shows the messages of a load, then its result. A read error is shown and logged, then <paramref name="onReadError"/> runs.
	/// </summary>
	/// <param name="messages">The messages of the load.</param>
	/// <param name="file">The file loaded, for the read error.</param>
	/// <param name="readErrorCaption">The caption of the read error.</param>
	/// <param name="show">Shows the result.</param>
	/// <param name="onReadError">Clears what could not be shown.</param>
	private void ShowLoaded(IReadOnlyList<LoadMessage> messages, string? file, string readErrorCaption, Action show, Action onReadError)
	{
		try
		{
			this.ShowLoadMessages(messages);
			show();
		}
		catch (IOException exception)
		{
			string msg = LoadMessages.ReadError(file, exception);
			this.Services.MessageBoxes.Inform(msg, readErrorCaption, MessageBoxIcon.Error);
			Log.LogError(exception, "{Message}", msg);
			onReadError();
		}
	}

	/// <summary>
	/// Shows the messages of a player or stash load, errors and informations.
	/// </summary>
	private void ShowLoadMessages(IReadOnlyList<LoadMessage> messages)
	{
		foreach (var message in messages)
			this.Services.MessageBoxes.Inform(message.Text, message.Caption, message.IsError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
	}

	/// <summary>
	/// Attempt to duplicate a character
	/// </summary>
	private void DuplicateCharacter()
	{
		var playerService = this.Services.PlayerService;
		var ps = this.comboBoxCharacter.SelectedItem as PlayerSave;
		if (ps is not null)
		{
			var existingNames = this.comboBoxCharacter.Items.OfType<PlayerSave>().Select(save => save.Name).ToList();

			// Ask for a new name until it is valid or the dialog is cancelled.
			string newname;
			while (true)
			{
				newname = InputDialog.Show(this.Services.Forms, Resources.DuplicateCharacter_NewNameRequired, Resources.DuplicateCharacter_ModalTitle, "NewName").Trim();
				if (newname == string.Empty) return;// Cancel button

				var nameError = playerService.ValidateNewCharacterName(ps, newname, existingNames);
				if (nameError == CharacterNameError.None)
					break;

				var message = nameError == CharacterNameError.AlreadyUsed
					? Resources.DuplicateCharacter_NewNameAlreadyUsed
					: Resources.DuplicateCharacter_NewNameMustBeValid;
				this.Services.MessageBoxes.Inform(message, Resources.DuplicateCharacter_ModalTitle, MessageBoxIcon.Error);
			}

			var result = playerService.DuplicateCharacter(ps, newname);
			if (!result.IsSuccess)
			{
				// Display Error; the partial copy is already removed.
				this.Services.MessageBoxes.ShowException(result.Error, Resources.DuplicateCharacter_Error, MessageBoxIcon.Error);
				return;
			}

			System.Windows.Forms.Application.Restart();
			return;
		}

		// Player must be selected
		this.Services.MessageBoxes.Inform(Resources.DuplicateCharacter_SourceRequired, Resources.DuplicateCharacter_ModalTitle, MessageBoxIcon.Warning);
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
			dialog.ShowDialog(this.FindForm());
			changed = dialog.LinkChanged;
		}

		if (!changed)
			return;

		// A link to a mod no other character uses brings its shared stash files in use.
		this.ModLinkChanged?.Invoke(this, EventArgs.Empty);

		if (this.comboBoxCharacter.SelectedItem == save)
			this.LoadPlayerAndStashes(save);

		// After the load: the class name may come from the new mod's text.
		this.comboBoxCharacter.RefreshItem(save);

		this.RefreshWindow();
	}

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
			this.Services.UIService.NotifyUser(problem, TQColor.Red);

		this.comboBoxCharacter.RefreshItem(save);

		if (this.comboBoxCharacter.SelectedItem == save)
		{
			// A selection made before cannot be acted on any more.
			this.ClearAllItemsSelected();
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

		this.Services.UIService.NotifyUser(string.Format(CultureInfo.CurrentCulture, Resources.CharacterReadOnlyRefused, save.Name), TQColor.Red);
		return true;
	}
}

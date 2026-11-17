using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Layout;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The floating character windows (<see cref="CharacterWindow"/>): one per character, several characters at once,
/// owned by the main window. They share the main window's loaded saves and drag, so an item moves between them as
/// between the main window's panels; an equipment change on one side is shown on the other when both show the same
/// character. The windows open at exit are reopened at the next start, where they were (<c>&lt;CharacterWindows&gt;</c>).
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>Cascade step of a new window, in pixels at 96 dpi.</summary>
	private const int CharacterWindowStep = 30;

	/// <summary>The floating character windows open, oldest first.</summary>
	internal IReadOnlyList<CharacterWindow> CharacterWindows
		=> this.FindForm()?.OwnedForms.OfType<CharacterWindow>().Where(f => !f.IsDisposed).ToList() ?? [];

	/// <summary>The window of a character (its key, case ignored), or null.</summary>
	internal CharacterWindow? CharacterWindowOf(string key)
		=> this.CharacterWindows.FirstOrDefault(w => string.Equals(w.Save.Key, key, StringComparison.OrdinalIgnoreCase));

	/// <summary>
	/// Opens the floating window of a character, or brings its window to the front when it has one. The character is
	/// read as when it is selected (the main window's loaded save when it shows it).
	/// </summary>
	/// <param name="save">The character.</param>
	/// <param name="place">Where the window was at the last exit; null to centre it on the main window, clear of the others.</param>
	/// <returns>The window; null when the character could not be read.</returns>
	internal CharacterWindow? OpenCharacterWindow(PlayerSave save, Point? place = null)
	{
		if (this.CharacterWindowOf(save.Key) is { } open)
		{
			if (open.WindowState == FormWindowState.Minimized)
				open.WindowState = FormWindowState.Normal;
			open.Activate();
			return open;
		}

		if (this.FindForm() is not MainForm owner)
			return null;

		var player = this.stashPanel.Player is { } shown && this.SelectedCharacter == save
			? shown
			: this.Services.PlayerService.LoadPlayer(save).Player;
		if (player is not { EquipmentSack: not null, PlayerInfo: not null } || player.ArgumentException is not null)
		{
			this.Log.LogWarning("The character {Character} could not be read for a floating window", save.Key);
			return null;
		}

		var window = new CharacterWindow(owner, save, player, this.DragInfo, this.stashPanel.SlotsGeometry);
		window.EquipmentChanged += this.CharacterWindowEquipmentChanged;
		window.PlaceChanged += (_, _) => this.RememberCharacterWindow(window);
		window.FormClosed += (_, e) =>
		{
			// Closed by the user: not reopened. Closed with the main window: reopened at the next start.
			if (e.CloseReason == CloseReason.UserClosing && this.Services.Settings.CharacterWindows.Forget(save.Key))
				this.Services.Session.ConfigChanged = true;
		};

		// On the main window's screen, then placed once laid out (its size known).
		window.Location = place ?? owner.Location;
		window.Show(owner);
		if (place is null)
			window.Location = this.CharacterWindowPlace(owner, window);

		this.RememberCharacterWindow(window);
		return window;
	}

	/// <summary>Where a new window goes: centred on the main window, one step down and right of each window open.</summary>
	private Point CharacterWindowPlace(MainForm owner, CharacterWindow window)
	{
		var area = Screen.FromControl(owner).WorkingArea;
		var centred = WindowLayout.KeepOnScreen(
			new Point(owner.Left + ((owner.Width - window.Width) / 2), owner.Top + ((owner.Height - window.Height) / 2)), window.Size, [area]);
		var taken = this.CharacterWindows.Where(w => w != window).Select(w => w.Location).ToList();
		return WindowLayout.Cascade(centred, window.Size, area, taken, (int)Math.Round(CharacterWindowStep * owner.DpiScale()));
	}

	/// <summary>Records a window's place in the settings, saved with them at exit.</summary>
	private void RememberCharacterWindow(CharacterWindow window)
	{
		if (window.IsDisposed || window.WindowState != FormWindowState.Normal)
			return;

		if (this.Services.Settings.CharacterWindows.Remember(window.Save.Key, window.Left, window.Top))
			this.Services.Session.ConfigChanged = true;
	}

	/// <summary>Reopens the windows open at the last exit, where they were; a character no longer listed is skipped.</summary>
	internal void RestoreCharacterWindows()
	{
		var characters = this.ListedCharacters();
		foreach (var place in this.Services.Settings.CharacterWindows.Windows.ToList())
		{
			var save = characters.FirstOrDefault(c => string.Equals(c.Key, place.Character, StringComparison.OrdinalIgnoreCase));
			if (save is not null)
				this.OpenCharacterWindow(save, new Point(place.X, place.Y));
		}
	}

	/// <summary>A floating window changed its equipment: the main window follows when it shows the same character.</summary>
	private void CharacterWindowEquipmentChanged(object? sender, EventArgs e)
	{
		if (sender is CharacterWindow window && ReferenceEquals(window.Player, this.stashPanel.Player))
			this.stashPanel.EquipmentReplaced();
	}

	/// <summary>The main window changed the equipment or the skills of the character it shows: its floating window follows.</summary>
	private void ShownCharacterChanged()
	{
		var player = this.stashPanel.Player;
		foreach (var window in this.CharacterWindows.Where(w => player is not null && ReferenceEquals(w.Player, player)))
			window.EquipmentChangedElsewhere();
	}

	/// <summary>
	/// The equipment of the shown character was replaced outside the panels (a loadout loaded, results moved): the stash
	/// panel and the character's floating window show it.
	/// </summary>
	private void ShownEquipmentReplaced()
	{
		this.stashPanel.EquipmentReplaced();
		this.ShownCharacterChanged();
	}

	/// <summary>
	/// A character's save was read (selected, or read again by hot reload or with another map): its floating window shows
	/// the instance read.
	/// </summary>
	private void CharacterRead(PlayerSave save, PlayerCollection? player)
	{
		if (player is not null && this.CharacterWindowOf(save.Key) is { } window)
			window.Rebind(player);
	}

	/// <summary>The main window's scale changed (zoom, DPI): the windows take the new geometry of the slots.</summary>
	private void RelayoutCharacterWindows()
	{
		foreach (var window in this.CharacterWindows)
			window.Relayout();
	}
}

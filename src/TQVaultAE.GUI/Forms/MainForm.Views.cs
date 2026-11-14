using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Views;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The views of the main window: under its toolbar, it shows one view at a time, the main content
/// (<see cref="MainFormUserControl"/>) by default, or in its place the settings (<see cref="SettingsUserControl"/>)
/// or the character editor (<see cref="CharacterEditorUserControl"/>), each an <see cref="IMainWindowView"/>.
/// </summary>
/// <remarks>
/// While another view is shown, the main content is left as it was but sleeps: no item held on the cursor (a held
/// item goes back to its place first), no tool tip, the window's shortcut keys and wheel turned off, the toolbar
/// buttons acting on it disabled, and the reloads of game files changed meanwhile kept for its return.
/// </remarks>
public partial class MainForm
{
	/// <summary>
	/// The main content: vault, character and their panels. Given by the container, placed by the designer code.
	/// </summary>
	private readonly MainFormUserControl mainView;

	#region Controls of the main content

	// The window builds and drives the panels of the main content: these are the view's controls it works on.
	private System.Windows.Forms.Panel itemTextPanel => this.mainView.itemTextPanel;
	internal ScalingLabel NotificationText => this.mainView.NotificationText;
	private ScalingComboBox vaultListComboBox => this.mainView.vaultListComboBox;
	private BufferedFlowLayoutPanel flowLayoutPanelVaultSelector => this.mainView.flowLayoutPanelVaultSelector;
	private System.Windows.Forms.PictureBox vaultPictureBox => this.mainView.vaultPictureBox;
	internal BufferedTableLayoutPanel tableLayoutPanelMain => this.mainView.tableLayoutPanelMain;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHighlight => this.mainView.tableLayoutPanelHighlight;
	private System.Windows.Forms.LinkLabel scalingLabelHighlight => this.mainView.scalingLabelHighlight;
	private ScalingTextBox scalingTextBoxHighlight => this.mainView.scalingTextBoxHighlight;
	private BufferedFlowLayoutPanel flowLayoutPanelRightPanels => this.mainView.flowLayoutPanelRightPanels;
	private BufferedFlowLayoutPanel flowLayoutPanelRightComboBox => this.mainView.flowLayoutPanelRightComboBox;
	private ComboBoxCharacter comboBoxCharacter => this.mainView.comboBoxCharacter;
	private BufferedFlowLayoutPanel bufferedFlowLayoutPanelsecondaryVaultList => this.mainView.bufferedFlowLayoutPanelsecondaryVaultList;
	private System.Windows.Forms.PictureBox pictureBoxSecondVault => this.mainView.pictureBoxSecondVault;
	private ScalingComboBox secondaryVaultListComboBox => this.mainView.secondaryVaultListComboBox;
	private HighlightFilters highlightFilters => this.mainView.highlightFilters;

	/// <summary>
	/// The main content's lists drive the window's loading of vaults and characters.
	/// </summary>
	private void WireMainView()
	{
		this.vaultListComboBox.SelectedIndexChanged += this.VaultListComboBoxSelectedIndexChanged;
		this.comboBoxCharacter.SelectedIndexChanged += this.CharacterComboBoxSelectedIndexChanged;
		this.secondaryVaultListComboBox.SelectedIndexChanged += this.SecondaryVaultListComboBoxSelectedIndexChanged;
	}

	#endregion

	#region Views

	/// <summary>
	/// The view shown in place of the main content (the settings or the character editor); null while the main content
	/// is shown.
	/// </summary>
	private Control? shownView;

	/// <summary>Called once the main content is shown again in place of <see cref="shownView"/>.</summary>
	private Action? shownViewLeft;

	/// <summary>Reloads of game save files changed while the main content was hidden, scheduled again at its return.</summary>
	private readonly Dictionary<string, Action> deferredReloads = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Shows a view in place of the main content, put to sleep first (see the class remarks): drawn once, switched.
	/// </summary>
	/// <param name="view">The view, built, not in the window yet.</param>
	/// <param name="left">Called once the main content is shown again: the view is kept for the next opening or disposed.</param>
	private void SwitchToView(Control view, Action? left)
	{
		// Drawn once, switched: else the view's controls showed one by one over the panels for a few frames.
		using (new LayoutBatch(this, this))
		{
			if (view.Parent != this)
			{
				this.Controls.Add(view);
				this.Controls.SetChildIndex(view, this.Controls.GetChildIndex(this.mainView));
			}

			view.Bounds = this.mainView.Bounds;
			this.shownView = view;
			this.shownViewLeft = left;
			this.mainView.Visible = false;
			view.Visible = true;
			this.EnableMainViewButtons(false);
		}

		this.Update();

		// As a new dialog did: the first control of the view takes the focus.
		this.ActiveControl = null;
		view.SelectNextControl(null, true, true, true, true);
	}

	/// <summary>
	/// Leaves the view shown for the main content, dropping its edits.
	/// </summary>
	/// <param name="confirm">Ask first when the view holds edits (the toolbar's Settings button, closing the window).</param>
	/// <returns>False when the user chose to stay in the view.</returns>
	private bool LeaveShownView(bool confirm)
	{
		if (this.shownView is not { } view)
			return true;

		if (confirm && view is IMainWindowView { HasChanges: true } edited
			&& !this.Services.MessageBoxes.Confirm(edited.DiscardChangesQuestion, view.Text, MessageBoxIcon.Warning, defaultNo: true))
			return false;

		this.ShowMainView();
		return true;
	}

	/// <summary>
	/// Shows the main content again in place of the view shown and wakes it up.
	/// </summary>
	private void ShowMainView()
	{
		if (this.shownView is not { } view)
			return;

		var left = this.shownViewLeft;

		// Drawn once, switched (see SwitchToView).
		using (new LayoutBatch(this, this))
		{
			this.shownView = null;
			this.shownViewLeft = null;
			this.mainView.Visible = true;
			view.Visible = false;
			this.Controls.Remove(view);
			this.EnableMainViewButtons(true);
		}

		this.Update();

		this.scalingTextBoxHighlight.Focus();

		left?.Invoke();

		// The game files changed meanwhile: reloaded as if they had just changed.
		var reloads = this.deferredReloads.ToList();
		this.deferredReloads.Clear();
		foreach (var (path, reload) in reloads)
			this.ScheduleReload(path, reload);
	}

	/// <summary>
	/// Makes the main content ready to be hidden: the item held on the cursor goes back to its place, the tool tips close.
	/// </summary>
	/// <returns>False when the held item cannot go back (a modified item), which the user is told: the content stays.</returns>
	private bool PutMainViewToSleep()
	{
		if (this.DragInfo.IsActive)
		{
			if (!this.DragInfo.CanBeCanceled)
			{
				this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
				return false;
			}

			this.DragInfo.Cancel();
		}

		Tooltips.ItemTooltip.HideTooltip();
		Tooltips.BagButtonTooltip.HideTooltip();
		Tooltips.BagButtonLabelTooltip.HideTooltip();
		Tooltips.FoundResultsTooltip.HideTooltip();
		return true;
	}

	/// <summary>
	/// The toolbar buttons acting on the main content work only while it is shown; Settings, About and Exit always do.
	/// </summary>
	private void EnableMainViewButtons(bool enabled)
		=> this.showVaulButton.Enabled = this.searchButton.Enabled = this.forgeButton.Enabled = this.saveButton.Enabled = enabled;

	/// <summary>
	/// Schedules the reload of a changed game file; while the main content is hidden, the reload waits for its return
	/// (the character editor works on the character as loaded).
	/// </summary>
	private void ScheduleReload(string path, Action reload)
		=> this.FileReloadDebouncer.Schedule(path, () =>
		{
			if (this.shownView is null)
			{
				this.Log.LogDebug("Hot reload of {File}", path);
				reload();
			}
			else
			{
				this.Log.LogDebug("Hot reload of {File} kept until the main content is shown again", path);
				this.deferredReloads[path] = reload;
			}
		});

	#endregion

	#region Character editor view

	/// <summary>
	/// Shows the character editor of <paramref name="player"/> in place of the main content, which goes to sleep (see
	/// the class remarks). The editor is built for this opening, at the window's DPI, and disposed when left; OK applies
	/// the edits to the character in memory (the Save button writes them), Cancel and Escape drop them.
	/// </summary>
	private void ShowCharacterEditorView(PlayerCollection player)
	{
		if (this.shownView is not null || !this.PutMainViewToSleep())
			return;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		var editor = new CharacterEditorUserControl(this.Services)
		{
			Name = "characterEditorView",
			AutoScroll = true,
			Visible = false,
		};
		editor.Applied += (_, _) => this.ShowMainView();
		editor.Cancelled += (_, _) => this.ShowMainView();

		using (new LayoutBatch(this, this))
		{
			// In the window first: the editor draws its game frame at the window's DPI.
			this.Controls.Add(editor);
			this.Controls.SetChildIndex(editor, this.Controls.GetChildIndex(this.mainView));
			editor.Bounds = this.mainView.Bounds;
			editor.ShowCharacter(player);
		}

		this.SwitchToView(editor, left: () =>
		{
			editor.Dispose();
			this.stashPanel.CharacterEdited();
		});

		this.Log.LogDebug("Character editor shown {Elapsed} ms after the click", watch.ElapsedMilliseconds);
	}

	#endregion

	#region Settings view

	/// <summary>
	/// The settings, built at their first opening and kept for the next ones (building them is the slow part), out of
	/// the window while the main content is shown; built again when the zoom or the window's DPI changed since: scaled
	/// from one DPI to another, a view built at a small zoom kept its rounded margins (3 px became 2).
	/// </summary>
	private readonly KeptDialog<SettingsUserControl> keptSettingsView = new();

	/// <summary>
	/// The toolbar's Settings button: shows the settings, or leaves them; from another view, leaves it first, asking when
	/// it holds edits.
	/// </summary>
	private void ToggleSettingsView()
	{
		if (this.shownView is SettingsUserControl)
			this.LeaveShownView(confirm: true);
		else if (this.LeaveShownView(confirm: true))
			this.ShowSettingsView();
	}

	/// <summary>
	/// Shows the settings in use in place of the main content, which goes to sleep (see the class remarks).
	/// </summary>
	private void ShowSettingsView()
	{
		if (this.shownView is not null || !this.PutMainViewToSleep())
			return;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		var zoom = UIService.Zoom;
		// Kept by the zoom and the DPI: a move to a monitor of another DPI builds them again too. Not by their product, the
		// scale: built at 120 dpi and zoom 1.2, they were reused at 144 dpi and zoom 1 and scaled a second time by 1.2.
		var settings = this.keptSettingsView.Get(this, zoom, () =>
		{
			var view = new SettingsUserControl(this.Services)
			{
				Name = "settingsView",
				AutoSize = false,
				AutoScroll = true,
				Visible = false,
			};
			// Windows Forms scaled it to the DPI: only the zoom is left.
			using (ZoomScope.Enter())
				view.Scale(new SizeF(zoom, zoom));
			view.Applied += this.SettingsView_Applied;
			view.Cancelled += this.SettingsView_Cancelled;
			return view;
		}, out bool built, dpi: this.DeviceDpi);

		settings.ShowSettingsInUse();

		// Kept for the next opening: nothing to do when it is left.
		this.SwitchToView(settings, left: null);

		this.Log.LogDebug("Settings shown {Elapsed} ms after the click ({How})", watch.ElapsedMilliseconds, built ? "built" : "kept");
	}

	/// <summary>Cancel in the settings: back to the main content, the edits dropped.</summary>
	private void SettingsView_Cancelled(object? sender, EventArgs e) => this.LeaveShownView(confirm: false);

	/// <summary>
	/// OK in the settings, checked and applied: back to the main content, which takes the changes; the window restarts
	/// when they need it and the user agrees.
	/// </summary>
	private void SettingsView_Applied(object? sender, EventArgs e)
	{
		var settings = (SettingsUserControl)sender!;
		this.ShowMainView();

		if (!settings.ConfigurationChanged)
			return;

		if (settings.VaultPathChanged)
		{
			GamePathResolver.TQVaultSaveFolder = settings.VaultPath;
			this.vaultService.UpdateVaultPath(settings.VaultPath);
			this.GetVaultList(true);
		}

		if (settings.EnableTQVaultSoundsChanged)
		{
			var soundSrv = this.Services.SoundService;
			soundSrv.InitAllPlayers();
		}

		var restartReason = settings.RestartReason;
		bool restart = false;

		if (restartReason != RestartReason.None)
		{
			var (title, message) = restartReason switch
			{
				RestartReason.Settings => (Resources.MainFormSettingsChanged, Resources.MainFormSettingsChangedMsg),
				RestartReason.GamePath => (Resources.MainFormPathsChanged, Resources.MainFormPathsChangedMsg),
				_ => (Resources.MainFormLanguageChanged, Resources.MainFormLanguageChangedMsg),
			};

			restart = this.Services.MessageBoxes.Confirm(message, title);
		}

		if (settings.ItemBGColorOpacityChanged || settings.EnableItemRequirementRestrictionChanged)
			this.Refresh();

		if (settings.EnableCharacterEditChanged)
			stashPanel?.UpdatePlayerInfo();

		this.userContext.ConfigChanged = true;
		this.SaveConfiguration();

		AdjustMenuButtonVisibility();

		if (restart)
		{
			if (this.DoCloseStuff())
				System.Windows.Forms.Application.Restart();
		}
	}

	#endregion
}

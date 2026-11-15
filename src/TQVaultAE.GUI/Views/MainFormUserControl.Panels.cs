using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The item panels of the main content: built here, placed in the view's layout, and wired to each other (the selection
/// stays in one panel, an item moves from one to another).
/// </summary>
public partial class MainFormUserControl
{
	/// <summary>
	/// The item held on the cursor, shared by every panel.
	/// </summary>
	internal ItemDragInfo DragInfo { get; private set; } = null!; // Created with the panels.

	/// <summary>
	/// Instance of the vault panel control
	/// </summary>
	private VaultPanel vaultPanel = null!; // Created with the panels.

	/// <summary>
	/// Instance of the second vault panel control
	/// This gets toggled with the player panel
	/// </summary>
	private VaultPanel secondaryVaultPanel = null!; // Created with the panels.

	/// <summary>
	/// Instance of the player panel control
	/// </summary>
	private PlayerPanel playerPanel = null!; // Created with the panels.

	/// <summary>
	/// Instance of the stash panel control
	/// </summary>
	private StashPanel stashPanel = null!; // Created with the panels.

	/// <summary>
	/// The forge, shown in place of the character's panels.
	/// </summary>
	private ForgePanel forgePanel = null!; // Created with the panels.

	/// <summary>The vault panel.</summary>
	internal VaultPanel VaultPanel => this.vaultPanel;

	/// <summary>The secondary vault panel, shown in place of the player panel.</summary>
	internal VaultPanel SecondaryVaultPanel => this.secondaryVaultPanel;

	/// <summary>The player panel.</summary>
	internal PlayerPanel PlayerPanel => this.playerPanel;

	/// <summary>The stash panel.</summary>
	internal StashPanel StashPanel => this.stashPanel;

	/// <summary>The forge panel.</summary>
	internal ForgePanel ForgePanel => this.forgePanel;

	/// <summary>
	/// Holds the last sack that had mouse focus
	/// Used for Automoving
	/// </summary>
	private SackCollection? lastSackHighlighted;

	/// <summary>
	/// Holds the last sack panel that had mouse focus
	/// Used for Automoving
	/// </summary>
	private SackPanel? lastSackPanelHighlighted;

	/// <summary>
	/// Holds that last stash that had focus
	/// </summary>
	private Stash? lastStash;

	/// <summary>
	/// Bag number of the last bag with focus
	/// </summary>
	private int lastBag;

	/// <summary>
	/// Flag which holds whether we are showing the secondary vault panel or the player panel.
	/// </summary>
	private bool showSecondaryVault;

	/// <summary>Whether the secondary vault panel is shown in place of the player panel.</summary>
	internal bool SecondaryVaultShown => this.showSecondaryVault;

	/// <summary>Raised when the secondary vault panel is shown in place of the player panel, or the player panel again.</summary>
	internal event EventHandler? SecondaryVaultShownChanged;

	/// <summary>
	/// Creates the view's internal panels
	/// </summary>
	/// <param name="characters">The characters to list.</param>
	internal void CreatePanels(PlayerSave[] characters)
	{
		this.DragInfo = new ItemDragInfo(this.Services.UIService);

		this.CreatePlayerPanel();

		// Put the secondary vault list on top of the player list drop down
		// since only one can be shown at a time.
		this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = false;

		this.ListCharacters(characters);

		this.vaultPanel = this.CreateVaultPanel(AutoMoveLocation.Vault, panel => this.tableLayoutPanelMain.Controls.Add(panel, 0, 1));
		this.secondaryVaultPanel = this.CreateVaultPanel(AutoMoveLocation.SecondaryVault, this.flowLayoutPanelRightPanels.Controls.Add);
		this.secondaryVaultPanel.Enabled = false;
		this.secondaryVaultPanel.Visible = false;
		this.lastBag = -1;

		this.itemTextPanel.Height = Convert.ToInt32(22.0F * this.Services.UIService.Scale);
		this.FitRowsToVault();
		this.vaultPanel.SizeChanged += (_, _) => this.FitRowsToVault();
		this.GetVaultList(false);

		// Now we always create the stash panel since everyone can have equipment
		this.CreateStashPanel();
		this.stashPanel.CurrentBag = 0; // set to default to the equipment panel

		// Add the Forge
		this.CreateForgePanel();

		this.SetupVaultExportButton();
	}

	/// <summary>
	/// Makes the highlight row and the notification line as wide as the vault at each of its sizes (zoom, DPI): scaled
	/// from their own width, they would drift from it, since the vault's grid frame keeps its thickness. Docked to fill
	/// its cell, the highlight row would set the width of that auto-sized column itself: a column it had widened (a
	/// maximize) would never narrow again.
	/// </summary>
	private void FitRowsToVault()
	{
		this.tableLayoutPanelHighlight.Width = this.vaultPanel.Width;
		this.itemTextPanel.Width = this.vaultPanel.Width;
	}

	/// <summary>
	/// Creates a vault panel of 12 bags.
	/// </summary>
	/// <param name="location">Where its items go on an automatic move: the vault or the secondary vault.</param>
	/// <param name="place">Adds the panel to its container.</param>
	private VaultPanel CreateVaultPanel(AutoMoveLocation location, Action<Control> place)
	{
		var panel = new VaultPanel(this.DragInfo, 12, new Size(18, 20), 1, location, this.Services);
		panel.UseCaravanFrame();
		this.WirePanel(panel, place);
		return panel;
	}

	/// <summary>
	/// Creates the player panel
	/// </summary>
	private void CreatePlayerPanel()
	{
		this.playerPanel = new PlayerPanel(this.DragInfo, 4, new Size(12, 5), new Size(8, 5), this.Services);
		this.playerPanel.UseCaravanFrame();
		this.WirePanel(this.playerPanel, this.flowLayoutPanelRightPanels.Controls.Add);
	}

	/// <summary>
	/// Wires an item panel to the others (highlight, automatic move, one selection at a time) and places it.
	/// </summary>
	/// <param name="panel">The panel.</param>
	/// <param name="place">Adds the panel to its container.</param>
	private void WirePanel(VaultPanel panel, Action<Control> place)
	{
		panel.OnNewItemHighlighted += this.NewItemHighlightedCallback;
		panel.OnAutoMoveItem += this.AutoMoveItemCallback;
		panel.OnItemSelected += this.ItemSelectedCallback;
		panel.OnClearAllItemsSelected += this.ClearAllItemsSelectedCallback;

		place(panel);
		panel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		panel.Margin = new Padding(0);
	}

	/// <summary>
	/// The sack panels shown by the item panels, read each time: the stash panel's changes with its bag.
	/// </summary>
	private IEnumerable<SackPanel> AllSackPanels
	{
		get
		{
			if (this.vaultPanel?.SackPanel is { } vault)
				yield return vault;
			if (this.secondaryVaultPanel?.SackPanel is { } secondaryVault)
				yield return secondaryVault;
			if (this.playerPanel?.SackPanel is { } player)
				yield return player;
			if (this.playerPanel?.BagSackPanel is { } playerBag)
				yield return playerBag;
			if (this.stashPanel?.SackPanel is { } stash)
				yield return stash;
		}
	}

	/// <summary>
	/// Creates the stash panel
	/// </summary>
	private void CreateStashPanel()
	{
		// size params are width, height
		Size panelSize = new Size(17, 16);

		this.stashPanel = new StashPanel(this.DragInfo, panelSize, this.Services);

		// New location in bottom right of the Main Form.
		// Align to playerPanel
		this.stashPanel.EditCharacterClicked += (_, _) => this.EditCharacterRequested?.Invoke(this, this.stashPanel.ShownPlayer);
		this.stashPanel.OnLoadoutAction += this.LoadoutActionCallback;

		this.WirePanel(this.stashPanel, this.flowLayoutPanelRightPanels.Controls.Add);
	}

	/// <summary>Raised by the Edit button of the stash panel, with the character it shows.</summary>
	internal event EventHandler<PlayerCollection>? EditCharacterRequested;

	/// <summary>
	/// The character edited in the character editor view is shown again: the stash panel shows its new state.
	/// </summary>
	internal void CharacterEdited() => this.stashPanel.CharacterEdited();

	/// <summary>
	/// Used to toggle the upper display between the player panel or another vault.
	/// </summary>
	internal void ShowSecondaryVault(bool show)
	{
		this.showSecondaryVault = show;
		this.UpdateTopPanel();
	}

	/// <summary>
	/// Used to toggle the upper display between the player panel or another vault.
	/// </summary>
	private void UpdateTopPanel()
	{
		if (this.showSecondaryVault)
		{
			this.playerPanel.Enabled = false;
			this.playerPanel.Visible = false;
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.stashPanel.Visible = false;
			this.stashPanel.Enabled = false;
			this.secondaryVaultPanel.Enabled = true;
			this.secondaryVaultPanel.Visible = true;
			this.SecondaryVaultShownChanged?.Invoke(this, EventArgs.Empty);
			this.comboBoxCharacter.Enabled = false;
			this.comboBoxCharacter.Visible = false;

			this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = true;


			this.lastStash = this.stashPanel.Stash;
			this.lastBag = this.stashPanel.CurrentBag;
			this.stashPanel.Player = null;
			this.stashPanel.Stash = null;
			if (this.stashPanel.CurrentBag != BagIdConstants.BAGID_TRANSFERSTASH)
			{
				this.stashPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.CurrentBag = BagIdConstants.BAGID_TRANSFERSTASH;
			}

			this.vaultPanel.SackPanel.SecondaryVaultShown = true;
			this.stashPanel.SackPanel.SecondaryVaultShown = true;

			this.secondaryVaultPanel.SackPanel.IsSecondaryVault = true;
			this.secondaryVaultPanel.SackPanel.SecondaryVaultShown = true;
			this.GetSecondaryVaultList();
		}
		else
		{
			this.stashPanel.Visible = true;
			this.stashPanel.Enabled = true;
			this.secondaryVaultPanel.Enabled = false;
			this.secondaryVaultPanel.Visible = false;
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.Enabled = true;
			this.playerPanel.Visible = true;
			this.SecondaryVaultShownChanged?.Invoke(this, EventArgs.Empty);
			this.comboBoxCharacter.Enabled = true;
			this.comboBoxCharacter.Visible = true;

			this.bufferedFlowLayoutPanelsecondaryVaultList.Visible = false;

			this.stashPanel.Player = this.playerPanel.Player;
			if (this.lastStash is not null)
			{
				this.stashPanel.Stash = this.lastStash;
				if (this.lastBag != -1 && this.lastBag != this.stashPanel.CurrentBag)
				{
					this.stashPanel.CurrentBag = this.lastBag;
					this.stashPanel.SackPanel.ClearSelectedItems();
				}
			}

			this.vaultPanel.SackPanel.SecondaryVaultShown = false;
			this.stashPanel.SackPanel.SecondaryVaultShown = false;
			this.secondaryVaultPanel.SackPanel.SecondaryVaultShown = false;
		}
	}

	/// <summary>
	/// Callback for highlighting a new item.
	/// Updates the text box on the main form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void NewItemHighlightedCallback(object? sender, SackPanelEventArgs e)
	{
		Item? item = e.Item;
		SackCollection? sack = e.Sack;
		var sackPanel = (SackPanel)sender!;
		if (item is null)
		{
			// Only do something if this sack is the "owner" of the current item highlighted.
			if (sack == this.lastSackHighlighted)
			{
				this.NotificationText.Text = string.Empty;

				// hide the tooltip
				ItemTooltip.HideTooltip();
			}
		}
		else
		{
			var itt = ItemTooltip.ShowTooltip(this.Services, item, sackPanel);

			if (itt.Data is { } data)
			{
				this.NotificationText.ForeColor = data.Item.ExtractTextColorOrItemColor(data.BaseItemInfoDescription);
				this.NotificationText.Text = data.FullNameBagTooltipClean;
			}
		}

		this.lastSackHighlighted = sack;
		this.lastSackPanelHighlighted = sackPanel;
	}

	/// <summary>
	/// Used to clear out selections on other panels if the user tries to select across multiple panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ItemSelectedCallback(object? sender, SackPanelEventArgs e)
	{
		var panels = this.AllSackPanels.ToList();
		if (sender is not SackPanel sackPanel || !panels.Contains(sackPanel))
			return;

		foreach (var panel in panels.Where(panel => panel != sackPanel))
			panel.ClearSelectedItems();
	}

	/// <summary>
	/// Used to clear the selection when a bag button is clicked.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void ClearAllItemsSelectedCallback(object? sender, SackPanelEventArgs e)
		=> this.ClearAllItemsSelected();

	/// <summary>
	/// Clears the selection of every panel.
	/// </summary>
	internal void ClearAllItemsSelected()
	{
		foreach (var panel in this.AllSackPanels)
			panel.ClearSelectedItems();
	}

	/// <summary>
	/// The items selected in the panel holding a selection; null when none does.
	/// </summary>
	internal IReadOnlyList<Item>? SelectedItems()
		=> this.AllSackPanels.FirstOrDefault(panel => panel.SelectionActive)?.GetSelectedItems();

	/// <summary>
	/// Used for sending items between sacks or panels.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">SackPanelEventArgs data</param>
	private void AutoMoveItemCallback(object? sender, SackPanelEventArgs e)
	{
		SackPanel? sackPanel = (SackPanel)sender!;

		if (!this.DragInfo.IsAutoMoveActive)
			return;

		if (this.DragInfo.AutoMoveDestination == AutoMoveLocation.Stash)
		{
			this.AutoMoveItemToStash();
			return;
		}

		this.AutoMoveItemToPanel(sackPanel);
	}

	/// <summary>
	/// Resolves the destination panel and sack for auto-move operations.
	/// </summary>
	// This is a sack to sack move on the same panel.
	private (VaultPanel? Panel, SackPanel? SackPanel, int SackNumber) GetAutoMoveDestination(SackPanel sourcePanel)
	{
		var sacNumber = this.DragInfo.DestIndex;
		if (sacNumber > -1)
		{
			VaultPanel panel = sourcePanel.SackType == SackType.Vault && sourcePanel.IsSecondaryVault
				? this.secondaryVaultPanel
				: sourcePanel.SackType == SackType.Vault
					? this.vaultPanel
					: this.playerPanel;

			return (panel, sourcePanel, sacNumber);
		}

		return this.DragInfo.AutoMoveDestination switch
		{
			AutoMoveLocation.Vault => (this.vaultPanel, this.vaultPanel.SackPanel, this.vaultPanel.CurrentBag),
			AutoMoveLocation.Player => (this.playerPanel, ((PlayerPanel)this.playerPanel).SackPanel, 0),
			AutoMoveLocation.SecondaryVault => (this.secondaryVaultPanel, this.secondaryVaultPanel.SackPanel, this.secondaryVaultPanel.CurrentBag),
			_ => (null, null, 0)
		};
	}

	/// <summary>
	/// Handles auto-moving an item to the stash panel.
	/// </summary>
	// Special Case for moving to stash.
	private void AutoMoveItemToStash()
	{
		var target = AutoMovePlanner.StashTarget(
			this.stashPanel.CurrentBag
			, this.stashPanel.Player is not null
			, this.stashPanel.TransferStash is not null
			, this.stashPanel.RelicVaultStash is not null);

		// The equipment is shown: show the transfer stash instead.
		if (target.Bag != this.stashPanel.CurrentBag)
			this.stashPanel.CurrentBag = target.Bag;

		if (!target.IsAvailable)
		{
			// We have nowhere to send the item so cancel the move.
			this.DragInfo.Cancel();
			return;
		}

		if (this.DragInfo.Item is not { } dragItem)
			return;

		var stashSackPanel = this.stashPanel.SackPanel;
		var stashSack = stashSackPanel.ShownSack;
		// The same place as a drop on the stash: the stash file.
		var place = stashSackPanel.PlaceHere();

		// No room, an item the relic vault does not take, or a read-only character's stash: the item goes back.
		if (!this.Services.ItemMovementService.TryPlace(dragItem, stashSack, stashSackPanel.SackSize, place, this.DragInfo.MarkPlaced))
		{
			this.DragInfo.Cancel();
			stashSackPanel.NotifyIfReadOnlyCharacter(stashSack);
			return;
		}

		// Out of a vault, an item without a link follows the character's map.
		this.stashPanel.AdoptVaultMod(stashSack, [dragItem], stashSackPanel.SackSize);

		this.lastSackPanelHighlighted?.Invalidate();
		this.stashPanel.Refresh();
		BagButtonTooltip.InvalidateCache(stashSack);
	}

	/// <summary>
	/// Handles auto-moving an item to a non-stash panel.
	/// </summary>
	private void AutoMoveItemToPanel(SackPanel sourcePanel)
	{
		var (destinationPanel, destinationSackPanel, destSackNumber) = this.GetAutoMoveDestination(sourcePanel);
		var destinationSack = destinationPanel?.Player?.GetSack(destSackNumber);

		if (destinationPanel?.Player is null || destinationSackPanel is null || destinationSack is null)
		{
			// We have nowhere to send the item so cancel the move.
			this.DragInfo.Cancel();
			return;
		}

		if (this.DragInfo.Item is not { } dragItem)
			return;

		int destination = this.GetDestinationIndex(destinationSackPanel, destinationPanel.CurrentBag, destSackNumber);
		var place = this.Services.ItemMovementService.PlaceIn(destinationPanel.Player, destinationSackPanel.SackType, destination, destinationSack.StashType);

		// We have no space in the sack, or it is a read-only character's, so we cancel.
		if (!this.Services.ItemMovementService.TryPlace(dragItem, destinationSack, destinationSackPanel.SackSize, place, this.DragInfo.MarkPlaced))
		{
			this.DragInfo.Cancel();
			destinationSackPanel.NotifyIfReadOnlyCharacter(destinationSack);
			return;
		}

		// Like a dropped item, it takes the data of its new place (the vault's mod or the base game, or the character's map).
		destinationPanel.AdoptVaultMod(destinationSack, [dragItem], destinationSackPanel.SackSize);

		sourcePanel.Invalidate();
		destinationPanel.Refresh();
		BagButtonTooltip.InvalidateCache(destinationSack, destinationSackPanel.Sack);
	}

	/// <summary>
	/// Calculates the destination index accounting for panel type offsets.
	/// </summary>
	private int GetDestinationIndex(SackPanel destinationSackPanel, int currentBag, int sackNumber)
	{
		if (destinationSackPanel.SackType is SackType.Vault or SackType.Player)
			return sackNumber;

		return currentBag;
	}
}

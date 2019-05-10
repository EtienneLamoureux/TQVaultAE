//-----------------------------------------------------------------------
// <copyright file="VaultPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Drawing;
	using System.Globalization;
	using System.Text;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultAE.GUI.Models;
	using TQVaultData;

	/// <summary>
	/// Represents a TQ Vault control that displays a frame around a group of TQ Vault panels with an optional caption.
	/// </summary>
	public class VaultPanel : Panel, INotifyPropertyChanged
	{
		/// <summary>
		/// player instance
		/// </summary>
		private PlayerCollection player;

		/// <summary>
		/// Array of Bag buttons for the panel
		/// </summary>
		private Collection<BagButtonBase> bagButtons;

		/// <summary>
		/// Array of autosort buttons
		/// </summary>
		private Collection<AutoSortButton> autoSortButtons;

		/// <summary>
		/// Holds the currently selected bag.
		/// </summary>
		private int currentBag;

		/// <summary>
		/// Context menu instance
		/// </summary>
		private ContextMenuStrip contextMenu;

		/// <summary>
		/// Initializes a new instance of the VaultPanel class.
		/// </summary>
		/// <param name="dragInfo">Instance of the ItemDragInfo</param>
		/// <param name="numberOfBags">Number of bags in this panel</param>
		/// <param name="panelSize">Main panel size</param>
		/// <param name="tooltip">Tooltip instance</param>
		/// <param name="numberOfAutosortButtons">The number of AutoSort buttons associated with this panel.,</param>
		/// <param name="autoMoveLocation">The automovelocation for this panel.</param>
		public VaultPanel(ItemDragInfo dragInfo, int numberOfBags, Size panelSize, TTLib tooltip, int numberOfAutosortButtons, AutoMoveLocation autoMoveLocation)
		{
			this.DragInfo = dragInfo;
			this.Tooltip = tooltip;
			this.AutoMoveLocation = autoMoveLocation;
			this.Text = Resources.PlayerPanelNoVault;
			this.NoPlayerString = Resources.PlayerPanelNoVault;
			this.BackColor = Color.Transparent;
			this.DrawAsGroupBox = false;

			// Setup the offset to make room for the autosort button
			int autosortOffset = 0;
			if (numberOfAutosortButtons > 0)
			{
				autosortOffset = Convert.ToInt32(27.0F * Database.DB.Scale);
			}

			this.Size = new Size(
				(panelSize.Width * Database.DB.ItemUnitSize) + Convert.ToInt32(10.0F * Database.DB.Scale) + autosortOffset + BorderPad,
				(panelSize.Height * Database.DB.ItemUnitSize) + Convert.ToInt32(56.0F * Database.DB.Scale) + BorderPad);
			this.TabStop = false;
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * Database.DB.Scale, this.Font.Style);

			this.BagPanelOffset = 0; // bag panel starts with bag #0
			this.BagSackPanel = new SackPanel(panelSize.Width, panelSize.Height, this.DragInfo, autoMoveLocation);
			this.BagSackPanel.SetLocation(new Point(autosortOffset + BorderPad, this.Size.Height - (this.BagSackPanel.Size.Height + BorderPad)));
			this.BagSackPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.BagSackPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.BagSackPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.BagSackPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.BagSackPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.BagSackPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			this.Controls.Add(this.BagSackPanel);
			this.BagSackPanel.IsPlayerBagPanel = false;
			this.BagSackPanel.MaxSacks = numberOfBags;

			// Create the buttons
			this.bagButtons = new Collection<BagButtonBase>();
			this.CreateBagButtons(numberOfBags);

			// Assume it's the trash panel if we are not autosorting it.
			if (numberOfAutosortButtons == 0)
			{
				this.BagSackPanel.SackType = SackType.Trash;
			}
			else
			{
				this.autoSortButtons = new Collection<AutoSortButton>();

				for (int i = 0; i < numberOfAutosortButtons; ++i)
				{
					this.autoSortButtons.Insert(i, this.CreateAutoSortButton(i));
					this.Controls.Add(this.autoSortButtons[i]);
				}

				this.BagSackPanel.SackType = SackType.Vault;
			}

			this.contextMenu = new ContextMenuStrip();
			this.contextMenu.BackColor = Color.FromArgb(46, 41, 31);
			this.contextMenu.DropShadowEnabled = true;
			this.contextMenu.Font = Program.GetFontAlbertusMT(9.0F * Database.DB.Scale);
			this.contextMenu.ForeColor = Color.FromArgb(200, 200, 200);
			this.contextMenu.Opacity = 0.80;
			this.contextMenu.ShowImageMargin = false;
			this.contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuItemClicked);
			this.contextMenu.Renderer = new CustomProfessionalRenderer();

			this.PropertyChanged += new PropertyChangedEventHandler(this.PropertyChangedCallback);
			this.Paint += new PaintEventHandler(this.PaintCallback);

			// to avoid flickering use double buffer and to force control to use OnPaint
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		}

		/// <summary>
		/// Event for signaling that a property has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region VaultPanel Properties

		/// <summary>
		/// Gets the Right to left reading options.
		/// </summary>
		public static MessageBoxOptions RightToLeftOptions
		{
			get
			{
				// Set options for Right to Left reading.
				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
				}

				return (MessageBoxOptions)0;
			}
		}

		/// <summary>
		/// Gets the scaled border pad.
		/// </summary>
		public static int BorderPad
		{
			get
			{
				{
					return Convert.ToInt32(2.0F * Database.DB.Scale);
				}
			}
		}

		/// <summary>
		/// Gets the bagbuttons array
		/// </summary>
		public Collection<BagButtonBase> BagButtons
		{
			get
			{
				return this.bagButtons;
			}
		}

		/// <summary>
		/// Gets the autosort buttons array
		/// </summary>
		public Collection<AutoSortButton> AutoSortButtons
		{
			get
			{
				return this.autoSortButtons;
			}
		}

		/// <summary>
		/// Gets or sets the Clear All Items Selected Event Handler
		/// </summary>
		public EventHandler<SackPanelEventArgs> OnClearAllItemsSelected { get; set; }

		/// <summary>
		/// Gets or sets the item selected Event Handler
		/// </summary>
		public EventHandler<SackPanelEventArgs> OnItemSelected { get; set; }

		/// <summary>
		/// Gets or sets the activate search Event Handler
		/// </summary>
		public EventHandler<SackPanelEventArgs> OnActivateSearch { get; set; }

		/// <summary>
		/// Gets or sets the resize form Event Handler
		/// </summary>
		public EventHandler<ResizeEventArgs> OnResizeForm { get; set; }

		/// <summary>
		/// Gets or sets the Automove Item Event Handler
		/// </summary>
		public EventHandler<SackPanelEventArgs> OnAutoMoveItem { get; set; }

		/// <summary>
		/// Gets or sets the New Item Highlighted Event Handler
		/// </summary>
		public EventHandler<SackPanelEventArgs> OnNewItemHighlighted { get; set; }

		/// <summary>
		/// Gets or sets the string used when no player is set.
		/// </summary>
		public string NoPlayerString { get; set; }

		/// <summary>
		/// Gets or sets the AutoMoveLocation for this panel.
		/// </summary>
		public AutoMoveLocation AutoMoveLocation { get; protected set; }

		/// <summary>
		/// Gets the Tooltip instance
		/// </summary>
		public TTLib Tooltip { get; private set; }

		/// <summary>
		/// Gets this panel's ItemDragInfo instance
		/// </summary>
		public ItemDragInfo DragInfo { get; private set; }

		/// <summary>
		/// Gets or sets the bag sack panel SackPanel instance
		/// </summary>
		public SackPanel BagSackPanel { get; set; }

		/// <summary>
		/// Gets or sets the bag offset.  Vaults are 1 based, player is 0 based.
		/// </summary>
		public int BagPanelOffset { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this form is rendered as a groupbox or panel.
		/// </summary>
		public bool DrawAsGroupBox { get; set; }

		/// <summary>
		/// Gets or sets the player instance
		/// </summary>
		public PlayerCollection Player
		{
			get
			{
				return this.player;
			}

			set
			{
				this.player = value;
				this.UpdateText();

				this.OnPropertyChanged("Player");
				this.Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the currently selected bag
		/// </summary>
		public int CurrentBag
		{
			get
			{
				return this.currentBag;
			}

			set
			{
				if (value != this.currentBag)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.currentBag].IsOn = false;
					this.BagButtons[value].IsOn = true;
					this.currentBag = value;
					this.BagSackPanel.Sack = this.Player.GetSack(this.currentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.currentBag;
				}
			}
		}

		/// <summary>
		/// Gets the SackPanel instance
		/// </summary>
		public SackPanel SackPanel
		{
			get
			{
				return this.BagSackPanel;
			}
		}

		#endregion VaultPanel Properties

		#region VaultPanel Public Methods

		/// <summary>
		/// Callback for the property changed event
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PropertyChangedEventArgs data</param>
		public void PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.ToUpperInvariant() == "PLAYER")
			{
				this.AssignSacks();
			}
		}

		/// <summary>
		/// Callback for the Paint event
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		public void PaintCallback(object sender, PaintEventArgs e)
		{
			if (this.DrawAsGroupBox)
			{
				GroupBoxRenderer.DrawGroupBox(e.Graphics, ClientRectangle, this.Text, this.Font, System.Windows.Forms.VisualStyles.GroupBoxState.Normal);
			}
		}

		/// <summary>
		/// Tooltip callback for displaying the bag summary
		/// </summary>
		/// <param name="windowHandle">Window handle</param>
		/// <returns>String containing the tooltip text</returns>
		public virtual string ToolTipCallback(int windowHandle)
		{
			string toolTipString = null;

			if (this.BagSackPanel != null)
			{
				toolTipString = SackPanel.ToolTipCallback(windowHandle);
			}

			if (toolTipString != null)
			{
				return toolTipString;
			}

			// check the bag buttons
			if (this.BagButtons != null && this.BagButtons.Count > 0)
			{
				foreach (BagButtonBase button in this.BagButtons)
				{
					if (button != null)
					{
						toolTipString = button.ToolTipCallback(windowHandle);
					}

					if (toolTipString != null)
					{
						return toolTipString;
					}
				}
			}

			return null;
		}

		#endregion VaultPanel Public Methods

		#region VaultPanel Protected Methods

		/// <summary>
		/// Assigns sacks to buttons.
		/// </summary>
		protected virtual void AssignSacks()
		{
			if ((this.Player == null) || (this.Player.NumberOfSacks < 1))
			{
				foreach (BagButtonBase button in this.BagButtons)
				{
					button.Visible = false;
				}

				this.BagSackPanel.Sack = null;

				this.HideAutoSortButtons();
			}
			else
			{
				this.UnhideAutoSortButtons();

				// figure out the current bag to use
				if (this.CurrentBag < 0)
				{
					this.CurrentBag = 0;
				}

				int numberOfBags = this.Player.NumberOfSacks - this.BagPanelOffset;

				if ((numberOfBags > 0) && (this.CurrentBag >= numberOfBags))
				{
					this.CurrentBag = this.Player.NumberOfSacks - 1;
				}

				// hide/show bag buttons and assign initial bitmaps
				int index = 0;
				foreach (BagButtonBase button in this.BagButtons)
				{
					button.Visible = index < numberOfBags;
					button.IsOn = index == this.CurrentBag;
					++index;
				}

				if (numberOfBags > 0)
				{
					this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
				}
				else
				{
					this.BagSackPanel.Sack = null;
				}
			}
		}

		/// <summary>
		/// Updates the text for the group box when the player is changed.
		/// </summary>
		protected virtual void UpdateText()
		{
			this.Text = string.Empty;
			if (this.DrawAsGroupBox)
			{
				this.Text = this.NoPlayerString;
				if (this.player != null)
				{
					this.Text = this.player.PlayerName;
				}
			}
		}

		/// <summary>
		/// Creates all of the bag buttons and adds them to the control.
		/// </summary>
		/// <param name="numberOfBags">total number of buttons to create</param>
		protected virtual void CreateBagButtons(int numberOfBags)
		{
			for (int i = 0; i < numberOfBags; ++i)
			{
				this.BagButtons.Insert(i, this.CreateBagButton(i, numberOfBags));
				this.Controls.Add(this.BagButtons[i]);
			}
		}

		/// <summary>
		/// Handler for clicking a bag button
		/// </summary>
		/// <remarks>
		/// Changed by VillageIdiot to support right and left mouse clicking.
		/// </remarks>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		protected virtual void BagButtonClick(object sender, MouseEventArgs e)
		{
			BagButtonBase b = (BagButtonBase)sender;

			int bagID = b.ButtonNumber;
			if (bagID != this.CurrentBag)
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.CurrentBag].IsOn = false;
				this.BagButtons[bagID].IsOn = true;
				this.CurrentBag = bagID;
				this.BagSackPanel.ClearSelectedItems();
				this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
				this.BagSackPanel.CurrentSack = this.CurrentBag;
			}

			if (e.Button == MouseButtons.Right)
			{
				// Only process vault bags with something in them.
				if (this.BagSackPanel.SackType == SackType.Vault && this.BagButtons[this.CurrentBag].ButtonText != null)
				{
					this.contextMenu.Items.Clear();

					// Add the move submenu
					this.AddSubMenu(Resources.PlayerPanelMenuMove, this.MoveBagClicked);

					// Only show Copy, Merge and Empty if something is in the bag.
					if (!this.BagSackPanel.Sack.IsEmpty)
					{
						if (Settings.Default.AllowItemCopy)
						{
							// Add the copy submenu
							this.AddSubMenu(Resources.PlayerPanelMenuCopy, this.CopyBagClicked);
						}

						// Add the merge submenu
						this.AddSubMenu(Resources.PlayerPanelMenuMerge, this.MergeBagClicked);

						this.contextMenu.Items.Add("-");
						this.contextMenu.Items.Add(Resources.PlayerPanelMenuEmpty);
					}

					this.contextMenu.Show(this.BagButtons[this.CurrentBag], new Point(e.X, e.Y));
				}
			}
		}

		/// <summary>
		/// Handler for clicking the autosort button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		protected virtual void AutoSortButtonClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.BagSackPanel.Autosort();
			}
		}

		/// <summary>
		/// Override of ScaleControl which supports font scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.contextMenu.Font = new Font(this.contextMenu.Font.FontFamily, 9.0F * Database.DB.Scale);
			this.Font = new Font(this.Font.Name, this.Font.SizeInPoints * factor.Height, this.Font.Style);

			base.ScaleControl(factor, specified);
		}

		/// <summary>
		/// New Item Highlighted Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		protected void NewItemHighlightedCallback(object sender, SackPanelEventArgs e)
		{
			this.OnNewItemHighlighted(sender, e);
		}

		/// <summary>
		/// Automove Item Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		protected void AutoMoveItemCallback(object sender, SackPanelEventArgs e)
		{
			this.OnAutoMoveItem(sender, e);
		}

		/// <summary>
		/// Activate Search Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		protected void ActivateSearchCallback(object sender, SackPanelEventArgs e)
		{
			this.OnActivateSearch(sender, e);
		}

		/// <summary>
		/// Resize Form Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResizeEventArgs data</param>
		protected void ResizeFormCallback(object sender, ResizeEventArgs e)
		{
			this.OnResizeForm(sender, e);
		}

		/// <summary>
		/// Item Selected Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		protected void ItemSelectedCallback(object sender, SackPanelEventArgs e)
		{
			this.OnItemSelected(sender, e);
		}

		/// <summary>
		/// Clear all selected items Callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		protected void ClearAllItemsSelectedCallback(object sender, SackPanelEventArgs e)
		{
			this.OnClearAllItemsSelected(sender, e);
		}

		/// <summary>
		/// Sets all of the autosort buttons to invisible.
		/// </summary>
		protected void HideAutoSortButtons()
		{
			if (this.autoSortButtons != null && this.autoSortButtons.Count > 0)
			{
				foreach (AutoSortButton autoSortButton in this.autoSortButtons)
				{
					autoSortButton.Visible = false;
				}
			}
		}

		/// <summary>
		/// Sets all of the autosort buttons to visible.
		/// </summary>
		protected void UnhideAutoSortButtons()
		{
			if (this.autoSortButtons != null && this.autoSortButtons.Count > 0)
			{
				this.autoSortButtons[0].Visible = true;

				if (this.autoSortButtons.Count == 2)
				{
					this.autoSortButtons[1].Visible = this.Player.NumberOfSacks - this.BagPanelOffset > 0;
				}
			}
		}

		/// <summary>
		/// Creates a new Autosort button
		/// </summary>
		/// <param name="buttonNumber">Number of the autosort button we are creating</param>
		/// <returns>A new AutoSortButton instance</returns>
		protected virtual AutoSortButton CreateAutoSortButton(int buttonNumber)
		{
			AutoSortButton button = new AutoSortButton(buttonNumber, true);
			button.Location = new Point(this.BagSackPanel.Location.X - button.Width, this.BagSackPanel.Location.Y);
			button.Visible = false;
			button.MouseDown += new MouseEventHandler(this.AutoSortButtonClick);

			return button;
		}

		/// <summary>
		/// Gets the scale factor for a particular button
		/// </summary>
		/// <param name="maxWidth">unscaled size of the button</param>
		/// <param name="numberOfBags">total number of bags to scale</param>
		/// <returns>scale factor float</returns>
		protected virtual float GetBagButtonScale(float maxWidth, int numberOfBags)
		{
			return ((float)this.BagSackPanel.Width / maxWidth) / (float)numberOfBags;
		}

		#endregion VaultPanel Protected Methods

		#region VaultPanel Private Methods

		/// <summary>
		/// Gets the index from the menu string using the delimiter in the resources since the strings are now regionalized.
		/// Helper for the context menu.
		/// </summary>
		/// <param name="selectedItem">string of the item selected in the menu</param>
		/// <returns>numeric index of the item selected.  -1 if it fails.</returns>
		private static int GetDestinationSackIndex(string selectedItem)
		{
			if (string.IsNullOrEmpty(selectedItem))
			{
				return -1;
			}

			int hashSign = Resources.GlobalMenuBag.IndexOf(Resources.GlobalMenuBagDelimiter, StringComparison.Ordinal) + 1;
			if (hashSign == -1)
			{
				return -1;
			}

			return Convert.ToInt32(selectedItem.Substring(hashSign), CultureInfo.InvariantCulture) - 1;
		}

		/// <summary>
		/// Creates a context menu submenu with choices for each bag in the panel
		/// except for the currently selected bag.
		/// </summary>
		/// <param name="menuText">Text label for the sub menu</param>
		/// <param name="menuCallback">handler for the submenu</param>
		private void AddSubMenu(string menuText, EventHandler menuCallback)
		{
			ToolStripItem[] menuChoices = new ToolStripItem[this.BagButtons.Count - 1];
			for (int i = 0, j = 0; i < this.BagButtons.Count; ++i)
			{
				if (i != this.CurrentBag)
				{
					int val = i + 1;
					menuChoices[j] = new ToolStripMenuItem(
						string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val),
						null,
						menuCallback,
						string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val));
					menuChoices[j].BackColor = this.contextMenu.BackColor;
					menuChoices[j].Font = this.contextMenu.Font;
					menuChoices[j].ForeColor = this.contextMenu.ForeColor;
					++j;
				}
			}

			ToolStripMenuItem subMenu = new ToolStripMenuItem(menuText, null, menuChoices);
			subMenu.BackColor = this.contextMenu.BackColor;
			subMenu.Font = this.contextMenu.Font;
			subMenu.ForeColor = this.contextMenu.ForeColor;
			subMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

			this.contextMenu.Items.Add(subMenu);

			return;
		}

		/// <summary>
		/// Property Changed event trigger
		/// </summary>
		/// <param name="name">Name of the property that has changed</param>
		private void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		/// <summary>
		/// Handler for clicking on the context menu
		/// </summary>
		/// <param name="sender">semder object</param>
		/// <param name="e">ToolStripItemClickedEventArgs data</param>
		private void ContextMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			string selectedItem = e.ClickedItem.Text;
			if (selectedItem == Resources.PlayerPanelMenuEmpty)
			{
				if (Settings.Default.SuppressWarnings || MessageBox.Show(
					Resources.PlayerPanelEmptyMsg,
					Resources.PlayerPanelEmpty,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					PlayerPanel.RightToLeftOptions) == DialogResult.Yes)
				{
					this.BagSackPanel.Sack.EmptySack();
					this.BagSackPanel.Refresh();
				}
			}
		}

		/// <summary>
		/// Handler for clicking on Copy Bag from the context menu
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CopyBagClicked(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (item != null)
			{
				int destinationIndex = VaultPanel.GetDestinationSackIndex(item.Name);

				if (destinationIndex > this.Player.NumberOfSacks)
				{
					return;
				}

				if (!this.Player.GetSack(destinationIndex + this.BagPanelOffset).IsEmpty)
				{
					if (Settings.Default.SuppressWarnings || MessageBox.Show(
						Resources.PlayerOverwriteSackMsg,
						Resources.PlayerOverwriteSack,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button1,
						VaultPanel.RightToLeftOptions) != DialogResult.Yes)
					{
						return;
					}
				}

				if (this.Player.CopySack(this.CurrentBag, destinationIndex))
				{
					if (this.CurrentBag != destinationIndex)
					{
						// turn off the current bag and turn on the new bag
						this.BagButtons[this.CurrentBag].IsOn = false;
						this.BagButtons[destinationIndex].IsOn = true;
						this.CurrentBag = destinationIndex;
						this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
						this.BagSackPanel.CurrentSack = this.CurrentBag;
					}
				}
			}
		}

		/// <summary>
		/// Handler for clicking move bag from the context menu
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MoveBagClicked(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (item != null)
			{
				int destinationIndex = VaultPanel.GetDestinationSackIndex(item.Name);
				if (this.Player.MoveSack(this.CurrentBag, destinationIndex))
				{
					if (this.CurrentBag != destinationIndex)
					{
						// turn off the current bag and turn on the new bag
						this.BagButtons[this.CurrentBag].IsOn = false;
						this.BagButtons[destinationIndex].IsOn = true;
						this.CurrentBag = destinationIndex;
						this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
						this.BagSackPanel.CurrentSack = this.CurrentBag;
					}
				}
			}
		}

		/// <summary>
		/// Handler for clicking Merge Bag on the context menu
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MergeBagClicked(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;
			if (item != null)
			{
				int destinationIndex = VaultPanel.GetDestinationSackIndex(item.Name);

				if (destinationIndex < 0 || destinationIndex > this.Player.NumberOfSacks || this.CurrentBag == destinationIndex)
				{
					return;
				}

				SackPanel dstSackPanel = this.BagSackPanel;
				if (dstSackPanel.MergeSack(destinationIndex))
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.CurrentBag].IsOn = false;
					this.BagButtons[destinationIndex].IsOn = true;
					this.CurrentBag = destinationIndex;
					this.BagSackPanel.Sack = this.Player.GetSack(this.CurrentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.CurrentBag;
				}
			}
		}

		/// <summary>
		/// Creates a new bag button
		/// </summary>
		/// <param name="bagNumber">Bag number that we are creating</param>
		/// <param name="numberOfBags">total number of buttons to create</param>
		/// <returns>Instance of the new BagButton</returns>
		private BagButton CreateBagButton(int bagNumber, int numberOfBags)
		{
			BagButton button = new BagButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.Tooltip);

			float buttonWidth = (float)Resources.inventorybagup01.Width;
			float buttonHeight = (float)Resources.inventorybagup01.Height;
			float pad = 2.0F;
			float slotWidth = buttonWidth + (2.0F * pad);

			// we need to scale down the bag size depending on the # we have
			float scale = this.GetBagButtonScale(slotWidth, numberOfBags);
			float bagSlotWidth = scale * slotWidth;

			button.Size = new Size((int)Math.Round(scale * buttonWidth), (int)Math.Round(scale * buttonHeight));
			float offset = (bagSlotWidth * bagNumber) + ((bagSlotWidth - button.Width) / 2.0F);

			button.Location = new Point(this.BagSackPanel.Location.X + (int)Math.Round(offset), this.BagSackPanel.Location.Y - button.Height);

			int bagLabelNumber = bagNumber - this.BagPanelOffset + 1;
			button.ButtonText = bagLabelNumber.ToString(CultureInfo.CurrentCulture);
			button.Visible = false;

			// Changed by VillageIdiot to support right and left mouse clicking.
			button.MouseDown += new MouseEventHandler(this.BagButtonClick);

			return button;
		}

		/// <summary>
		/// Gets the tooltip for the sack contents.
		/// </summary>
		/// <param name="button">Button number of the sack</param>
		/// <returns>string listing the sack's contents.</returns>
		private string GetSackToolTip(BagButtonBase button)
		{
			// Get the list of items and return them as a string
			SackCollection sack = this.Player.GetSack(button.ButtonNumber + this.BagPanelOffset);
			if (sack == null)
			{
				return null;
			}

			if (sack.IsEmpty)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0}<b>{1}</b>", Database.DB.TooltipTitleTag, Database.MakeSafeForHtml(Resources.VaultGroupBoxEmpty));
			}

			StringBuilder toolTipStringBuilder = new StringBuilder();
			toolTipStringBuilder.Append(Database.DB.TooltipTitleTag);
			bool first = true;
			foreach (Item item in sack)
			{
				if (this.DragInfo.IsActive && item == this.DragInfo.Item)
				{
					// skip the item being dragged
					continue;
				}

				if (!first)
				{
					toolTipStringBuilder.Append("<br>");
				}

				first = false;
				string itemString = Database.MakeSafeForHtml(item.ToString());
				Color color = item.GetColorTag(itemString);
				itemString = Item.ClipColorTag(itemString);
				string htmlcolor = Database.HtmlColor(color);
				string htmlLine = string.Format(CultureInfo.CurrentCulture, "<font color={0}><b>{1}</b></font>", htmlcolor, itemString);
				toolTipStringBuilder.Append(htmlLine);
			}

			return toolTipStringBuilder.ToString();
		}

		#endregion VaultPanel Private Methods

		/// <summary>
		/// Class for rendering the context menu.
		/// </summary>
		private class CustomProfessionalRenderer : ToolStripProfessionalRenderer
		{
			/// <summary>
			/// Handler for rendering the context menu items
			/// </summary>
			/// <param name="e">ToolStripItemTextRenderEventArgs data</param>
			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				if (e.Item.Selected)
				{
					e.TextColor = Color.Black;
				}
				else
				{
					e.TextColor = Color.FromArgb(200, 200, 200);
				}

				base.OnRenderItemText(e);
			}
		}
	}
}
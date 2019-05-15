//-----------------------------------------------------------------------
// <copyright file="StashPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Text;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultData;

	/// <summary>
	/// Class for handling the stash panel ui functions
	/// </summary>
	public class StashPanel : VaultPanel
	{
		#region StashPanel Fields

		/// <summary>
		/// Localized tab names
		/// </summary>
		private static string[] buttonNames =
		{
			Resources.StashPanelBtn1,
			Resources.StashPanelBtn2,
			Resources.StashPanelBtn3,
			Resources.GlobalRelicVaultStash
		};

		/// <summary>
		/// Stash file instance
		/// </summary>
		private Stash stash;

		/// <summary>
		/// Transfer stash file instance
		/// </summary>
		private Stash transferStash;

		/// <summary>
		/// Relic Vault stash file instance
		/// </summary>
		private Stash relicVaultStash;

		/// <summary>
		/// background bitmap
		/// </summary>
		private Bitmap background;

		/// <summary>
		/// Currently selected bag
		/// </summary>
		private int currentBag;

		/// <summary>
		/// Maximum size of the panel.  Uses the initial constructor size.
		/// </summary>
		private Size maxPanelSize;

		/// <summary>
		/// Equipment Panel instance
		/// </summary>
		private EquipmentPanel equipmentPanel;

		/// <summary>
		/// Background image for the equipment panel
		/// </summary>
		private Bitmap equipmentBackground;

		/// <summary>
		/// Background image for the stash vault panels
		/// </summary>
		private Bitmap stashBackground;

		#endregion StashPanel Fields

		/// <summary>
		/// Initializes a new instance of the StashPanel class.  Hard coded to 4 stash buttons and no autosort buttons.
		/// </summary>
		/// <param name="dragInfo">ItemDragInfo instance</param>
		/// <param name="panelSize">Size of the panel in cells</param>
		/// <param name="tooltip">ToolTip instance</param>
		public StashPanel(ItemDragInfo dragInfo, Size panelSize, TTLib tooltip) : base(dragInfo, 4, panelSize, tooltip, 0, AutoMoveLocation.Stash)
		{
			this.equipmentPanel = new EquipmentPanel(10, 14, dragInfo, AutoMoveLocation.Stash);

			this.Controls.Add(this.equipmentPanel);
			this.equipmentPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.equipmentPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.equipmentPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.equipmentPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.equipmentPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.equipmentPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

			this.Text = Resources.StashPanelText;
			this.NoPlayerString = Resources.StashPanelText;
			this.Size = new Size(
				(panelSize.Width * Database.DB.ItemUnitSize) + Convert.ToInt32(10.0F * Database.DB.Scale) + BorderPad,
				(panelSize.Height * Database.DB.ItemUnitSize) + Convert.ToInt32(60.0F * Database.DB.Scale) + BorderPad);
			this.Paint += new PaintEventHandler(this.PaintCallback);

			this.BagSackPanel.SetLocation(new Point(BorderPad, this.Size.Height - (this.BagSackPanel.Size.Height + BorderPad)));
			this.BagSackPanel.MaxSacks = 1;
			this.BagSackPanel.Anchor = AnchorStyles.None;

			this.EquipmentBackground = Resources.Equipment_bg_new;
			this.StashBackground = Resources.caravan_bg;

			// Set up the inital font size
			if (Database.DB.Scale != 1.0F)
			{
				this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * Database.DB.Scale, this.Font.Style);
			}

			this.background = Resources.Equipment_bg_new;

			// Now that the buttons are set we can move the panel
			this.BagSackPanel.SetLocation(new Point(
				BorderPad,
				this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + Database.DB.HalfUnitSize));

			int offsetX = (panelSize.Width - this.equipmentPanel.SackSize.Width) * Database.DB.HalfUnitSize;
			if (offsetX < 0)
			{
				offsetX = 0;
			}

			int offsetY = (panelSize.Height - this.equipmentPanel.SackSize.Height) * Database.DB.HalfUnitSize;
			if (offsetY < 0)
			{
				offsetY = 0;
			}

			this.equipmentPanel.SetLocation(new Point(
				offsetX,
				this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + offsetY));

			this.maxPanelSize = panelSize;

			this.BagSackPanel.Visible = false;
			this.BagSackPanel.Enabled = false;
		}

		#region StashPanel Properties

		/// <summary>
		/// Gets or sets the stash instance
		/// </summary>
		public Stash Stash
		{
			get
			{
				return this.stash;
			}

			set
			{
				this.stash = value;
				this.Text = Resources.StashPanelText;
				this.AssignSacks();
			}
		}

		/// <summary>
		/// Gets or sets the transfer stash instance
		/// </summary>
		public Stash TransferStash
		{
			get
			{
				return this.transferStash;
			}

			set
			{
				this.transferStash = value;
				this.Text = Resources.StashPanelText;
				this.AssignSacks();
			}
		}

		/// <summary>
		/// Gets or sets the relic vault stash instance
		/// </summary>
		public Stash RelicVaultStash
		{
			get
			{
				return this.relicVaultStash;
			}

			set
			{
				this.relicVaultStash = value;
				this.Text = Resources.StashPanelText;
				this.AssignSacks();
			}
		}

		/// <summary>
		/// Gets the SackPanel instance
		/// </summary>
		public new SackPanel SackPanel
		{
			get
			{
				if (this.CurrentBag == 0 && this.equipmentPanel != null)
				{
					return this.equipmentPanel;
				}

				return this.BagSackPanel;
			}
		}

		/// <summary>
		/// Gets or sets the background image for the equipment panel scaled by the panel size
		/// </summary>
		public Bitmap EquipmentBackground
		{
			get
			{
				if (Database.DB.Scale != 1.0F)
				{
					// We need to scale this since we use it to redraw the background.
					return new Bitmap(
						this.equipmentBackground,
						(this.maxPanelSize.Width * Database.DB.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
						(this.maxPanelSize.Height * Database.DB.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
				}

				return this.equipmentBackground;
			}

			set
			{
				this.equipmentBackground = value;
			}
		}

		/// <summary>
		/// Gets or sets the bitmap for the stash panel background scaled by the panel size.
		/// </summary>
		public Bitmap StashBackground
		{
			get
			{
				if (Database.DB.Scale != 1.0F)
				{
					// We need to scale this since we use it to redraw the background.
					return new Bitmap(
						this.stashBackground,
						(this.maxPanelSize.Width * Database.DB.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
						(this.maxPanelSize.Height * Database.DB.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
				}

				return this.stashBackground;
			}

			set
			{
				this.stashBackground = value;
			}
		}

		/// <summary>
		/// Gets or sets the currently selected bag id
		/// </summary>
		public new int CurrentBag
		{
			get
			{
				return this.currentBag;
			}

			set
			{
				int bagID = value;

				// figure out the current bag to use
				if (bagID < 0)
				{
					bagID = 0;
				}

				if (bagID >= 4)
				{
					bagID = 3;
				}

				if (bagID != this.currentBag)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.currentBag].IsOn = false;
					this.BagButtons[bagID].IsOn = true;
					this.currentBag = bagID;
					BagButtonBase button = this.BagButtons[this.currentBag];
					int buttonOffsetY = button.Location.Y + button.Size.Height;

					if (this.currentBag == 0)
					{
						// Equipment Panel
						if (this.Player == null)
						{
							this.equipmentPanel.Sack = null;
						}
						else
						{
							this.equipmentPanel.Sack = this.Player.EquipmentSack;
						}

						this.background = this.EquipmentBackground;
						this.equipmentPanel.Visible = true;
						this.equipmentPanel.Enabled = true;
						this.BagSackPanel.Visible = false;
						this.BagSackPanel.Enabled = false;
					}
					else if (this.currentBag == 1)
					{
						// Transfer Stash
						this.background = this.StashBackground;
						this.BagSackPanel.Sack = this.transferStash.Sack;
						this.BagSackPanel.SackType = SackType.Stash;
						this.BagSackPanel.ResizeSackPanel(this.transferStash.Width, this.transferStash.Height);

						// Adjust location based on size.
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.transferStash.Width) * Database.DB.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - this.transferStash.Height) * Database.DB.HalfUnitSize);

						this.BagSackPanel.Location = new Point(BorderPad + offsetX, buttonOffsetY + offsetY);
						this.equipmentPanel.Visible = false;
						this.equipmentPanel.Enabled = false;
						this.BagSackPanel.Visible = true;
						this.BagSackPanel.Enabled = true;
					}
					else if (this.currentBag == 2)
					{
						// Stash
						this.background = this.StashBackground;
						this.BagSackPanel.Sack = this.stash.Sack;
						this.BagSackPanel.SackType = SackType.Stash;
						this.BagSackPanel.ResizeSackPanel(this.stash.Width, this.stash.Height);

						// Adjust location based on size so it will be centered.
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.stash.Width) * Database.DB.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - Math.Max(15, this.stash.Height)) * Database.DB.HalfUnitSize);

						this.BagSackPanel.Location = new Point(BorderPad + offsetX, buttonOffsetY + offsetY);
						this.equipmentPanel.Visible = false;
						this.equipmentPanel.Enabled = false;
						this.BagSackPanel.Visible = true;
						this.BagSackPanel.Enabled = true;
					}
					else if (this.currentBag == 3)
					{
						// Relic Vault Stash
						this.background = this.StashBackground;
						this.BagSackPanel.Sack = this.relicVaultStash.Sack;
						this.BagSackPanel.SackType = SackType.Stash;
						this.BagSackPanel.ResizeSackPanel(this.relicVaultStash.Width, this.relicVaultStash.Height);

						// Adjust location based on size.
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.relicVaultStash.Width) * Database.DB.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - this.relicVaultStash.Height) * Database.DB.HalfUnitSize);

						this.BagSackPanel.Location = new Point(BorderPad + offsetX, buttonOffsetY + offsetY);
						this.equipmentPanel.Visible = false;
						this.equipmentPanel.Enabled = false;
						this.BagSackPanel.Visible = true;
						this.BagSackPanel.Enabled = true;
					}

					this.Refresh();
				}
			}
		}

		#endregion StashPanel Properties

		/// <summary>
		/// Sets the equipment panel background image and cursor background image.
		/// </summary>
		/// <param name="background">Bitmap which is to be used for the background.</param>
		public void SetEquipmentBackground(Bitmap background)
		{
			this.EquipmentBackground = background;
			this.background = background;
		}

		#region StashPanel Protected Methods

		/// <summary>
		/// Override of ScaleControl which supports scaling of the fonts and internal items.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl(factor, specified);

			// This should really only be set for the equipment panel.
		}

		/// <summary>
		/// Creates a new stash button
		/// </summary>
		/// <param name="bagNumber">The bag number for button that is being created.</param>
		/// <param name="numberOfBags">The total number of bags which is used for scaling.</param>
		/// <returns>The new StashButton that is created.</returns>
		protected StashButton CreateBagButton(int bagNumber, int numberOfBags)
		{
			StashButton button = new StashButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.Tooltip);

			float buttonWidth = (float)Resources.StashTabUp.Width;
			float buttonHeight = (float)Resources.StashTabUp.Height;
			float pad = 2.0F;
			float slotWidth = buttonWidth + (2.0F * pad);

			// we need to scale down the button size depending on the # we have
			float scale = this.GetBagButtonScale(slotWidth, numberOfBags);
			float bagSlotWidth = scale * slotWidth;

			// We are using tabs so only scale horizontally
			button.Size = new Size((int)Math.Round(scale * buttonWidth), Convert.ToInt32(buttonHeight * Database.DB.Scale));
			float offset = (bagSlotWidth * bagNumber) + ((bagSlotWidth - button.Width) / 2.0F);

			button.Location = new Point(this.SackPanel.Location.X + (int)Math.Round(offset), this.SackPanel.Location.Y - button.Height);
			button.Visible = false;
			button.MouseDown += new MouseEventHandler(this.BagButtonClick);
			button.ButtonText = buttonNames[bagNumber];

			return button;
		}

		/// <summary>
		/// Creates all of the stash buttons.
		/// </summary>
		/// <param name="numberOfBags">The total number of buttons to create.</param>
		/// <remarks>We need this override since the CreateBagButton() method above has a different return type than the base class.</remarks>
		protected override void CreateBagButtons(int numberOfBags)
		{
			for (int i = 0; i < numberOfBags; ++i)
			{
				this.BagButtons.Insert(i, this.CreateBagButton(i, numberOfBags));
				this.Controls.Add(this.BagButtons[i]);
			}
		}

		/// <summary>
		/// Updates the text for the group box.  Override the base method since we do not need to change it based on the player name.
		/// </summary>
		protected override void UpdateText()
		{
			this.Text = string.Empty;
			if (this.DrawAsGroupBox)
			{
				this.Text = Resources.StashPanelText;
			}
		}

		/// <summary>
		/// Assigns sacks to bag buttons
		/// </summary>
		protected override void AssignSacks()
		{
			// figure out the current bag to use
			if (this.currentBag < 0)
			{
				this.currentBag = 0;
			}

			if (this.currentBag >= 4)
			{
				this.currentBag = 3;
			}

			// hide/show bag buttons and assign initial bitmaps
			int buttonOffset = 0;
			foreach (StashButton button in this.BagButtons)
			{
				button.Visible = buttonOffset < 4;
				button.IsOn = buttonOffset == this.currentBag;
				++buttonOffset;
			}

			if ((this.relicVaultStash == null) || (this.relicVaultStash.NumberOfSacks < 1))
			{
				this.BagButtons[3].Visible = false;
			}

			if ((this.stash == null) || (this.stash.NumberOfSacks < 1))
			{
				this.BagButtons[2].Visible = false;
			}

			if ((this.transferStash == null) || (this.transferStash.NumberOfSacks < 1))
			{
				this.BagButtons[1].Visible = false;
			}

			if (this.currentBag == 0)
			{
				if (this.Player == null)
				{
					this.SackPanel.Sack = null;
				}
				else
				{
					this.SackPanel.Sack = this.Player.EquipmentSack;
				}
			}
			else if (this.currentBag == 1)
			{
				// Assign the transfer stash
				if ((this.transferStash == null) || (this.transferStash.NumberOfSacks < 1))
				{
					this.SackPanel.Sack = null;
				}
				else
				{
					if (this.transferStash.NumberOfSacks > 0)
					{
						this.SackPanel.Sack = this.transferStash.Sack;
					}
					else
					{
						this.SackPanel.Sack = null;
					}
				}
			}
			else if (this.currentBag == 2)
			{
				if ((this.stash == null) || (this.stash.NumberOfSacks < 1))
				{
					this.SackPanel.Sack = null;
				}
				else
				{
					if (this.stash.NumberOfSacks > 0)
					{
						this.SackPanel.Sack = this.stash.Sack;
					}
					else
					{
						this.SackPanel.Sack = null;
					}
				}
			}
			else if (this.currentBag == 3)
			{
				// Assign the relic vault stash
				if ((this.relicVaultStash == null) || (this.relicVaultStash.NumberOfSacks < 1))
				{
					this.SackPanel.Sack = null;
				}
				else
				{
					if (this.relicVaultStash.NumberOfSacks > 0)
					{
						this.SackPanel.Sack = this.relicVaultStash.Sack;
					}
					else
					{
						this.SackPanel.Sack = null;
					}
				}
			}
		}

		/// <summary>
		/// Handler for clicking on a bag button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected override void BagButtonClick(object sender, MouseEventArgs e)
		{
			BagButtonBase button = (BagButtonBase)sender;

			int bagID = button.ButtonNumber;

			// filter the bagID
			if (bagID < 0)
			{
				bagID = 0;
			}

			if (bagID >= 4)
			{
				bagID = 3;
			}

			if (bagID != this.currentBag)
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.currentBag].IsOn = false;
				this.BagButtons[bagID].IsOn = true;

				// set the current bag to the selected bag
				this.CurrentBag = bagID;
				this.SackPanel.ClearSelectedItems();
			}
		}

		/// <summary>
		/// Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		protected new void PaintCallback(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(this.background, this.GetBackgroundRect());
			base.PaintCallback(sender, e);
		}

		#endregion StashPanel Protected Methods

		#region StashPanel Private Methods

		/// <summary>
		/// Gets the tooltip for a sack.  Summarizes the items within the sack
		/// </summary>
		/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
		/// <returns>string to be displayed in the tooltip</returns>
		private string GetSackToolTip(BagButtonBase button)
		{
			// Get the list of items and return them as a string
			int bagID = button.ButtonNumber;
			SackCollection sack;

			if (bagID == 0)
			{
				if (this.Player == null)
				{
					sack = null;
				}
				else
				{
					sack = this.Player.EquipmentSack;
				}
			}
			else if (bagID == 1)
			{
				sack = this.transferStash.Sack;
			}
			else if (bagID == 2)
			{
				sack = this.stash.Sack;
			}
			else
			{
				sack = this.relicVaultStash.Sack;
			}

			if (sack == null || sack.IsEmpty)
			{
				return null;
			}

			StringBuilder answer = new StringBuilder();
			answer.Append(Database.DB.TooltipTitleTag);
			bool first = true;
			foreach (Item item in sack)
			{
				if (this.DragInfo.IsActive && item == this.DragInfo.Item)
				{
					// skip the item being dragged
					continue;
				}

				if (item.BaseItemId.Length == 0)
				{
					// skip empty items
					continue;
				}

				if (!first)
				{
					answer.Append("<br>");
				}

				first = false;
				string text = Database.MakeSafeForHtml(item.ToString());
				Color color = item.GetColorTag(text);
				text = Item.ClipColorTag(text);
				string htmlcolor = Database.HtmlColor(color);
				string htmlLine = string.Format(CultureInfo.CurrentCulture, "<font color={0}><b>{1}</b></font>", htmlcolor, text);
				answer.Append(htmlLine);
			}

			return answer.ToString();
		}

		/// <summary>
		/// Gets a rectangle scaled by the size of the child vault panels.
		/// </summary>
		/// <returns>Rectangle containg the size and position of the child vault panel.</returns>
		private Rectangle GetBackgroundRect()
		{
			if (this.BagButtons == null || this.BagButtons.Count == 0)
			{
				return Rectangle.Empty;
			}

			return new Rectangle(
				BorderPad,
				this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height,
				this.Width - (BorderPad * 2),
				this.Height - (this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + BorderPad));
		}

		#endregion StashPanel Private Methods
	}
}
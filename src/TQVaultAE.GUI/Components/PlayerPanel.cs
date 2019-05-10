//-----------------------------------------------------------------------
// <copyright file="PlayerPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultAE.GUI.Models;
	using TQVaultData;

	/// <summary>
	/// Class for holding the UI functions on the player panel.
	/// </summary>
	public class PlayerPanel : VaultPanel
	{
		/// <summary>
		/// Main sack panel instance
		/// </summary>
		private SackPanel mainSackPanel;

		/// <summary>
		/// Initializes a new instance of the PlayerPanel class.
		/// </summary>
		/// <param name="dragInfo">Instance of the ItemDragInfo</param>
		/// <param name="numberOfBags">Number of bags in this panel</param>
		/// <param name="panel1Size">Main panel size</param>
		/// <param name="panel2Size">Secondary panel size for players who have the additional game sacks</param>
		/// <param name="tooltip">Tooltip instance</param>
		public PlayerPanel(ItemDragInfo dragInfo, int numberOfBags, Size panel1Size, Size panel2Size, TTLib tooltip)
			: base(dragInfo, numberOfBags, panel2Size, tooltip, 2, AutoMoveLocation.Player)
		{
			this.Text = Resources.PlayerPanelNoPlayer;
			this.NoPlayerString = Resources.PlayerPanelNoPlayer;

			this.Size = new Size(
				((panel1Size.Width + panel2Size.Width) * Database.DB.ItemUnitSize) + Convert.ToInt32(24.0F * Database.DB.Scale),
				(Math.Max(panel1Size.Height, panel2Size.Height) * Database.DB.ItemUnitSize) + Convert.ToInt32(58.0F * Database.DB.Scale));
			this.TabStop = false;
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * Database.DB.Scale, this.Font.Style);

			int borderPad = Convert.ToInt32(2.0F * Database.DB.Scale);

			this.BagPanelOffset = 1; // bag panel starts with bag #1
			this.mainSackPanel = new SackPanel(panel1Size.Width, panel1Size.Height, this.DragInfo, AutoMoveLocation.Player);
			this.mainSackPanel.SetLocation(new Point(borderPad, this.Size.Height - this.mainSackPanel.Size.Height - borderPad));
			this.mainSackPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.mainSackPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.mainSackPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.mainSackPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.mainSackPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.mainSackPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			this.Controls.Add(this.mainSackPanel);
			this.mainSackPanel.MaxSacks = 1;
			this.mainSackPanel.SackType = SackType.Player;
			this.mainSackPanel.IsMainPlayerPanel = true;

			this.BagSackPanel.SetLocation(new Point(this.mainSackPanel.Right + borderPad, this.Size.Height - this.BagSackPanel.Size.Height - borderPad));
			this.BagSackPanel.IsPlayerBagPanel = true;

			// Recalculate the button sizing and placement since we moved the BagSackPanel.
			if (this.BagButtons != null && this.BagButtons.Count > 0)
			{
				float buttonWidth = (float)Resources.inventorybagup01.Width;
				float buttonHeight = (float)Resources.inventorybagup01.Height;
				float pad = 2.0F;
				float slotWidth = buttonWidth + (2.0F * pad);

				// we need to scale down the bag size depending on the # we have
				// but keep room for the autosort button so the buttons only use half of the panel size.
				float scale = this.GetBagButtonScale(slotWidth, (this.BagButtons.Count * 2));
				float bagSlotWidth = scale * slotWidth;

				int index = 0;
				foreach (BagButtonBase button in this.BagButtons)
				{
					button.Size = new Size((int)Math.Round(scale * buttonWidth), (int)Math.Round(scale * buttonHeight));
					float offset = (bagSlotWidth * index) + ((bagSlotWidth - button.Width) / 2.0F);

					button.Location = new Point(this.BagSackPanel.Location.X + (int)Math.Round(offset), this.BagSackPanel.Location.Y - button.Height);
					index++;
				}
			}

			// Move the autosort buttons to their place since the panels got moved.
			AutoSortButtons[0].Location = new Point(this.mainSackPanel.Location.X, this.mainSackPanel.Location.Y - AutoSortButtons[0].Height);
			AutoSortButtons[1].Location = new Point(
				this.BagSackPanel.Location.X + this.BagSackPanel.Width - AutoSortButtons[1].Width,
				this.BagSackPanel.Location.Y - AutoSortButtons[1].Height);

			this.BagSackPanel.SackType = SackType.Sack;
		}

		/// <summary>
		/// Gets the SackPanel instance
		/// </summary>
		public new SackPanel SackPanel
		{
			get
			{
				if (this.mainSackPanel != null)
				{
					return this.mainSackPanel;
				}
				else
				{
					return this.BagSackPanel;
				}
			}
		}

		/// <summary>
		/// Tooltip callback for displaying the bag summary
		/// </summary>
		/// <param name="windowHandle">Window handle</param>
		/// <returns>String containing the tooltip text</returns>
		public override string ToolTipCallback(int windowHandle)
		{
			string toolTipString = null;
			if (this.mainSackPanel != null)
			{
				toolTipString = SackPanel.ToolTipCallback(windowHandle);
			}

			if (toolTipString != null)
			{
				return toolTipString;
			}

			return base.ToolTipCallback(windowHandle);
		}

		/// <summary>
		/// Creates a new Autosort button
		/// </summary>
		/// <param name="buttonNumber">Number of the autosort button we are creating</param>
		/// <returns>A new AutoSortButton instance</returns>
		protected override AutoSortButton CreateAutoSortButton(int buttonNumber)
		{
			AutoSortButton button = new AutoSortButton(buttonNumber, false);

			// Temporary location since we will be moving the panel later.
			// This is called from the base constructor.
			button.Location = new Point(
				this.BagSackPanel.Location.X + this.BagSackPanel.Width - button.Width,
				this.BagSackPanel.Location.Y - button.Height);

			button.Visible = false;
			button.MouseDown += new MouseEventHandler(this.AutoSortButtonClick);
			return button;
		}

		/// <summary>
		/// Assigns sack to buttons
		/// </summary>
		protected override void AssignSacks()
		{
			if (this.mainSackPanel != null)
			{
				if ((this.Player == null) || (this.Player.NumberOfSacks < 1))
				{
					this.mainSackPanel.Sack = null;
				}
				else
				{
					this.mainSackPanel.Sack = this.Player.GetSack(0);
					this.mainSackPanel.MaxSacks = this.Player.NumberOfSacks;
				}
			}

			base.AssignSacks();
		}

		/// <summary>
		/// Handler for clicking the autosort button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		protected override void AutoSortButtonClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				AutoSortButton button = (AutoSortButton)sender;
				int buttonID = button.ButtonNumber;
				if (buttonID == 1)
				{
					// Secondary panel autosort was clicked.
					this.BagSackPanel.Autosort();
				}
				else if (buttonID == 0)
				{
					// Main panel autosort was clicked.
					this.mainSackPanel.Autosort();
				}
			}
		}
	}
}
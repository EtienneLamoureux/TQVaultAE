//-----------------------------------------------------------------------
// <copyright file="StashPanel.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using TQVaultAE.GUI.Models;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Presentation;
	using System.Collections.Generic;
	using Microsoft.Extensions.DependencyInjection;
	using TQVaultAE.Domain.Contracts.Services;

	/// <summary>
	/// Class for handling the stash panel ui functions
	/// </summary>
	public class StashPanel : VaultPanel, IScalingControl
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

		private BufferedTableLayoutPanel PlayerPanel;

		string DisplayPlayerInfoLastName;
		private const int NORMAL_PLAYERINFO_HEIGHT = 480;//BGImage = 490
		private const int NORMAL_PLAYERINFO_WIDTH = 186;//BGImage = 186
		private const int NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_WIDTH = 17;
		private const int NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_HEIGHT = 25;

		int PLAYERINFO_TOPRIGHT => this.ClientRectangle.Right - Convert.ToInt32(NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_WIDTH * this.UIService.Scale);
		int PLAYERINFO_TOPHEIGHT =>
			this.BagSackPanel.ClientRectangle.Top + this.BagButtons[0].Size.Height + UIService.HalfUnitSize
			+ Convert.ToInt32(NORMAL_PLAYERINFO_TOPRIGHT_ORIGIN_HEIGHT * this.UIService.Scale);
		int PLAYERINFO_WIDTH => Convert.ToInt32(NORMAL_PLAYERINFO_WIDTH * this.UIService.Scale);
		int PLAYERINFO_HEIGHT => Convert.ToInt32(NORMAL_PLAYERINFO_HEIGHT * this.UIService.Scale);

		private readonly ITranslationService TranslationService;

		#endregion StashPanel Fields

		/// <summary>
		/// Initializes a new instance of the StashPanel class.  Hard coded to 4 stash buttons and no autosort buttons.
		/// </summary>
		/// <param name="dragInfo">ItemDragInfo instance</param>
		/// <param name="panelSize">Size of the panel in cells</param>
		/// <param name="tooltip">ToolTip instance</param>
		public StashPanel(ItemDragInfo dragInfo, Size panelSize, IServiceProvider serviceProvider) : base(dragInfo, 4, panelSize, 0, AutoMoveLocation.Stash, serviceProvider)
		{
			this.TranslationService = this.ServiceProvider.GetService<ITranslationService>();

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			this.equipmentPanel = new EquipmentPanel(16, 14, dragInfo, AutoMoveLocation.Stash, serviceProvider);
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
				(panelSize.Width * UIService.ItemUnitSize) + Convert.ToInt32(10.0F * UIService.Scale) + BorderPad,
				(panelSize.Height * UIService.ItemUnitSize) + Convert.ToInt32(60.0F * UIService.Scale) + BorderPad);
			this.Paint += new PaintEventHandler(this.PaintCallback);

			this.BagSackPanel.SetLocation(new Point(BorderPad, this.Size.Height - (this.BagSackPanel.Size.Height + BorderPad)));
			this.BagSackPanel.MaxSacks = 1;
			this.BagSackPanel.Anchor = AnchorStyles.None;

			//this.EquipmentBackground = Resources.Equipment_bg_new;
			this.background = this.EquipmentBackground = Resources.equipment_bg_and_char;
			this.StashBackground = Resources.caravan_bg;

			// Set up the inital font size
			if (UIService.Scale != 1.0F)
				this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * UIService.Scale, this.Font.Style);

			// Now that the buttons are set we can move the panel
			this.BagSackPanel.SetLocation(new Point(
				BorderPad,
				this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + UIService.HalfUnitSize));

			int offsetX = (panelSize.Width - this.equipmentPanel.SackSize.Width) * UIService.HalfUnitSize;
			if (offsetX < 0)
				offsetX = 0;

			int offsetY = (panelSize.Height - this.equipmentPanel.SackSize.Height) * UIService.HalfUnitSize;
			if (offsetY < 0)
				offsetY = 0;

			this.equipmentPanel.SetLocation(new Point(
				offsetX,
				this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + offsetY));

			this.maxPanelSize = panelSize;

			this.BagSackPanel.Visible = false;
			this.BagSackPanel.Enabled = false;

			#region Init Player Panel

			// Based on testing at scale 1
			var table = new BufferedTableLayoutPanel()
			{
				Location = new Point(
					PLAYERINFO_TOPRIGHT - PLAYERINFO_WIDTH
					, PLAYERINFO_TOPHEIGHT
				),
				Size = new Size(PLAYERINFO_WIDTH, PLAYERINFO_HEIGHT),
				//CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
				Margin = new Padding(0),
				Padding = new Padding(1),
				BackColor = Color.Transparent,
			};
			table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
			table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

			this.Controls.Add(table);
			this.PlayerPanel = table;

			table.BringToFront();

			this.equipmentPanel.VisibleChanged += EquipmentPanel_VisibleChanged;

			#endregion

		}

		private void EquipmentPanel_VisibleChanged(object sender, EventArgs e)
			=> DisplayPlayerInfo();

		private void DisplayPlayerInfo()
		{
			if (this.Player == null || !this.equipmentPanel.Visible || this.BagButtons[this.CurrentBag].ButtonText != Resources.StashPanelBtn1)
			{
				this.PlayerPanel.Visible = false;
				return;
			}

			this.PlayerPanel.Visible = true;

			if (DisplayPlayerInfoLastName == this.Player.PlayerName) return;

			this.SuspendLayout();
			this.PlayerPanel.SuspendLayout();

			this.PlayerPanel.Visible = false;
			this.PlayerPanel.Controls.Clear();

			if (this.Player?.PlayerInfo != null)
			{
				DisplayPlayerInfoLastName = this.Player.PlayerName;
				var pclass = TranslationService.TranslateXTag(this.Player.PlayerInfo.Class) ?? string.Empty;
				var mclass = TranslationService.TranslateMastery(this.Player.PlayerInfo.Class) ?? string.Empty;
				mclass = mclass == Resources.Masteries ? string.Empty : mclass;
				var pi = new Dictionary<string, string>
				{
					[Resources.CurrentLevel] = this.Player.PlayerInfo.CurrentLevel.ToString(),
					[Resources.Class] = pclass,
					[Resources.Masteries] = mclass,
					[Resources.CurrentXP] = this.Player.PlayerInfo.CurrentXP.ToString(),
					[Resources.DifficultyUnlocked] = TranslationService.TranslateDifficulty(this.Player.PlayerInfo.DifficultyUnlocked) ?? "unknown",
					[Resources.Money] = this.Player.PlayerInfo.Money.ToString(),
					[Resources.SkillPoints] = this.Player.PlayerInfo.SkillPoints.ToString(),
					[Resources.AttributesPoints] = this.Player.PlayerInfo.AttributesPoints.ToString(),
					[Resources.BaseStrength] = this.Player.PlayerInfo.BaseStrength.ToString(),
					[Resources.BaseDexterity] = this.Player.PlayerInfo.BaseDexterity.ToString(),
					[Resources.BaseIntelligence] = this.Player.PlayerInfo.BaseIntelligence.ToString(),
					[Resources.BaseHealth] = this.Player.PlayerInfo.BaseHealth.ToString(),
					[Resources.BaseMana] = this.Player.PlayerInfo.BaseMana.ToString(),
					[Resources.PlayTimeInSeconds] = this.Player.PlayerInfo.PlayTimeInSeconds.ToString(),
					[Resources.NumberOfDeaths] = this.Player.PlayerInfo.NumberOfDeaths.ToString(),
					[Resources.NumberOfKills] = this.Player.PlayerInfo.NumberOfKills.ToString(),
					[Resources.ExperienceFromKills] = this.Player.PlayerInfo.ExperienceFromKills.ToString(),
					[Resources.HealthPotionsUsed] = this.Player.PlayerInfo.HealthPotionsUsed.ToString(),
					[Resources.ManaPotionsUsed] = this.Player.PlayerInfo.ManaPotionsUsed.ToString(),
					[Resources.NumHitsReceived] = this.Player.PlayerInfo.NumHitsReceived.ToString(),
					[Resources.NumHitsInflicted] = this.Player.PlayerInfo.NumHitsInflicted.ToString(),
					[Resources.CriticalHitsInflicted] = this.Player.PlayerInfo.CriticalHitsInflicted.ToString(),
					[Resources.CriticalHitsReceived] = this.Player.PlayerInfo.CriticalHitsReceived.ToString(),
				};

				var labStyle = BorderStyle.None;
				var mg = new Padding(0, 0, 0, 1);
				var pd = new Padding(0);
				int rowIdx = 0;
				var labFnt = new Font(this.Font.FontFamily, 7.5f * UIService.Scale);

				// Add Edit Button
				var editButton = new ScalingButton()
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Right,
					BackColor = Color.Transparent,
					DialogResult = DialogResult.OK,
					DownBitmap = Resources.MainButtonDown,
					FlatStyle = FlatStyle.Flat,
					ForeColor = Color.FromArgb(51, 44, 28),
					Image = Resources.MainButtonUp,
					Name = "editButton",
					OverBitmap = Resources.MainButtonOver,
					Size = new Size(137, 30),
					SizeToGraphic = false,
					Text = Resources.CharacterEditBtn,
					UpBitmap = Resources.MainButtonUp,
					UseCustomGraphic = true,
					UseVisualStyleBackColor = false,
					Font = FontService.GetFontLight(11F),
					Margin = new Padding(0, 0, 0, 5),
					Padding = new Padding(10, 0, 10, 0),
					Visible = Config.Settings.Default.AllowCharacterEdit,
				};
				editButton.FlatAppearance.BorderSize = 0;
				editButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
				editButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
				editButton.Click += EditButton_Click;

				this.PlayerPanel.SetColumnSpan(editButton, 2);
				this.PlayerPanel.Controls.Add(editButton, 0, rowIdx++);

				// Add Player Data
				foreach (var row in pi)
				{
					// Add Row
					this.PlayerPanel.Controls.Add(new ScalingLabel()
					{
						Margin = mg,
						Padding = pd,
						BorderStyle = labStyle,
						AutoSize = true,
						TextAlign = ContentAlignment.TopRight,
						Anchor = AnchorStyles.Top | AnchorStyles.Right,
						ForeColor = Color.White,
						Font = labFnt,
						Text = row.Key
					}, 0, rowIdx);
					this.PlayerPanel.Controls.Add(new ScalingLabel()
					{
						Margin = mg,
						Padding = pd,
						BorderStyle = labStyle,
						AutoSize = true,
						TextAlign = ContentAlignment.TopLeft,
						Anchor = AnchorStyles.Top | AnchorStyles.Left,
						ForeColor = Color.YellowGreen,
						Font = labFnt,
						Text = row.Value
					}, 1, rowIdx);

					rowIdx++;
				}
			}

			this.PlayerPanel.ResumeLayout();
			this.ResumeLayout();

			this.PlayerPanel.Visible = true;
		}

		private void EditButton_Click(object sender, EventArgs e)
		{
			var dlg = this.ServiceProvider.GetService<CharacterEditDialog>();
			dlg.PlayerCollection = this.Player;
			dlg.ShowDialog();
			this.DisplayPlayerInfoLastName = null;
			this.DisplayPlayerInfo();
		}

		#region StashPanel Properties

		/// <summary>
		/// Gets or sets the stash instance
		/// </summary>
		public Stash Stash
		{
			get => this.stash;

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
			get => this.transferStash;

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
			get => this.relicVaultStash;

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
					return this.equipmentPanel;

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
				if (UIService.Scale != 1.0F)
				{
					// We need to scale this since we use it to redraw the background.
					return new Bitmap(
						this.equipmentBackground,
						(this.maxPanelSize.Width * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
						(this.maxPanelSize.Height * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
				}

				return this.equipmentBackground;
			}

			set => this.equipmentBackground = value;
		}

		/// <summary>
		/// Gets or sets the bitmap for the stash panel background scaled by the panel size.
		/// </summary>
		public Bitmap StashBackground
		{
			get
			{
				if (UIService.Scale != 1.0F)
				{
					// We need to scale this since we use it to redraw the background.
					return new Bitmap(
						this.stashBackground,
						(this.maxPanelSize.Width * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2),
						(this.maxPanelSize.Height * UIService.ItemUnitSize) + (Convert.ToInt32(SackPanel.BorderWidth) * 2));
				}

				return this.stashBackground;
			}

			set => this.stashBackground = value;
		}

		/// <summary>
		/// Gets or sets the currently selected bag id
		/// </summary>
		public new int CurrentBag
		{
			get => this.currentBag;

			set
			{
				int bagID = value;

				// figure out the current bag to use
				if (bagID < 0)
					bagID = 0;

				if (bagID >= 4)
					bagID = 3;

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
							this.equipmentPanel.Sack = null;
						else
							this.equipmentPanel.Sack = this.Player.EquipmentSack;

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
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.transferStash.Width) * UIService.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - this.transferStash.Height) * UIService.HalfUnitSize);

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
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.stash.Width) * UIService.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - Math.Max(15, this.stash.Height)) * UIService.HalfUnitSize);

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
						int offsetX = Math.Max(0, (this.maxPanelSize.Width - this.relicVaultStash.Width) * UIService.HalfUnitSize);
						int offsetY = Math.Max(0, (this.maxPanelSize.Height - this.relicVaultStash.Height) * UIService.HalfUnitSize);

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

		#region StashPanel Protected Methods

		/// <summary>
		/// Override of ScaleControl which supports scaling of the fonts and internal items.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl(factor, specified);
		}

		/// <summary>
		/// Creates a new stash button
		/// </summary>
		/// <param name="bagNumber">The bag number for button that is being created.</param>
		/// <param name="numberOfBags">The total number of bags which is used for scaling.</param>
		/// <returns>The new StashButton that is created.</returns>
		protected StashButton CreateBagButton(int bagNumber, int numberOfBags)
		{
			StashButton button = new StashButton(bagNumber, new GetToolTip(this.GetSackToolTip), this.ServiceProvider);

			float buttonWidth = (float)Resources.StashTabUp.Width;
			float buttonHeight = (float)Resources.StashTabUp.Height;
			float pad = 2.0F;
			float slotWidth = buttonWidth + (2.0F * pad);

			// we need to scale down the button size depending on the # we have
			float scale = this.GetBagButtonScale(slotWidth, numberOfBags);
			float bagSlotWidth = scale * slotWidth;

			// We are using tabs so only scale horizontally
			button.Size = new Size((int)Math.Round(scale * buttonWidth), Convert.ToInt32(buttonHeight * UIService.Scale));
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
				this.Text = Resources.StashPanelText;

			DisplayPlayerInfo();
		}

		/// <summary>
		/// Assigns sacks to bag buttons
		/// </summary>
		protected override void AssignSacks()
		{
			// figure out the current bag to use
			if (this.currentBag < 0)
				this.currentBag = 0;

			if (this.currentBag >= 4)
				this.currentBag = 3;

			// hide/show bag buttons and assign initial bitmaps
			int buttonOffset = 0;
			foreach (StashButton button in this.BagButtons)
			{
				button.Visible = buttonOffset < 4;
				button.IsOn = buttonOffset == this.currentBag;
				++buttonOffset;
			}

			if ((this.relicVaultStash == null) || (this.relicVaultStash.NumberOfSacks < 1))
				this.BagButtons[3].Visible = false;

			if ((this.stash == null) || (this.stash.NumberOfSacks < 1))
				this.BagButtons[2].Visible = false;

			if ((this.transferStash == null) || (this.transferStash.NumberOfSacks < 1))
				this.BagButtons[1].Visible = false;

			if (this.currentBag == 0)
			{
				if (this.Player == null)
					this.SackPanel.Sack = null;
				else
					this.SackPanel.Sack = this.Player.EquipmentSack;
			}
			else if (this.currentBag == 1)
			{
				// Assign the transfer stash
				if ((this.transferStash == null) || (this.transferStash.NumberOfSacks < 1))
					this.SackPanel.Sack = null;
				else
				{
					if (this.transferStash.NumberOfSacks > 0)
						this.SackPanel.Sack = this.transferStash.Sack;
					else
						this.SackPanel.Sack = null;
				}
			}
			else if (this.currentBag == 2)
			{
				if ((this.stash == null) || (this.stash.NumberOfSacks < 1))
					this.SackPanel.Sack = null;
				else
				{
					if (this.stash.NumberOfSacks > 0)
						this.SackPanel.Sack = this.stash.Sack;
					else
						this.SackPanel.Sack = null;
				}
			}
			else if (this.currentBag == 3)
			{
				// Assign the relic vault stash
				if ((this.relicVaultStash == null) || (this.relicVaultStash.NumberOfSacks < 1))
					this.SackPanel.Sack = null;
				else
				{
					if (this.relicVaultStash.NumberOfSacks > 0)
						this.SackPanel.Sack = this.relicVaultStash.Sack;
					else
						this.SackPanel.Sack = null;
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
				bagID = 0;

			if (bagID >= 4)
				bagID = 3;

			if (bagID != this.currentBag)
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.currentBag].IsOn = false;
				this.BagButtons[bagID].IsOn = true;

				// set the current bag to the selected bag
				this.CurrentBag = bagID;
				this.SackPanel.ClearSelectedItems();
			}

			DisplayPlayerInfo();
		}

		/// <summary>
		/// Paint callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">PaintEventArgs data</param>
		protected new void PaintCallback(object sender, PaintEventArgs e)
		{
			var rect = this.GetBackgroundRect();
			e.Graphics.DrawImage(this.background, rect);
			base.PaintCallback(sender, e);
		}

		#endregion StashPanel Protected Methods

		#region StashPanel Private Methods


		/// <summary>
		/// Gets the tooltip for a sack.  Summarizes the items within the sack
		/// </summary>
		/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
		/// <returns>string to be displayed in the tooltip</returns>
		private void GetSackToolTip(BagButtonBase button)
		{
			// Get the list of items and return them as a string
			int bagID = button.ButtonNumber;
			SackCollection sack;

			if (bagID == 0)
			{
				if (this.Player == null)
					sack = null;
				else
					sack = this.Player.EquipmentSack;
			}
			else if (bagID == 1)
				sack = this.transferStash.Sack;
			else if (bagID == 2)
				sack = this.stash.Sack;
			else
				sack = this.relicVaultStash.Sack;

			button.Sack = sack;
		}

		/// <summary>
		/// Gets a rectangle scaled by the size of the child vault panels.
		/// </summary>
		/// <returns>Rectangle containg the size and position of the child vault panel.</returns>
		private Rectangle GetBackgroundRect()
		{
			if (this.BagButtons == null || this.BagButtons.Count == 0)
				return Rectangle.Empty;

			return new Rectangle(
				BorderPad
				, this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height
				, this.Width - (BorderPad * 2)
				, this.Height - (this.BagButtons[0].Location.Y + this.BagButtons[0].Size.Height + BorderPad)
			);
		}

		#endregion StashPanel Private Methods
	}
}
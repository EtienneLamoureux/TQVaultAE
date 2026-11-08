//-----------------------------------------------------------------------
// <copyright file="StashPanel.PlayerInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for handling the stash panel ui functions
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Rendering;

public partial class StashPanel
{
	/// <summary>
	/// Calculates the stat bonuses for the skills and updates the player
	/// </summary>
	private void GetSkillStatBonus()
	{
		if (this.Player is not null)
			this.Services.PlayerService.UpdateSkillStatBonuses(this.Player);
	}

	private void DisplayPlayerInfo()
	{
		if (this.Player is null || !this.equipmentPanel.Visible || this.CurrentBag != BagIdConstants.BAGID_EQUIPMENTPANEL)
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

		if (this.Player?.PlayerInfo is not null)
		{
			DisplayPlayerInfoLastName = this.Player.PlayerName;
			var pi = PlayerInfoText.BuildLines(this.Player.PlayerInfo, this.TranslationService);

			var labStyle = BorderStyle.None;
			var mg = new Padding(0, 0, 0, 1);
			var pd = new Padding(0);
			int rowIdx = 0;
			// Zoom, not FontZoom: these labels are new, and Windows Forms scales them to the DPI of the panel when they are
			// added. With FontZoom, a player shown on a monitor of another DPI got the DPI ratio twice, and the text stayed
			// smaller (by that ratio) on every monitor until the next player change.
			var labFnt = new Font(this.Font.FontFamily, 7.5f * UIService.Zoom);

			// Add Edit Button
			var editButton = new ScalingButton()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = Color.Transparent,
				DialogResult = DialogResult.OK,
				DownBitmap = Images.MainButtonDown,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Theme.ButtonText,
				Image = Images.MainButtonUp,
				Name = "editButton",
				OverBitmap = Images.MainButtonOver,
				Size = new Size(137, 30),
				SizeToGraphic = false,
				Text = Resources.CharacterEditBtn,
				UpBitmap = Images.MainButtonUp,
				UseCustomGraphic = true,
				UseVisualStyleBackColor = false,
				Font = FontService.GetFontLight(11F),
				Margin = new Padding(0, 0, 0, 5),
				Padding = new Padding(10, 0, 10, 0),
				Visible = this.USettings.AllowCharacterEdit,
			};
			editButton.FlatAppearance.BorderSize = 0;
			editButton.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
			editButton.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
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
}

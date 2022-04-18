using System;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private ForgePanel forgePanel;

		private (bool configureButton, bool showVaulButton, bool searchButton, bool duplicateButton, bool saveButton, bool playerPanel, bool stashPanel, bool secondaryVaultPanel, bool flowLayoutPanelRightComboBox) lastVisibility;

		private ForgePanel CreateForgePanel()
		{
			forgePanel = new ForgePanel(DragInfo, ServiceProvider);

			flowLayoutPanelRightPanels.Controls.Add(forgePanel);
			forgePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			forgePanel.Visible = false;
			forgePanel.NotifyAction = ForgeNotify;
			forgePanel.CancelAction = ForgeHideUI;
			forgePanel.ForgeAction = ForgeHideUI;
			return forgePanel;
		}

		private void ForgeNotify(string message)
		{
			itemText.ForeColor = Color.Red;
			itemText.Text = message;
		}

		private void scalingButtonForge_Click(object sender, EventArgs e)
		{
			if (!forgePanel.Visible)
			{
				ForgeShowUI();

				return;
			}

			ForgeHideUI();
		}

		private void ForgeShowUI()
		{
			SoundService.PlayRandomMetalHit();

			// Save UI visibility
			lastVisibility = (
				configureButton: configureButton.Visible
				, showVaulButton: showVaulButton.Visible
				, searchButton: searchButton.Visible
				, duplicateButton: duplicateButton.Visible
				, saveButton: saveButton.Visible
				, playerPanel: playerPanel.Visible
				, stashPanel: stashPanel.Visible
				, secondaryVaultPanel: secondaryVaultPanel.Visible
				, flowLayoutPanelRightComboBox: flowLayoutPanelRightComboBox.Visible
			);

			// Hide buttons
			configureButton.Visible =
			showVaulButton.Visible =
			searchButton.Visible =
			duplicateButton.Visible =
			saveButton.Visible =

			// Hide right panels
			flowLayoutPanelRightComboBox.Visible =
			playerPanel.Visible =
			stashPanel.Visible =
			secondaryVaultPanel.Visible =
			false;

			// Show the forge
			forgePanel.Dock = DockStyle.Left;
			forgePanel.Visible = true;

		}

		private void ForgeHideUI()
		{
			SoundService.PlayRandomCancel();

			// Restore UI visibility
			configureButton.Visible = lastVisibility.configureButton;
			showVaulButton.Visible = lastVisibility.showVaulButton;
			searchButton.Visible = lastVisibility.searchButton;
			duplicateButton.Visible = lastVisibility.duplicateButton;
			saveButton.Visible = lastVisibility.saveButton;
			flowLayoutPanelRightComboBox.Visible = lastVisibility.flowLayoutPanelRightComboBox;
			playerPanel.Visible = lastVisibility.playerPanel;
			stashPanel.Visible = lastVisibility.stashPanel;
			secondaryVaultPanel.Visible = lastVisibility.secondaryVaultPanel;
			// Hide the forge
			forgePanel.Dock = DockStyle.None;
			forgePanel.Visible = false;

			Refresh();
		}
	}
}

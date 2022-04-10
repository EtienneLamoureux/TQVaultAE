using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	public partial class MainForm
	{
		private (bool configureButton, bool showVaulButton, bool searchButton, bool duplicateButton, bool saveButton, bool playerPanel, bool stashPanel, bool secondaryVaultPanel, bool flowLayoutPanelRightComboBox) lastVisibility;

		private ForgePanel CreateForgePanel()
		{
			this.forgePanel = new ForgePanel(this.UIService, this.FontService);

			this.flowLayoutPanelRightPanels.Controls.Add(this.forgePanel);
			this.forgePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			this.forgePanel.Visible = false;
			this.forgePanel.CancelAction = ForgeHideUI;
			return this.forgePanel;
		}

		private void scalingButtonForge_Click(object sender, EventArgs e)
		{
			if (!this.forgePanel.Visible)
			{
				ForgeShowUI();

				return;
			}

			ForgeHideUI();
		}

		private void ForgeShowUI()
		{
			// Save UI visibility
			this.lastVisibility = (
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
			this.forgePanel.Dock = DockStyle.Left;
			this.forgePanel.Visible = true;
		}

		private void ForgeHideUI()
		{
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
			this.forgePanel.Dock = DockStyle.None;
			this.forgePanel.Visible = false;
		}
	}
}

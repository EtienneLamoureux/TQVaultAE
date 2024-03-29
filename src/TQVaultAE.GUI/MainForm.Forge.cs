﻿using System;
using System.Windows.Forms;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI;

public partial class MainForm
{
	private ForgePanel forgePanel;

	private (
		bool configureButton
		, bool showVaulButton
		, bool searchButton
		, bool saveButton
		, bool playerPanel
		, bool stashPanel
		, bool secondaryVaultPanel
		, bool flowLayoutPanelRightComboBox
	) lastVisibility;

	private bool lastEnableDetailedTooltipView;

	private ForgePanel CreateForgePanel()
	{
		forgePanel = new ForgePanel(DragInfo, ServiceProvider);

		flowLayoutPanelRightPanels.Controls.Add(forgePanel);
		forgePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		forgePanel.Visible = false;
		forgePanel.CancelAction = ForgeActionCanceled;
		forgePanel.ForgeAction = ForgeActionForged;
		return forgePanel;
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

		this.lastEnableDetailedTooltipView = Config.UserSettings.Default.EnableDetailedTooltipView;
		Config.UserSettings.Default.EnableDetailedTooltipView = true;
	}

	private void ForgeActionForged()
	{
		ForgeHideUI();
	}

	private void ForgeActionCanceled()
	{
		SoundService.PlayRandomCancel();
		ForgeHideUI();
	}

	private void ForgeHideUI()
	{
		SoundService.PlayRandomCancel();

		// Restore UI visibility
		configureButton.Visible = lastVisibility.configureButton;
		showVaulButton.Visible = lastVisibility.showVaulButton;
		searchButton.Visible = lastVisibility.searchButton;
		saveButton.Visible = lastVisibility.saveButton;
		flowLayoutPanelRightComboBox.Visible = lastVisibility.flowLayoutPanelRightComboBox;
		playerPanel.Visible = lastVisibility.playerPanel;
		stashPanel.Visible = lastVisibility.stashPanel;
		secondaryVaultPanel.Visible = lastVisibility.secondaryVaultPanel;
		// Hide the forge
		forgePanel.Dock = DockStyle.None;
		forgePanel.Visible = false;

		Config.UserSettings.Default.EnableDetailedTooltipView = this.lastEnableDetailedTooltipView;

		Refresh();
	}
}

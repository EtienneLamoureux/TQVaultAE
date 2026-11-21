using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Forms;

/// <summary>Debug access to the main window: zoom, layout, character list and character windows.</summary>
public partial class MainForm
{
	/// <summary>Gets the table that lays out the vault, player and stash panels.</summary>
	internal TableLayoutPanel AutomationLayoutTable => this.mainView.tableLayoutPanelMain;

	/// <summary>The characters of the drop-down list.</summary>
	internal TQVaultAE.Application.Session.PlayerSave[] AutomationListedCharacters => this.mainView.ListedCharacters();

	/// <summary>Opens a floating character window like the button beside Edit.</summary>
	internal Dialogs.CharacterWindow? AutomationOpenCharacterWindow(TQVaultAE.Application.Session.PlayerSave save, Point? place)
		=> this.mainView.OpenCharacterWindow(save, place);

	/// <summary>Zooms like Ctrl + / Ctrl - (delta 0.1 / -0.1) or Ctrl Home (delta 1, back to 100 %).</summary>
	internal void AutomationZoom(float delta) => this.ResizeFormCallback(this, new ResizeEventArgs(delta));

	/// <summary>The largest client size Windows lets the window take, and the largest zoom step that fits it.</summary>
	internal (Size MaxClientSize, int LargestStep) AutomationZoomLimit => (this.MaxClientSize, this.LargestStep());

	/// <summary>The window's zoom step.</summary>
	internal int AutomationZoomStep => this.ZoomStep;
}

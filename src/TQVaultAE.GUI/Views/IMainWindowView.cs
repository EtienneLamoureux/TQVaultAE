namespace TQVaultAE.GUI.Views;

/// <summary>
/// A view the main window shows in place of its main content (<see cref="MainFormUserControl"/>), under its toolbar:
/// the settings, the character editor. The view is left by its own OK or Cancel; left another way (the toolbar's
/// Settings button, closing the window) while it holds edits, the window asks first.
/// </summary>
internal interface IMainWindowView
{
	/// <summary>Whether the user changed something that leaving the view without its OK drops.</summary>
	bool HasChanges { get; }

	/// <summary>The question asked before such edits are dropped, under the view's title (<see cref="Control.Text"/>).</summary>
	string DiscardChangesQuestion { get; }
}

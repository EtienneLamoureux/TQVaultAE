
namespace TQVaultAE.GUI.Components.Panels;

/// <summary>Debug access to the stash backgrounds.</summary>
public partial class StashPanel
{
	/// <summary>Gets the background drawn now: a source picture at zoom 1, else a scaled copy made at the tab switch.</summary>
	internal System.Drawing.Bitmap? AutomationBackground => this.background;

	/// <summary>Whether <paramref name="bitmap"/> is one of the source pictures (equipment or stash), kept for the session.</summary>
	internal bool AutomationIsSourceBackground(System.Drawing.Bitmap bitmap)
		=> ReferenceEquals(bitmap, this.equipmentBackground) || ReferenceEquals(bitmap, this.stashBackground);
}

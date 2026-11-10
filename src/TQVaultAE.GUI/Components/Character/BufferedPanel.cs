using System.Windows.Forms;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// A panel painted through a buffer, so that its background and its transparent children are drawn in one go: a plain
/// panel repaints them on screen one after the other, which flickers when a page with a picture background is shown.
/// </summary>
internal sealed class BufferedPanel : Panel
{
	public BufferedPanel()
	{
		this.DoubleBuffered = true;
		this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
	}
}

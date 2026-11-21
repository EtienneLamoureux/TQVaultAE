namespace TQVaultAE.GUI.Components.Layout;

using System.Windows.Forms;

public class BufferedTableLayoutPanel : TableLayoutPanel
{
	public BufferedTableLayoutPanel()
	{
		DoubleBuffered = true;
	}
}
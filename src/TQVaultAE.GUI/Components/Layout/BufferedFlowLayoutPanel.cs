namespace TQVaultAE.GUI.Components.Layout;

using System.Windows.Forms;

public class BufferedFlowLayoutPanel : FlowLayoutPanel
{
	public BufferedFlowLayoutPanel()
	{
		DoubleBuffered = true;
	}
}
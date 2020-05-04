namespace TQVaultAE.GUI.Components
{
	using System.Windows.Forms;

	public class BufferedFlowLayoutPanel : FlowLayoutPanel
	{
		public BufferedFlowLayoutPanel()
		{
			DoubleBuffered = true;
		}
	}
}

namespace TQVaultAE.GUI.Components
{
	using System.Windows.Forms;

	public class BufferedTableLayoutPanel : TableLayoutPanel
	{
		public BufferedTableLayoutPanel()
		{
			DoubleBuffered = true;
		}
	}
}

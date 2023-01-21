using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TQVaultAE.GUI.Inputs.Filters;
/// <summary>
/// Capture all mouse button event globally and trigger dedicated events
/// </summary>
public class FormFilterMouseButtonGlobally : IMessageFilter
{
	// Inspired by https://stackoverflow.com/questions/1371810/subscribing-to-mouse-events-of-all-controls-in-form

	internal const int WM_LBUTTONDOWN = 0x0201;
	internal const int WM_RBUTTONDOWN = 0x0204;

	private readonly VaultForm Form;

	public FormFilterMouseButtonGlobally(VaultForm Form)
	{
		this.Form = Form;
	}

	public bool PreFilterMessage(ref Message m)
	{
		int mouseInfo;
		//short y, x;
		Point point;
		switch (m.Msg)
		{
			case WM_LBUTTONDOWN:
				mouseInfo = m.LParam.ToInt32();
				//y = (short)(mouseInfo >> 16);
				//x = (short)(mouseInfo & 0xFFFF);
				point = new Point(mouseInfo);
				this.Form.RaiseGlobalMouseButtonLeft(point);
				break;
			case WM_RBUTTONDOWN:
				mouseInfo = m.LParam.ToInt32();
				//y = (short)(mouseInfo >> 16);
				//x = (short)(mouseInfo & 0xFFFF);
				point = new Point(mouseInfo);
				this.Form.RaiseGlobalMouseButtonRight(point);
				break;
		}

		return false;// Keep going
	}
}

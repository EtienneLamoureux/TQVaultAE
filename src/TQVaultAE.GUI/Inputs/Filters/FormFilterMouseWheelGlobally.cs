using System.Windows.Forms;

namespace TQVaultAE.GUI.Inputs.Filters;

/// <summary>
/// Capture all mouse wheel event globally and trigger dedicated events
/// </summary>
public class FormFilterMouseWheelGlobally : IMessageFilter
{
	// Inspired by https://www.appsloveworld.com/csharp/100/924/detect-mouse-wheel-on-a-button
	// and https://www.programmerall.com/article/67001647661/

	internal const int WM_MOUSEWHEEL = 0x020A;
	internal const int WM_MOUSEHWHEEL = 0x020E;

	private readonly VaultForm Form;

	public FormFilterMouseWheelGlobally(VaultForm Form)
	{
		this.Form = Form;
	}

	public bool PreFilterMessage(ref Message m)
	{
		switch (m.Msg)
		{
			case WM_MOUSEWHEEL:
			case WM_MOUSEHWHEEL:
				var param = m.WParam.ToInt64();
				var IsDown = ((int)param) < 0;

				if (IsDown)
					this.Form.RaiseGlobalMouseWheelDown();
				else
					this.Form.RaiseGlobalMouseWheelUp();
				break;
		}

		return false;// Keep going
	}
}
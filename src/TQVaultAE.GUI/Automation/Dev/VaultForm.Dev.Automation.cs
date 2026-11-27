using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.GUI.Interop;

namespace TQVaultAE.GUI.Forms;

/// <summary>Debug access to a window's border.</summary>
public partial class VaultForm
{
	/// <summary>
	/// Gets the space the border pictures drawn now take on each side (they follow the zoom); empty without the custom
	/// border.
	/// </summary>
	internal Padding AutomationBorder => this.DrawCustomBorder
		? new Padding(this.sideBorder.Width, this.topBorder.Height, this.sideBorder.Width, this.bottomBorder.Height)
		: Padding.Empty;
}

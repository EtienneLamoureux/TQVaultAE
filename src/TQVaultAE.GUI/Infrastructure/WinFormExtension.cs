namespace TQVaultAE.GUI.Infrastructure;

public static class WinFormExtension
{
	/// <summary>
	/// Text of the selected item, or an empty string when nothing is selected.
	/// </summary>
	public static string SelectedItemText(this ComboBox comboBox)
		=> comboBox.SelectedItem?.ToString() ?? string.Empty;

	/// <summary>
	/// Width of a vertical scroll bar on the monitor of <paramref name="ctrl"/>. <see cref="SystemInformation.VerticalScrollBarWidth"/>
	/// stays at the system DPI when the window is on a monitor of another DPI.
	/// </summary>
	public static int VerticalScrollBarWidth(this Control ctrl)
		=> SystemInformation.GetVerticalScrollBarWidthForDpi(ctrl.DeviceDpi);

	/// <summary>Height of a horizontal scroll bar on the monitor of <paramref name="ctrl"/> (see <see cref="VerticalScrollBarWidth"/>).</summary>
	public static int HorizontalScrollBarHeight(this Control ctrl)
		=> SystemInformation.GetHorizontalScrollBarHeightForDpi(ctrl.DeviceDpi);

	/// <summary>
	/// The DPI of the monitor of <paramref name="ctrl"/> over the design DPI (96): 1.25 at 120 DPI. For what is drawn or
	/// sized in code at the DPI only; Windows Forms scales the designer's controls by itself, and the user's zoom comes on
	/// top of it where it applies (<see cref="TQVaultAE.Presentation.Win32.Contracts.IUIService.Scale"/>).
	/// </summary>
	public static float DpiScale(this Control ctrl) => ctrl.DeviceDpi / 96F;

	/// <summary><paramref name="padding"/> with each side multiplied by <paramref name="factor"/>, rounded to the pixel.</summary>
	public static Padding Scaled(this Padding padding, float factor)
	{
		int Scale(int value) => (int)Math.Round(value * factor);
		return new Padding(Scale(padding.Left), Scale(padding.Top), Scale(padding.Right), Scale(padding.Bottom));
	}

	/// <summary>
	/// <paramref name="padding"/> in design pixels (96 DPI) in pixels of the monitor of <paramref name="ctrl"/>, as
	/// <see cref="Control.LogicalToDeviceUnits(int)"/> does for a value: Windows Forms does not scale a window's own
	/// padding when its DPI changes.
	/// </summary>
	public static Padding LogicalToDeviceUnits(this Control ctrl, Padding padding) => padding.Scaled(ctrl.DpiScale());

	public static void ProcessAllControls(this Control rootControl, Action<Control> action)
	{
		foreach (Control childControl in rootControl.Controls)
		{
			ProcessAllControls(childControl, action);
			action(childControl);
		}
	}

	public static void ProcessAllControlsWithExclusion(this Control rootControl, Func<Control, bool> action, Control? extraControl = null)
	{
		if (extraControl is not null)
			action(extraControl);// Used to add rootControl in the loop

		foreach (Control childControl in rootControl.Controls)
		{
			var excludeFromHere = action(childControl);
			if (excludeFromHere)
				continue;

			ProcessAllControlsWithExclusion(childControl, action);
		}
	}

	public static void AdjustToMaxTextWidth(this CheckedListBox ctrl, int? maxVerticalItems)
	{
		var width = ctrl.GetMaxTextWidth();

		// i add this for the size of the checkbox control in the begining of the item {CheckBoxWidth} + {TextWidth}
		width += ctrl.VerticalScrollBarWidth();

		ctrl.Width = ctrl.ColumnWidth = width;// The control must fit the size of the column
	}

	/// <summary>Sizes the list to one column as wide as its widest item, check box included.</summary>
	public static void AdjustToMaxTextWidth(this Components.Layout.VirtualCheckedListBox ctrl, int? maxVerticalItems)
	{
		// Check box, the gap before the text and a little room after it.
		var width = ctrl.TextLeft + ctrl.GetMaxTextWidth() + 6;

		ctrl.Width = ctrl.ColumnWidth = width;// The control must fit the size of the column
	}

	public static int GetMaxTextWidth(this CheckedListBox ctrl)
	{
		int maxwidth = 0, width;
		foreach (var item in ctrl.Items)
		{
			width = TextRenderer.MeasureText(item.ToString(), ctrl.Font).Width;
			maxwidth = Math.Max(width, maxwidth);
		}
		return maxwidth;
	}

}
using System;
using System.Windows.Forms;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models.SearchDialogAdvanced;

namespace TQVaultAE.GUI.Helpers
{
	public static class WinFormExtension
	{
		public static void ProcessAllControls(this Control rootControl, Action<Control> action)
		{
			foreach (Control childControl in rootControl.Controls)
			{
				ProcessAllControls(childControl, action);
				action(childControl);
			}
		}
		public static void AdjustToMaxTextWidth(this CheckedListBox ctrl, int? maxVerticalItems)
		{
			var width = ctrl.GetMaxTextWidth();

			// i add this for the size of the checkbox control in the begining of the item {CheckBoxWidth} + {TextWidth}
			width += SystemInformation.VerticalScrollBarWidth;

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
}

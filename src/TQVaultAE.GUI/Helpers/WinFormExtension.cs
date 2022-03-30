using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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


		// From https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio

		/// <summary>Resizes an image to a new width and height value.</summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="newWidth">The width of the new image.</param>
		/// <param name="newHeight">The height of the new image.</param>
		/// <param name="mode">Interpolation mode.</param>
		/// <param name="maintainAspectRatio">If true, the image is centered in the middle of the returned image, maintaining the aspect ratio of the original image.</param>
		/// <returns>The new image. The old image is unaffected.</returns>
		public static Image ResizeImage(this Image image, int newWidth, int newHeight, InterpolationMode mode = InterpolationMode.Default, bool maintainAspectRatio = false)
		{
			Bitmap output = new Bitmap(newWidth, newHeight, image.PixelFormat);

			using (Graphics gfx = Graphics.FromImage(output))
			{
				gfx.Clear(Color.FromArgb(0, 0, 0, 0));
				gfx.InterpolationMode = mode;
				if (mode == InterpolationMode.NearestNeighbor)
				{
					gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
					gfx.SmoothingMode = SmoothingMode.HighQuality;
				}

				double ratioW = (double)newWidth / (double)image.Width;
				double ratioH = (double)newHeight / (double)image.Height;
				double ratio = ratioW < ratioH ? ratioW : ratioH;
				int insideWidth = (int)(image.Width * ratio);
				int insideHeight = (int)(image.Height * ratio);

				gfx.DrawImage(image, new Rectangle((newWidth / 2) - (insideWidth / 2), (newHeight / 2) - (insideHeight / 2), insideWidth, insideHeight));
			}

			return output;
		}
	}
}

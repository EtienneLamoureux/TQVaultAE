using System.Drawing;

namespace TQVaultAE.Domain.Entities
{
	public class IconInfo
	{
		public IconInfo(IconCategory category, string on, Bitmap onBitmap, string off, Bitmap offBitmap, string over, Bitmap overBitmap)
		{
			Category = category;
			On = on;
			OnBitmap = onBitmap;
			Off = off;
			OffBitmap = offBitmap;
			Over = over;
			OverBitmap = overBitmap;
		}

		public readonly IconCategory Category;
		public readonly string On;
		public readonly Bitmap OnBitmap;
		public readonly string Off;
		public readonly Bitmap OffBitmap;
		public readonly string Over;
		public readonly Bitmap OverBitmap;

		/// <summary>
		/// Tell if <see cref="Over"/> is a duplicate of <see cref="On"/> or <see cref="Off"/>
		/// </summary>
		public bool IsOverSameAsOthers
			=> Over == Off || Over == On;


		/// <summary>
		/// Tell if <paramref name="resourceId"/> belong to this record
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns></returns>
		public bool Own(string resourceId)
		{
			if (string.IsNullOrWhiteSpace(resourceId)) return false;
			return resourceId == this.Off || resourceId == this.On || resourceId == this.Over;
		}

	}
}

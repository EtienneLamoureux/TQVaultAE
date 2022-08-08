using System.Drawing;

namespace TQVaultAE.Domain.Entities
{
	public record IconInfo(IconCategory Category, string On, Bitmap OnBitmap, string Off, Bitmap OffBitmap, string Over, Bitmap OverBitmap)
	{
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

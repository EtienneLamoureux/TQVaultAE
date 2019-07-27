using System.Drawing;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IFontService
	{
		IAddFontToOS FontLoader { get; }

		Font GetFontAlbertusMT(float fontSize, float? scale = null);
		Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, float? scale = null);
		Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b);
		Font GetFontAlbertusMT(float fontSize, GraphicsUnit unit);
		Font GetFontAlbertusMTLight(float fontSize);
		Font GetFontAlbertusMTLight(float fontSize, float? scale = null);
		Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, float? scale = null);
		Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b);
		Font GetFontAlbertusMTLight(float fontSize, GraphicsUnit unit);
	}
}
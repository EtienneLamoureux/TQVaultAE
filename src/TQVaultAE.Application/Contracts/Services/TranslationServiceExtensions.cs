namespace TQVaultAE.Application.Contracts.Services;

/// <summary>Helpers over <see cref="ITranslationService"/>.</summary>
public static class TranslationServiceExtensions
{
	/// <summary>
	/// The text of a game tag, or <paramref name="fallback"/> when the tag is empty or has no text (a mod or language
	/// missing it).
	/// </summary>
	/// <param name="translation">The translation service.</param>
	/// <param name="tag">The tag, like <c>tagSkillName01</c>.</param>
	/// <param name="fallback">The text shown without a translation.</param>
	/// <param name="removeAllTQTags">Remove the colour and format tags of the text.</param>
	public static string TranslateOr(this ITranslationService translation, string? tag, string fallback = "", bool removeAllTQTags = true)
		=> string.IsNullOrEmpty(tag)
			? fallback
			: translation.TranslateXTag(tag, removeAllTQTags) is { Length: > 0 } text ? text : fallback;
}

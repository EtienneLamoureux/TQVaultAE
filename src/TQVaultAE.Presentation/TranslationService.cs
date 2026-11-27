using System.Collections.Concurrent;
using System.Globalization;
using EnumsNET;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation;

public class TranslationService : ITranslationService
{
	private readonly IDatabase Database;

	public TranslationService(IDatabase database)
	{
		Database = database;
	}

	public string Translate(GameDlc extension)
		=> this.TranslateXTag(extension.GetTranslationTag());

	public string Translate(ItemStyle itemStyle)
	{
		var tags = itemStyle.AsString(EnumFormat.Description) ?? string.Empty;
		var values = tags.Split('|');
		var trans = (values.Length > 1)
			? string.Format(values.Last(), this.TranslateXTag(values.First()))
			: this.TranslateXTag(values.First());
		return trans;
	}

	public string Translate(Rarity level)
	{
		var style = level.GetItemStyle();
		if (style is null) return level.ToString();
		return Translate(style.Value);
	}

	public string ItemWith => Resources.ItemWith;

	public string ItemRelicBonus => Resources.ItemRelicBonus;

	public string ItemRelicCompleted => Resources.ItemRelicCompleted;

	public string ItemQuest => Resources.ItemQuest;

	public string ItemSeed => Resources.ItemSeed;

	public string TranslateXTag(string xTagName, bool removeAllTQTags = false, bool doCleanup = false, bool doCleanupKeepLeadingColorTag = false)
	{
		string? resx = LookFortranslation(xTagName);

		if (string.IsNullOrWhiteSpace(resx))
			return xTagName;

		resx = doCleanup ? resx.TQCleanup(doCleanupKeepLeadingColorTag) : resx;
		resx = removeAllTQTags ? resx.RemoveAllTQTags() : resx;

		return resx;
	}

	public bool TryTranslateXTag(string xTagName, out string translation, bool removeAllTQTags = false, bool doCleanup = false, bool doCleanupKeepLeadingColorTag = false)
	{
		string? resx = LookFortranslation(xTagName);

		// A tag has no space: some records write the text itself where a tag is expected ("Storm Orb" as a skill
		// name), and the game shows it as is.
		if (string.IsNullOrWhiteSpace(resx) && xTagName is not null && xTagName.Trim().Contains(' '))
			resx = xTagName.Trim();

		if (string.IsNullOrWhiteSpace(resx))
		{
			// Like TranslateXTag: show the tag itself, so a missing translation is visible in the UI.
			translation = xTagName!;
			return false;
		}

		resx = doCleanup ? resx.TQCleanup(doCleanupKeepLeadingColorTag) : resx;
		resx = removeAllTQTags ? resx.RemoveAllTQTags() : resx;

		translation = resx;
		return true;
	}

	/// <summary>
	/// <c>TextTag_</c> resource strings by culture and tag, misses included. Most tags have none, and a miss walks the
	/// culture fallback chain under the <see cref="System.Resources.ResourceManager"/> lock, which the parallel search
	/// warm-up contended on.
	/// </summary>
	private readonly ConcurrentDictionary<(string Culture, string Tag), string?> _textTags = new();

	private string? LookFortranslation(string xTagName)
	{
		// all xtag substitute must have a "TextTag_" prefix in resource file (avoid colision & strong naming rule).
		var culture = CultureInfo.CurrentUICulture;
		var resx = this._textTags.GetOrAdd((culture.Name, xTagName)
			, static (key, culture) => Resources.ResourceManager.GetString($"TextTag_{key.Tag}", culture), culture);

		// Check if the value is a @redirectTag 
		if (resx is not null && resx.StartsWith('@'))
			resx = this.Database.GetFriendlyName(resx.TrimStart('@'));

		if (resx is null)
			resx = this.Database.GetFriendlyName(xTagName);
		return resx;
	}

	public string TranslateDifficulty(int difficultyFromSaveFile)
		=> this.Database.GetFriendlyName($"tagRDifficultyTitle0{++difficultyFromSaveFile}");

	public string TranslateMastery(string characterXtagClass)
	{
		var masteries = CharacterClasses.MasteriesOf(characterXtagClass);
		return masteries.Count == 0
			? TranslateXTag(characterXtagClass)
			: string.Join("-", masteries.Select(m => TranslateXTag(CharacterClasses.BriefTag(m))));
	}

	public string ItemIT => Resources.ItemIT;

	public string ItemRagnarok => Resources.ItemRagnarok;

	public string ItemAtlantis => Resources.ItemAtlantis;

	public string ItemEmbers => Resources.ItemEmbers;
}

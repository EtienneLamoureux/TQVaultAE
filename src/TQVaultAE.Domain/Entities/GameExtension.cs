using EnumsNET;
using System;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Game extension enumeration
/// </summary>
public enum GameExtension
{
	[GameExtensionDescription("TQ", "tagBackground01")]
	TitanQuest,
	[GameExtensionDescription("IT", "tagBackground02")]
	ImmortalThrone,
	[GameExtensionDescription("RAG", "tagBackground03")]
	Ragnarok,
	[GameExtensionDescription("ATL", "tagBackground04")]
	Atlantis,
	[GameExtensionDescription("EEM", "x4tagBackground05")]
	EternalEmbers
}

#region Related code

public static class GameExtensionExtension
{
	public static string GetCode(this GameExtension ext)
		=> Enums.GetAttributes(ext).Get<GameExtensionDescriptionAttribute>().Code;
	public static string GetTranslationTag(this GameExtension ext)
		=> Enums.GetAttributes(ext).Get<GameExtensionDescriptionAttribute>().TranslationTag;
	public static string GetSuffix(this GameExtension ext)
	{
		switch (ext)
		{
			case GameExtension.TitanQuest:
				return string.Empty;
			default:
				return $"(" + GetCode(ext) + ")";
		}
	}
}

public class GameExtensionDescriptionAttribute : Attribute
{
	public readonly string Code;
	public readonly string TranslationTag;

	public GameExtensionDescriptionAttribute(string GameExtensionCode, string translationTag)
	{
		this.Code = GameExtensionCode;
		this.TranslationTag = translationTag;
	}
}

#endregion

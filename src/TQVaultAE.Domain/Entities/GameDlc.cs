using EnumsNET;
using System;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Game DLC enumeration
/// </summary>
public enum GameDlc
{
	[GameDlcDescription("TQ", "tagBackground01")]
	TitanQuest,
	[GameDlcDescription("IT", "tagBackground02")]
	ImmortalThrone,
	[GameDlcDescription("RAG", "tagBackground03")]
	Ragnarok,
	[GameDlcDescription("ATL", "tagBackground04")]
	Atlantis,
	[GameDlcDescription("EEM", "x4tagBackground05")]
	EternalEmbers
}

#region Related code

public static class GameDlcExtension
{
	public static string GetCode(this GameDlc ext)
		=> Enums.GetAttributes(ext).Get<GameDlcDescriptionAttribute>().Code;
	public static string GetTranslationTag(this GameDlc ext)
		=> Enums.GetAttributes(ext).Get<GameDlcDescriptionAttribute>().TranslationTag;
	public static string GetSuffix(this GameDlc ext)
	{
		switch (ext)
		{
			case GameDlc.TitanQuest:
				return string.Empty;
			default:
				return '(' + GetCode(ext) + ')';
		}
	}
}

public class GameDlcDescriptionAttribute : Attribute
{
	public readonly string Code;
	public readonly string TranslationTag;

	public GameDlcDescriptionAttribute(string GameExtensionCode, string translationTag)
	{
		this.Code = GameExtensionCode;
		this.TranslationTag = translationTag;
	}
}

#endregion

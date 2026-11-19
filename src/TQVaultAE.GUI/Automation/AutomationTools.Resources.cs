using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that read the game files the way the app does: resources (textures, sounds) and text tags.
/// </summary>
internal static partial class AutomationTools
{

	[McpServerTool(Name = "game_text", ReadOnly = true)]
	[Description("Translates text tags with the loaded text database (game, expansions, then the custom map's strings): an empty text means the tag is unknown.")]
	public static string GameText([Description("Text tags, separated by commas or new lines, like 'tagSkillName01'.")] string tags)
	{
		var database = Service<IDatabase>();
		return Serialize(tags.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(tag => new { tag, text = database.GetFriendlyName(tag) }));
	}
}

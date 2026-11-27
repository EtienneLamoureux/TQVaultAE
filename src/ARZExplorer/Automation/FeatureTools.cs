using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ArzExplorer.Automation;

/// <summary>
/// MCP tools for the About box, drag and drop and the sound player of ARZExplorer (Debug builds only).
/// </summary>
[McpServerToolType]
internal static class FeatureTools
{
	private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

	[McpServerTool(Name = "about", ReadOnly = true)]
	[Description("Opens the About box (Help > About) without blocking, returns its texts (title, product, version, copyright, company, description) and a PNG of it, then closes it.")]
	public static IEnumerable<ContentBlock> About()
	{
		var (texts, png) = AutomationHost.OnUi(form => form.AutomationAbout(), TimeSpan.FromSeconds(10));
		return [new TextContentBlock { Text = string.Join('\n', texts) }, ImageContentBlock.FromBytes(png, "image/png")];
	}

	[McpServerTool(Name = "drop_files", Destructive = false)]
	[Description("Drops files on the window as Explorer does, through its DragEnter and DragDrop handlers: the first .arc or .arz file opens. Returns the drop effect and get_state.")]
	public static string DropFiles(
		[Description("Full paths of the dropped files.")] string[] paths,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 120)
	{
		var effect = AutomationHost.OnUi(form => form.AutomationDropFiles(paths), TimeSpan.FromSeconds(timeoutSeconds));
		return $"effect: {effect}\n{AutomationTools.GetState()}";
	}

	[McpServerTool(Name = "sound_player", Destructive = false)]
	[Description("The sound player shown for a .wav or .mp3 entry: 'state', or a click on 'play', 'loop' (toggles looping), 'pause' or 'stop'. After a click the sound is loaded, which checks its wave data. In headless mode the process' audio volume is 0, so nothing is heard.")]
	public static string SoundPlayer(
		[Description("state, play, loop, pause or stop.")] string action = "state")
		=> JsonSerializer.Serialize(AutomationHost.OnUi(form => action == "state"
			? form.AutomationSoundPlayer.AutomationState()
			: form.AutomationSoundPlayer.AutomationClick(action), TimeSpan.FromSeconds(10)), JsonOptions);
}

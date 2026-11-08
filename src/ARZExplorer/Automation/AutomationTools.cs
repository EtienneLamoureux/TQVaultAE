using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ArzExplorer.Automation;

/// <summary>
/// MCP tools that inspect and drive the running ARZExplorer window (Debug builds only).
/// </summary>
[McpServerToolType]
internal static class AutomationTools
{
	private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

	private static string Json(object value) => JsonSerializer.Serialize(value, JsonOptions);

	[McpServerTool(Name = "get_state", ReadOnly = true)]
	[Description("The window title, the opened files, the number of tree nodes, the selected node, the path box, the status bar, which detail view is shown (picture, grid, text, sound) and the shown picture's size. Fails within 5 s when the UI thread is blocked.")]
	public static string GetState()
		=> Json(AutomationHost.OnUi(form => form.AutomationState(), TimeSpan.FromSeconds(5)));

	[McpServerTool(Name = "open_file", Destructive = false)]
	[Description("Opens an ARC or ARZ file (full path) as the Open menu does and adds it to the tree.")]
	public static string OpenFile([Description("Full path of the .arc or .arz file.")] string path)
	{
		AutomationHost.OnUi(form => { form.AutomationOpenFile(path); return true; }, TimeSpan.FromMinutes(2));
		return GetState();
	}

	[McpServerTool(Name = "dialogs", ReadOnly = true)]
	[Description("Headless mode: whether ARZExplorer runs on its hidden desktop and the dialogs cancelled there because nobody could answer them (message boxes with their caption and text, file dialogs), oldest first. clear=true empties the record.")]
	public static string Dialogs([Description("Empty the record after reading it.")] bool clear = false)
	{
		var result = new { headless = Headless.Enabled, desktop = Headless.DesktopName, muted = Headless.Muted, cancelled = Headless.CancelledDialogs };
		if (clear)
			Headless.ClearCancelled();
		return Json(result);
	}

	[McpServerTool(Name = "exit", Destructive = false)]
	[Description("Closes ARZExplorer (it saves nothing), the way to stop a headless instance.")]
	public static string Exit()
	{
		AutomationHost.OnUi(form => { form.BeginInvoke(form.Close); return true; }, TimeSpan.FromSeconds(5));
		return "Closing.";
	}

	[McpServerTool(Name = "list_items", ReadOnly = true)]
	[Description("Lists the menu and toolbar items: name, text, enabled.")]
	public static string ListItems()
		=> Json(AutomationHost.OnUi(form => form.AutomationListItems(), TimeSpan.FromSeconds(5)));

	[McpServerTool(Name = "click_item", Destructive = false)]
	[Description("Clicks a menu or toolbar item by name (from list_items), e.g. the Load database or Load all files buttons, then returns get_state. Reports a hang when the UI thread stays busy longer than the timeout. Items that open a file dialog wait for it.")]
	public static string ClickItem(
		[Description("Item name from list_items.")] string name,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 120)
	{
		AutomationHost.OnUi(form => { form.AutomationClickItem(name); return true; }, TimeSpan.FromSeconds(timeoutSeconds));
		return GetState();
	}

	[McpServerTool(Name = "search", Destructive = false)]
	[Description("Types the text in the search box and runs Find Next (forward) or Find Previous, as the toolbar or F3 does: it selects the next node whose key, text or record values contain the text. Returns the time it took and get_state (the status bar says where it was found).")]
	public static string Search(
		[Description("Text to find.")] string text,
		[Description("Find Next when true, Find Previous when false.")] bool forward = true,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 300)
	{
		var ms = AutomationHost.OnUi(form => form.AutomationSearch(text, forward), TimeSpan.FromSeconds(timeoutSeconds));
		return $"{ms} ms\n{GetState()}";
	}

	[McpServerTool(Name = "clipboard", ReadOnly = true)]
	[Description("What the clipboard holds after a Copy menu item: text (start, length, lines), image size, audio length.")]
	public static string ReadClipboard()
		=> Json(AutomationHost.OnUi(form => form.AutomationClipboard(), TimeSpan.FromSeconds(5)));

	[McpServerTool(Name = "extract", Destructive = true)]
	[Description("Extract menu without its file dialogs: all=false writes the selected record or entry to the file 'path' (Extract > Selected File), all=true writes every record or entry of the selected file under the folder 'path' with the progress dialog (Extract > All Files). Write only under a temporary folder.")]
	public static string Extract(
		[Description("Destination file (all=false) or folder (all=true).")] string path,
		[Description("Every record or entry of the selected file.")] bool all = false,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 900)
	{
		var watch = Stopwatch.StartNew();
		AutomationHost.OnUi(form =>
		{
			if (all)
				form.ExtractAllTo(path);
			else
				form.ExtractSelectedTo(path);
			return true;
		}, TimeSpan.FromSeconds(timeoutSeconds));
		return $"{watch.ElapsedMilliseconds} ms";
	}

	[McpServerTool(Name = "find_nodes", ReadOnly = true)]
	[Description("Lists the keys of the leaf nodes (files and records) whose key contains the text, e.g. '.tex' or 'items\\\\'. Keys are '<file id>;<entry>'.")]
	public static string FindNodes(
		[Description("Text the node key must contain (case-insensitive).")] string contains,
		[Description("Maximum number of keys.")] int max = 50)
		=> Json(AutomationHost.OnUi(form => form.AutomationFindNodes(contains, max), TimeSpan.FromSeconds(10)));

	[McpServerTool(Name = "select_node", Destructive = false)]
	[Description("Selects a tree node as a click does (shows its record, text, picture or sound), then returns get_state. Reports a hang when the UI thread stays busy longer than the timeout.")]
	public static string SelectNode(
		[Description("Node key from find_nodes.")] string key,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 10)
	{
		AutomationHost.OnUi(form => { form.AutomationSelectNode(key); return true; }, TimeSpan.FromSeconds(timeoutSeconds));
		return GetState();
	}

	[McpServerTool(Name = "grid_links", ReadOnly = true)]
	[Description("Lists the link cells of the record grid shown for a DBR record (row, column, value): file and record references.")]
	public static string GridLinks()
		=> Json(AutomationHost.OnUi(form => form.AutomationGridLinks(), TimeSpan.FromSeconds(5)));

	[McpServerTool(Name = "click_grid_cell", Destructive = false)]
	[Description("Clicks a cell of the record grid as the mouse does: a link cell navigates to its record or file (loading its archive when needed), then returns get_state. Reports a hang when the UI thread stays busy longer than the timeout.")]
	public static string ClickGridCell(
		[Description("Row index from grid_links.")] int row,
		[Description("Column index from grid_links.")] int column,
		[Description("Seconds to wait for the UI thread.")] int timeoutSeconds = 30)
	{
		AutomationHost.OnUi(form => { form.AutomationClickGridCell(row, column); return true; }, TimeSpan.FromSeconds(timeoutSeconds));
		return GetState();
	}

	[McpServerTool(Name = "check_links", ReadOnly = true)]
	[Description("Checks every link of the database records (load the database and the archives first, e.g. click_item loadAllFilesToolStripMenuItem): the grid's link detection, NavigateTo's resolution, the archive a click opens with only the database loaded, and (render=true) the loading each view does for every distinct target (record, texture decoded, text, sound data). Returns counts, problems by kind with examples, and writes every problem to the JSON file 'path' when given. Do not use the window while it runs.")]
	public static string CheckLinks(
		[Description("Only the records whose id contains this text; empty for all.")] string? contains = null,
		[Description("Maximum number of records; 0 for all.")] int maxRecords = 0,
		[Description("Examples per kind of problem.")] int examples = 5,
		[Description("Also load every distinct target as its view does.")] bool render = true,
		[Description("JSON file for the full report; empty for none.")] string? path = null,
		[Description("Seconds before giving up.")] int timeoutSeconds = 1800)
	{
		var form = AutomationHost.OnUi(f => f, TimeSpan.FromSeconds(5));
		var task = Task.Run(() => form.AutomationCheckLinks(contains, maxRecords, examples, render));
		if (!task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
			throw new McpException($"The check did not finish within {timeoutSeconds} s.");

		var report = Json(task.Result);
		if (!string.IsNullOrEmpty(path))
			File.WriteAllText(path, report);

		// The full list goes to the file only.
		using var doc = JsonDocument.Parse(report);
		return Json(doc.RootElement.EnumerateObject().Where(p => p.Name != "all").ToDictionary(p => p.Name, p => p.Value.Clone()));
	}

	[McpServerTool(Name = "decode_tex", ReadOnly = true)]
	[Description("Reads a TEX entry of an ARC node and decodes it with the BitmapService off the UI thread, timing the read and the decoding: tells a slow or failing decoder from a UI problem.")]
	public static string DecodeTex(
		[Description("Node key of a .tex entry.")] string key,
		[Description("Seconds before reporting a hang.")] int timeoutSeconds = 20)
	{
		var (form, entry) = AutomationHost.OnUi(f => (f, f.AutomationArcEntry(key)), TimeSpan.FromSeconds(5));
		var task = Task.Run(() => form.AutomationDecodeTex(entry.Arc, entry.Id));
		if (!task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
			throw new McpException($"Decoding did not finish within {timeoutSeconds} s.");
		return Json(task.Result);
	}

	[McpServerTool(Name = "capture", ReadOnly = true)]
	[Description("Renders the main window to PNG with Control.DrawToBitmap (works when it is hidden behind others).")]
	public static IEnumerable<ContentBlock> Capture()
	{
		var png = AutomationHost.OnUi(form =>
		{
			using var bitmap = new Bitmap(form.Width, form.Height);
			form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return stream.ToArray();
		}, TimeSpan.FromSeconds(5));

		var path = Path.Combine(Path.GetTempPath(), $"arzexplorer-capture-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
		File.WriteAllBytes(path, png);
		return [ImageContentBlock.FromBytes(png, "image/png"), new TextContentBlock { Text = $"Saved to {path}" }];
	}
}

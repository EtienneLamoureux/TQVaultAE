using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace ArzExplorer.Automation;

/// <summary>
/// MCP tools for the open forms and the process resources of ARZExplorer (Debug builds only).
/// </summary>
[McpServerToolType]
internal static partial class DiagnosticTools
{
	private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

	private const int WM_SYSCOMMAND = 0x0112;
	private const int SC_CLOSE = 0xF060;

	[McpServerTool(Name = "forms", ReadOnly = true)]
	[Description("Lists the open forms (Application.OpenForms): name, title, visible, modal, and for the forms other than the main window the texts of their controls (e.g. the message of an exception dialog).")]
	public static string Forms()
		=> JsonSerializer.Serialize(AutomationHost.OnUi(main => Application.OpenForms.Cast<Form>()
			.Select(f => new
			{
				name = f.Name,
				text = f.Text,
				visible = f.Visible,
				modal = f.Modal,
				controls = f == main ? null : ControlTexts(f).ToList(),
			})
			.ToList(), TimeSpan.FromSeconds(5)), JsonOptions);

	/// <summary>The non-empty texts of every control of <paramref name="parent"/>, depth first.</summary>
	private static IEnumerable<string> ControlTexts(Control parent)
	{
		foreach (Control child in parent.Controls)
		{
			if (!string.IsNullOrWhiteSpace(child.Text))
				yield return child.Text.Length > 2000 ? child.Text[..2000] : child.Text;
			foreach (var text in ControlTexts(child))
				yield return text;
		}
	}

	[McpServerTool(Name = "close_form", Destructive = false)]
	[Description("Closes an open form (name or title from forms) as its title bar X does: posts WM_SYSCOMMAND SC_CLOSE to its window (close reason UserClosing) and returns at once. Works while a modal dialog such as the Extract progress runs (call it while extract all=true is still waiting).")]
	public static string CloseForm([Description("Form name or title from forms.")] string name)
	{
		var found = AutomationHost.OnUi(_ =>
		{
			var form = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Name == name || f.Text == name);
			if (form is null)
				return false;
			// What the title bar X and Alt+F4 send: WinForms then reports CloseReason.UserClosing (a bare WM_CLOSE gives TaskManagerClosing).
			PostMessage(form.Handle, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
			return true;
		}, TimeSpan.FromSeconds(5));
		return found ? $"SC_CLOSE posted to {name}." : throw new McpException($"No open form named '{name}'.");
	}

	[McpServerTool(Name = "process_resources", ReadOnly = true)]
	[Description("Resources of the ARZExplorer process: GDI and USER object counts (GetGuiResources), handle count, private bytes, managed heap bytes, and the disposable objects the root service provider keeps until exit, by type. collect=true runs a full garbage collection with finalizers first (objects only freed by a finalizer then disappear).")]
	public static string ProcessResources([Description("Run a full garbage collection with finalizers first.")] bool collect = false)
	{
		if (collect)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		using var process = Process.GetCurrentProcess();
		var result = new
		{
			gdiObjects = GetGuiResources(process.Handle, 0),
			userObjects = GetGuiResources(process.Handle, 1),
			handles = process.HandleCount,
			privateBytes = process.PrivateMemorySize64,
			managedBytes = GC.GetTotalMemory(false),
			trackedDisposables = TrackedDisposables(),
		};
		return JsonSerializer.Serialize(result, JsonOptions);
	}

	/// <summary>
	/// The disposable objects the root service provider holds until it is disposed, counted by type name
	/// (read by reflection: Microsoft.Extensions.DependencyInjection keeps them in a private list of its root scope).
	/// </summary>
	private static Dictionary<string, int> TrackedDisposables()
	{
		const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		var root = Program.ServiceProvider.GetType().GetProperty("Root", Flags)?.GetValue(Program.ServiceProvider);
		var list = root?.GetType().GetField("_disposables", Flags)?.GetValue(root) as IEnumerable;
		if (root is null)
			return new() { ["(root scope not found)"] = 0 };

		// Read on the UI thread, which is the one resolving forms.
		return AutomationHost.OnUi(_ => (list?.Cast<object>() ?? [])
			.GroupBy(o => o.GetType().Name)
			.ToDictionary(g => g.Key, g => g.Count()), TimeSpan.FromSeconds(5));
	}

	[LibraryImport("user32.dll")]
	private static partial uint GetGuiResources(IntPtr process, uint flags);

	[LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}

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

	[McpServerTool(Name = "mouse_click", Destructive = false)]
	[Description("Clicks a control of an open form with mouse messages (WM_MOUSEMOVE, WM_LBUTTONDOWN, WM_LBUTTONUP) posted to its window, as a user's click arrives through the message loop, unlike a call of its click handler: e.g. the Cancel button ('cancelButton') of the Extract progress dialog ('ArzExtractProgress') while extract(all=true) is still waiting. x and y are in the control's client coordinates (its centre when omitted). Returns at once.")]
	public static string MouseClick(
		[Description("Form name or title from forms.")] string form,
		[Description("Control name.")] string control,
		[Description("X in the control's client coordinates; -1 for its centre.")] int x = -1,
		[Description("Y in the control's client coordinates; -1 for its centre.")] int y = -1)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindForm(form);
			var c = AllControls(target).FirstOrDefault(c => c.Name == control)
				?? throw new McpException($"No control '{control}' in '{target.Text}'.");
			x = x < 0 ? c.ClientSize.Width / 2 : x;
			y = y < 0 ? c.ClientSize.Height / 2 : y;

			const int WM_MOUSEMOVE = 0x0200, WM_LBUTTONDOWN = 0x0201, WM_LBUTTONUP = 0x0202, MK_LBUTTON = 0x0001;
			var position = (IntPtr)((y << 16) | (x & 0xFFFF));
			PostMessage(c.Handle, WM_MOUSEMOVE, IntPtr.Zero, position);
			PostMessage(c.Handle, WM_LBUTTONDOWN, MK_LBUTTON, position);
			PostMessage(c.Handle, WM_LBUTTONUP, IntPtr.Zero, position);
			return $"Click posted to {target.Name}.{c.Name} at ({x}, {y}), enabled={c.Enabled}.";
		}, TimeSpan.FromSeconds(5));

	[McpServerTool(Name = "alt_f4", Destructive = false)]
	[Description("Presses Alt+F4 in an open form as the keyboard does: posts WM_SYSKEYDOWN Alt, WM_SYSKEYDOWN F4 and WM_SYSKEYUP F4 with the Alt context flag to its focused control (the form when nothing has the focus), so the default window procedure turns it into SC_CLOSE, unlike close_form, which posts SC_CLOSE itself. The keyboard state is not changed: Control.ModifierKeys does not see Alt. Returns at once.")]
	public static string AltF4([Description("Form name or title from forms.")] string form)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindForm(form);
			Control receiver = target.ActiveControl ?? target;
			while (receiver is ContainerControl { ActiveControl: { } inner })
				receiver = inner;

			const int WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105, VK_MENU = 0x12, VK_F4 = 0x73;
			const int AltFlag = 1 << 29, PreviousDown = 1 << 30, Released = 1 << 31;
			PostMessage(receiver.Handle, WM_SYSKEYDOWN, VK_MENU, (IntPtr)(1 | (0x38 << 16) | AltFlag));
			PostMessage(receiver.Handle, WM_SYSKEYDOWN, VK_F4, (IntPtr)(1 | (0x3E << 16) | AltFlag));
			PostMessage(receiver.Handle, WM_SYSKEYUP, VK_F4, (IntPtr)(1 | (0x3E << 16) | AltFlag | PreviousDown | Released));
			return $"Alt+F4 posted to {target.Name}.{receiver.Name} ({receiver.GetType().Name}).";
		}, TimeSpan.FromSeconds(5));

	/// <summary>The open form with that name or title.</summary>
	private static Form FindForm(string name)
		=> Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Name == name || f.Text == name)
			?? throw new McpException($"No open form named '{name}'.");

	/// <summary>Every control of <paramref name="parent"/>, depth first.</summary>
	private static IEnumerable<Control> AllControls(Control parent)
	{
		foreach (Control child in parent.Controls)
		{
			yield return child;
			foreach (var inner in AllControls(child))
				yield return inner;
		}
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

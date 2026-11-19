using System.ComponentModel;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Counts what an action leaves behind: GDI and USER objects, and the GDI+ objects (bitmaps, fonts, pens, brushes) dropped
/// without being disposed, which the GDI counter does not see.
/// </summary>
internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "gdi_census", Destructive = true)]
		[Description("Runs an action count times and reports what it leaves: GDI and USER objects (GetGuiResources) and handles, before, right after and after the finalizers ran, and 'undisposed': the finalizable objects (bitmaps, fonts, pens, brushes, graphics...) the action dropped without disposing them, counted by a full garbage collection run without garbage collections during the action ('gcDuring' must be 0 for an exact count). Actions: 'repaint' (target: a control name of the main window, a panel name as in get_state, or empty for the whole main window), 'zoom' (zoom in then out, like zoom), 'characterList' (describes the character list entries again, as after a tag change), 'openForm' (target: a form type as in list_forms, created like open_form and disposed), 'none'.")]
		public static string GdiCensus(
			[Description("repaint, zoom, characterList, openForm or none.")] string action,
			[Description("The panel to repaint or the form to open.")] string target = "",
			[Description("How many times to run the action.")] int count = 10)
			=> AutomationHost.OnUi(main =>
			{
				if (count < 0 || count > 1000)
					throw new McpException("count must be 0..1000.");

				Action run = action.ToLowerInvariant() switch
				{
					"repaint" => string.IsNullOrEmpty(target) ? main.Refresh : PanelOf(main, target).Refresh,
					"zoom" => () =>
					{
						main.AutomationZoom(0.1F);
						main.AutomationZoom(-0.1F);
					},
					"characterlist" => main.AutomationCharacterCombo.RefreshItems,
					"openform" => OpenAndDispose(target),
					"none" => () => { },
					_ => throw new McpException("action must be repaint, zoom, characterList, openForm or none."),
				};

				// A collection with the finalizers first, so the queue only gets what the action drops.
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				var before = Counters();

				bool noGc = TryStartNoGc();
				int gen0 = GC.CollectionCount(0);
				for (int i = 0; i < count; i++)
					run();

				int gcDuring = GC.CollectionCount(0) - gen0;
				if (noGc && GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
					GC.EndNoGCRegion();

				var after = Counters();
				GC.Collect(2, GCCollectionMode.Forced, blocking: true);
				long undisposed = GC.GetGCMemoryInfo(GCKind.FullBlocking).FinalizationPendingCount;
				GC.WaitForPendingFinalizers();
				GC.Collect();
				var finalized = Counters();

				return Serialize(new { action, target, count, gcDuring, undisposed, before, after, finalized });
			});

		/// <summary>Stops the garbage collections during the action, so the last collection sees every object it dropped.</summary>
		private static bool TryStartNoGc()
		{
			foreach (long size in new long[] { 240_000_000, 64_000_000, 16_000_000 })
			{
				try
				{
					if (GC.TryStartNoGCRegion(size))
						return true;
				}
				catch (ArgumentOutOfRangeException)
				{
					// Larger than this runtime allows: try a smaller budget.
				}
			}

			return false;
		}

		/// <summary>The panel holding the bag buttons of the named grid, or the grid itself; else the control of that name.</summary>
		private static Control PanelOf(Forms.MainForm main, string name)
		{
			if (main.Controls.Find(name, searchAllChildren: true).FirstOrDefault() is { } control)
				return control;

			var panel = AutomationPanels.Resolve(main, name);
			return (Control?)panel.Container ?? panel.Panel;
		}

		private static Action OpenAndDispose(string name)
		{
			var type = FormTypes().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"Unknown form '{name}'.");
			var factory = Program.ServiceProvider.GetRequiredService<FormFactory>();
			var first = factory.Create(type);
			var second = factory.Create(type);
			bool singleton = ReferenceEquals(first, second);
			if (!singleton)
			{
				first.Dispose();
				second.Dispose();
			}

			return singleton
				? throw new McpException($"{type.Name} is a singleton: it is never disposed.")
				: () => factory.Create(type).Dispose();
		}

		private static object Counters()
		{
			using var process = Process.GetCurrentProcess();
			return new
			{
				gdi = GetGuiResources(process.Handle, 0),
				user = GetGuiResources(process.Handle, 1),
				handles = process.HandleCount,
			};
		}

		[LibraryImport("user32.dll")]
		private static partial uint GetGuiResources(IntPtr process, uint flags);
	}
}

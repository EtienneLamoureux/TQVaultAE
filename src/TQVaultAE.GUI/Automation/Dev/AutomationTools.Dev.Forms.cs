using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "zoom", Destructive = true)]
		[Description("Zooms the main window like its keyboard shortcuts: 'in' (Ctrl +, +10 %), 'out' (Ctrl -, -10 %) or 'reset' (Ctrl Home, 100 %). Returns the zoom step (0 to 16, nominal zoom 0.4 to 2 by 0.1), its nominal zoom, the item cell size in pixels, the effective zoom (the cell over the design cell without the DPI), the DPI factor, the resulting scale and the client size. The zoom is saved to UserConfig.xml like a user zoom: end with 'reset'.")]
		public static string Zoom([Description("in, out or reset.")] string step)
			=> AutomationHost.OnUi(main =>
			{
				float delta = step.ToLowerInvariant() switch
				{
					"in" => 0.1F,
					"out" => -0.1F,
					"reset" => 1.0F,
					_ => throw new McpException("step must be in, out or reset."),
				};

				main.AutomationZoom(delta);
				var ui = main.Services.UIService;
				return Serialize(new
				{
					step = main.AutomationZoomStep,
					nominalZoom = main.NominalZoom,
					cell = ui.ItemUnitSize,
					zoom = ui.Zoom,
					dpiScale = ui.DpiScale,
					scale = ui.Scale,
					clientSize = main.ClientSize,
				});
			});

		[McpServerTool(Name = "control_tree", ReadOnly = true)]
		[Description("Lists the controls of an open form, or of the settings shown in the main window ('Settings'), with their type, bounds (client coordinates of their parent, in pixels), font, visibility and enabled state, to check layout and scaling. Also reports the form's DPI and TQVault's scale, zoom and DPI factor.")]
		public static string ControlTree(
			[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form = "",
			[Description("Maximum depth below the form (default 3).")] int depth = 3)
			=> AutomationHost.OnUi(main =>
			{
				var target = FormOrMain(main, form);

				object Describe(Control c, int level) => new
				{
					name = c.Name,
					type = c.GetType().Name,
					bounds = c.Bounds,
					font = FontText(c.Font),
					visible = c.Visible,
					enabled = c.Enabled,
					autoScale = c is ContainerControl container && container.AutoScaleMode != AutoScaleMode.Inherit
						? $"{container.AutoScaleMode} {container.AutoScaleDimensions.Width:0.##}x{container.AutoScaleDimensions.Height:0.##} -> {container.CurrentAutoScaleDimensions.Width:0.##}x{container.CurrentAutoScaleDimensions.Height:0.##}"
						: null,
					controls = level < depth && c.Controls.Count > 0
						? c.Controls.Cast<Control>().Select(child => Describe(child, level + 1)).ToList()
						: null,
				};

				var ui = main.Services.UIService;
				return Serialize(new
				{
					form = target.Text,
					deviceDpi = target.DeviceDpi,
					clientSize = target.ClientSize,
					scale = ui.Scale,
					zoom = ui.Zoom,
					dpiScale = ui.DpiScale,
					controls = target.Controls.Cast<Control>().Select(c => Describe(c, 1)).ToList(),
				});
			});

		[McpServerTool(Name = "list_forms", ReadOnly = true)]
		[Description("Lists the form and dialog types registered in the container, which open_form can create.")]
		public static string ListForms()
			=> Serialize(FormTypes().Select(t => new { name = t.Name, type = t.FullName }));

		[McpServerTool(Name = "open_form", Destructive = true)]
		[Description("Creates a form or dialog from the container, as the app does (FormFactory), so its InitializeComponent and resources run. With show=false (the default) it reports the created form and disposes it; with show=true it opens it modeless over the main window, to capture or close afterwards with capture and close_dialog. Singletons (MainForm, BagButtonSettings) are only reported. Dialogs that need data first (ResultsDialog, ItemProperties...) may fail when shown: see recent_log.")]
		public static string OpenForm(
			[Description("Type name, as listed by list_forms, e.g. AboutBox or KeyBindsDialog.")] string name,
			[Description("true to show the form modeless instead of disposing it.")] bool show = false)
			=> AutomationHost.OnUi(main =>
			{
				var type = FormTypes().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
					?? throw new McpException($"Unknown form '{name}'. Forms: {string.Join(", ", FormTypes().Select(t => t.Name))}");

				// As the app makes it (FormFactory), so a dialog designed at zoom 1 gets the user's zoom.
				var form = Services.Forms.Create(type);

				// A transient registration returns a new instance on each call.
				var other = (Form)Program.ServiceProvider.GetRequiredService(type);
				var singleton = ReferenceEquals(form, other);
				if (!singleton)
					other.Dispose();

				var report = new
				{
					type = type.FullName,
					singleton,
					title = form.Text,
					size = form.Size,
					controls = CountControls(form),
					backgroundImage = form.BackgroundImage is not null,
					icon = form.Icon is not null,
					shown = show && !singleton,
				};

				if (singleton)
					return Serialize(report);

				if (show)
				{
					// Shown after this call returns, so a Load handler opening a message box cannot block the endpoint.
					main.BeginInvoke(() =>
					{
						try
						{
							form.Show(main);
						}
						catch (Exception ex)
						{
							AutomationHost.Log?.LogError(ex, "open_form: {Form} failed to show", type.Name);
							form.Dispose();
						}
					});
				}
				else
				{
					form.Dispose();
				}

				return Serialize(report);
			});

		[McpServerTool(Name = "open_input_dialog", Destructive = true)]
		[Description("Opens the text input dialog (InputDialog, used to name a duplicated character or a loadout) modeless over the main window, created like the application does, to check its layout with capture and close it with close_dialog (button Cancel or OK).")]
		public static string OpenInputDialog(
			[Description("Prompt above the text box.")] string prompt = "New name of the character:",
			[Description("Dialog title.")] string title = "Input",
			[Description("Text box content.")] string value = "NewName")
			=> AutomationHost.OnUi(main =>
			{
				var dialog = main.Services.Forms.Create<Dialogs.InputDialog>().With(d =>
				{
					d.Prompt = prompt;
					d.Text = title;
					d.InputValue = value;
				});
				dialog.Show(main);
				return Serialize(new { title = dialog.Text, size = dialog.Size, deviceDpi = dialog.DeviceDpi });
			});

		[McpServerTool(Name = "bag_settings_hover_icon", Destructive = true)]
		[Description("In the open bag button settings dialog, rests the pointer on an icon of a tab's grid through the grid's hover handler: the magnifier (control iconMagnifier) shows the icon enlarged below it, with its name and size. Capture it with capture('Bag Button Settings'); omit index to hide it.")]
		public static string BagSettingsHoverIcon(
			[Description("Tab: Skills, Relics, Artifacts, Jewellery, Scrolls, Potions, Buttons, Misc, Helmets, Shields, Armbands, Greaves.")] string tab,
			[Description("Zero-based icon index in that grid; omit to leave the grid.")] int? index = null)
			=> AutomationHost.OnUi(_ =>
			{
				var dialog = OpenDialog<Dialogs.BagButtonSettings>("bag button settings");
				return Serialize(new { hovered = dialog.AutomationHoverIcon(tab, index) });
			});

		[McpServerTool(Name = "bag_settings_drag_icon", Destructive = true)]
		[Description("In the open bag button settings dialog, drags an icon of a tab's grid onto a picture box like the user: left button down on the icon, the virtual cursor moved onto the target box, button up to the grid (which has the mouse capture), whose handler finds the box under the cursor. target: on, off or over (Detailed pictures) or simple (Simple picture); a box hidden in the current mode is refused. The drop switches to Custom icon. Then read the dialog with control_values and apply it with click_control('Bag Button Settings', 'applyButton').")]
		public static string BagSettingsDragIcon(
			[Description("Tab: Skills, Relics, Artifacts, Jewellery, Scrolls, Potions, Buttons, Misc, Helmets, Shields, Armbands, Greaves.")] string tab,
			[Description("Zero-based icon index in that grid.")] int index,
			[Description("on, off, over or simple.")] string target)
			=> AutomationHost.OnUi(_ =>
			{
				var dialog = OpenDialog<Dialogs.BagButtonSettings>("bag button settings");
				return Serialize(new { dragged = dialog.AutomationDragIcon(tab, index, target), target });
			});

		[McpServerTool(Name = "window_bounds", ReadOnly = true)]
		[Description("The main window's bounds (virtual screen pixels), its state (Normal, Maximized, Minimized), its DPI, zoom and scale, the right and bottom edges of its content (its visible child controls), and the working area of the monitor it is on: checks a maximize on each monitor, and that the content still fits the window after a maximize and a restore. layout: the panels' table against the area inside the border (overflow above 0 reaches under the border), the panels' edges (shownRight/shownBottom), the control under each resize grip ((window) when the grip works), column widths and row heights.")]
		public static string WindowBounds()
			=> AutomationHost.OnUi(main =>
			{
				var screen = Screen.FromHandle(main.Handle);
				var children = main.Controls.Cast<Control>().Where(c => c.Visible).ToList();
				var ui = main.Services.UIService;
				var table = main.AutomationLayoutTable;
				var border = main.AutomationBorder;
				var inside = new Rectangle(border.Left, border.Top, main.ClientSize.Width - border.Horizontal, main.ClientSize.Height - border.Vertical);
				var cells = table.Controls.Cast<Control>().Where(c => c.Visible).ToList();
				string Under(Point point) => main.GetChildAtPoint(point, GetChildAtPointSkip.Invisible)?.Name ?? "(window)";
				return Serialize(new
				{
					bounds = main.Bounds,
					clientSize = main.ClientSize,
					contentRight = children.Count == 0 ? 0 : children.Max(c => c.Right),
					contentBottom = children.Count == 0 ? 0 : children.Max(c => c.Bottom),
					layout = new
					{
						border = new { left = border.Left, top = border.Top, right = border.Right, bottom = border.Bottom },
						inside,
						table = table.Bounds,
						preferred = table.PreferredSize,
						overflowRight = table.Right - inside.Right,
						overflowBottom = table.Bottom - inside.Bottom,
						// The panels in the table (without their empty margins): past the inside edge a human sees them cut.
						shownRight = cells.Count == 0 ? 0 : table.Left + cells.Max(c => c.Right),
						shownBottom = cells.Count == 0 ? 0 : table.Top + cells.Max(c => c.Bottom),
						// A control under a resize grip of the border takes the mouse: the window can't be resized from there.
						gripRight = Under(new Point(main.ClientSize.Width - 2, main.ClientSize.Height / 2)),
						gripBottom = Under(new Point(main.ClientSize.Width / 2, main.ClientSize.Height - 2)),
						gripBottomRight = Under(new Point(main.ClientSize.Width - 2, main.ClientSize.Height - 2)),
						columns = table.GetColumnWidths(),
						rows = table.GetRowHeights(),
					},
					state = main.WindowState.ToString(),
					zoom = ui.Zoom,
					scale = ui.Scale,
					deviceDpi = main.DeviceDpi,
					monitor = screen.DeviceName,
					workingArea = screen.WorkingArea,
					insideWorkingArea = screen.WorkingArea.Contains(main.Bounds),
					// The window as Windows has it (Bounds is what Windows Forms last asked for), the largest client size
					// Windows lets it take and the largest zoom that fits it (a zoom in stops there).
					windowRect = NativeWindows.WindowRect(main.Handle),
					maxClientSize = main.AutomationZoomLimit.MaxClientSize,
					step = main.AutomationZoomStep,
					nominalZoom = main.NominalZoom,
					cell = ui.ItemUnitSize,
					largestStep = main.AutomationZoomLimit.LargestStep,
					largestZoom = TQVaultAE.Application.Layout.ZoomSteps.Nominal(main.AutomationZoomLimit.LargestStep),
					// The window sizes a resize by the user is kept between.
					resizeBounds = new
					{
						minimum = main.FormMinimumSize,
						maximum = main.FormMaximumSize,
					},
				});
			});

		[McpServerTool(Name = "window_state", Destructive = true)]
		[Description("Sets the main window's state like the taskbar or the caption buttons do: 'minimize', 'restore' (back from minimized or maximized, as a click on the taskbar icon) or 'maximize'. Returns the state, bounds and client size after it, and the bounds of the custom border's caption buttons (to check they are back in place after a restore).")]
		public static async Task<string> WindowState(
			[Description("minimize, restore or maximize.")] string state)
		{
			var target = state.ToLowerInvariant() switch
			{
				"minimize" => FormWindowState.Minimized,
				"restore" => FormWindowState.Normal,
				"maximize" => FormWindowState.Maximized,
				_ => throw new McpException($"Unknown state '{state}': minimize, restore or maximize."),
			};
			AutomationHost.OnUi(main => { main.WindowState = target; });

			// The resize handlers run on posted messages.
			await Task.Delay(500);

			return AutomationHost.OnUi(main =>
			{
				object Bounds(string name) => main.Controls.Find(name, false).FirstOrDefault() is { } c
					? new { x = c.Left, y = c.Top, width = c.Width, height = c.Height, visible = c.Visible }
					: null!;
				return Serialize(new
				{
					state = main.WindowState.ToString(),
					bounds = main.Bounds,
					clientSize = main.ClientSize,
					buttons = new
					{
						close = Bounds("buttonClose"),
						maximize = Bounds("buttonMaximize"),
						normalize = Bounds("ButtonScaleTo1"),
						minimize = Bounds("buttonMinimize"),
					},
				});
			});
		}

		[McpServerTool(Name = "move_window", Destructive = true)]
		[Description("Moves the main window, or another open form, to a screen position (virtual screen pixels, so negative on a monitor left of the primary one), like dragging it there (the size is left to Windows): on a monitor of another DPI, Windows sends the DPI change. Returns the monitors (bounds) and the window's DPI and scales after the move. Move it back to its monitor when done.")]
		public static async Task<string> MoveWindow(
			[Description("Left of the window in virtual screen pixels.")] int x,
			[Description("Top of the window in virtual screen pixels.")] int y,
			[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form = "")
		{
			// A view shown in the main window (the settings) moves with the main window.
			Form Target(Forms.MainForm main)
			{
				var target = FormOrMain(main, form);
				return target as Form ?? target.FindForm() ?? main;
			}

			AutomationHost.OnUi(main =>
			{
				// Without the size: Location would send the old size too, over the one the DPI change gives.
				NativeWindows.MoveTo(Target(main).Handle, x, y);
			});

			// The DPI change comes as a posted window message.
			await Task.Delay(500);

			return AutomationHost.OnUi(main =>
			{
				var ui = main.Services.UIService;
				var target = Target(main);
				return Serialize(new
				{
					location = target.Location,
					size = target.Size,
					clientSize = target.ClientSize,
					deviceDpi = target.DeviceDpi,
					zoom = ui.Zoom,
					dpiScale = ui.DpiScale,
					systemDpiScale = ui.SystemDpiScale,
					scale = ui.Scale,
					fontZoom = ui.FontZoom,
					monitors = Screen.AllScreens.Select(s => new
					{
						deviceName = s.DeviceName,
						bounds = s.Bounds,
						primary = s.Primary,
					}),
				});
			});
		}
	}
}

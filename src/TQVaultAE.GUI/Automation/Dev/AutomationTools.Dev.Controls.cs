using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Automation;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "control_values", ReadOnly = true)]
		[Description("Reads the input controls of an open form or dialog, or of the settings shown in the main window ('Settings'), (check boxes, radio buttons, numeric boxes, track bars, combo boxes, list boxes, text boxes, check group boxes): name, type, enabled, visible and value. Use it to check the rules a dialog applies when another control changes.")]
		public static string ControlValues(
			[Description("Part of the form title or its type name (case-insensitive).")] string form,
			[Description("true to also list the labels and their text.")] bool labels = false)
			=> AutomationHost.OnUi(_ =>
			{
				var target = FindOpenForm(form);
				var controls = AllControls(target)
					.Where(c => !string.IsNullOrEmpty(c.Name) && (IsInputControl(c) || (labels && c is Label)))
					.Select(DescribeValue)
					.ToList();
				return Serialize(new { form = target.Text, type = target.GetType().Name, controls });
			});

		[McpServerTool(Name = "set_control", Destructive = true)]
		[Description("Sets the value of a control of an open form or dialog, or of the settings shown in the main window ('Settings'), like a user would, so its own events run (CheckedChanged, ValueChanged, SelectedIndexChanged...). Value: true/false for check boxes, radio buttons and check group boxes; a number for numeric boxes and track bars (then Scroll is raised, as after a drag); the item text or '#index' for combo boxes (then SelectionChangeCommitted is raised, as after a pick in the list; any text for an editable one) and list boxes; the text for text boxes (then Validating, Validated and Leave are raised, as when the user leaves the box). Other control types are refused (use click_control or mouse_click). Returns the values of the form's input controls afterwards. Changes made in a dialog are applied by its OK button (close_dialog).")]
		public static string SetControl(
			[Description("Part of the form title or its type name (case-insensitive).")] string form,
			[Description("Control name, as listed by control_values.")] string control,
			[Description("New value.")] string value)
			=> AutomationHost.OnUi(_ =>
			{
				var target = FindOpenForm(form);
				var c = FindControl(target, control, " Use control_values.");
				if (!c.Enabled)
					throw new McpException($"'{c.Name}' is disabled: a user could not change it.");

				switch (c)
				{
					case NumericUpDown number:
						var n = decimal.Parse(value, CultureInfo.InvariantCulture);
						if (n < number.Minimum || n > number.Maximum)
							throw new McpException($"{n} is out of range {number.Minimum}..{number.Maximum}.");
						number.Value = n;
						break;
					case TrackBar track:
						// A drag or an arrow key moves the thumb (ValueChanged), then the bar raises Scroll.
						var position = int.Parse(value, CultureInfo.InvariantCulture);
						if (position < track.Minimum || position > track.Maximum)
							throw new McpException($"{position} is out of range {track.Minimum}..{track.Maximum}.");
						track.Value = position;
						RaiseProtected(track, "OnScroll");
						break;
					case ComboBox combo:
						int index = -1;
						for (int i = 0; i < combo.Items.Count; i++)
						{
							if (string.Equals(combo.GetItemText(combo.Items[i]), value, StringComparison.OrdinalIgnoreCase))
							{
								index = i;
								break;
							}
						}

						// An editable combo box takes any text, as typed.
						if (index < 0 && combo.DropDownStyle != ComboBoxStyle.DropDownList && !value.StartsWith('#'))
						{
							combo.Text = value;
							break;
						}

						if (index < 0 && value.StartsWith('#'))
							value = value[1..];
						if (index < 0 && !int.TryParse(value, out index))
							throw new McpException($"No item '{value}' in '{c.Name}'. Items: {string.Join(", ", combo.Items.Cast<object>().Select(combo.GetItemText))}");
						if (index < 0 || index >= combo.Items.Count)
							throw new McpException($"Index {index} is out of range 0..{combo.Items.Count - 1}.");
						// A pick in the list changes the selection, then commits it (SelectionChangeCommitted).
						combo.SelectedIndex = index;
						RaiseProtected(combo, "OnSelectionChangeCommitted");
						break;
					case ListBox list:
						int row = -1;
						for (int i = 0; i < list.Items.Count; i++)
						{
							if (string.Equals(list.GetItemText(list.Items[i]), value, StringComparison.OrdinalIgnoreCase))
							{
								row = i;
								break;
							}
						}

						if (row < 0 && !(value.StartsWith('#') && int.TryParse(value[1..], out row)))
							throw new McpException($"No item '{value}' in '{c.Name}'. Items: {string.Join(", ", list.Items.Cast<object>().Select(list.GetItemText))}");
						list.SelectedIndex = row;
						break;
					case TextBoxBase text:
						text.Text = value;
						// A user types, then leaves the box: some boxes apply their text on Leave or Validated.
						RaiseProtected(text, "OnValidating", new CancelEventArgs());
						RaiseProtected(text, "OnValidated");
						RaiseProtected(text, "OnLeave");
						break;
					default:
						var checkedProperty = c.GetType().GetProperty("Checked", typeof(bool));
						if (checkedProperty?.CanWrite != true)
							throw new McpException($"'{c.Name}' is a {c.GetType().Name}: set_control sets check boxes, radio buttons, check group boxes, numeric boxes, track bars, combo boxes, list boxes and text boxes. Use click_control or mouse_click for the others.");
						checkedProperty.SetValue(c, bool.Parse(value));
						break;
				}

				var controls = AllControls(target).Where(x => !string.IsNullOrEmpty(x.Name) && IsInputControl(x)).Select(DescribeValue).ToList();
				return Serialize(new { set = DescribeValue(c), controls });
			});

		[McpServerTool(Name = "click_control", Destructive = true)]
		[Description("Clicks a button of an open form, or of the settings shown in the main window ('Settings'), like a user (form '' for the main window, e.g. control 'configureButton' shows the settings in the main window, or leaves them as their Cancel does, asking first when they were changed). The click runs after this call returns, since a button may open a modal dialog: follow with list_windows, control_values or close_dialog.")]
		public static string ClickControl(
			[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form,
			[Description("Button name, as listed by control_values or control_tree.")] string control)
			=> AutomationHost.OnUi(main =>
			{
				var target = FormOrMain(main, form);
				var c = FindControl(target, control);
				if (c is not IButtonControl and not ButtonBase)
					throw new McpException($"'{c.Name}' is a {c.GetType().Name}, not a button.");
				if (!c.Enabled || !c.Visible)
					throw new McpException($"'{c.Name}' is disabled or hidden: a user could not click it.");

				// OnClick is what a mouse click raises; every button (Button, ScalingButton, check boxes) has it.
				var onClick = typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
				main.BeginInvoke(() => onClick.Invoke(c, [EventArgs.Empty]));
				return Serialize(new { form = target.Text, clicked = c.Name });
			});

		[McpServerTool(Name = "mouse_click", Destructive = true)]
		[Description("Clicks a point of a control with mouse messages sent to the window under that point, as Windows sends them (WM_MOUSEMOVE, button down, button up; a double click adds WM_*BUTTONDBLCLK and up; the right button ends with WM_CONTEXTMENU, so a control's own context menu opens), unlike click_control, which raises OnClick: the control's own mouse handling, hit testing and repaint run as for a user's click. x and y are in the control's client coordinates (bounds from control_tree); the messages go to the deepest visible child at that point, e.g. the check box in the caption of a check group box (control 'detectGamePathsCheckBox', on its glyph or its text), the loadout bar ('loadoutBar': [+] and Load at the bounds loadouts reports), a link label, the character editor's tabs. The click runs after this call returns: read the result with control_values, list_windows or capture.")]
		public static string MouseClick(
			[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form,
			[Description("Control name, as listed by control_tree.")] string control,
			[Description("X in the control's client coordinates.")] int x,
			[Description("Y in the control's client coordinates.")] int y,
			[Description("left or right.")] string button = "left",
			[Description("1 for a click, 2 for a double click.")] int clicks = 1)
			=> AutomationHost.OnUi(main =>
			{
				var mouseButton = ParseButton(button);
				if (clicks is not (1 or 2))
					throw new McpException("clicks must be 1 or 2.");

				var target = FormOrMain(main, form);
				var c = FindControl(target, control);

				// The deepest visible child under the point, as Windows finds the window that gets the mouse.
				var (hit, point) = AutomationInput.ChildAt(c, new Point(x, y));

				main.BeginInvoke(() => AutomationInput.SendMouse(hit, point, mouseButton, clicks));
				return Serialize(new { form = target.Text, control = c.Name, receiver = hit.Name, receiverType = hit.GetType().Name, point, button = mouseButton.ToString(), clicks });
			});

		[McpServerTool(Name = "mouse_wheel", Destructive = true)]
		[Description("Turns the mouse wheel over a point of a control: WM_MOUSEWHEEL sent to the deepest visible window under that point, as Windows does for the window under the cursor; a window that does not use the wheel passes it to its parents, as for the user. notches > 0 turns it away from the user (scrolls up), < 0 towards (down). modifiers ('ctrl', 'shift', 'alt', 'ctrl+shift') are held during the message. Runs before this call returns, which then gives the receiver and the scroll position of each scrollable control from the receiver up to the form (AutoScroll panels: x and y; list boxes: topIndex; visible scroll bar children: value), to read the effect. Works headless: the search dialog's lists, the bag settings' icon grids, the character list.")]
		public static string MouseWheel(
			[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form,
			[Description("Control name, as listed by control_tree.")] string control,
			[Description("X in the control's client coordinates.")] int x,
			[Description("Y in the control's client coordinates.")] int y,
			[Description("Wheel notches: positive up (away from the user), negative down.")] int notches,
			[Description("Modifier keys held: ctrl, shift, alt or a combination like ctrl+shift; empty for none.")] string? modifiers = null)
			=> AutomationHost.OnUi(main =>
			{
				var target = FormOrMain(main, form);
				var c = FindControl(target, control);
				if (notches == 0)
					throw new McpException("notches must not be 0.");

				var held = ClickModifiers(modifiers);

				var (hit, point) = AutomationInput.ChildAt(c, new Point(x, y));
				AutomationInput.SendWheel(hit, point, notches, held);

				var scroll = new List<object>();
				for (Control? up = hit; up is not null; up = up.Parent)
				{
					if (up is ScrollableControl { AutoScroll: true } scrollable)
						scroll.Add(new { control = up.Name, type = up.GetType().Name, x = -scrollable.AutoScrollPosition.X, y = -scrollable.AutoScrollPosition.Y });
					else if (up is ListBox list)
						scroll.Add(new { control = up.Name, type = up.GetType().Name, topIndex = list.TopIndex });

					// A control drawing its own content with a scroll bar child (the bag settings' icon grids).
					foreach (var bar in up.Controls.OfType<ScrollBar>().Where(b => b.Visible))
						scroll.Add(new { control = up.Name, type = up.GetType().Name, scrollBar = bar.GetType().Name, value = bar.Value, maximum = bar.Maximum });
				}

				return Serialize(new { form = target.Text, control = c.Name, receiver = hit.Name, receiverType = hit.GetType().Name, point, notches, scroll });
			});
	}
}

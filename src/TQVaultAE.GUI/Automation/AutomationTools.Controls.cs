using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Form controls

	[McpServerTool(Name = "control_values", ReadOnly = true)]
	[Description("Reads the input controls of an open form or dialog (check boxes, radio buttons, numeric boxes, combo boxes, text boxes, check group boxes): name, type, enabled, visible and value. Use it to check the rules a dialog applies when another control changes.")]
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
	[Description("Sets the value of a control of an open form or dialog, like a user would, so its own events run (CheckedChanged, ValueChanged, SelectedIndexChanged...). Value: true/false for check boxes, radio buttons and check group boxes; a number for numeric boxes; the item text or index for combo boxes; the text for text boxes (then Validating, Validated and Leave are raised, as when the user leaves the box). Returns the values of the form's input controls afterwards. Changes made in a dialog are applied by its OK button (close_dialog).")]
	public static string SetControl(
		[Description("Part of the form title or its type name (case-insensitive).")] string form,
		[Description("Control name, as listed by control_values.")] string control,
		[Description("New value.")] string value)
		=> AutomationHost.OnUi(_ =>
		{
			var target = FindOpenForm(form);
			var c = AllControls(target).FirstOrDefault(x => string.Equals(x.Name, control, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No control '{control}' in '{target.Text}'. Use control_values.");
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

					if (index < 0 && !int.TryParse(value, out index))
						throw new McpException($"No item '{value}' in '{c.Name}'. Items: {string.Join(", ", combo.Items.Cast<object>().Select(combo.GetItemText))}");
					combo.SelectedIndex = index;
					break;
				case TextBoxBase text:
					text.Text = value;
					// A user types, then leaves the box: some boxes apply their text on Leave or Validated.
					foreach (var (name, args) in new (string, EventArgs)[] { ("OnValidating", new CancelEventArgs()), ("OnValidated", EventArgs.Empty), ("OnLeave", EventArgs.Empty) })
						typeof(Control).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(text, [args]);
					break;
				default:
					var checkedProperty = c.GetType().GetProperty("Checked", typeof(bool));
					if (checkedProperty?.CanWrite == true)
						checkedProperty.SetValue(c, bool.Parse(value));
					else
						c.Text = value;
					break;
			}

			var controls = AllControls(target).Where(x => !string.IsNullOrEmpty(x.Name) && IsInputControl(x)).Select(DescribeValue).ToList();
			return Serialize(new { set = DescribeValue(c), controls });
		});


	[McpServerTool(Name = "click_control", Destructive = true)]
	[Description("Clicks a button of an open form, like a user (form '' for the main window, e.g. control 'configureButton' opens the settings from the main window, so its OK runs the main window's rules). The click runs after this call returns, since a button may open a modal dialog: follow with list_windows, control_values or close_dialog.")]
	public static string ClickControl(
		[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form,
		[Description("Button name, as listed by control_values or control_tree.")] string control)
		=> AutomationHost.OnUi(main =>
		{
			Form target = string.IsNullOrEmpty(form) ? main : FindOpenForm(form);
			var c = AllControls(target).FirstOrDefault(x => string.Equals(x.Name, control, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"No control '{control}' in '{target.Text}'.");
			if (c is not IButtonControl and not ButtonBase)
				throw new McpException($"'{c.Name}' is a {c.GetType().Name}, not a button.");
			if (!c.Enabled || !c.Visible)
				throw new McpException($"'{c.Name}' is disabled or hidden: a user could not click it.");

			// OnClick is what a mouse click raises; every button (Button, ScalingButton, check boxes) has it.
			var onClick = typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
			main.BeginInvoke(() => onClick.Invoke(c, [EventArgs.Empty]));
			return Serialize(new { form = target.Text, clicked = c.Name });
		});

	private static Form FindOpenForm(string form)
		=> System.Windows.Forms.Application.OpenForms.Cast<Form>().LastOrDefault(f => f.Visible
			&& (f.Text.Contains(form, StringComparison.OrdinalIgnoreCase) || string.Equals(f.GetType().Name, form, StringComparison.OrdinalIgnoreCase)))
		?? throw new McpException($"No open form '{form}'. Open forms: {string.Join(", ", System.Windows.Forms.Application.OpenForms.Cast<Form>().Where(f => f.Visible).Select(f => $"'{f.Text}' ({f.GetType().Name})"))}");

	private static IEnumerable<Control> AllControls(Control parent)
	{
		foreach (Control child in parent.Controls)
		{
			yield return child;
			foreach (var grandChild in AllControls(child))
				yield return grandChild;
		}
	}

	private static bool IsInputControl(Control c)
		=> c is NumericUpDown or ComboBox or TextBoxBase or CheckBox or RadioButton
		|| c.GetType().GetProperty("Checked", typeof(bool)) is not null;

	private static object? ValueOf(Control c) => c switch
	{
		NumericUpDown number => number.Value,
		ComboBox combo => combo.SelectedItem is null ? null : combo.GetItemText(combo.SelectedItem),
		TextBoxBase text => text.Text,
		Label label => label.Text,
		_ => c.GetType().GetProperty("Checked", typeof(bool))?.GetValue(c) ?? c.Text,
	};

	private static object DescribeValue(Control c) => new
	{
		name = c.Name,
		type = c.GetType().Name,
		enabled = c.Enabled,
		visible = c.Visible,
		value = ValueOf(c),
	};

	#endregion

	#region Character editor and tags

	[McpServerTool(Name = "open_character_editor", Destructive = true)]
	[Description("Opens the character editor of the selected character, like the Edit button of the stash panel. It is modal: read and change it with control_values and set_control (form 'CharacterEditDialog'), then close it with close_dialog (OK applies the changes in memory; save_files writes them). Work only on claude-* test characters.")]
	public static string OpenCharacterEditor()
		=> AutomationHost.OnUi(form =>
		{
			if (form.AutomationCurrentPlayerFile is null)
				throw new McpException("No character is selected. Use select_character first.");
			form.AutomationOpenCharacterEditor();
			return Serialize(new { opening = "CharacterEditDialog", character = form.AutomationSelectedCharacter });
		});

	[McpServerTool(Name = "tag_box", Destructive = true)]
	[Description("Types into a tag box of the character drop down's context menu and presses Enter, like a user: box 'add' (new tag, assigned to the selected character) or 'rename' (renames the tag 'tag'). A refused name shows an error box (answer it with close_dialog). Box 'delete' removes the test tag 'text' without asking. Names must start with 'claude-'. Read the tags with list_tags.")]
	public static string TagBox(
		[Description("add, rename or delete.")] string box,
		[Description("The text typed (add, rename) or the tag to delete.")] string text,
		[Description("For rename: the tag being renamed.")] string? tag = null)
		=> AutomationHost.OnUi(form =>
		{
			var combo = form.AutomationCharacterCombo;
			var comboType = typeof(ComboBoxCharacter);
			const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

			// Test tags only; an empty or invalid name is allowed through to test the refusals.
			bool isTestName(string? name) => name is not null && name.StartsWith(TestFilePrefix, StringComparison.OrdinalIgnoreCase);

			comboType.GetMethod("RefreshMenuTagList", Private)!.Invoke(combo, null);

			switch (box.ToLowerInvariant())
			{
				case "add":
				{
					if (!string.IsNullOrWhiteSpace(text) && !isTestName(text))
						throw new McpException($"Test tags only: the name must start with '{TestFilePrefix}'.");
					var textBox = (ToolStripTextBox)comboType.GetField("toolStripTextBoxAdd", Private)!.GetValue(combo)!;
					PressEnter(form, combo, comboType.GetMethod("ToolStripTextBoxAdd_KeyDown", Private)!, textBox, text);
					break;
				}
				case "rename":
				{
					if (!isTestName(tag))
						throw new McpException($"Test tags only: 'tag' must start with '{TestFilePrefix}'.");
					var tags = (ToolStripMenuItem)comboType.GetField("tagsToolStripMenuItem", Private)!.GetValue(combo)!;
					var tagItem = tags.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(i => i.Text == tag)
						?? throw new McpException($"No tag '{tag}' in the menu.");
					var textBox = tagItem.DropDownItems.OfType<ToolStripMenuItem>()
						.SelectMany(i => i.DropDownItems.OfType<ToolStripTextBox>())
						.First();
					PressEnter(form, combo, comboType.GetMethod("TagRenameTextBoxItem_KeyDown", Private)!, textBox, text);
					break;
				}
				case "delete":
				{
					if (!isTestName(text))
						throw new McpException($"Test tags only: the name must start with '{TestFilePrefix}'.");
					Program.ServiceProvider.GetRequiredService<ITagService>().DeleteTag(text);
					comboType.GetMethod("RefreshMenuTagList", Private)!.Invoke(combo, null);
					break;
				}
				default:
					throw new McpException("box must be add, rename or delete.");
			}

			return Serialize(new { box, text, tag, sent = true });
		});

	[McpServerTool(Name = "list_tags", ReadOnly = true)]
	[Description("Lists the character tags (name and color), and the tags of the selected character.")]
	public static string ListTags()
		=> AutomationHost.OnUi(form =>
		{
			var tags = Program.ServiceProvider.GetRequiredService<ITagService>().Tags;
			var selected = form.AutomationCharacterCombo.SelectedItem as PlayerSave;
			return Serialize(new
			{
				tags = tags.Select(t => new { name = t.Key, color = $"#{t.Value.R:X2}{t.Value.G:X2}{t.Value.B:X2}" }).ToList(),
				selectedCharacterTags = selected?.Tags.Keys.ToList(),
			});
		});

	/// <summary>
	/// Types <paramref name="text"/> in a menu text box and raises its Enter key handler after this call returns,
	/// since a refusal opens a modal error box.
	/// </summary>
	private static void PressEnter(Form form, object owner, MethodInfo handler, ToolStripTextBox textBox, string text)
	{
		textBox.Text = text;
		form.BeginInvoke(() => handler.Invoke(owner, [textBox, new KeyEventArgs(Keys.Enter)]));
	}

	#endregion
}

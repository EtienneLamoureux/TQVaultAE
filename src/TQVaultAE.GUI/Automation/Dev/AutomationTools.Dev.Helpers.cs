using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Automation;
using static TQVaultAE.Automation.ControlTree;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.GUI.Automation;

/// <summary>Control lookups and helpers of the developer tools.</summary>
internal static partial class AutomationTools
{
	private static object DescribeValue(Control c) => new
	{
		name = c.Name,
		type = c.GetType().Name,
		enabled = c.Enabled,
		visible = c.Visible,
		value = ValueOf(c),
	};

	/// <summary>The control of <paramref name="target"/> named <paramref name="name"/> (ignoring case), at any depth.</summary>
	/// <param name="hint">Added to the error message.</param>
	private static Control FindControl(Control target, string name, string? hint = null)
		=> AllControls(target).FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"No control '{name}' in '{target.Text}'.{hint}");

	private static bool IsInputControl(Control c)
		=> c is NumericUpDown or TrackBar or ComboBox or ListBox or TextBoxBase or CheckBox or RadioButton
		|| c.GetType().GetProperty("Checked", typeof(bool)) is not null;

	/// <summary>Raises a protected event method of a control (OnScroll, OnLeave...), as its own input handling does.</summary>
	private static void RaiseProtected(Control c, string method, EventArgs? args = null)
		=> c.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, [args?.GetType() ?? typeof(EventArgs)])!.Invoke(c, [args ?? EventArgs.Empty]);

	private static int CountControls(Control control)
		=> control.Controls.Cast<Control>().Sum(c => 1 + CountControls(c));

	private static IEnumerable<Type> FormTypes()
	{
		var check = Service<IServiceProviderIsService>();
		return typeof(Program).Assembly.GetTypes()
			.Where(t => typeof(Form).IsAssignableFrom(t) && !t.IsAbstract && check.IsService(t))
			.OrderBy(t => t.Name);
	}

	private static object? ValueOf(Control c) => c switch
	{
		NumericUpDown number => number.Value,
		TrackBar track => track.Value,
		ComboBox combo => combo.SelectedItem is not null ? combo.GetItemText(combo.SelectedItem) : combo.DropDownStyle != ComboBoxStyle.DropDownList && combo.Text.Length > 0 ? combo.Text : null,
		ListBox list => list.SelectedItem is not null ? list.GetItemText(list.SelectedItem) : null,
		TextBoxBase text => text.Text,
		Label label => label.Text,
		_ => c.GetType().GetProperty("Checked", typeof(bool))?.GetValue(c) ?? c.Text,
	};
}

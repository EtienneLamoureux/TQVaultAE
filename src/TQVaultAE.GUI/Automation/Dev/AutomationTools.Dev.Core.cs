//-----------------------------------------------------------------------
// <copyright file="AutomationTools.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Automation;
using static TQVaultAE.Automation.ControlTree;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	/// <summary>
	/// The developer tools of the automation endpoint, compiled in Debug builds only (the files of <c>Automation/Dev</c>):
	/// UI internals probes (control trees, text fit, zoom, window state), raw mouse and keyboard input, diagnostic dumps,
	/// virtual cursor hovers and fault injection. The user tools, shipped in Release builds, are the methods of
	/// <see cref="AutomationTools"/> itself. See the Release column of documentation/AUTOMATION.md.
	/// </summary>
	internal static partial class DevTools
	{
		[McpServerTool(Name = "recent_log", ReadOnly = true)]
		[Description("Returns the last log lines written by TQVault, oldest first.")]
		public static string RecentLog([Description("Number of lines (default 50).")] int n = 50)
			=> string.Join(Environment.NewLine, AutomationHost.RecentLog.GetLast(Math.Clamp(n, 1, 2000)));

		[McpServerTool(Name = "press_key", Destructive = true)]
		[Description("Rests the cursor on a cell of a panel and presses a key there like the keyboard: the key messages go to the window that has the keyboard focus, through the message loop (ProcessCmdKey, IsInputKey, ProcessDialogKey, the main window's key preview, then the control's KeyDown), the modifiers held in the keyboard state; so the key binds apply as for the user: 'c' copies, 'd' duplicates, 'backspace' deletes the item under the cursor by default (with its confirmation: queue the answer first), 'escape' cancels a drag, 'ctrl+a', 'alt+w', 'f5'... With an empty panel the cursor does not move (the forge keys 'enter' / 'space'). Returns the control that received the key; runs after the call returns: follow with get_state, forge_state or dialog_answers.")]
		public static string PressKey(
			[Description("Panel id, or empty for the main window alone.")] string panel,
			[Description("Cell column.")] int x,
			[Description("Cell row.")] int y,
			[Description("The key with its modifiers: a character, or a key bind gesture like 'ctrl+alt+d', 'backspace', 'escape', 'enter', 'space'.")] string key)
			=> AutomationHost.OnUi(form =>
			{
				var keys = ParseKey(key);
				static string Describe(Control target) => $"{target.GetType().Name} {target.Name}".TrimEnd();

				if (string.IsNullOrEmpty(panel))
				{
					var focused = AutomationInput.PostKey(form, keys);
					return Serialize(new { panel = "mainForm", key = keys.ToString(), target = Describe(focused) });
				}

				var named = ResolveShownPanel(form, panel);
				var item = named.Panel.AutomationFindItem(new Point(x, y));
				var target = named.Panel.AutomationKeyDown(new Point(x, y), keys, form);
				return Serialize(new { panel = named.Id, item = DescribeItem(item, null), key = keys.ToString(), target = Describe(target) });
			});

		[McpServerTool(Name = "drag_select", Destructive = true)]
		[Description("Presses the left button on a cell of a panel with modifier keys held, moves to another cell and releases there, through the panel's mouse handlers: with the rectangle selection's modifiers (Shift by default, from the key binds) it selects the items in the rectangle. Returns the selection and the dragged item.")]
		public static string DragSelect(
			[Description("Panel id.")] string panel,
			[Description("Start cell column.")] int x1,
			[Description("Start cell row.")] int y1,
			[Description("End cell column.")] int x2,
			[Description("End cell row.")] int y2,
			[Description("Modifiers held, like 'shift'; empty for none.")] string modifiers)
			=> AutomationHost.OnUi(form =>
			{
				var named = ResolveShownPanel(form, panel);
				RequireNoDrag(form);

				var held = ClickModifiers(modifiers);
				named.Panel.AutomationDragSelect(new Point(x1, y1), new Point(x2, y2), held);
				var drag = form.AutomationDragInfo;
				return Serialize(new
				{
					panel = named.Id,
					modifiers = held.ToString(),
					dragged = drag.IsActive ? DescribeItem(drag.Item, null) : null,
					selected = named.Panel.AutomationSelectedItems.Select(i => DescribeItem(i, null)).ToList(),
				});
			});
	}
}

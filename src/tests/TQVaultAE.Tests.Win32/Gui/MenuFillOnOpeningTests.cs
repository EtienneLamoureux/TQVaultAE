using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// A menu filled when it opens (<c>MenuStyle.FillOnOpening</c>): a menu empty when asked to open gets its opening already
/// cancelled, which made such menus (the loadout bar, the results window) open only on the second click.
/// </summary>
public sealed class MenuFillOnOpeningTests
{
	private static readonly MethodInfo FillOnOpening = typeof(TQVaultAE.GUI.Program).Assembly
		.GetType("TQVaultAE.GUI.Components.Menus.MenuStyle", throwOnError: true)!
		.GetMethod("FillOnOpening", BindingFlags.Public | BindingFlags.Static)!;

	private static readonly MethodInfo OnOpening = typeof(ToolStripDropDown)
		.GetMethod("OnOpening", BindingFlags.Instance | BindingFlags.NonPublic)!;

	/// <summary>Raises the opening of an empty menu as Windows Forms does: already cancelled.</summary>
	private static bool OpensWhenEmpty(ContextMenuStrip menu)
	{
		var e = new CancelEventArgs(cancel: true);
		OnOpening.Invoke(menu, [e]);
		return !e.Cancel;
	}

	[Fact]
	public void EmptyMenu_FilledOnOpening_OpensOnTheFirstClick()
	{
		using var menu = new ContextMenuStrip();
		FillOnOpening.Invoke(null, [menu, (Action)(() => menu.Items.Add("Entry"))]);

		OpensWhenEmpty(menu).Should().BeTrue();
		menu.Items.Count.Should().Be(1);
	}

	[Fact]
	public void MenuLeftEmpty_StaysClosed()
	{
		using var menu = new ContextMenuStrip();
		FillOnOpening.Invoke(null, [menu, (Action)(() => { })]);

		OpensWhenEmpty(menu).Should().BeFalse();
	}

	[Fact]
	public void MenuRefilled_ReplacesItsEntries()
	{
		using var menu = new ContextMenuStrip();
		int opened = 0;
		FillOnOpening.Invoke(null, [menu, (Action)(() =>
		{
			menu.Items.Clear();
			menu.Items.Add($"Entry {++opened}");
		})]);

		OpensWhenEmpty(menu);
		OpensWhenEmpty(menu).Should().BeTrue();
		menu.Items.Count.Should().Be(1);
		menu.Items[0].Text.Should().Be("Entry 2");
	}
}

using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.GUI.Components.Layout;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The widest text width of <see cref="VirtualCheckedListBox"/>, kept per font until the items change: a width kept too
/// long would size the category lists of the search dialog for texts or a font they no longer have.
/// </summary>
public sealed class VirtualCheckedListBoxTests : IDisposable
{
	private readonly Font small = new("Segoe UI", 9F);
	private readonly Font large = new("Segoe UI", 18F);
	private readonly VirtualCheckedListBox list = new();

	public VirtualCheckedListBoxTests()
	{
		this.list.Font = this.small;
		this.list.Items.Add("short");
	}

	public void Dispose()
	{
		this.list.Dispose();
		this.small.Dispose();
		this.large.Dispose();
	}

	[Fact]
	public void GetMaxTextWidth_AfterAnItemIsAdded_MeasuresTheNewItem()
	{
		int before = this.list.GetMaxTextWidth();

		this.list.Items.Add("a much longer entry than the first one");

		this.list.GetMaxTextWidth().Should().BeGreaterThan(before);
	}

	[Fact]
	public void GetMaxTextWidth_AfterTheItemsAreReplaced_MeasuresOnlyTheNewOnes()
	{
		this.list.Items.Add("a much longer entry than the first one");
		int before = this.list.GetMaxTextWidth();

		this.list.Items.Clear();
		this.list.Items.AddRange(["tiny"]);

		this.list.GetMaxTextWidth().Should().BeLessThan(before);
	}

	[Fact]
	public void GetMaxTextWidth_AfterAFontChangeAndBack_GivesTheWidthOfEachFont()
	{
		int small = this.list.GetMaxTextWidth();

		this.list.Font = this.large;
		int large = this.list.GetMaxTextWidth();
		this.list.Font = this.small;

		large.Should().BeGreaterThan(small);
		this.list.GetMaxTextWidth().Should().Be(small);
	}
}

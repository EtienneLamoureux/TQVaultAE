using ArzExplorer.Models;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.ArzExplorer;

/// <summary>
/// <see cref="SearchOrder"/> gives the nodes Find Next and Find Previous visit.
/// </summary>
public sealed class SearchOrderTests
{
	[Fact]
	public void Forward_NewSearch_StartsAtTheFirstNode()
		=> SearchOrder.Indices(4, -1, forward: true).Should().Equal(0, 1, 2, 3);

	[Fact]
	public void Backward_NewSearch_StartsAtTheLastNode()
		=> SearchOrder.Indices(4, 4, forward: false).Should().Equal(3, 2, 1, 0);

	[Fact]
	public void Forward_AfterAMatch_WrapsAroundAndEndsOnThatMatch()
		=> SearchOrder.Indices(4, 2, forward: true).Should().Equal(3, 0, 1, 2);

	[Fact]
	public void Backward_AfterAMatch_WrapsAroundAndEndsOnThatMatch()
		=> SearchOrder.Indices(4, 1, forward: false).Should().Equal(0, 3, 2, 1);

	[Fact]
	public void Forward_AfterFindPreviousFoundNothing_StartsAtTheFirstNode()
		// Find Previous leaves the position past the last node when nothing matches
		=> SearchOrder.Indices(3, 3, forward: true).Should().Equal(0, 1, 2);

	[Fact]
	public void Backward_AfterFindNextFoundNothing_StartsAtTheLastNode()
		// Find Next leaves the position before the first node when nothing matches
		=> SearchOrder.Indices(3, -1, forward: false).Should().Equal(2, 1, 0);

	[Fact]
	public void NoNodes_VisitsNothing()
		=> SearchOrder.Indices(0, -1, forward: true).Should().BeEmpty();
}

using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

/// <summary><see cref="ClassSetDraft"/>: the class sets changed in the character editor before OK.</summary>
public class ClassSetDraftTests
{
	private static readonly ClassSet Build = new("Storm", [new ClassSetMastery(Masteries.Storm, 10, [new("""Records\Skills\Storm\IceShard.dbr""", 5)])], new ClassSetAttributes(0, 2, 0, 10, 0));

	private static ClassSetDraft Draft(bool canChange, bool readFailed, params ClassSet[] sets)
		=> new(new ClassSetBook("file") { Sets = sets, ReadFailed = readFailed }, canChange);

	[Fact]
	public void Add_KeepsTheSelectionAndRefusesAUsedOrEmptyName()
	{
		var draft = Draft(true, false, Build);
		draft.Select("Storm");

		draft.Add(" Empty ").Should().Be(SetNameError.None);
		draft.Add("storm").Should().Be(SetNameError.AlreadyUsed);
		draft.Add("  ").Should().Be(SetNameError.Empty);

		draft.Names.Should().Equal("Storm", "Empty");
		draft.Find("Empty")!.SameBuild(ClassSet.Empty("x")).Should().BeTrue();
		draft.Selected.Should().Be("Storm");
		draft.Changed.Should().BeTrue();
	}

	[Fact]
	public void Rename_SelectedSet_TheSelectionFollows()
	{
		var draft = Draft(true, false, Build);
		draft.Select("storm");

		draft.Rename("Storm", "STORM").Should().Be(SetNameError.None, "a set may keep its name with another case");

		draft.Selected.Should().Be("STORM");
		draft.Names.Should().Equal("STORM");
	}

	[Fact]
	public void Delete_SelectedSet_LeavesNoneSelected()
	{
		var draft = Draft(true, false, Build, ClassSet.Empty("Empty"));
		draft.Select("Storm");

		draft.Delete("Storm").Should().BeTrue();

		draft.Selected.Should().BeNull();
		draft.Names.Should().Equal("Empty");
	}

	[Fact]
	public void Update_WritesTheBuildIntoAnotherSetOnly()
	{
		var draft = Draft(true, false, ClassSet.Empty("Empty"), ClassSet.Empty("Other"));
		draft.Select("Empty");

		draft.Update("Empty", Build).Should().BeFalse("the selected set is never updated");
		draft.Update("Other", Build).Should().BeTrue();

		draft.Find("Other")!.Name.Should().Be("Other");
		draft.Find("Other")!.SameBuild(Build).Should().BeTrue();
		draft.Find("Empty")!.Masteries.Should().BeEmpty();
	}

	[Fact]
	public void SelectMatching_SelectsTheSetHoldingTheBuildOrNone()
	{
		var draft = Draft(true, false, ClassSet.Empty("Empty"), Build);

		draft.SelectMatching(Build with { Name = string.Empty });
		draft.Selected.Should().Be("Storm");

		draft.SelectMatching(ClassSet.Empty(string.Empty) with { Attributes = new ClassSetAttributes(1, 0, 0, 0, 0) });
		draft.Selected.Should().BeNull();
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void ReadOnlyCharacterOrUnreadFile_SetsCanBeSelectedNotChanged(bool canChange, bool readFailed)
	{
		var draft = Draft(canChange, readFailed, Build, ClassSet.Empty("Empty"));

		draft.Add("New");
		draft.Rename("Storm", "Other");
		draft.Delete("Empty");
		draft.Update("Empty", Build);

		draft.CanChange.Should().BeFalse();
		draft.Changed.Should().BeFalse();
		draft.Names.Should().Equal("Storm", "Empty");
		draft.Select("Storm").Should().Be(Build);
	}
}

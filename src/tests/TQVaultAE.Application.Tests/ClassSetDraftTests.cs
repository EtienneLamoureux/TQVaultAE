using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

/// <summary><see cref="ClassSetDraft"/>: the class sets changed in the character editor before OK.</summary>
public class ClassSetDraftTests
{
	private const string IceShard = """Records\Skills\Storm\IceShard.dbr""";

	/// <summary>Storm 10 with Ice Shard 5, Earth 4; 2 energy and 10 intelligence points.</summary>
	private static readonly ClassSet Build = new("Storm",
		[new ClassSetMastery(Masteries.Storm, 10, [new(IceShard, 5)]), new ClassSetMastery(Masteries.Earth, 4, [])],
		new ClassSetAttributes(0, 2, 0, 10, 0));

	/// <summary>The same build, 6 more Ice Shard points and 2 more intelligence points.</summary>
	private static readonly ClassSet Played = new("x",
		[new ClassSetMastery(Masteries.Storm, 10, [new(IceShard, 11)]), new ClassSetMastery(Masteries.Earth, 4, [])],
		new ClassSetAttributes(0, 2, 0, 12, 0));

	private static ClassSetDraft Draft(ClassSet opening, string? lastApplied, params ClassSet[] sets)
		=> new(new ClassSetBook("file") { Sets = sets, LastApplied = lastApplied }, canChange: true, opening);

	[Fact]
	public void Opening_SameMasteriesAsTheRememberedSet_SelectsItEvenWithOtherPointsOrSlotOrder()
	{
		var swapped = Played with { Masteries = [.. Played.Masteries.Reverse()] };

		var draft = Draft(swapped, "storm", Build);

		draft.Selected.Should().Be("Storm");
		draft.Changed.Should().BeFalse("nothing changed yet");
	}

	[Fact]
	public void Opening_OtherMasteriesOrDeletedSet_SelectsNone()
	{
		var otherClass = Build with { Masteries = [Build.Masteries[0]] };

		Draft(otherClass, "Storm", Build).Selected.Should().BeNull("the character dropped Earth since");
		Draft(Build, "Gone", Build).Selected.Should().BeNull("the set remembered is no longer there");
	}

	[Fact]
	public void Status_PointsSpentInTheGameSinceTheSetWasSaved_TellsTheGameAndTheDifference()
	{
		var draft = Draft(Played, "Storm", Build);

		var status = draft.Status(Played);

		status.Should().NotBeNull();
		status!.ChangedInGame.Should().BeTrue();
		status.Difference.SkillPoints.Should().Equal((Masteries.Storm, 6), (Masteries.Earth, 0));
		status.Difference.Attributes.Should().Be(new ClassSetAttributes(0, 0, 0, 2, 0));
	}

	[Fact]
	public void Status_EditingAfterAnOpeningChangedInTheGame_TellsTheEditor()
	{
		var draft = Draft(Played, "Storm", Build);
		var edited = Played with { Attributes = new ClassSetAttributes(1, 2, 0, 12, 0) };

		draft.Status(edited)!.ChangedInGame.Should().BeFalse("the build shown is no longer the save's");
		draft.Status(Played)!.ChangedInGame.Should().BeTrue("back to the save's build");
	}

	[Fact]
	public void Changed_OpeningOutOfStep_HasNothingToWrite()
	{
		var otherClass = Build with { Masteries = [Build.Masteries[0]] };

		var draft = Draft(otherClass, "Storm", Build);

		draft.Changed.Should().BeFalse("leaving the editor untouched asks nothing and writes nothing");
	}

	[Fact]
	public void Status_AfterPickingTheSetThenEditing_TellsTheEditor()
	{
		var draft = Draft(Played, "Storm", Build);

		draft.Select("Storm");

		draft.Status(Build).Should().BeNull("the build shown is the set's again");
		draft.Status(Played)!.ChangedInGame.Should().BeFalse();
	}

	[Fact]
	public void Save_SelectedSetTakesTheBuildShownAndTheMarkGoes()
	{
		var draft = Draft(Played, "Storm", Build);

		draft.Save(Played).Should().BeTrue();

		draft.Find("Storm")!.SameBuild(Played).Should().BeTrue();
		draft.Find("Storm")!.Name.Should().Be("Storm");
		draft.Status(Played).Should().BeNull();
		draft.Changed.Should().BeTrue();
	}

	[Fact]
	public void SaveAs_AddsTheBuildShownAndSelectsItRefusingAUsedOrEmptyName()
	{
		var draft = Draft(Played, null, Build);

		draft.SaveAs("storm", Played).Should().Be(SetNameError.AlreadyUsed);
		draft.SaveAs("  ", Played).Should().Be(SetNameError.Empty);
		draft.SaveAs(" Played ", Played).Should().Be(SetNameError.None);

		draft.Names.Should().Equal("Storm", "Played");
		draft.Selected.Should().Be("Played");
		draft.Find("Played")!.SameBuild(Played).Should().BeTrue();
	}

	[Fact]
	public void Save_NoSetSelected_IsRefused()
		=> Draft(Played, null, Build).Save(Played).Should().BeFalse();

	[Fact]
	public void Changed_AnotherSetSelected_IsWrittenOnOkEvenWithoutSetChanges()
	{
		var draft = Draft(Build, "Storm", Build, ClassSet.Empty("Empty"));

		draft.Select("Empty");

		draft.Changed.Should().BeTrue("the set selected is remembered for the next opening");
	}

	[Fact]
	public void Rename_SelectedSet_TheSelectionFollows()
	{
		var draft = Draft(Build, "Storm", Build);

		draft.Rename("Storm", "STORM").Should().Be(SetNameError.None, "a set may keep its name with another case");

		draft.Selected.Should().Be("STORM");
		draft.Names.Should().Equal("STORM");
	}

	[Fact]
	public void Delete_SelectedSet_LeavesNoneSelectedAndNoMark()
	{
		var draft = Draft(Played, "Storm", Build, ClassSet.Empty("Empty"));

		draft.Delete("Storm").Should().BeTrue();

		draft.Selected.Should().BeNull();
		draft.Status(Played).Should().BeNull();
		draft.Names.Should().Equal("Empty");
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void ReadOnlyCharacterOrUnreadFile_SetsCanBeSelectedNotChanged(bool canChange, bool readFailed)
	{
		var draft = new ClassSetDraft(new ClassSetBook("file") { Sets = [Build, ClassSet.Empty("Empty")], ReadFailed = readFailed }, canChange, Build);
		draft.Select("Storm");

		draft.SaveAs("New", Played);
		draft.Save(Played);
		draft.Rename("Storm", "Other");
		draft.Delete("Empty");

		draft.CanChange.Should().BeFalse();
		draft.Names.Should().Equal("Storm", "Empty");
		draft.Find("Storm")!.SameBuild(Build).Should().BeTrue();
		draft.Select("Empty").Should().NotBeNull();
	}
}

using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="CharacterLabels.JoinMasteryNames"/>: the masteries line of the character drop down.
/// </summary>
public class CharacterLabelsTests
{
	private readonly Mock<ITranslationService> _translations = new();

	[Fact]
	public void JoinMasteryNames_JoinsTheTranslatedNamesInThePaneOrder()
	{
		// Chosen Earth first: the drop down still lists Defense first. A mod's own mastery in the Earth pane is named
		// by its tree.
		var earth = new MasteryTree(Masteries.Earth, """records\skills\lifegiving\lifegiving_mastery.dbr""".ToRecordId(), "tagLifegiving", "", "", 40, []);
		var defense = new MasteryTree(Masteries.Defensive, """records\skills\defensive\defensivemastery.dbr""".ToRecordId(), "tagDefense", "", "", 40, []);
		this._translations.Setup(x => x.TranslateXTag("tagLifegiving", false, false, false)).Returns("Lifegiving Mastery");
		this._translations.Setup(x => x.TranslateXTag("tagDefense", false, false, false)).Returns("Defense Mastery");

		var names = CharacterLabels.JoinMasteryNames([earth, defense], this._translations.Object);

		names.Should().Be("Defense Mastery + Lifegiving Mastery");
	}

	[Fact]
	public void JoinMasteryNames_NoMastery_IsEmpty()
		=> CharacterLabels.JoinMasteryNames([], this._translations.Object).Should().BeEmpty();
}

using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemComparisonService"/>: which texts of the items it shows and which it compares.
/// </summary>
public class ItemComparisonServiceTests
{
	private const FriendlyNamesExtraScopes Classic = FriendlyNamesExtraScopes.ItemFullDisplay;
	private const FriendlyNamesExtraScopes SkillsApart = FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills;

	private readonly Mock<IItemProvider> _provider = new();
	private readonly Mock<IItemAttributeProvider> _attributes = new();
	private readonly Mock<ITranslationService> _translation = new();
	private readonly Item _first = new();
	private readonly Item _second = new();

	private void Setup(Item item, FriendlyNamesExtraScopes scopes, ToFriendlyNameResult result)
		=> this._provider.Setup(p => p.GetFriendlyNames(item, scopes, It.IsAny<bool>())).Returns(result);

	[Fact]
	public void Compare_ShowsTheClassicTexts_ComparesTheTextsWithSkillsApart()
	{
		// The classic display describes the granted skill inline: its effects must not be compared as item attributes.
		var firstClassic = new ToFriendlyNameResult(this._first) { ClassicAttributes = ["+42 Strength", "6.0 Meter Radius"] };
		var secondClassic = new ToFriendlyNameResult(this._second) { ClassicAttributes = ["+30 Strength"] };
		this.Setup(this._first, Classic, firstClassic);
		this.Setup(this._second, Classic, secondClassic);
		this.Setup(this._first, SkillsApart, new ToFriendlyNameResult(this._first)
		{
			ClassicAttributes = ["+42 Strength"],
			GrantedSkills = [new GrantedSkill(RecordId.Create("records/skills/boil.dbr"), "Blood Boil", 1, "Level: 1", null, null, [], ["6.0 Meter Radius"])],
		});
		this.Setup(this._second, SkillsApart, new ToFriendlyNameResult(this._second) { ClassicAttributes = ["+30 Strength"] });

		var comparison = this.Service().Compare(this._first, this._second);

		comparison.First.Should().BeSameAs(firstClassic);
		comparison.Second.Should().BeSameAs(secondClassic);
		comparison.Stats.FirstMinusSecond.Select(g => g.Title).Should().Equal(null, "Blood Boil");
		comparison.Stats.FirstMinusSecond[0].Lines.Select(l => l.Text).Should().Equal("+12 Strength");
	}

	[Fact]
	public void Compare_LowerIsBetter_FromTheTranslatedLabelOfEachListedAttribute()
	{
		// skillCooldownReduction -> text tag SkillCooldownReduction -> "-{%.0f0}% Recharge" in the current language.
		this._attributes.Setup(a => a.GetAttributeTextTag(It.IsAny<string>())).Returns((string name) => name);
		string label = "-{%.0f0}% Recharge";
		this._translation.Setup(t => t.TryTranslateXTag("skillCooldownReduction", out label, false, false, false)).Returns(true);
		foreach (var item in new[] { this._first, this._second })
			this.Setup(item, Classic, new ToFriendlyNameResult(item));
		this.Setup(this._first, SkillsApart, new ToFriendlyNameResult(this._first) { ClassicAttributes = ["-10% Recharge", "+10% Attack Speed"] });
		this.Setup(this._second, SkillsApart, new ToFriendlyNameResult(this._second) { ClassicAttributes = ["-20% Recharge", "+20% Attack Speed"] });

		var lines = this.Service().Compare(this._first, this._second).Stats.FirstMinusSecond.Single().Lines;

		lines.Select(l => (l.Text, l.LowerIsBetter)).Should().Equal(("+10% Recharge", true), ("-10% Attack Speed", false));
		LowerIsBetterStats.AttributeNames.Should().Contain("skillCooldownReduction");
	}

	private ItemComparisonService Service() => new(this._provider.Object, this._attributes.Object, this._translation.Object);
}

using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="Variable.TextContains"/> answers exactly as <c>ToString().Contains(text, OrdinalIgnoreCase)</c>, the search
/// of ARZExplorer, without building the text.
/// </summary>
public sealed class VariableTextContainsTests
{
	private static Variable Make(string name, VariableDataType type, params object[] values)
	{
		var variable = new Variable(name, type, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];
		return variable;
	}

	// A fresh variable for each check: ToString caches its text, which TextContains then uses.
	private static readonly Func<Variable>[] Variables =
	[
		() => Make("offensivePhysicalMin", VariableDataType.Float, 12.5f, -3f, 0f),
		() => Make("levelRequirement", VariableDataType.Integer, 36, -1, 0),
		() => Make("itemNameTag", VariableDataType.StringVar, "tagUWeapon_Lypikouri"),
		() => Make("lootNames", VariableDataType.StringVar, @"Records\Item\a.dbr", @"Records\Item\b.dbr"),
		() => Make("weird", VariableDataType.Float, float.NaN, float.PositiveInfinity),
		() => Make("bitmap", VariableDataType.StringVar, @"Items\x.tex"),
	];

	public static TheoryData<int, string> Cases()
	{
		string[] texts =
		[
			"physical", "PHYSICALMIN", "12.5", "12.500000", "-3", "3.0", "0.0", ".", "-", "36", "-1", "6,", "min,",
			",12", "0;-3", "lypikouri", "tagu", @"item\a", "a.dbr;rec", ".dbr,", "nan", "infinity", "∞", "x.tex", "z",
			"levelrequirement,36", "0,", "requirement",
		];
		var data = new TheoryData<int, string>();
		for (int v = 0; v < Variables.Length; v++)
			foreach (var text in texts)
				data.Add(v, text);
		return data;
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void AnswersAsToStringContains(int variable, string text)
	{
		var expected = Variables[variable]().ToString().Contains(text, StringComparison.OrdinalIgnoreCase);

		Variables[variable]().TextContains(text).Should().Be(expected);
	}
}

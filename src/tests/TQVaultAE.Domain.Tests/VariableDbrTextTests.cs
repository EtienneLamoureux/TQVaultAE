using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests;

/// <summary>
/// <see cref="Variable.ToString"/> writes the same <c>.dbr</c> line as tqdb (<see cref="DbrText"/>), so the two can share it.
/// </summary>
public sealed class VariableDbrTextTests
{
	[Theory]
	[InlineData(VariableDataType.Float)]
	[InlineData(VariableDataType.Integer)]
	[InlineData(VariableDataType.StringVar)]
	public void ToString_IsTheDbrTextLine(VariableDataType type)
	{
		object[] values = type switch
		{
			VariableDataType.Float => [12.5f, 0.1f],
			VariableDataType.Integer => [25, -3],
			_ => ["tagSword", """records\item\a.dbr"""],
		};
		var variable = new Variable("name", type, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];

		variable.ToString().Should().Be(DbrText.Line("name", values));
	}
}

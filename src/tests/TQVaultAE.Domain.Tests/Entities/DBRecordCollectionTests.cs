using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Unit tests for DBRecordCollection entity
/// </summary>
public class DBRecordCollectionTests
{
	[Fact]
	public void DBRecordCollection_Set_AddsVariable()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var record = new DBRecordCollection(recordId, "item");
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 42;

		// Act
		record.Set(variable);

		// Assert
		record.Count.Should().Be(1);
	}

	[Fact]
	public void DBRecordCollection_Indexer_ReturnsVariable()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var record = new DBRecordCollection(recordId, "item");
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		variable[0] = 42;
		record.Set(variable);

		// Act
		var result = record["testVar"];

		// Assert
		result.Should().NotBeNull();
		result.Should().Be(variable);
	}

	[Fact]
	public void DBRecordCollection_Indexer_CaseInsensitive()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var record = new DBRecordCollection(recordId, "item");
		var variable = new Variable("testVar", VariableDataType.Integer, 1);
		record.Set(variable);

		// Act - access with different case
		var result = record["TESTVAR"];

		// Assert
		result.Should().BeSameAs(variable);
	}

	[Fact]
	public void DBRecordCollection_Indexer_MissingVariable_ReturnsNull()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var record = new DBRecordCollection(recordId, "item");

		// Act
		var result = record["nonexistent"];

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void DBRecordCollection_GetEnumerator_IteratesVariables()
	{
		// Arrange
		var recordId = RecordId.Create("test/record");
		var record = new DBRecordCollection(recordId, "item");
		var var1 = new Variable("var1", VariableDataType.Integer, 1);
		var var2 = new Variable("var2", VariableDataType.Integer, 1);
		record.Set(var1);
		record.Set(var2);

		// Act
		var count = 0;
		foreach (var v in record)
			count++;

		// Assert
		count.Should().Be(2);
	}

	[Fact]
	public void GroupNumberedVariables_GroupsByNumberInFirstAppearanceOrder_AndSkipsRejectedVariables()
	{
		var record = new DBRecordCollection(RecordId.Create("test/table"), "table");
		Variable Add(string name, int value)
		{
			var variable = new Variable(name, VariableDataType.Integer, 1) { [0] = value };
			record.Set(variable);
			return variable;
		}
		var weight2 = Add("randomizerWeight2", 5);
		var name1 = Add("RandomizerName1", 1);
		Add("randomizerName2", 0);
		var weight1 = Add("randomizerWeight1", 7);
		Add("otherVariable3", 1);

		var groups = record.GroupNumberedVariables(v => v.GetInt32(0) != 0, "randomizerName", "randomizerWeight");

		groups.Keys.Should().Equal("2", "1");
		groups["2"].Should().Equal(null, weight2);
		groups["1"].Should().Equal(name1, weight1);
	}
}

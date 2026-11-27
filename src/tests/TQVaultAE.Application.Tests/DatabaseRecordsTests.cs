using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Tests.Application;

public class DatabaseRecordsTests
{
	private static ArzFile Arz(string name, params (string Id, string Class)[] records)
	{
		var arz = new ArzFile(name);
		foreach (var (id, recordClass) in records)
			arz.RecordInfo[id.ToRecordId()] = new ArzRecordInfo(0, id, recordClass, 0, 0, default);

		return arz;
	}

	[Fact]
	public void RecordsOfClass_ListsEachDatabaseByPriority_IgnoringCase_AndImmortalThroneOnceWhenItIsTheBaseFile()
	{
		var baseGame = Arz("base", (@"records\a.dbr", "LootRandomizer"), (@"records\b.dbr", "Skill_Buff"));
		var mod = Arz("mod", (@"records\a.dbr", "lootrandomizer"), (@"records\c.dbr", "LootRandomizer"));
		var database = new Mock<IDatabase>();
		database.Setup(d => d.ArzFile).Returns(baseGame);
		database.Setup(d => d.ArzFileIT).Returns(baseGame);
		database.Setup(d => d.ArzFileMod).Returns(mod);

		var records = database.Object.RecordsOfClass("LootRandomizer").ToList();

		records.Select(r => (r.Id.Raw, r.Priority)).Should().Equal(
			(@"records\a.dbr", 0), (@"records\c.dbr", 0), (@"records\a.dbr", 2));
	}
}

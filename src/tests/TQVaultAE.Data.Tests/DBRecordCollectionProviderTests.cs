using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Unit tests for DBRecordCollectionProvider class
/// </summary>
public class DBRecordCollectionProviderTests
{
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IFileIO> _mockFileIO;
	private readonly Mock<ILogger<DBRecordCollectionProvider>> _mockLogger;
	private readonly DBRecordCollectionProvider _provider;

	public DBRecordCollectionProviderTests()
	{
		_mockLogger = new Mock<ILogger<DBRecordCollectionProvider>>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockPathIO = new Mock<IPathIO>();
		_mockFileIO = new Mock<IFileIO>();

		_provider = new DBRecordCollectionProvider(
			_mockDirectoryIO.Object,
			_mockPathIO.Object,
			_mockFileIO.Object);
	}

	private DBRecordCollection CreateTestDBRecordCollection(string id)
	{
		var recordId = RecordId.Create(id);
		var drc = new DBRecordCollection(recordId, "testRecordType");
		
		var var1 = new Variable("var1", VariableDataType.StringVar, 1);
		var1[0] = "value1";
		drc.Set(var1);
		
		var var2 = new Variable("var2", VariableDataType.Integer, 1);
		var2[0] = 42;
		drc.Set(var2);
		
		return drc;
	}

	#region Write Tests

	[Fact]
	public void Write_WithoutFilename_CreatesPathFromNormalizedId()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockPathIO
			.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns("/test/base/records");

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		// Act
		_provider.Write(drc, baseFolder);

		// Assert
		_mockPathIO.Verify(
			p => p.Combine(baseFolder, drc.Id.Normalized),
			Times.Once);

		_mockPathIO.Verify(
			p => p.GetDirectoryName(It.IsAny<string>()),
			Times.Once);

		_mockDirectoryIO.Verify(
			d => d.Exists("/test/base/records"),
			Times.Once);

		_mockDirectoryIO.Verify(
			d => d.CreateDirectory(It.IsAny<string>()),
			Times.Never);

		_mockFileIO.Verify(
			f => f.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
			Times.Once);
	}

	[Fact]
	public void Write_WithoutFilename_DirectoryDoesNotExist_CreatesDirectory()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";
		var expectedFolder = "/test/base/records";

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockPathIO
			.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns(expectedFolder);

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(false);

		// Act
		_provider.Write(drc, baseFolder);

		// Assert
		_mockDirectoryIO.Verify(
			d => d.Exists(expectedFolder),
			Times.Once);

		_mockDirectoryIO.Verify(
			d => d.CreateDirectory(expectedFolder),
			Times.Once);
	}

	[Fact]
	public void Write_WithFilename_UsesFilenameInsteadOfNormalizedId()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";
		var fileName = "customFile.txt";

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		// Act
		_provider.Write(drc, baseFolder, fileName);

		// Assert - should call Combine twice (first with normalized id, then with filename)
		_mockPathIO.Verify(
			p => p.Combine(baseFolder, It.IsAny<string>()),
			Times.Exactly(2));

		// The second Combine call should use filename
		_mockPathIO.Verify(
			p => p.Combine(baseFolder, fileName),
			Times.Once);

		// destinationFolder should be baseFolder when filename is provided
		_mockDirectoryIO.Verify(
			d => d.Exists(baseFolder),
			Times.Once);

		_mockFileIO.Verify(
			f => f.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
			Times.Once);
	}

	[Fact]
	public void Write_WithFilename_DirectoryDoesNotExist_CreatesDirectory()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";
		var fileName = "customFile.txt";

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(false);

		// Act
		_provider.Write(drc, baseFolder, fileName);

		// Assert - should create baseFolder since that's the destination
		_mockDirectoryIO.Verify(
			d => d.CreateDirectory(baseFolder),
			Times.Once);
	}

	[Fact]
	public void Write_WritesAllVariablesToFile()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";

		string? capturedPath = null;
		IEnumerable<string>? capturedContent = null;

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockPathIO
			.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns("/test/base/records");

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		_mockFileIO
			.Setup(f => f.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
			.Callback<string, IEnumerable<string>>((path, content) =>
			{
				capturedPath = path;
				capturedContent = content;
			});

		// Act
		_provider.Write(drc, baseFolder);

		// Assert
		capturedPath.Should().NotBeNull();
		capturedPath.Should().Contain("test");
		capturedContent.Should().NotBeNull();
		capturedContent.Should().HaveCount(2); // Two variables: var1 and var2
	}

	[Fact]
	public void Write_WithNullFileName_DoesNotCallCombineTwice()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";

		int combineCallCount = 0;
		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Callback(() => combineCallCount++)
			.Returns((string[] paths) => string.Join("/", paths));

		_mockPathIO
			.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns("/test/base/records");

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		// Act
		_provider.Write(drc, baseFolder, null);

		// Assert - Combine should be called only once when filename is null
		combineCallCount.Should().Be(1);
	}

	[Fact]
	public void Write_WithNonNullFileName_CallsCombineTwice()
	{
		// Arrange
		var drc = CreateTestDBRecordCollection("records/test");
		var baseFolder = "/test/base";
		var fileName = "custom.txt";

		int combineCallCount = 0;
		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Callback(() => combineCallCount++)
			.Returns((string[] paths) => string.Join("/", paths));

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		// Act
		_provider.Write(drc, baseFolder, fileName);

		// Assert - Combine should be called twice when filename is provided
		combineCallCount.Should().Be(2);
	}

	[Fact]
	public void Write_EmptyRecordCollection_WritesEmptyFile()
	{
		// Arrange
		var recordId = RecordId.Create("records/empty");
		var drc = new DBRecordCollection(recordId, "testRecordType");
		var baseFolder = "/test/base";

		_mockPathIO
			.Setup(p => p.Combine(It.IsAny<string[]>()))
			.Returns((string[] paths) => string.Join("/", paths));

		_mockPathIO
			.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns("/test/base/records");

		_mockDirectoryIO
			.Setup(d => d.Exists(It.IsAny<string>()))
			.Returns(true);

		// Act
		_provider.Write(drc, baseFolder);

		// Assert
		_mockFileIO.Verify(
			f => f.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
			Times.Once);

		// Verify empty list was passed
		_mockFileIO.Verify(
			f => f.WriteAllLines(
				It.IsAny<string>(),
				It.Is<IEnumerable<string>>(e => !e.Any())),
			Times.Once);
	}

	#endregion
}

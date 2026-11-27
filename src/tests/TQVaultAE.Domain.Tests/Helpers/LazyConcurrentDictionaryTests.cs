using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Helpers;

public class LazyConcurrentDictionaryTests
{
	[Fact]
	public void GetOrAddAtomic_WithNewKey_ReturnsValueAndCreatesLazy()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		var callCount = 0;

		// Act
		var result = dict.GetOrAddAtomic("key1", k =>
		{
			callCount++;
			return 42;
		});

		// Assert
		result.Should().Be(42);
		callCount.Should().Be(1);
		dict.Should().HaveCount(1);
	}

	[Fact]
	public void GetOrAddAtomic_WithExistingKey_DoesNotRecreateValue()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		var callCount = 0;

		dict.GetOrAddAtomic("key1", k =>
		{
			callCount++;
			return 42;
		});

		// Act
		var result = dict.GetOrAddAtomic("key1", k =>
		{
			callCount++;
			return 100;
		});

		// Assert
		result.Should().Be(42); // Original value
		callCount.Should().Be(1); // Factory was only called once
		dict.Should().HaveCount(1);
	}

	[Fact]
	public void GetOrAddAtomic_IncrementsVersion_WhenNewKeyAdded()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.Version.Should().Be(0);

		// Act
		dict.GetOrAddAtomic("key1", k => 42);

		// Assert
		// Version increments because Value was accessed on the newly created Lazy
		dict.Version.Should().Be(1);
	}

	[Fact]
	public void GetOrAddAtomic_DoesNotIncrementVersion_WhenKeyExists()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.GetOrAddAtomic("key1", k => 42);
		// Version is 1 after first add

		// Act - Second call with same key
		dict.GetOrAddAtomic("key1", k => 100);

		// Assert - Version should NOT increment when key already exists
		// Getting an existing value does not change the version
		dict.Version.Should().Be(1);
	}

	[Fact]
	public void AddOrUpdateAtomic_WithNewKey_AddsValue()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();

		// Act
		var result = dict.AddOrUpdateAtomic("key1", 42);

		// Assert
		result.Should().Be(42);
		dict.Should().HaveCount(1);
		dict.Version.Should().Be(1);
	}

	[Fact]
	public void AddOrUpdateAtomic_WithExistingKey_UpdatesValue()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 42);

		// Act
		var result = dict.AddOrUpdateAtomic("key1", 100);

		// Assert
		result.Should().Be(100);
		dict.Should().HaveCount(1);
		dict.Version.Should().Be(2);
	}

	[Fact]
	public void AddOrUpdateAtomic_WithUpdateFactory_UpdatesUsingFactory()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 42);

		// Act
		var result = dict.AddOrUpdateAtomic("key1", 0, (k, oldValue) => oldValue + 10);

		// Assert
		result.Should().Be(52);
		dict.Should().HaveCount(1);
	}

	[Fact]
	public void AddOrUpdateAtomic_WithAddFactory_AddsUsingFactory()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		var callCount = 0;

		// Act
		var result = dict.AddOrUpdateAtomic("key1", 
			k => { callCount++; return 42; }, 
			(k, oldValue) => oldValue + 10);

		// Assert
		result.Should().Be(42);
		callCount.Should().Be(1);
	}

	[Fact]
	public void AddOrUpdateAtomic_WithExistingKey_UsesUpdateFactory()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 42);
		var callCount = 0;

		// Act
		var result = dict.AddOrUpdateAtomic("key1", 
			k => { callCount++; return 0; }, 
			(k, oldValue) => { callCount++; return oldValue + 10; });

		// Assert
		result.Should().Be(52);
		callCount.Should().Be(1); // Only update factory was called
	}

	[Fact]
	public void Clear_RemovesAllItemsAndIncrementsVersion()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 1);
		dict.AddOrUpdateAtomic("key2", 2);
		dict.Version.Should().Be(2);

		// Act
		dict.Clear();

		// Assert
		dict.Should().BeEmpty();
		dict.Version.Should().Be(3);
	}

	[Fact]
	public void TryRemove_WithExistingKey_ReturnsTrueAndRemoves()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 42);

		// Act
		var removed = dict.TryRemove("key1", out var value);

		// Assert
		removed.Should().BeTrue();
		value.Should().Be(42);
		dict.Should().BeEmpty();
		dict.Version.Should().Be(2);
	}

	[Fact]
	public void TryRemove_WithNonExistingKey_ReturnsFalse()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();

		// Act
		var removed = dict.TryRemove("nonexistent", out var value);

		// Assert
		removed.Should().BeFalse();
		dict.Should().BeEmpty();
	}

	[Fact]
	public void TryRemove_IncrementsVersion()
	{
		// Arrange
		var dict = new LazyConcurrentDictionary<string, int>();
		dict.AddOrUpdateAtomic("key1", 42);
		dict.AddOrUpdateAtomic("key2", 43);
		var versionBeforeRemove = dict.Version;

		// Act
		dict.TryRemove("key1", out _);

		// Assert
		dict.Version.Should().BeGreaterThan(versionBeforeRemove);
	}
}
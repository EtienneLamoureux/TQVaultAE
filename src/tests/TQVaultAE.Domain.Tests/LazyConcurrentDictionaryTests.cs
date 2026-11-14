using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="LazyConcurrentDictionary{TKey, TValue}"/>: the session's caches of loaded characters, vaults and stashes.
/// </summary>
public class LazyConcurrentDictionaryTests
{
	[Fact]
	public void GetOrAddAtomic_FactoryThrows_DoesNotKeepTheFailure()
	{
		// Arrange: a file that cannot be read once
		var cache = new LazyConcurrentDictionary<string, string>();
		int calls = 0;
		string Load(string key) => ++calls == 1 ? throw new InvalidOperationException("unreadable") : "loaded";

		// Act
		var first = () => cache.GetOrAddAtomic("vault", Load);

		// Assert: the failure is not cached, so the readers of the whole cache (the files to save) do not meet it,
		// and the next load tries again
		first.Should().Throw<InvalidOperationException>();
		cache.Should().BeEmpty();
		cache.GetOrAddAtomic("vault", Load).Should().Be("loaded");
	}

	[Fact]
	public void GetOrAddAtomic_SecondCall_ReturnsTheFirstValue()
	{
		var cache = new LazyConcurrentDictionary<string, string>();

		cache.GetOrAddAtomic("vault", _ => "first");

		cache.GetOrAddAtomic("vault", _ => "second").Should().Be("first");
	}

	[Fact]
	public void LoadedEntries_LeavesOutAndDoesNotReadTheEntriesNotReadYet()
	{
		// Arrange: one file read, one added but never read
		var cache = new LazyConcurrentDictionary<string, string>();
		cache.GetOrAddAtomic("read", _ => "value");
		bool pendingRead = false;
		cache.TryAdd("pending", new Lazy<string>(() => { pendingRead = true; return "late"; }));

		// Act
		var loaded = cache.LoadedEntries().ToList();

		// Assert: listing the files to save must not load the others
		loaded.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("read", "value"));
		pendingRead.Should().BeFalse();
	}

	[Fact]
	public void TryGetLoaded_EntryNotReadYet_ReturnsFalseWithoutReadingIt()
	{
		var cache = new LazyConcurrentDictionary<string, string>();
		bool read = false;
		cache.TryAdd("pending", new Lazy<string>(() => { read = true; return "late"; }));
		cache.GetOrAddAtomic("read", _ => "value");

		cache.TryGetLoaded("pending", out _).Should().BeFalse();
		read.Should().BeFalse();
		cache.TryGetLoaded("read", out var value).Should().BeTrue();
		value.Should().Be("value");
		cache.TryGetLoaded("missing", out _).Should().BeFalse();
	}
}

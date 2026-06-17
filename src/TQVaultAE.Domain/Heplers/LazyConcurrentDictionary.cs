using System.Collections.Concurrent;
using System.Threading;

namespace TQVaultAE.Domain.Helpers;

// From : https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/
public class LazyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, Lazy<TValue>>
{
	private volatile int _version;

	/// <summary>
	/// Gets the current version number that increments on any modification.
	/// </summary>
	public int Version => _version;

	private void IncrementVersion() => Interlocked.Increment(ref _version);

	public TValue GetOrAddAtomic(TKey key, Func<TKey, TValue> valueFactory)
	{
		var keyExisted = this.ContainsKey(key);
		var lazyResult = this.GetOrAdd(key
			, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)
		);
		// Only increment version when a new key was added (not when retrieving existing)
		if (!keyExisted)
			IncrementVersion();
		return lazyResult.Value;
	}

	public TValue AddOrUpdateAtomic(TKey key, TValue addValue)
	{
		var lazyResult = this.AddOrUpdate(key
			, new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
			, (k, oldValue) => new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
		);
		IncrementVersion();
		return lazyResult.Value;
	}

	public TValue AddOrUpdateAtomic(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
	{
		var lazyResult = this.AddOrUpdate(key
			, new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
			, (k, oldValue) => new Lazy<TValue>(() => updateValueFactory(k, oldValue.Value), LazyThreadSafetyMode.ExecutionAndPublication)
		);
		IncrementVersion();
		return lazyResult.Value;
	}

	public TValue AddOrUpdateAtomic(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
	{
		var lazyResult = this.AddOrUpdate(key
			, k => new Lazy<TValue>(() => addValueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)
			, (k, oldValue) => new Lazy<TValue>(() => updateValueFactory(k, oldValue.Value), LazyThreadSafetyMode.ExecutionAndPublication)
		);
		IncrementVersion();
		return lazyResult.Value;
	}

	public new void Clear()
	{
		base.Clear();
		IncrementVersion();
	}

	public new bool TryRemove(TKey key, out TValue value)
	{
		// For reference types: use base.TryRemove and extract value from Lazy
		// For value types: the Lazy wrapper behavior is complex
		// Fall back to removing and hoping value was created
		if (base.TryRemove(key, out var lazyValue))
		{
			try
			{
				value = lazyValue!.Value;
			}
			catch
			{
				// Value wasn't created yet - can't retrieve the original value
				// Return default for value types, or we can't help
				value = default!;
			}
			IncrementVersion();
			return true;
		}
		value = default!;
		return false;
	}
}
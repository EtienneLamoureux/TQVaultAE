using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers
{
	// From : https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/
	public class LazyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, Lazy<TValue>>
	{
		public TValue GetOrAddAtomic(TKey key, Func<TKey, TValue> valueFactory)
		{
			var lazyResult = this.GetOrAdd(key
				, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)
			);
			return lazyResult.Value;
		}

		public TValue AddOrUpdateAtomic(TKey key, TValue addValue)
		{
			var lazyResult = this.AddOrUpdate(key
				, new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
				, (k, oldValue) => new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
			);
			return lazyResult.Value;
		}

		public TValue AddOrUpdateAtomic(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
		{
			var lazyResult = this.AddOrUpdate(key
				, new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication)
				, (k, oldValue) => new Lazy<TValue>(() => updateValueFactory(k, oldValue.Value), LazyThreadSafetyMode.ExecutionAndPublication)
			);
			return lazyResult.Value;
		}
	}
}

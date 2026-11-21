using System.Drawing;

namespace TQVaultAE.Presentation.Win32;

/// <summary>What every <see cref="TooltipImageCache{TKey, TPayload}"/> shares, whatever its key.</summary>
public static class TooltipImageCache
{
	private static int s_generation;

	/// <summary>Raised by <see cref="InvalidateAll"/>: a cache drops its images on its next use.</summary>
	internal static int Generation => Volatile.Read(ref s_generation);

	/// <summary>
	/// Drops the images of every cache, on their next use: what they were rendered with changed for all of them (the base
	/// font), which their keys do not hold.
	/// </summary>
	public static void InvalidateAll() => Interlocked.Increment(ref s_generation);
}

/// <summary>
/// Rendered tooltip images, kept within a memory budget: the least recently used images are disposed first.
/// </summary>
/// <remarks>
/// The cache owns its bitmaps and disposes them when they are evicted, invalidated or replaced.
/// Callers that display an image must use a copy (<see cref="TryGetCopy"/>), because the cached bitmap may be
/// disposed while their control still paints it. Thread-safe.
/// </remarks>
/// <typeparam name="TKey">What the image was rendered for (item, sack, search results, plus scale and view options).</typeparam>
/// <typeparam name="TPayload">Data kept alongside the image (e.g. the tooltip text), or <see cref="object"/> when none.</typeparam>
public sealed class TooltipImageCache<TKey, TPayload> : IDisposable where TKey : notnull
{
	private sealed record Entry(TKey Key, Bitmap Image, TPayload Payload, long Bytes);

	private readonly object _lock = new();
	private readonly Dictionary<TKey, LinkedListNode<Entry>> _byKey = [];
	/// <summary>Most recently used first.</summary>
	private readonly LinkedList<Entry> _usage = new();

	/// <summary>The <see cref="TooltipImageCache.Generation"/> the images held were rendered in.</summary>
	private int _generation = TooltipImageCache.Generation;

	/// <param name="maxBytes">Memory budget for the cached pixels (32 bits per pixel).</param>
	public TooltipImageCache(long maxBytes)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBytes);
		this.MaxBytes = maxBytes;
	}

	public long MaxBytes { get; }

	/// <summary>Bytes of pixels currently held.</summary>
	public long Bytes { get; private set; }

	public int Count
	{
		get { lock (this._lock) return this._byKey.Count; }
	}

	/// <summary>Bytes of pixels of a bitmap as rendered by the tooltips (32 bits per pixel).</summary>
	private static long SizeOf(Bitmap image) => (long)image.Width * image.Height * 4;

	/// <summary>
	/// Stores <paramref name="image"/> for <paramref name="key"/>; the cache takes ownership of it.
	/// An image larger than the whole budget is disposed and not kept.
	/// </summary>
	public void Add(TKey key, Bitmap image, TPayload payload)
	{
		ArgumentNullException.ThrowIfNull(image);
		long bytes = SizeOf(image);
		lock (this._lock)
		{
			this.ClearIfStaleLocked();
			this.RemoveLocked(key);
			if (bytes > this.MaxBytes)
			{
				image.Dispose();
				return;
			}

			this._byKey[key] = this._usage.AddFirst(new Entry(key, image, payload, bytes));
			this.Bytes += bytes;

			while (this.Bytes > this.MaxBytes && this._usage.Last is { } oldest)
				this.RemoveLocked(oldest.Value.Key);
		}
	}

	/// <summary>
	/// Gets a copy of the image for <paramref name="key"/>, owned by the caller, and marks the entry as recently used.
	/// </summary>
	public bool TryGetCopy(TKey key, out Bitmap image, out TPayload payload)
	{
		lock (this._lock)
		{
			this.ClearIfStaleLocked();
			if (this._byKey.TryGetValue(key, out var node))
			{
				this._usage.Remove(node);
				this._usage.AddFirst(node);
				image = new Bitmap(node.Value.Image);
				payload = node.Value.Payload;
				return true;
			}
		}

		image = null!;
		payload = default!;
		return false;
	}

	/// <summary>Disposes every image whose key matches <paramref name="predicate"/> (content changed).</summary>
	/// <returns>The number of images removed.</returns>
	public int RemoveWhere(Func<TKey, bool> predicate)
	{
		lock (this._lock)
		{
			var keys = this._byKey.Keys.Where(predicate).ToList();
			foreach (var key in keys)
				this.RemoveLocked(key);
			return keys.Count;
		}
	}

	public void Clear()
	{
		lock (this._lock)
			this.ClearLocked();
	}

	private void ClearLocked()
	{
		foreach (var entry in this._usage)
			entry.Image.Dispose();
		this._usage.Clear();
		this._byKey.Clear();
		this.Bytes = 0;
	}

	private void ClearIfStaleLocked()
	{
		int generation = TooltipImageCache.Generation;
		if (generation == this._generation)
			return;

		this.ClearLocked();
		this._generation = generation;
	}

	public void Dispose() => this.Clear();

	private void RemoveLocked(TKey key)
	{
		if (!this._byKey.Remove(key, out var node))
			return;
		this._usage.Remove(node);
		this.Bytes -= node.Value.Bytes;
		node.Value.Image.Dispose();
	}
}

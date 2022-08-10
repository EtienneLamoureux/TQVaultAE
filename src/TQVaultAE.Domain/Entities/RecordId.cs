using TQVaultAE.Domain.Helpers;
using System;

namespace TQVaultAE.Domain.Entities;

public class RecordId : IEquatable<RecordId>, IComparable, IComparable<RecordId>
{
	public static readonly RecordId Empty = Create(string.Empty);

	public readonly string Raw;
	public readonly string Normalized;

	private RecordId(string rawRecordId)
	{
		rawRecordId ??= string.Empty;
		rawRecordId = rawRecordId.Trim();

		this.Raw = rawRecordId;
		this.Normalized = rawRecordId;

		if (this.Raw != string.Empty)
			this.Normalized = rawRecordId.NormalizeRecordPath();
	}

	/// <summary>
	/// Factory
	/// </summary>
	/// <param name="rawRecordId"></param>
	/// <returns></returns>
	public static RecordId Create(string rawRecordId) => new RecordId(rawRecordId);

	public bool IsEmpty => this.Raw == string.Empty;
	public static bool IsNullOrEmpty(RecordId Id) => Id is null || Id.IsEmpty;

	string _PrettyFileName;
	public string PrettyFileName
	{
		get
		{
			if (IsEmpty) return string.Empty;

			if (_PrettyFileName is null) _PrettyFileName = this.Raw.PrettyFileName();
			return _PrettyFileName;
		}
	}

	(string PrettyFileName, string Effect, string Number, bool IsMatch) _PrettyFileNameExploded = default!;
	public (string PrettyFileName, string Effect, string Number, bool IsMatch) PrettyFileNameExploded
	{
		get
		{
			if (!IsEmpty && _PrettyFileNameExploded.PrettyFileName is null)
				_PrettyFileNameExploded = PrettyFileName.ExplodePrettyFileName();

			return _PrettyFileNameExploded;
		}
	}

	public override string ToString() => this.Normalized;

	#region Equal over Normalized only

	public bool Equals(RecordId other)
	{
		if (other is null) return false;
		return Normalized.Equals(other.Normalized);
	}

	public override bool Equals(object obj) => Equals(obj as RecordId);

	public static bool operator ==(RecordId lhs, RecordId rhs) => object.Equals(lhs, rhs);

	public static bool operator !=(RecordId lhs, RecordId rhs) => !(lhs == rhs);

	public override int GetHashCode() => Normalized.GetHashCode();

	#endregion

	#region Compare over Normalized Only

	public int CompareTo(RecordId other)
	{
		if (other is null)
			return 1;

		return string.Compare(this.Normalized, other.Normalized, StringComparison.Ordinal);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
			return 1;

		RecordId other = obj as RecordId; // avoid double casting
		if (other == null)
			throw new ArgumentException($"A {nameof(RecordId)} object is required for comparison.", nameof(obj));

		return CompareTo(other);
	}

	public static bool operator <(RecordId left, RecordId right)
	{
		return Compare(left, right) < 0;
	}

	public static bool operator >(RecordId left, RecordId right)
	{
		return Compare(left, right) > 0;
	}

	public static int Compare(RecordId left, RecordId right)
	{
		if (object.ReferenceEquals(left, right))
			return 0;

		if (left is null)
			return -1;

		return left.CompareTo(right);
	}

	#endregion

}

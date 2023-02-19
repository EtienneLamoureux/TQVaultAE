using System;

namespace TQVaultAE.Domain.Entities;

public partial class RecordId
{
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

	#region implicit

	public static implicit operator string(RecordId recordId) => recordId.Raw;
	public static implicit operator RecordId(string recordId) => Create(recordId);

	#endregion
}

using EnumsNET;
using System.Linq;

namespace TQ.SaveFilesExplorer.Entities;

/// <summary>
/// A record whose known keys are the members of <typeparamref name="TKey"/>: their <see cref="TQFileDataTypeAttribute"/>
/// give the data type of each key per game version.
/// </summary>
public abstract class TQFileKeyedRecord<TKey> : TQFileRecord
	where TKey : struct, Enum
{
	private sealed record KeyDescriptor(TKey Enum, string Name, TQFileDataType DataType, TQVersion Version);

	// One descriptor per key, data type and detailed version.
	private static readonly KeyDescriptor[] Descriptors = (
		from descriptor in (
			from e in Enums.GetMembers<TKey>()
				.SelectMany(
					k => k.Attributes.GetAll<TQFileDataTypeAttribute>()
					, (Key, Attr) => new { Key, Attr.Version, Attr.DataType })
			from v in new TQVersion[] { TQVersion.TQ, TQVersion.TQIT, TQVersion.TQAE }
			where e.Version.HasFlag(v)
			select new KeyDescriptor(e.Key.Value, e.Key.AsString(EnumFormat.Description, EnumFormat.Name) ?? e.Key.Name, e.DataType, v)
		)
		group descriptor by new { descriptor.DataType, descriptor.Enum, descriptor.Version } into grp
		select grp.First()
	).ToArray();

	public TKey KeyAsEnum { get; set; }

	public override void DefineDataType()
	{
		// Find the corresponding datatype according to the enum & version
		var found = Descriptors.FirstOrDefault(d => d.Name == this.KeyName && d.Version == this.AttachedFile.Version);
		if (found is not null)
		{
			this.DataType = found.DataType;
			this.KeyAsEnum = found.Enum;
		}
	}

	/// <summary>
	/// Try to read value from the file
	/// </summary>
	public override void ReadValue()
	{
		this.DefineDataType();
		base.ReadValue();
	}
}

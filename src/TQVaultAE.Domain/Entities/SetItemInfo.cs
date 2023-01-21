using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TQVaultAE.Domain.Entities;

public class SetItemInfo
{
	/// <summary>
	/// Id of the Set
	/// </summary>
	public readonly string itemSetName;

	/// <summary>
	/// Records of this set
	/// </summary>
	public readonly DBRecordCollection setRecords;

	/// <summary>
	/// Id of the Set Name
	/// </summary>
	public readonly string setName;

	/// <summary>
	/// Id list of the set items
	/// </summary>
	public readonly ReadOnlyDictionary<string, Info> setMembers;

	/// <summary>
	/// Translations
	/// </summary>
	public readonly Dictionary<string, string> Translations = new();

	public SetItemInfo(string itemSetName, string setName, Dictionary<string, Info> setMembers, DBRecordCollection setRecords)
	{
		this.itemSetName = itemSetName;
		this.setName = setName;
		this.setRecords = setRecords;
		this.setMembers = new ReadOnlyDictionary<string, Info>(setMembers);
	}
}

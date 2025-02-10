using TQVaultAE.Domain.Helpers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TQVaultAE.Domain.Entities;

public partial class RecordId : IEquatable<RecordId>, IComparable, IComparable<RecordId>
{
	public readonly string Raw;
	public readonly string Normalized;

	#region Ctor

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

	#endregion

	public override string ToString() => this.Normalized;

	#region Dlc

	GameDlc? _Dlc;
	/// <summary>
	/// Resolve the Dlc that <see cref="RecordId"/> belongs to.
	/// </summary>
	public GameDlc Dlc
	{
		get
		{
			if (_Dlc is null)
				_Dlc = this.Normalized switch
				{
					var x when x.Contains(@"\XPACK4\") || this.IsHardCoreDungeonEE => GameDlc.EternalEmbers,
					var x when x.Contains(@"\XPACK3\") => GameDlc.Atlantis,
					var x when x.Contains(@"\XPACK2\") => GameDlc.Ragnarok,
					var x when x.Contains(@"\XPACK\") => GameDlc.ImmortalThrone,
					_ => GameDlc.TitanQuest
				};

			return _Dlc.Value;
		}
	}

	#endregion

	#region IsOld

	bool? _IsOld;
	/// <summary>
	/// This <see cref="RecordId"/> leads to old, obsolete content.
	/// </summary>
	public bool IsOld
	{
		get
		{
			if (_IsOld is null)
				_IsOld = this.Normalized.Contains(@"\OLD\");
			return _IsOld.Value;
		}
	}

	#endregion

	#region Empty

	public static readonly RecordId Empty = Create(string.Empty);

	public bool IsEmpty => this.Raw == string.Empty;
	public static bool IsNullOrEmpty(RecordId Id) => Id is null || Id.IsEmpty;

	#endregion

	#region PrettyFileName

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

	#endregion

	#region Tokens

	ReadOnlyCollection<string> _Tokens;
	public ReadOnlyCollection<string> TokensRaw
	{
		get
		{
			if (_Tokens is null)
				_Tokens = this.Raw.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();

			return _Tokens;
		}
	}
	
	ReadOnlyCollection<string> _TokensNormalized;
	public ReadOnlyCollection<string> TokensNormalized
	{
		get
		{
			if (_TokensNormalized is null)
				_TokensNormalized = this.TokensRaw.Select(t => t.ToUpper()).ToList().AsReadOnly();

			return _TokensNormalized;
		}
	}

	#endregion

}

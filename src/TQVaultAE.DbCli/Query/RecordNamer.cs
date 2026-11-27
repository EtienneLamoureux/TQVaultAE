//-----------------------------------------------------------------------
// <copyright file="RecordNamer.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>Name and text resolution used by filters and writers (abstracted so tests can fake it).</summary>
internal interface IRecordNamer
{
	/// <summary>Name tag of a record (e.g. <c>tagItemHookedBlade</c>), <see langword="null"/> if none.</summary>
	string? NameTag(ArzRecord record);

	/// <summary>Resolved display name, <see langword="null"/> if none.</summary>
	string? DisplayName(ArzRecord record);

	/// <summary>Resolves a localization tag as plain text (game format codes stripped, line breaks as \n), <see langword="null"/> when unknown.</summary>
	string? ResolveTag(string? tag);
}

/// <summary>Namer backed by the Core <see cref="DisplayNames"/> helpers and a text database.</summary>
internal sealed class CoreRecordNamer(ITextDatabase text) : IRecordNamer
{
	private readonly ITextDatabase _text = text;

	/// <inheritdoc/>
	public string? NameTag(ArzRecord record) => DisplayNames.NameTag(record);

	/// <inheritdoc/>
	public string? DisplayName(ArzRecord record) => DisplayNames.Resolve(record, this._text);

	/// <inheritdoc/>
	public string? ResolveTag(string? tag) =>
		!string.IsNullOrEmpty(tag) && this._text.Resolve(tag) is { } text ? TextTags.ToPlainText(text) : null;
}

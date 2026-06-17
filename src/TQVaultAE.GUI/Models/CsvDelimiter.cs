using System.ComponentModel;

namespace TQVaultAE.GUI.Models;

internal enum CsvDelimiter
{
	[Description(",")]
	Comma,
	[Description("\t")]
	Tab,
	[Description("|")]
	Pipe,
	[Description(":")]
	Colon,
	[Description(";")]
	Semicolon
}


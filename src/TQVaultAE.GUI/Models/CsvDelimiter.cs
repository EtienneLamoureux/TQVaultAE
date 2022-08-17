using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


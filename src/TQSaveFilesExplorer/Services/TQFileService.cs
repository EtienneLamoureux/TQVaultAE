using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnumsNET;
using System.Text.RegularExpressions;
using TQ.SaveFilesExplorer.Entities;

namespace TQ.SaveFilesExplorer.Services
{
	public class TQFileService
	{
		public TQFile ReadFile(string path)
		{
			var file = TQFile.ReadFile(path);
			file.Parse();
			file.Analyse();
			return file;
		}
	}
}

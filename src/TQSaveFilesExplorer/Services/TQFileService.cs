using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnumsNET;
using System.Text.RegularExpressions;
using TQ.SaveFilesExplorer.Entities;
using AutoMapper;

namespace TQ.SaveFilesExplorer.Services
{
	public class TQFileService
	{
		private IMapper mapper;

		public TQFileService(IMapper mapper)
		{
			this.mapper = mapper;
		}

		public TQFile ReadFile(string path)
		{
			var file = TQFile.ReadFile(path, mapper);
			file.Parse();
			file.Analyse();
			return file;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities.Results
{
	public class GamePathEntry
	{
		public readonly string Path;
		public readonly string DisplayName;
		public GamePathEntry(string path, string displayName)
		{
			this.Path = path;
			this.DisplayName = displayName;
		}
		public override string ToString()
		{
			return DisplayName ?? Path ?? "Empty";
		}
	}
}
